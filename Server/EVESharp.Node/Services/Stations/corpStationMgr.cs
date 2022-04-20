using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.EVE.Client.Exceptions.corpStationMgr;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.EVE.Wallet;
using EVESharp.Node.Client.Notifications.Corporations;
using EVESharp.Node.Client.Notifications.Wallet;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Nodes.Corps;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Character = EVESharp.Node.Inventory.Items.Types.Character;
using Groups = EVESharp.EVE.StaticData.Inventory.Groups;
using ItemDB = EVESharp.Node.Database.ItemDB;
using Type = EVESharp.EVE.StaticData.Inventory.Type;

namespace EVESharp.Node.Services.Stations;

[MustBeCharacter]
public class corpStationMgr : ClientBoundService
{
    public override AccessLevel        AccessLevel   => AccessLevel.None;
    private         ItemFactory        ItemFactory   { get; }
    private         ItemDB             ItemDB        => ItemFactory.ItemDB;
    private         MarketDB           MarketDB      { get; }
    private         StationDB          StationDB     { get; }
    private         BillsDB            BillsDB       { get; }
    private         TypeManager        TypeManager   => ItemFactory.TypeManager;
    private         SystemManager      SystemManager => ItemFactory.SystemManager;
    private         WalletManager      WalletManager { get; }
    private         Constants          Constants     { get; }
    private         NotificationSender Notifications { get; }
    private         DatabaseConnection Database      { get; }

    public corpStationMgr (
        MarketDB            marketDB, StationDB stationDb, BillsDB billsDb, NotificationSender notificationSender, ItemFactory itemFactory, Constants constants,
        BoundServiceManager manager,  WalletManager walletManager, DatabaseConnection database
    ) : base (manager)
    {
        MarketDB      = marketDB;
        StationDB     = stationDb;
        BillsDB       = billsDb;
        Notifications = notificationSender;
        ItemFactory   = itemFactory;
        Constants     = constants;
        WalletManager = walletManager;
        Database      = database;
    }

    // TODO: PROVIDE OBJECTID PROPERLY
    protected corpStationMgr (
        MarketDB            marketDB, StationDB stationDb, BillsDB billsDb, NotificationSender notificationSender, ItemFactory itemFactory, Constants constants,
        BoundServiceManager manager,  WalletManager walletManager, Session session
    ) : base (manager, session, 0)
    {
        MarketDB      = marketDB;
        StationDB     = stationDb;
        BillsDB       = billsDb;
        Notifications = notificationSender;
        ItemFactory   = itemFactory;
        Constants     = constants;
        WalletManager = walletManager;
    }

    [MustBeInStation]
    public PyList GetCorporateStationOffice (CallInformation call)
    {
        return StationDB.GetOfficesList (call.Session.StationID);
    }

    [MustBeInStation]
    public PyDataType DoStandingCheckForStationService (PyInteger stationServiceID, CallInformation call)
    {
        // TODO: CHECK ACTUAL STANDING VALUE

        return null;
    }

    private List <Station> GetPotentialHomeStations (Session session)
    {
        int stationID = session.StationID;
        
        // TODO: CHECK STANDINGS TO ENSURE THIS STATION CAN BE USED
        // TODO: ADD CURRENT CORPORATION'S STATION BACK TO THE LIST
        List <Station> availableStations = new List <Station>
        {
            ItemFactory.Stations [stationID]
            // this.ItemFactory.Stations[character.Corporation.StationID]
        };

        return availableStations;
    }

    [MustBeInStation]
    public PyDataType GetPotentialHomeStations (CallInformation call)
    {
        List <Station> availableStations = this.GetPotentialHomeStations (call.Session);
        Rowset result = new Rowset (
            new PyList <PyString> (3)
            {
                [0] = "stationID",
                [1] = "typeID",
                [2] = "serviceMask"
            }
        );

        // build the return
        foreach (Station station in availableStations)
            result.Rows.Add (
                new PyList (3)
                {
                    [0] = station.ID,
                    [1] = station.Type.ID,
                    [2] = station.Operations.ServiceMask
                }
            );

        return result;
    }

    public PyDataType SetHomeStation (PyInteger stationID, CallInformation call)
    {
        int callerCharacterID = call.Session.CharacterID;

        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

        // ensure the station selected is in the list of available stations for this character
        Station station = this.GetPotentialHomeStations (call.Session).Find (x => x.ID == stationID);

        if (station is null)
            throw new CustomError ("The selected station is not in your allowed list...");

        // we could check if the current station is the same as the new one
        // but in reality it doesn't matter much, if the user wants to pay twice for it, sure, why not
        // in practice it doesn't make much difference
        // it also simplifies code that needs to communicate between nodes

        // what we need to do tho is ensure there's no other clone in here in the first place
        Rowset clones = ItemDB.GetClonesForCharacter (character.ID, (int) character.ActiveCloneID);

        foreach (PyList entry in clones.Rows)
        {
            int locationID = entry [2] as PyInteger;

            // if a clone is already there, refuse to have the medical in there
            if (locationID == stationID)
                throw new MedicalYouAlreadyHaveACloneContractAtThatStation ();
        }

        using Wallet wallet = WalletManager.AcquireWallet (character.ID, Keys.MAIN);
        {
            double contractCost = Constants.CostCloneContract;

            wallet.EnsureEnoughBalance (contractCost);
            wallet.CreateJournalRecord (MarketReference.CloneTransfer, null, station.ID, -contractCost, $"Moved clone to {station.Name}");
        }

        // TODO: REIMPLEMENT THIS
        // set clone's station
        // character.ActiveClone.LocationID = stationID;
        // character.ActiveClone.Persist();

        // persist character info
        character.Persist ();

        // invalidate client's cache
        // TODO: REIMPLEMENT THIS CALL
        // this.Client.ServiceManager.jumpCloneSvc.OnCloneUpdate(character.ID);

        return null;
    }

    [MustBeInStation]
    public PyBool DoesPlayersCorpHaveJunkAtStation (CallInformation call)
    {
        if (ItemRanges.IsNPCCorporationID (call.Session.CorporationID))
            return false;

        // TODO: PROPERLY IMPLEMENT THIS ONE
        return false;
    }

    [MustBeInStation]
    public PyTuple GetCorporateStationInfo (CallInformation call)
    {
        int stationID = call.Session.StationID;

        return new PyTuple (3)
        {
            [0] = StationDB.GetOfficesOwners (stationID), // eveowners list
            [1] = StationDB.GetCorporations (stationID), // corporations list
            [2] = StationDB.GetOfficesList (stationID) // offices list
        };
    }

    [MustBeInStation]
    public PyInteger GetNumberOfUnrentedOffices (CallInformation call)
    {
        int stationID = call.Session.StationID;

        // if no amount of office slots are indicated in the station type return 24 as a default value
        int maximumOffices = ItemFactory.GetItem <Station> (stationID).StationType.OfficeSlots ?? 24;

        return maximumOffices - StationDB.CountRentedOffices (stationID);
    }

    [MustBeInStation]
    public PyDataType SetCloneTypeID (PyInteger cloneTypeID, CallInformation call)
    {
        int callerCharacterID = call.Session.CharacterID;
        int stationID         = call.Session.StationID;

        Character character    = ItemFactory.GetItem <Character> (callerCharacterID);
        Type      newCloneType = TypeManager [cloneTypeID];

        if (newCloneType.Group.ID != (int) Groups.Clone)
            throw new CustomError ("Only clone types allowed!");
        // TODO: REIMPLEMENT THIS CHECK
        //if (character.ActiveClone.Type.BasePrice > newCloneType.BasePrice)
        //    throw new MedicalThisCloneIsWorse();

        Station station = ItemFactory.GetStaticStation (stationID);

        using Wallet wallet = WalletManager.AcquireWallet (character.ID, Keys.MAIN);
        {
            wallet.EnsureEnoughBalance (newCloneType.BasePrice);
            wallet.CreateTransactionRecord (
                TransactionType.Buy, character.ID, ItemFactory.LocationSystem.ID, newCloneType.ID, 1, newCloneType.BasePrice, station.ID
            );
        }

        // TODO: REIMPLEMENT THIS
        // update active clone's information
        // character.ActiveClone.Type = newCloneType;
        // character.ActiveClone.Name = newCloneType.Name;
        // character.ActiveClone.Persist();
        character.Persist ();

        return null;
    }

    [MustBeInStation]
    [MustHaveCorporationRole(typeof (RentingOfficeQuotesOnlyGivenToActiveCEOsOrEquivale), CorporationRole.Director, CorporationRole.CanRentOffice)]
    public PyInteger GetQuoteForRentingAnOffice (CallInformation call)
    {
        return ItemFactory.Stations [call.Session.StationID].OfficeRentalCost;
    }

    [MustBeInStation]
    [MustHaveCorporationRole(typeof (RentingOfficeQuotesOnlyGivenToActiveCEOsOrEquivale), CorporationRole.Director, CorporationRole.CanRentOffice)]
    public PyDataType RentOffice (PyInteger cost, CallInformation call)
    {
        int rentalCost  = this.GetQuoteForRentingAnOffice (call);
        int stationID   = call.Session.StationID;
        int characterID = call.Session.CharacterID;

        // double check to ensure the amount we're paying is what we require now
        if (rentalCost != cost)
            throw new RentingAnOfficeCostsMore (rentalCost);

        // check that there's enoug offices left
        if (this.GetNumberOfUnrentedOffices (call) <= 0)
            throw new NoOfficesAreAvailableForRenting ();

        // check if there's any office rented by us already
        if (StationDB.CorporationHasOfficeRentedAt (call.Session.CorporationID, stationID))
            throw new RentingYouHaveAnOfficeHere ();

        // ensure the character has the required skill to manage offices
        ItemFactory.GetItem <Character> (characterID).EnsureSkillLevel (Types.PublicRelations);
        // RentingOfficeRequestDenied
        int ownerCorporationID = ItemFactory.Stations [stationID].OwnerID;

        // perform the transaction
        using (Wallet corpWallet = WalletManager.AcquireWallet (call.Session.CorporationID, call.Session.CorpAccountKey, true))
        {
            corpWallet.EnsureEnoughBalance (rentalCost);
            corpWallet.CreateJournalRecord (MarketReference.OfficeRentalFee, ownerCorporationID, null, -rentalCost);
        }

        // create the office folder
        ItemEntity item = ItemFactory.CreateSimpleItem (
            TypeManager [Types.OfficeFolder], call.Session.CorporationID,
            stationID, Flags.Office, 1, false, true
        );
        long dueDate = DateTime.UtcNow.AddDays (30).ToFileTimeUtc ();
        // create the bill record for the renewal
        int billID = (int) BillsDB.CreateBill (
            BillTypes.RentalBill, call.Session.CorporationID, ownerCorporationID,
            rentalCost, dueDate, 0, (int) Types.OfficeFolder, stationID
        );
        // create the record in the database
        StationDB.RentOffice (call.Session.CorporationID, stationID, item.ID, dueDate, rentalCost, billID);
        // notify all characters of the corporation in the station about the office change
        Notifications.NotifyOwnerAtLocation (call.Session.CorporationID, stationID, new OnOfficeRentalChanged (call.Session.CorporationID, item.ID, item.ID));
        // notify all the characters about the bill received
        Notifications.NotifyCorporation (call.Session.CorporationID, new OnBillReceived ());
        // notify the node with the new office
        int nodeID = ItemFactory.ItemDB.GetItemNode (call.Session.CorporationID);
        
        if (nodeID > 0)
            Notifications.NotifyNode (nodeID, new OnCorporationOfficeRented () {CorporationID = call.Session.CorporationID, StationID = stationID, OfficeFolderID = item.ID, TypeID = (int) Types.OfficeFolder});
        
        // return the new officeID
        return item.ID;
    }

    [MustBeInStation]
    public PyDataType MoveCorpHQHere (CallInformation call)
    {
        // TODO: IMPLEMENT THIS!
        return null;
    }

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        return Database.CluResolveAddress ("station", parameters.ObjectID);
    }

    protected override BoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
        if (this.MachoResolveObject (bindParams, call) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new corpStationMgr (
            MarketDB, StationDB, BillsDB, Notifications, ItemFactory, Constants, BoundServiceManager, WalletManager,
            call.Session
        );
    }
}