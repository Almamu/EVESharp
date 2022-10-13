using System;
using System.Collections.Generic;
using System.Data;
using EVESharp.Database;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Market;
using EVESharp.Database.Old;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.contractMgr;
using EVESharp.EVE.Market;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Contracts;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types;
using EVESharp.Node.Notifications.Nodes.Inventory;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Container = EVESharp.EVE.Data.Inventory.Items.Types.Container;

namespace EVESharp.Node.Services.Contracts;

[MustBeCharacter]
public class contractMgr : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Station;

    // TODO: THE TYPEID FOR THE BOX IS 24445
    private ContractDB          DB                 { get; }
    private ItemDB              ItemDB             { get; }
    private MarketDB            MarketDB           { get; }
    private OldCharacterDB         CharacterDB        { get; }
    private IItems              Items              { get; }
    private ITypes              Types              => this.Items.Types;
    private ISolarSystems       SolarSystems       { get; }
    private INotificationSender Notifications      { get; }
    private IWallets            Wallets            { get; }
    private IDogmaNotifications DogmaNotifications { get; }

    public contractMgr
    (
        ContractDB db,      ItemDB              itemDB, MarketDB marketDB, OldCharacterDB characterDB, IItems items, INotificationSender notificationSender,
        IWallets   wallets, IDogmaNotifications dogmaNotifications, ISolarSystems solarSystems
    )
    {
        DB                 = db;
        ItemDB             = itemDB;
        MarketDB           = marketDB;
        CharacterDB        = characterDB;
        Items              = items;
        Notifications      = notificationSender;
        this.Wallets       = wallets;
        DogmaNotifications = dogmaNotifications;
        SolarSystems       = solarSystems;
    }

    public PyDataType NumRequiringAttention (ServiceCall call)
    {
        // check for contracts that we've been outbid at and send notifications
        // TODO: HANDLE CORPORATION CONTRACTS TOO!
        int callerCharacterID = call.Session.CharacterID;

        List <int> outbidContracts   = DB.FetchLoginCharacterContractBids (callerCharacterID);
        List <int> assignedContracts = DB.FetchLoginCharacterContractAssigned (callerCharacterID);

        foreach (int contractID in outbidContracts)
            this.DogmaNotifications.QueueMultiEvent (callerCharacterID, new OnContractOutbid (contractID));

        foreach (int contractID in assignedContracts)
            this.DogmaNotifications.QueueMultiEvent (callerCharacterID, new OnContractAssigned (contractID));

        return DB.NumRequiringAttention (callerCharacterID, call.Session.CorporationID);
    }

    public PyDataType NumOutstandingContracts (ServiceCall call)
    {
        return DB.NumOutstandingContracts (call.Session.CharacterID, call.Session.CorporationID);
    }

    public PyDataType CollectMyPageInfo (ServiceCall call, PyDataType ignoreList)
    {
        // TODO: TAKE INTO ACCOUNT THE IGNORE LIST

        return DB.CollectMyPageInfo (call.Session.CharacterID, call.Session.CorporationID);
    }

    public PyDataType GetContractListForOwner (ServiceCall call, PyInteger ownerID, PyInteger contractStatus, PyInteger contractType, PyBool issuedToUs)
    {
        call.NamedPayload.TryGetValue ("startContractID", out PyInteger startContractID);
        int resultsPerPage = call.NamedPayload ["num"] as PyInteger;

        // limit the number of results to 100
        if (resultsPerPage > 100)
            resultsPerPage = 100;

        PyList <PyInteger> issuedByIDs = null;

        if (issuedToUs == false)
            issuedByIDs = new PyList <PyInteger> (1) {[0] = ownerID};

        List <int> contractList = DB.GetContractList (
            startContractID, resultsPerPage, null, null, issuedByIDs, issuedToUs == true ? ownerID : null,
            null, null, null, 0, 0, contractType, null,
            call.Session.CharacterID, call.Session.CorporationID, ownerID,
            contractStatus, true
        );

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["contracts"] = DB.GetInformationForContractList (contractList),
                ["bids"]      = DB.GetBidsForContractList (contractList),
                ["items"]     = DB.GetItemsForContractList (contractList)
            }
        );
    }

    public PyDataType GetItemsInStation (ServiceCall call, PyInteger stationID, PyInteger forCorp)
    {
        // TODO: HANDLE CORPORATION!
        if (forCorp == 1)
            throw new CustomError ("This call doesn't support forCorp parameter yet!");

        return DB.GetItemsInStationForPlayer (call.Session.CharacterID, stationID);
    }

    private void PrepareItemsForCourierOrAuctionContract
    (
        DbLock dbLock, ulong   contractID,
        PyList <PyList> itemList,   Station station, int ownerID, int shipID
    )
    {
        // create the container in the system to ensure it's not visible to the player
        Container container = this.Items.CreateSimpleItem (
            this.Types [TypeID.PlasticWrap],
            this.Items.LocationSystem.ID, station.ID, Flags.None
        ) as Container;

        Dictionary <int, ContractDB.ItemQuantityEntry> items =
            DB.PrepareItemsForContract (dbLock, contractID, itemList, station, ownerID, container.ID, shipID);

        double volume = 0;

        // build notification for item changes
        OnItemChange changes            = new OnItemChange ();
        long         stationNode        = this.SolarSystems.GetNodeStationBelongsTo (station.ID);
        bool         stationBelongsToUs = this.SolarSystems.StationBelongsToUs (station.ID);

        // notify the changes in the items to the nodes
        foreach ((int _, ContractDB.ItemQuantityEntry item) in items)
        {
            if (stationNode == 0 || stationBelongsToUs)
            {
                ItemEntity entity = this.Items.LoadItem (item.ItemID);

                entity.LocationID = container.ID;
                entity.Persist ();

                // notify the character
                Notifications.NotifyCharacter (ownerID, EVE.Notifications.Inventory.OnItemChange.BuildLocationChange (entity, station.ID));
            }
            else
            {
                // queue the notification
                changes.AddChange (item.ItemID, "locationID", station.ID, container.ID);
            }

            // ensure the volume is taken into account
            volume += item.Volume;
        }

        // notify the proper node if needed
        if (changes.Updates.Count > 0)
            Notifications.NotifyNode (stationNode, changes);

        // update the contract with the crate and the new volume
        DB.UpdateContractCrateAndVolume (dbLock, contractID, container.ID, volume);
    }

    public PyDataType CreateContract
    (
        ServiceCall call,       PyInteger contractType,            PyInteger availability,   PyInteger assigneeID,
        PyInteger       expireTime, PyInteger courierContractDuration, PyInteger startStationID, PyInteger endStationID, PyInteger priceOrStartingBid,
        PyInteger       reward,     PyInteger collateralOrBuyoutPrice, PyString  title,          PyString  description
    )
    {
        if (assigneeID != null && (ItemRanges.IsNPC (assigneeID) || ItemRanges.IsNPCCorporationID (assigneeID)))
            throw new ConNPCNotAllowed ();

        // check for limits on contract creation
        int callerCharacterID = call.Session.CharacterID;

        if (expireTime < 1440 || (courierContractDuration < 1 && contractType == (int) ContractTypes.Courier))
            throw new ConDurationZero ();

        if (startStationID == endStationID)
            throw new ConDestinationSame ();

        if (call.NamedPayload.TryGetValue ("forCorp", out PyBool forCorp) == false)
            forCorp = false;

        Character character = this.Items.GetItem <Character> (callerCharacterID);

        if (forCorp == false)
        {
            // check limits for the character
            long maximumContracts = 1 + 4 * character.GetSkillLevel (TypeID.Contracting);

            if (maximumContracts <= DB.GetOutstandingContractsCountForPlayer (callerCharacterID))
                throw new ConTooManyContractsMax (maximumContracts);
        }
        else
        {
            throw new CustomError ("Not supported yet!");
        }

        Station station = this.Items.GetStaticStation (startStationID);

        using DbLock dbLock = MarketDB.AcquireMarketLock ();
        
        // take reward from the character
        if (reward > 0)
        {
            using IWallet wallet = this.Wallets.AcquireWallet (callerCharacterID, WalletKeys.MAIN);

            {
                wallet.EnsureEnoughBalance (reward);
                wallet.CreateJournalRecord (MarketReference.ContractRewardAdded, null, null, -reward);
            }
        }

        // named payload contains itemList, flag, requestItemTypeList and forCorp
        ulong contractID = DB.CreateContract (
            dbLock, call.Session.CharacterID,
            call.Session.CorporationID, call.Session.AllianceID, (ContractTypes) (int) contractType, availability,
            assigneeID ?? 0, expireTime, courierContractDuration, startStationID, endStationID, priceOrStartingBid,
            reward, collateralOrBuyoutPrice, title, description, WalletKeys.MAIN
        );

        // TODO: take broker's tax, deposit and sales tax

        switch ((int) contractType)
        {
            case (int) ContractTypes.ItemExchange:
            case (int) ContractTypes.Auction:
            case (int) ContractTypes.Courier:
                this.PrepareItemsForCourierOrAuctionContract (
                    dbLock,
                    contractID,
                    (call.NamedPayload ["itemList"] as PyList).GetEnumerable <PyList> (),
                    station,
                    callerCharacterID,
                    (int) call.Session.ShipID
                );
                break;

            case (int) ContractTypes.Loan: break;
            default:                       throw new CustomError ("Unknown contract type");
        }

        if (contractType == (int) ContractTypes.ItemExchange)
            DB.PrepareRequestedItems (dbLock, contractID, (call.NamedPayload ["requestItemTypeList"] as PyList).GetEnumerable <PyList> ());

        return contractID;
    }

    public PyDataType GetContractList (ServiceCall call, PyObjectData filtersKeyval)
    {
        PyDictionary <PyString, PyDataType> filters        = KeyVal.ToDictionary (filtersKeyval).GetEnumerable <PyString, PyDataType> ();
        PyList <PyInteger>                  notIssuedByIDs = null;
        PyList <PyInteger>                  issuedByIDs    = null;

        call.NamedPayload.TryGetValue ("startContractID", out PyInteger startContractID);
        int resultsPerPage = call.NamedPayload ["num"] as PyInteger;

        filters.TryGetValue ("regionID",       out PyInteger regionID);
        filters.TryGetValue ("stationID",      out PyInteger stationID);
        filters.TryGetValue ("solarSystemID",  out PyInteger solarSystemID);
        filters.TryGetValue ("itemTypeID",     out PyInteger itemTypeID);
        filters.TryGetValue ("assigneeID",     out PyInteger assigneeID);
        filters.TryGetValue ("itemGroupID",    out PyInteger itemGroupID);
        filters.TryGetValue ("itemCategoryID", out PyInteger itemCategoryID);
        filters.TryGetValue ("priceMax",       out PyInteger priceMax);
        filters.TryGetValue ("priceMin",       out PyInteger priceMin);
        filters.TryGetValue ("type",           out PyInteger type);
        filters.TryGetValue ("description",    out PyString description);

        if (priceMax < 0 || priceMin < 0 || priceMax < priceMin)
            throw new ConMinMaxPriceError ();

        if (filters.TryGetValue ("issuedByIDs", out PyList issuedIDs) && issuedIDs is not null)
            issuedByIDs = issuedIDs.GetEnumerable <PyInteger> ();

        if (filters.TryGetValue ("notIssuedByIDs", out PyList notIssuedIDs) && notIssuedIDs is not null)
            notIssuedByIDs = notIssuedIDs.GetEnumerable <PyInteger> ();

        // limit the number of results to 100
        if (resultsPerPage > 100)
            resultsPerPage = 100;

        int? locationID = null;

        if (stationID is not null)
            locationID = stationID;
        else if (solarSystemID is not null)
            locationID = solarSystemID;
        else if (regionID is not null)
            locationID = regionID;

        List <int> contractList = DB.GetContractList (
            startContractID, resultsPerPage, itemTypeID, notIssuedByIDs, issuedByIDs, assigneeID,
            locationID, itemGroupID, itemCategoryID, priceMax ?? 0, priceMin ?? 0, type, description,
            call.Session.CharacterID, call.Session.CorporationID
        );

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["contracts"] = DB.GetInformationForContractList (contractList),
                ["bids"]      = DB.GetBidsForContractList (contractList),
                ["items"]     = DB.GetItemsForContractList (contractList)
            }
        );
    }

    public PyDataType GetContract (ServiceCall call, PyInteger contractID)
    {
        int callerCharacterID = call.Session.CharacterID;

        // TODO: Check for regionID ConWrongRegion

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["contract"] = DB.GetContractInformation (contractID, callerCharacterID, call.Session.CorporationID),
                ["bids"]     = DB.GetContractBids (contractID, callerCharacterID, call.Session.CorporationID),
                ["items"]    = DB.GetContractItems (contractID, callerCharacterID, call.Session.CorporationID)
            }
        );
    }

    public PyDataType DeleteContract (ServiceCall call, PyInteger contractID, PyObjectData keyVal)
    {
        using DbLock dbLock = MarketDB.AcquireMarketLock ();

        // get contract type and status

        // get the items back to where they belong (if any)

        // 

        return null;
    }

    public PyDataType SplitStack
    (
        ServiceCall call, PyInteger stationID, PyInteger itemID, PyInteger newStack, PyInteger forCorp,
        PyInteger       flag
    )
    {
        return null;
    }

    public PyDataType GetItemsInContainer
    (
        ServiceCall call, PyInteger locationID, PyInteger containerID, PyInteger forCorp,
        PyInteger       flag
    )
    {
        return DB.GetItemsInContainer (call.Session.CharacterID, containerID);
    }

    public PyDataType GetMyExpiredContractList (ServiceCall call, PyBool isCorp)
    {
        int ownerID = 0;

        if (isCorp == true)
            ownerID = call.Session.CorporationID;
        else
            ownerID = call.Session.CharacterID;

        List <int> contractList = DB.GetContractList (
            null, 0, null, null, null, null, null, null,
            null, 0, 0,
            null, null, call.Session.CharacterID, call.Session.CorporationID, ownerID, null, true, true
        );

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["contracts"] = DB.GetInformationForContractList (contractList),
                ["bids"]      = DB.GetBidsForContractList (contractList),
                ["items"]     = DB.GetItemsForContractList (contractList)
            }
        );
    }

    public PyDataType GetMyBids (ServiceCall call, PyInteger isCorp)
    {
        return this.GetMyBids (call, isCorp == 1);
    }

    public PyDataType GetMyBids (ServiceCall call, PyBool isCorp)
    {
        int ownerID = 0;

        if (isCorp == true)
            ownerID = call.Session.CorporationID;
        else
            ownerID = call.Session.CharacterID;

        List <int> contractList = DB.GetContractListByOwnerBids (ownerID);

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["contracts"] = DB.GetInformationForContractList (contractList),
                ["bids"]      = DB.GetBidsForContractList (contractList),
                ["items"]     = DB.GetItemsForContractList (contractList)
            }
        );
    }

    public PyDataType GetMyCurrentContractList (ServiceCall call, PyBool acceptedByMe, PyBool isCorp)
    {
        int ownerID = 0;

        if (isCorp == true)
            ownerID = call.Session.CorporationID;
        else
            ownerID = call.Session.CharacterID;

        List <int> contractList = null;

        if (acceptedByMe == true)
            contractList = DB.GetContractListByAcceptor (ownerID);
        else
            contractList = DB.GetContractList (
                null, 0, null, null, new PyList <PyInteger> (1) {[0] = ownerID},
                null, null, null, null, 0, 0, null,
                null, call.Session.CharacterID, call.Session.CorporationID,
                ownerID, (int) ContractStatus.InProgress, true, true
            );

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["contracts"] = DB.GetInformationForContractList (contractList),
                ["bids"]      = DB.GetBidsForContractList (contractList),
                ["items"]     = DB.GetItemsForContractList (contractList)
            }
        );
    }

    public PyDataType PlaceBid (ServiceCall call, PyInteger contractID, PyInteger quantity, PyBool forCorp, PyObjectData locationData)
    {
        using DbLock dbLock = MarketDB.AcquireMarketLock ();
    
        // TODO: SUPPORT PROPER CORP WALLET
        int bidderID = call.Session.CharacterID;

        if (forCorp == true)
        {
            bidderID = call.Session.CorporationID;

            throw new UserError ("Corp bidding is not supported for now!");
        }

        ContractDB.Contract contract = DB.GetContract (dbLock, contractID);

        // ensure the contract is still in progress
        if (contract.Status != ContractStatus.Outstanding)
            throw new ConAuctionAlreadyClaimed ();

        DB.GetMaximumBid (dbLock, contractID, out int maximumBidderID, out int maximumBid);

        // calculate next bid slot
        int nextMinimumBid = maximumBid + (int) Math.Max (0.1 * (double) contract.Price, 1000);

        if (quantity < nextMinimumBid)
            throw new ConBidTooLow (quantity, nextMinimumBid);

        // take the bid's money off the wallet
        using IWallet bidderWallet = this.Wallets.AcquireWallet (bidderID, WalletKeys.MAIN);

        {
            bidderWallet.EnsureEnoughBalance (quantity);
            bidderWallet.CreateJournalRecord (MarketReference.ContractAuctionBid, null, null, -quantity);
        }

        // check who we'd outbid and notify them
        DB.GetOutbids (dbLock, contractID, quantity, out List <int> characterIDs, out List <int> corporationIDs);

        OnContractOutbid notification = new OnContractOutbid (contractID);

        foreach (int corporationID in corporationIDs)
            if (corporationID != bidderID)
                Notifications.NotifyCorporation (corporationID, notification);

        foreach (int characterID in characterIDs)
            if (characterID != bidderID)
                Notifications.NotifyCharacter (characterID, notification);

        // finally place the bid
        ulong bidID = DB.PlaceBid (dbLock, contractID, quantity, bidderID, forCorp);

        // return the money for the player that was the highest bidder
        using IWallet maximumBidderWallet = this.Wallets.AcquireWallet (maximumBidderID, WalletKeys.MAIN);

        {
            maximumBidderWallet.CreateJournalRecord (MarketReference.ContractAuctionBidRefund, null, null, maximumBid);
        }

        return bidID;
    }

    private void AcceptItemExchangeContract
    (
        DbLock dbLock, Session session, ContractDB.Contract contract, Station station, int ownerID, Flags flag = Flags.Hangar
    )
    {
        List <ContractDB.ItemQuantityEntry> offeredItems = DB.GetOfferedItems (dbLock, contract.ID);
        Dictionary <int, int>               itemsToCheck = DB.GetRequiredItemTypeIDs (dbLock, contract.ID);
        List <ContractDB.ItemQuantityEntry> changedItems = DB.CheckRequiredItemsAtStation (dbLock, station, ownerID, contract.IssuerID, flag, itemsToCheck);

        // extract the crate
        DB.ExtractCrate (dbLock, contract.CrateID, station.ID, ownerID);

        long stationNode = this.SolarSystems.GetNodeStationBelongsTo (station.ID);

        if (stationNode == 0 || this.SolarSystems.StationBelongsToUs (station.ID))
        {
            foreach (ContractDB.ItemQuantityEntry change in changedItems)
            {
                ItemEntity item = this.Items.LoadItem (change.ItemID);

                if (change.Quantity == 0)
                {
                    // remove item from the meta inventories
                    this.Items.MetaInventories.OnItemDestroyed (item);
                    // temporarily move the item to the recycler, let the current owner know
                    item.LocationID = this.Items.LocationRecycler.ID;

                    this.DogmaNotifications.QueueMultiEvent (
                        session.CharacterID, EVE.Notifications.Inventory.OnItemChange.BuildLocationChange (item, station.ID)
                    );

                    // now set the item to the correct owner and place and notify it's new owner
                    // TODO: TAKE forCorp INTO ACCOUNT
                    item.LocationID = station.ID;
                    item.OwnerID    = contract.IssuerID;
                    Notifications.NotifyCharacter (contract.IssuerID, EVE.Notifications.Inventory.OnItemChange.BuildNewItemChange (item));
                    // add the item back to meta inventories if required
                    this.Items.MetaInventories.OnItemLoaded (item);
                }
                else
                {
                    int oldQuantity = item.Quantity;
                    item.Quantity = change.Quantity;

                    this.DogmaNotifications.QueueMultiEvent (
                        session.CharacterID, EVE.Notifications.Inventory.OnItemChange.BuildQuantityChange (item, oldQuantity)
                    );

                    item.Persist ();

                    // unload the item if required
                    this.Items.UnloadItem (item);
                }
            }

            // move the offered items
            foreach (ContractDB.ItemQuantityEntry entry in offeredItems)
            {
                ItemEntity item = this.Items.LoadItem (entry.ItemID);

                item.LocationID = station.ID;
                item.OwnerID    = ownerID;

                this.DogmaNotifications.QueueMultiEvent (
                    session.CharacterID, EVE.Notifications.Inventory.OnItemChange.BuildLocationChange (item, contract.CrateID)
                );

                item.Persist ();

                // unload the item if possible
                this.Items.UnloadItem (item);
            }
        }
        else
        {
            OnItemChange changes = new OnItemChange ();

            foreach (ContractDB.ItemQuantityEntry change in changedItems)
                if (change.Quantity == 0)
                {
                    changes
                        .AddChange (change.ItemID, "locationID", contract.CrateID,  station.ID)
                        .AddChange (change.ItemID, "ownerID",    contract.IssuerID, ownerID);
                }
                else
                {
                    // change the item quantity
                    changes.AddChange (change.ItemID, "quantity", change.OldQuantity, change.Quantity);

                    // create a new item and notify the new node about it
                    // TODO: HANDLE BLUEPRINTS TOO! RIGHT NOW NO DATA IS COPIED FOR THEM
                    ItemEntity item = this.Items.CreateSimpleItem (
                        this.Types [change.TypeID], contract.IssuerID, station.ID, Flags.Hangar, change.OldQuantity - change.Quantity
                    );

                    // unload the created item
                    this.Items.UnloadItem (item);
                    changes.AddChange (item.ID, "location", 0, station.ID);
                }

            // move the offered items
            foreach (ContractDB.ItemQuantityEntry entry in offeredItems)
                // TODO: TAKE INTO ACCOUNT forCorp
                changes
                    .AddChange (entry.ItemID, "locationID", contract.CrateID,  station.ID)
                    .AddChange (entry.ItemID, "ownerID",    contract.IssuerID, station.ID);
        }

        // the contract was properly accepted, update it's status
        DB.UpdateContractStatus (dbLock, contract.ID, ContractStatus.Finished);
        DB.UpdateAcceptorID (dbLock, contract.ID, ownerID);
        DB.UpdateAcceptedDate (dbLock, contract.ID);
        DB.UpdateCompletedDate (dbLock, contract.ID);

        // notify the contract as being accepted
        if (contract.ForCorp == false)
            Notifications.NotifyCharacter (contract.IssuerID, new OnContractAccepted (contract.ID));
        else
            Notifications.NotifyCorporation (contract.IssuerCorpID, new OnContractAccepted (contract.ID));
    }

    public PyDataType AcceptContract (ServiceCall call, PyInteger contractID, PyBool forCorp)
    {
        if (forCorp == true)
            throw new UserError ("Cannot accept contracts for corporation yet");

        using DbLock dbLock = MarketDB.AcquireMarketLock ();
        
        int callerCharacterID = call.Session.CharacterID;

        // TODO: SUPPORT forCorp
        ContractDB.Contract contract = DB.GetContract (dbLock, contractID);

        if (contract.Status != ContractStatus.Outstanding)
            throw new ConContractNotOutstanding ();

        if (contract.ExpireTime < DateTime.UtcNow.ToFileTimeUtc ())
            throw new ConContractExpired ();

        Station station = this.Items.GetStaticStation (contract.StationID);

        // TODO: CHECK REWARD/PRICE
        switch (contract.Type)
        {
            case ContractTypes.ItemExchange:
                this.AcceptItemExchangeContract (dbLock, call.Session, contract, station, callerCharacterID);
                break;

            case ContractTypes.Auction:  throw new CustomError ("Auctions cannot be accepted!");
            case ContractTypes.Courier:  throw new CustomError ("Courier contracts not supported yet!");
            case ContractTypes.Loan:     throw new CustomError ("Loan contracts not supported yet!");
            case ContractTypes.Freeform: throw new CustomError ("Freeform contracts not supported yet!");
            default:                     throw new CustomError ("Unknown contract type to accept!");
        }
        
        return null;
    }

    public PyDataType FinishAuction (ServiceCall call, PyInteger contractID, PyBool forCorp)
    {
        return null;
    }

    public PyDataType HasFittedCharges (ServiceCall call, PyInteger stationID, PyInteger itemID, PyInteger forCorp, PyInteger flag)
    {
        // TODO: IMPLEMENT THIS!
        return null;
    }
}