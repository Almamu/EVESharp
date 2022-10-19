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
using EVESharp.EVE.Dogma.Interpreter.Opcodes;
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

// TODO: REWRITE THE USAGE OF THE CHARACTER CLASS HERE TO FETCH THE DATA OFF THE DATABASE TO PREVENT ISSUES ON MULTI-NODE INSTALLATIONS
[MustBeCharacter]
public class contractMgr : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Station;

    // TODO: THE TYPEID FOR THE BOX IS 24445
    private ContractDB          DB                 { get; }
    private ItemDB              ItemDB             { get; }
    private MarketDB            MarketDB           { get; }
    private OldCharacterDB      CharacterDB        { get; }
    private IItems              Items              { get; }
    private ITypes              Types              => this.Items.Types;
    private ISolarSystems       SolarSystems       { get; }
    private INotificationSender Notifications      { get; }
    private IWallets            Wallets            { get; }
    private IDogmaNotifications DogmaNotifications { get; }
    private IContracts          Contracts          { get; }

    public contractMgr
    (
        ContractDB db,      ItemDB              itemDB, MarketDB marketDB, OldCharacterDB characterDB, IItems items, INotificationSender notificationSender,
        IWallets   wallets, IDogmaNotifications dogmaNotifications, ISolarSystems solarSystems, IContracts contracts
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
        Contracts          = contracts;
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

    private void PrepareItemsForCourierOrAuctionContract (IContract contract, PyList <PyList> itemList, Station station, int ownerID, int shipID)
    {
        // ensure there's a crate created for the items
        contract.CreateCrate ();
        
        foreach (PyList itemEntryList in itemList)
        {
            PyList <PyInteger> itemEntry = itemEntryList.GetEnumerable <PyInteger> ();
            PyInteger          itemID    = itemEntry [0];
            PyInteger          quantity  = itemEntry [1];

            if (itemID == shipID)
                throw new ConCannotTradeCurrentShip ();

            contract.AddItem (itemID, quantity, ownerID, station.ID);
        }
        
        foreach (PyList itemEntryList in itemList)
        {
            PyList <PyInteger> itemEntry = itemEntryList.GetEnumerable <PyInteger> ();
            PyInteger          itemID    = itemEntry [0];

            ItemEntity entity = this.Items.LoadItem (itemID, out bool loadRequired);

            entity.LocationID = contract.CrateID;
            entity.Persist ();

            // notify the character
            Notifications.NotifyCharacter (ownerID, EVE.Notifications.Inventory.OnItemChange.BuildLocationChange (entity, station.ID));

            if (loadRequired)
                this.Items.UnloadItem (entity);
        }
    }

    private void PrepareRequestedItems (IContract contract, PyList <PyList> itemList)
    {
        foreach (PyList itemEntryList in itemList)
        {
            PyList <PyInteger> itemEntry = itemEntryList.GetEnumerable <PyInteger> ();
            PyInteger          typeID    = itemEntry [0];
            PyInteger          quantity  = itemEntry [1];
            
            contract.AddRequestedItem (typeID, quantity);
        }
    }

    public PyInteger CreateContract
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
        
        // take reward from the character
        if (reward > 0)
        {
            using (IWallet wallet = this.Wallets.AcquireWallet (callerCharacterID, WalletKeys.MAIN))
            {
                wallet.EnsureEnoughBalance (reward);
                wallet.CreateJournalRecord (MarketReference.ContractRewardAdded, null, null, -reward);
            }
        }

        // named payload contains itemList, flag, requestItemTypeList and forCorp
        using (IContract contract = Contracts.CreateContract (
                   call.Session.CharacterID,
                   call.Session.CorporationID, call.Session.AllianceID, (ContractTypes) (int) contractType, availability,
                   assigneeID ?? 0, expireTime, courierContractDuration, startStationID, endStationID, priceOrStartingBid,
                   reward, collateralOrBuyoutPrice, title, description, WalletKeys.MAIN
               ))
        {
            // TODO: take broker's tax, deposit and sales tax

            switch ((int) contractType)
            {
                case (int) ContractTypes.ItemExchange:
                case (int) ContractTypes.Auction:
                case (int) ContractTypes.Courier:
                    this.PrepareItemsForCourierOrAuctionContract (
                        contract,
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
                this.PrepareRequestedItems (contract, (call.NamedPayload ["requestItemTypeList"] as PyList).GetEnumerable <PyList> ());

            return contract.ID;            
        }
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
        using (IContract contract = Contracts.AcquireContract (contractID))
        {
            return contract.PlaceBid (quantity, call.Session, forCorp);
        }
    }

    public PyDataType AcceptContract (ServiceCall call, PyInteger contractID, PyBool forCorp)
    {
        using (IContract contract = Contracts.AcquireContract (contractID))
        {
            contract.Accept (call.Session);

            return null;
        }
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