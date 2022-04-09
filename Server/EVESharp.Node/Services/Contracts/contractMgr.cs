using System;
using System.Collections.Generic;
using EVESharp.EVE.Client.Exceptions.contractMgr;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.EVE.Wallet;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Client.Contracts;
using EVESharp.Node.Notifications.Nodes.Inventory;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;
using Container = EVESharp.Node.Inventory.Items.Types.Container;

namespace EVESharp.Node.Services.Contracts;

public class contractMgr : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Station;

    // TODO: THE TYPEID FOR THE BOX IS 24445
    private ContractDB                  DB                  { get; }
    private ItemDB                      ItemDB              { get; }
    private MarketDB                    MarketDB            { get; }
    private CharacterDB                 CharacterDB         { get; }
    private ItemFactory                 ItemFactory         { get; }
    private TypeManager                 TypeManager         => ItemFactory.TypeManager;
    private SystemManager               SystemManager       => ItemFactory.SystemManager;
    private Notifications.Notifications Notifications { get; }
    private WalletManager               WalletManager       { get; }
    private Node.Dogma.Dogma            Dogma               { get; }

    public contractMgr (
        ContractDB    db, ItemDB itemDB, MarketDB marketDB, CharacterDB characterDB, ItemFactory itemFactory, Notifications.Notifications notifications,
        WalletManager walletManager, Node.Dogma.Dogma dogma
    )
    {
        DB                  = db;
        ItemDB              = itemDB;
        MarketDB            = marketDB;
        CharacterDB         = characterDB;
        ItemFactory         = itemFactory;
        Notifications = notifications;
        WalletManager       = walletManager;
        Dogma               = dogma;
    }

    public PyDataType NumRequiringAttention (CallInformation call)
    {
        // check for contracts that we've been outbid at and send notifications
        // TODO: HANDLE CORPORATION CONTRACTS TOO!
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        List <int> outbidContracts   = DB.FetchLoginCharacterContractBids (callerCharacterID);
        List <int> assignedContracts = DB.FetchLoginCharacterContractAssigned (callerCharacterID);

        foreach (int contractID in outbidContracts)
            Dogma.QueueMultiEvent (callerCharacterID, new OnContractOutbid (contractID));
        foreach (int contractID in assignedContracts)
            Dogma.QueueMultiEvent (callerCharacterID, new OnContractAssigned (contractID));

        return DB.NumRequiringAttention (callerCharacterID, call.Session.CorporationID);
    }

    public PyDataType NumOutstandingContracts (CallInformation call)
    {
        return DB.NumOutstandingContracts (call.Session.EnsureCharacterIsSelected (), call.Session.CorporationID);
    }

    public PyDataType CollectMyPageInfo (PyDataType ignoreList, CallInformation call)
    {
        // TODO: TAKE INTO ACCOUNT THE IGNORE LIST

        return DB.CollectMyPageInfo (call.Session.EnsureCharacterIsSelected (), call.Session.CorporationID);
    }

    public PyDataType GetContractListForOwner (PyInteger ownerID, PyInteger contractStatus, PyInteger contractType, PyBool issuedToUs, CallInformation call)
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
            call.Session.EnsureCharacterIsSelected (), call.Session.CorporationID, ownerID,
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

    public PyDataType GetItemsInStation (PyInteger stationID, PyInteger forCorp, CallInformation call)
    {
        // TODO: HANDLE CORPORATION!
        if (forCorp == 1)
            throw new CustomError ("This call doesn't support forCorp parameter yet!");

        return DB.GetItemsInStationForPlayer (call.Session.EnsureCharacterIsSelected (), stationID);
    }

    private void PrepareItemsForCourierOrAuctionContract (
        MySqlConnection connection, ulong   contractID,
        PyList <PyList> itemList,   Station station, int ownerID, int shipID
    )
    {
        // create the container in the system to ensure it's not visible to the player
        Container container = ItemFactory.CreateSimpleItem (
            TypeManager [Types.PlasticWrap],
            ItemFactory.LocationSystem.ID, station.ID, Flags.None
        ) as Container;

        Dictionary <int, ContractDB.ItemQuantityEntry> items =
            DB.PrepareItemsForContract (connection, contractID, itemList, station, ownerID, container.ID, shipID);

        double volume = 0;

        // build notification for item changes
        OnItemChange changes            = new OnItemChange ();
        long         stationNode        = SystemManager.GetNodeStationBelongsTo (station.ID);
        bool         stationBelongsToUs = SystemManager.StationBelongsToUs (station.ID);

        // notify the changes in the items to the nodes
        foreach ((int _, ContractDB.ItemQuantityEntry item) in items)
        {
            if (stationNode == 0 || stationBelongsToUs)
            {
                ItemEntity entity = ItemFactory.LoadItem (item.ItemID);

                entity.LocationID = container.ID;
                entity.Persist ();

                // notify the character
                Notifications.NotifyCharacter (ownerID, Node.Notifications.Client.Inventory.OnItemChange.BuildLocationChange (entity, station.ID));
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
        DB.UpdateContractCrateAndVolume (ref connection, contractID, container.ID, volume);
    }

    public PyDataType CreateContract (
        PyInteger contractType, PyInteger availability,            PyInteger assigneeID,
        PyInteger expireTime,   PyInteger courierContractDuration, PyInteger startStationID, PyInteger endStationID, PyInteger       priceOrStartingBid,
        PyInteger reward,       PyInteger collateralOrBuyoutPrice, PyString  title,          PyString  description,  CallInformation call
    )
    {
        if (assigneeID != null && (ItemRanges.IsNPC (assigneeID) || ItemRanges.IsNPCCorporationID (assigneeID)))
            throw new ConNPCNotAllowed ();

        // check for limits on contract creation
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        if (expireTime < 1440 || (courierContractDuration < 1 && contractType == (int) ContractTypes.Courier))
            throw new ConDurationZero ();
        if (startStationID == endStationID)
            throw new ConDestinationSame ();

        if (call.NamedPayload.TryGetValue ("forCorp", out PyBool forCorp) == false)
            forCorp = false;

        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

        if (forCorp == false)
        {
            // check limits for the character
            long maximumContracts = 1 + 4 * character.GetSkillLevel (Types.Contracting);

            if (maximumContracts <= DB.GetOutstandingContractsCountForPlayer (callerCharacterID))
                throw new ConTooManyContractsMax (maximumContracts);
        }
        else
        {
            throw new CustomError ("Not supported yet!");
        }

        Station station = ItemFactory.GetStaticStation (startStationID);

        using MySqlConnection connection = MarketDB.AcquireMarketLock ();

        try
        {
            // take reward from the character
            if (reward > 0)
            {
                using Wallet wallet = WalletManager.AcquireWallet (callerCharacterID, Keys.MAIN);
                {
                    wallet.EnsureEnoughBalance (reward);
                    wallet.CreateJournalRecord (MarketReference.ContractRewardAdded, null, null, -reward);
                }
            }

            // named payload contains itemList, flag, requestItemTypeList and forCorp
            ulong contractID = DB.CreateContract (
                connection, call.Session.EnsureCharacterIsSelected (),
                call.Session.CorporationID, call.Session.AllianceID, (ContractTypes) (int) contractType, availability,
                assigneeID ?? 0, expireTime, courierContractDuration, startStationID, endStationID, priceOrStartingBid,
                reward, collateralOrBuyoutPrice, title, description, Keys.MAIN
            );

            // TODO: take broker's tax, deposit and sales tax

            switch ((int) contractType)
            {
                case (int) ContractTypes.ItemExchange:
                case (int) ContractTypes.Auction:
                case (int) ContractTypes.Courier:
                    this.PrepareItemsForCourierOrAuctionContract (
                        connection,
                        contractID,
                        (call.NamedPayload ["itemList"] as PyList).GetEnumerable <PyList> (),
                        station,
                        callerCharacterID,
                        (int) call.Session.ShipID
                    );

                    break;
                case (int) ContractTypes.Loan:
                    break;
                default:
                    throw new CustomError ("Unknown contract type");
            }

            if (contractType == (int) ContractTypes.ItemExchange)
                DB.PrepareRequestedItems (connection, contractID, (call.NamedPayload ["requestItemTypeList"] as PyList).GetEnumerable <PyList> ());

            return contractID;
        }
        finally
        {
            MarketDB.ReleaseMarketLock (connection);
        }
    }

    public PyDataType GetContractList (PyObjectData filtersKeyval, CallInformation call)
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
            call.Session.EnsureCharacterIsSelected (), call.Session.CorporationID
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

    public PyDataType GetContract (PyInteger contractID, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

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

    public PyDataType DeleteContract (PyInteger contractID, PyObjectData keyVal, CallInformation call)
    {
        using MySqlConnection connection = MarketDB.AcquireMarketLock ();

        try
        {
            // get contract type and status

            // get the items back to where they belong (if any)

            // 
        }
        finally
        {
            MarketDB.ReleaseMarketLock (connection);
        }

        return null;
    }

    public PyDataType SplitStack (
        PyInteger stationID, PyInteger       itemID, PyInteger newStack, PyInteger forCorp,
        PyInteger flag,      CallInformation call
    )
    {
        return null;
    }

    public PyDataType GetItemsInContainer (
        PyInteger locationID, PyInteger       containerID, PyInteger forCorp,
        PyInteger flag,       CallInformation call
    )
    {
        return DB.GetItemsInContainer (call.Session.EnsureCharacterIsSelected (), containerID);
    }

    public PyDataType GetMyExpiredContractList (PyBool isCorp, CallInformation call)
    {
        int ownerID = 0;

        if (isCorp == true)
            ownerID = call.Session.CorporationID;
        else
            ownerID = call.Session.EnsureCharacterIsSelected ();

        List <int> contractList = DB.GetContractList (
            null, 0, null, null, null, null, null, null,
            null, 0, 0,
            null, null, call.Session.EnsureCharacterIsSelected (), call.Session.CorporationID, ownerID, null, true, true
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

    public PyDataType GetMyBids (PyInteger isCorp, CallInformation call)
    {
        return this.GetMyBids (isCorp == 1, call);
    }

    public PyDataType GetMyBids (PyBool isCorp, CallInformation call)
    {
        int ownerID = 0;

        if (isCorp == true)
            ownerID = call.Session.CorporationID;
        else
            ownerID = call.Session.EnsureCharacterIsSelected ();

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

    public PyDataType GetMyCurrentContractList (PyBool acceptedByMe, PyBool isCorp, CallInformation call)
    {
        int ownerID = 0;

        if (isCorp == true)
            ownerID = call.Session.CorporationID;
        else
            ownerID = call.Session.EnsureCharacterIsSelected ();

        List <int> contractList = null;

        if (acceptedByMe == true)
            contractList = DB.GetContractListByAcceptor (ownerID);
        else
            contractList = DB.GetContractList (
                null, 0, null, null, new PyList <PyInteger> (1) {[0] = ownerID},
                null, null, null, null, 0, 0, null,
                null, call.Session.EnsureCharacterIsSelected (), call.Session.CorporationID,
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

    public PyDataType PlaceBid (PyInteger contractID, PyInteger quantity, PyBool forCorp, PyObjectData locationData, CallInformation call)
    {
        using MySqlConnection connection = MarketDB.AcquireMarketLock ();

        try
        {
            // TODO: SUPPORT PROPER CORP WALLET
            int bidderID = call.Session.EnsureCharacterIsSelected ();

            if (forCorp == true)
            {
                bidderID = call.Session.CorporationID;

                throw new UserError ("Corp bidding is not supported for now!");
            }

            ContractDB.Contract contract = DB.GetContract (connection, contractID);

            // ensure the contract is still in progress
            if (contract.Status != ContractStatus.Outstanding)
                throw new ConAuctionAlreadyClaimed ();

            DB.GetMaximumBid (connection, contractID, out int maximumBidderID, out int maximumBid);

            // calculate next bid slot
            int nextMinimumBid = maximumBid + (int) Math.Max (0.1 * (double) contract.Price, 1000);

            if (quantity < nextMinimumBid)
                throw new ConBidTooLow (quantity, nextMinimumBid);

            // take the bid's money off the wallet
            using Wallet bidderWallet = WalletManager.AcquireWallet (bidderID, Keys.MAIN);
            {
                bidderWallet.EnsureEnoughBalance (quantity);
                bidderWallet.CreateJournalRecord (MarketReference.ContractAuctionBid, null, null, -quantity);
            }

            // check who we'd outbid and notify them
            DB.GetOutbids (connection, contractID, quantity, out List <int> characterIDs, out List <int> corporationIDs);

            OnContractOutbid notification = new OnContractOutbid (contractID);

            foreach (int corporationID in corporationIDs)
                if (corporationID != bidderID)
                    Notifications.NotifyCorporation (corporationID, notification);

            foreach (int characterID in characterIDs)
                if (characterID != bidderID)
                    Notifications.NotifyCharacter (characterID, notification);

            // finally place the bid
            ulong bidID = DB.PlaceBid (connection, contractID, quantity, bidderID, forCorp);

            // return the money for the player that was the highest bidder
            using Wallet maximumBidderWallet = WalletManager.AcquireWallet (maximumBidderID, Keys.MAIN);
            {
                maximumBidderWallet.CreateJournalRecord (MarketReference.ContractAuctionBidRefund, null, null, maximumBid);
            }

            return bidID;
        }
        finally
        {
            MarketDB.ReleaseMarketLock (connection);
        }
    }

    private void AcceptItemExchangeContract (
        MySqlConnection connection, Session session, ContractDB.Contract contract, Station station, int ownerID, Flags flag = Flags.Hangar
    )
    {
        List <ContractDB.ItemQuantityEntry> offeredItems = DB.GetOfferedItems (connection, contract.ID);
        Dictionary <int, int>               itemsToCheck = DB.GetRequiredItemTypeIDs (connection, contract.ID);
        List <ContractDB.ItemQuantityEntry> changedItems = DB.CheckRequiredItemsAtStation (connection, station, ownerID, contract.IssuerID, flag, itemsToCheck);

        // extract the crate
        DB.ExtractCrate (connection, contract.CrateID, station.ID, ownerID);

        long stationNode = SystemManager.GetNodeStationBelongsTo (station.ID);

        if (stationNode == 0 || SystemManager.StationBelongsToUs (station.ID))
        {
            foreach (ContractDB.ItemQuantityEntry change in changedItems)
            {
                ItemEntity item = ItemFactory.LoadItem (change.ItemID);

                if (change.Quantity == 0)
                {
                    // remove item from the meta inventories
                    ItemFactory.MetaInventoryManager.OnItemDestroyed (item);
                    // temporarily move the item to the recycler, let the current owner know
                    item.LocationID = ItemFactory.LocationRecycler.ID;
                    Dogma.QueueMultiEvent (
                        session.EnsureCharacterIsSelected (), Node.Notifications.Client.Inventory.OnItemChange.BuildLocationChange (item, station.ID)
                    );
                    // now set the item to the correct owner and place and notify it's new owner
                    // TODO: TAKE forCorp INTO ACCOUNT
                    item.LocationID = station.ID;
                    item.OwnerID    = contract.IssuerID;
                    Notifications.NotifyCharacter (contract.IssuerID, Node.Notifications.Client.Inventory.OnItemChange.BuildNewItemChange (item));
                    // add the item back to meta inventories if required
                    ItemFactory.MetaInventoryManager.OnItemLoaded (item);
                }
                else
                {
                    int oldQuantity = item.Quantity;
                    item.Quantity = change.Quantity;
                    Dogma.QueueMultiEvent (
                        session.EnsureCharacterIsSelected (), Node.Notifications.Client.Inventory.OnItemChange.BuildQuantityChange (item, oldQuantity)
                    );

                    item.Persist ();

                    // unload the item if required
                    ItemFactory.UnloadItem (item);
                }
            }

            // move the offered items
            foreach (ContractDB.ItemQuantityEntry entry in offeredItems)
            {
                ItemEntity item = ItemFactory.LoadItem (entry.ItemID);

                item.LocationID = station.ID;
                item.OwnerID    = ownerID;

                Dogma.QueueMultiEvent (
                    session.EnsureCharacterIsSelected (), Node.Notifications.Client.Inventory.OnItemChange.BuildLocationChange (item, contract.CrateID)
                );

                item.Persist ();

                // unload the item if possible
                ItemFactory.UnloadItem (item);
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
                    ItemEntity item = ItemFactory.CreateSimpleItem (
                        TypeManager [change.TypeID], contract.IssuerID, station.ID, Flags.Hangar, change.OldQuantity - change.Quantity
                    );
                    // unload the created item
                    ItemFactory.UnloadItem (item);
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
        DB.UpdateContractStatus (ref connection, contract.ID, ContractStatus.Finished);
        DB.UpdateAcceptorID (ref connection, contract.ID, ownerID);
        DB.UpdateAcceptedDate (ref connection, contract.ID);
        DB.UpdateCompletedDate (ref connection, contract.ID);
        // notify the contract as being accepted
        if (contract.ForCorp == false)
            Notifications.NotifyCharacter (contract.IssuerID, new OnContractAccepted (contract.ID));
        else
            Notifications.NotifyCorporation (contract.IssuerCorpID, new OnContractAccepted (contract.ID));
    }

    public PyDataType AcceptContract (PyInteger contractID, PyBool forCorp, CallInformation call)
    {
        if (forCorp == true)
            throw new UserError ("Cannot accept contracts for corporation yet");

        using MySqlConnection connection = MarketDB.AcquireMarketLock ();

        try
        {
            int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

            // TODO: SUPPORT forCorp
            ContractDB.Contract contract = DB.GetContract (connection, contractID);

            if (contract.Status != ContractStatus.Outstanding)
                throw new ConContractNotOutstanding ();
            if (contract.ExpireTime < DateTime.UtcNow.ToFileTimeUtc ())
                throw new ConContractExpired ();

            Station station = ItemFactory.GetStaticStation (contract.StationID);

            // TODO: CHECK REWARD/PRICE
            switch (contract.Type)
            {
                case ContractTypes.ItemExchange:
                    this.AcceptItemExchangeContract (connection, call.Session, contract, station, callerCharacterID);

                    break;
                case ContractTypes.Auction:
                    throw new CustomError ("Auctions cannot be accepted!");
                case ContractTypes.Courier:
                    throw new CustomError ("Courier contracts not supported yet!");
                case ContractTypes.Loan:
                    throw new CustomError ("Loan contracts not supported yet!");
                case ContractTypes.Freeform:
                    throw new CustomError ("Freeform contracts not supported yet!");
                default:
                    throw new CustomError ("Unknown contract type to accept!");
            }
        }
        finally
        {
            MarketDB.ReleaseMarketLock (connection);
        }

        return null;
    }

    public PyDataType FinishAuction (PyInteger contractID, PyBool forCorp, CallInformation call)
    {
        return null;
    }

    public PyDataType HasFittedCharges (PyInteger stationID, PyInteger itemID, PyInteger forCorp, PyInteger flag, CallInformation call)
    {
        // TODO: IMPLEMENT THIS!
        return null;
    }
}