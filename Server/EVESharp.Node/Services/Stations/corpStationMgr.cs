using System;
using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Database.Configuration;
using EVESharp.Database.Corporations;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Groups;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Market;
using EVESharp.Database.Old;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.corpStationMgr;
using EVESharp.EVE.Market;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Corporations;
using EVESharp.EVE.Notifications.Wallet;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types;
using EVESharp.Node.Notifications.Nodes.Corps;
using EVESharp.Types;
using EVESharp.Types.Collections;
using ItemDB = EVESharp.Database.Old.ItemDB;
using Type = EVESharp.Database.Inventory.Types.Type;

namespace EVESharp.Node.Services.Stations;

[MustBeCharacter]
public class corpStationMgr : ClientBoundService
{
    public override AccessLevel         AccessLevel   => AccessLevel.None;
    private         IItems              Items         { get; }
    private         ItemDB              ItemDB        { get; }
    private         StationDB           StationDB     { get; }
    private         ITypes              Types         => this.Items.Types;
    private         ISolarSystems       SolarSystems  { get; }
    private         IWallets            Wallets       { get; }
    private         IConstants          Constants     { get; }
    private         INotificationSender Notifications { get; }
    private         IDatabase Database      { get; }

    public corpStationMgr
    (
        StationDB            stationDb, INotificationSender notificationSender, IItems              items,    IConstants constants,
        IBoundServiceManager manager,   ISolarSystems solarSystems, IWallets            wallets,            IDatabase database, ItemDB     itemDB
    ) : base (manager)
    {
        StationDB     = stationDb;
        Notifications = notificationSender;
        Items         = items;
        Constants     = constants;
        this.Wallets  = wallets;
        Database      = database;
        SolarSystems  = solarSystems;
        ItemDB        = itemDB;
    }

    // TODO: PROVIDE OBJECTID PROPERLY
    protected corpStationMgr
    (
        StationDB            stationDb, INotificationSender notificationSender, IItems  items,   IConstants constants,
        IBoundServiceManager manager,   IWallets            wallets,            Session session, ItemDB     itemDB, IDatabase database
    ) : base (manager, session, 0)
    {
        StationDB     = stationDb;
        Notifications = notificationSender;
        Items         = items;
        Constants     = constants;
        this.Wallets  = wallets;
        ItemDB        = itemDB;
        Database      = database;
    }

    [MustBeInStation]
    public PyList GetCorporateStationOffice (ServiceCall call)
    {
        return StationDB.GetOfficesList (call.Session.StationID);
    }

    [MustBeInStation]
    public PyDataType DoStandingCheckForStationService (ServiceCall call, PyInteger stationServiceID)
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
            this.Items.Stations [stationID]
            // this.ItemFactory.Stations[character.Corporation.StationID]
        };

        return availableStations;
    }

    [MustBeInStation]
    public PyDataType GetPotentialHomeStations (ServiceCall call)
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

    public PyDataType SetHomeStation (ServiceCall call, PyInteger stationID)
    {
        int callerCharacterID = call.Session.CharacterID;

        Character character = this.Items.GetItem <Character> (callerCharacterID);

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

        using IWallet wallet = this.Wallets.AcquireWallet (character.ID, WalletKeys.MAIN);

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
    public PyBool DoesPlayersCorpHaveJunkAtStation (ServiceCall call)
    {
        if (ItemRanges.IsNPCCorporationID (call.Session.CorporationID))
            return false;

        // TODO: PROPERLY IMPLEMENT THIS ONE
        return false;
    }

    [MustBeInStation]
    public PyTuple GetCorporateStationInfo (ServiceCall call)
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
    public PyInteger GetNumberOfUnrentedOffices (ServiceCall call)
    {
        int stationID = call.Session.StationID;

        // if no amount of office slots are indicated in the station type return 24 as a default value
        int maximumOffices = this.Items.GetItem <Station> (stationID).StationType.OfficeSlots ?? 24;

        return maximumOffices - StationDB.CountRentedOffices (stationID);
    }

    [MustBeInStation]
    public PyDataType SetCloneTypeID (ServiceCall call, PyInteger cloneTypeID)
    {
        int callerCharacterID = call.Session.CharacterID;
        int stationID         = call.Session.StationID;

        Character character    = this.Items.GetItem <Character> (callerCharacterID);
        Type      newCloneType = this.Types [cloneTypeID];

        if (newCloneType.Group.ID != (int) GroupID.Clone)
            throw new CustomError ("Only clone types allowed!");
        // TODO: REIMPLEMENT THIS CHECK
        //if (character.ActiveClone.Type.BasePrice > newCloneType.BasePrice)
        //    throw new MedicalThisCloneIsWorse();

        Station station = this.Items.GetStaticStation (stationID);

        using (IWallet wallet = this.Wallets.AcquireWallet (character.ID, WalletKeys.MAIN))
        {
            wallet.EnsureEnoughBalance (newCloneType.BasePrice);

            wallet.CreateTransactionRecord (
                TransactionType.Buy, character.ID, this.Items.LocationSystem.ID, newCloneType.ID, 1, newCloneType.BasePrice, station.ID
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
    [MustHaveCorporationRole (typeof (RentingOfficeQuotesOnlyGivenToActiveCEOsOrEquivale), CorporationRole.Director, CorporationRole.CanRentOffice)]
    public PyInteger GetQuoteForRentingAnOffice (ServiceCall call)
    {
        return this.Items.Stations [call.Session.StationID].OfficeRentalCost;
    }

    [MustBeInStation]
    [MustHaveCorporationRole (typeof (RentingOfficeQuotesOnlyGivenToActiveCEOsOrEquivale), CorporationRole.Director, CorporationRole.CanRentOffice)]
    public PyDataType RentOffice (ServiceCall call, PyInteger cost)
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
        this.Items.GetItem <Character> (characterID).EnsureSkillLevel (TypeID.PublicRelations);
        // RentingOfficeRequestDenied
        int ownerCorporationID = this.Items.Stations [stationID].OwnerID;

        // perform the transaction
        using (IWallet corpWallet = this.Wallets.AcquireWallet (call.Session.CorporationID, call.Session.CorpAccountKey, true))
        {
            corpWallet.EnsureEnoughBalance (rentalCost);
            corpWallet.CreateJournalRecord (MarketReference.OfficeRentalFee, ownerCorporationID, null, -rentalCost);
        }

        // create the office folder
        ItemEntity item = this.Items.CreateSimpleItem (
            this.Types [TypeID.OfficeFolder], call.Session.CorporationID,
            stationID, Flags.Office, 1, false, true
        );

        long dueDate = DateTime.UtcNow.AddDays (30).ToFileTimeUtc ();

        // create the bill record for the renewal
        int billID = (int) Database.MktBillsCreate (
            BillTypes.RentalBill, call.Session.CorporationID, ownerCorporationID,
            rentalCost, dueDate, 0, (int) TypeID.OfficeFolder, stationID
        );

        // create the record in the database
        StationDB.RentOffice (call.Session.CorporationID, stationID, item.ID, dueDate, rentalCost, billID);
        // notify all characters of the corporation in the station about the office change
        Notifications.NotifyOwnerAtLocation (call.Session.CorporationID, stationID, new OnOfficeRentalChanged (call.Session.CorporationID, item.ID, item.ID));
        // notify all the characters about the bill received
        Notifications.NotifyCorporation (call.Session.CorporationID, new OnBillReceived ());
        // notify the node with the new office
        long nodeID = Database.InvGetItemNode (call.Session.CorporationID);

        if (nodeID > 0)
            Notifications.NotifyNode (
                nodeID, new OnCorporationOfficeRented
                {
                    CorporationID  = call.Session.CorporationID,
                    StationID      = stationID,
                    OfficeFolderID = item.ID,
                    TypeID         = (int) TypeID.OfficeFolder
                }
            );

        // return the new officeID
        return item.ID;
    }

    [MustBeInStation]
    public PyDataType MoveCorpHQHere (ServiceCall call)
    {
        // TODO: IMPLEMENT THIS!
        return null;
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        return Database.CluResolveAddress ("station", parameters.ObjectID);
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new corpStationMgr (
            StationDB, Notifications, this.Items, Constants, BoundServiceManager, this.Wallets,
            call.Session, this.ItemDB, this.Database
        );
    }
}