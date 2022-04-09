using System;
using System.Collections.Generic;
using EVESharp.EVE.Client.Exceptions.corpStationMgr;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.EVE.Wallet;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Client.Corporations;
using EVESharp.Node.Notifications.Client.Wallet;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Character = EVESharp.Node.Inventory.Items.Types.Character;
using Groups = EVESharp.EVE.StaticData.Inventory.Groups;
using Type = EVESharp.EVE.StaticData.Inventory.Type;

namespace EVESharp.Node.Services.Stations;

public class corpStationMgr : ClientBoundService
{
    public override AccessLevel                 AccessLevel   => AccessLevel.None;
    private         ItemFactory                 ItemFactory   { get; }
    private         ItemDB                      ItemDB        => ItemFactory.ItemDB;
    private         MarketDB                    MarketDB      { get; }
    private         StationDB                   StationDB     { get; }
    private         BillsDB                     BillsDB       { get; }
    private         TypeManager                 TypeManager   => ItemFactory.TypeManager;
    private         SystemManager               SystemManager => ItemFactory.SystemManager;
    private         WalletManager               WalletManager { get; }
    private         Constants     Constants     { get; }
    private         Notifications.Notifications Notifications { get; }

    public corpStationMgr (
        MarketDB marketDB, StationDB stationDb, BillsDB billsDb, Notifications.Notifications notifications, ItemFactory itemFactory, Constants constants,
        BoundServiceManager manager, WalletManager walletManager
    ) : base (manager)
    {
        MarketDB      = marketDB;
        StationDB     = stationDb;
        BillsDB       = billsDb;
        Notifications = notifications;
        ItemFactory   = itemFactory;
        Constants     = constants;
        WalletManager = walletManager;
    }

    // TODO: PROVIDE OBJECTID PROPERLY
    protected corpStationMgr (
        MarketDB marketDB, StationDB stationDb, BillsDB billsDb, Notifications.Notifications notifications, ItemFactory itemFactory, Constants constants,
        BoundServiceManager manager, WalletManager walletManager, Session session
    ) : base (manager, session, 0)
    {
        MarketDB      = marketDB;
        StationDB     = stationDb;
        BillsDB       = billsDb;
        Notifications = notifications;
        ItemFactory   = itemFactory;
        Constants     = constants;
        WalletManager = walletManager;
    }

    public PyList GetCorporateStationOffice (CallInformation call)
    {
        // TODO: IMPLEMENT WHEN CORPORATION SUPPORT IS IN PLACE
        return new PyList ();
    }

    public PyDataType DoStandingCheckForStationService (PyInteger stationServiceID, CallInformation call)
    {
        call.Session.EnsureCharacterIsSelected ();
        call.Session.EnsureCharacterIsInStation ();

        // TODO: CHECK ACTUAL STANDING VALUE

        return null;
    }

    private List <Station> GetPotentialHomeStations (Session session)
    {
        int stationID = session.EnsureCharacterIsInStation ();

        Character character = ItemFactory.GetItem <Character> (session.EnsureCharacterIsSelected ());

        // TODO: CHECK STANDINGS TO ENSURE THIS STATION CAN BE USED
        // TODO: ADD CURRENT CORPORATION'S STATION BACK TO THE LIST
        List <Station> availableStations = new List <Station>
        {
            ItemFactory.Stations [stationID]
            // this.ItemFactory.Stations[character.Corporation.StationID]
        };

        return availableStations;
    }

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
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();
        call.Session.EnsureCharacterIsInStation ();

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

    public PyBool DoesPlayersCorpHaveJunkAtStation (CallInformation call)
    {
        if (ItemRanges.IsNPCCorporationID (call.Session.CorporationID))
            return false;

        // TODO: PROPERLY IMPLEMENT THIS ONE
        return false;
    }

    public PyTuple GetCorporateStationInfo (CallInformation call)
    {
        int stationID = call.Session.EnsureCharacterIsInStation ();

        return new PyTuple (3)
        {
            [0] = StationDB.GetOfficesOwners (stationID), // eveowners list
            [1] = StationDB.GetCorporations (stationID), // corporations list
            [2] = StationDB.GetOfficesList (stationID) // offices list
        };
    }

    public PyInteger GetNumberOfUnrentedOffices (CallInformation call)
    {
        int stationID = call.Session.EnsureCharacterIsInStation ();

        // if no amount of office slots are indicated in the station type return 24 as a default value
        int maximumOffices = ItemFactory.GetItem <Station> (stationID).StationType.OfficeSlots ?? 24;

        return maximumOffices - StationDB.CountRentedOffices (stationID);
    }

    public PyDataType SetCloneTypeID (PyInteger cloneTypeID, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();
        int stationID         = call.Session.EnsureCharacterIsInStation ();

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

    public PyInteger GetQuoteForRentingAnOffice (CallInformation call)
    {
        int stationID = call.Session.EnsureCharacterIsInStation ();

        // make sure the user is director or allowed to rent
        if (CorporationRole.Director.Is (call.Session.CorporationRole) == false && CorporationRole.CanRentOffice.Is (call.Session.CorporationRole) == false)
            throw new RentingOfficeQuotesOnlyGivenToActiveCEOsOrEquivale ();

        return ItemFactory.Stations [stationID].OfficeRentalCost;
    }

    public PyDataType RentOffice (PyInteger cost, CallInformation call)
    {
        int rentalCost  = this.GetQuoteForRentingAnOffice (call);
        int stationID   = call.Session.EnsureCharacterIsInStation ();
        int characterID = call.Session.EnsureCharacterIsSelected ();

        // double check to ensure the amout we're paying is what we require now
        if (rentalCost != cost)
            throw new RentingAnOfficeCostsMore (rentalCost);

        // check that there's enoug offices left
        if (this.GetNumberOfUnrentedOffices (call) <= 0)
            throw new NoOfficesAreAvailableForRenting ();

        // check if there's any office rented by us already
        if (StationDB.CorporationHasOfficeRentedAt (call.Session.CorporationID, stationID))
            throw new RentingYouHaveAnOfficeHere ();
        if (CorporationRole.CanRentOffice.Is (call.Session.CorporationRole) == false && CorporationRole.Director.Is (call.Session.CorporationRole) == false)
            throw new RentingOfficeQuotesOnlyGivenToActiveCEOsOrEquivale ();

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
        // notify all characters in the station about the office change
        Notifications.NotifyStation (stationID, new OnOfficeRentalChanged (call.Session.CorporationID, item.ID, item.ID));
        // notify all the characters about the bill received
        Notifications.NotifyCorporation (call.Session.CorporationID, new OnBillReceived ());

        // return the new officeID
        return item.ID;
        // TODO: NOTIFY THE CORPREGISTRY SERVICE TO UPDATE THIS LIST OF OFFICES
    }

    public PyDataType MoveCorpHQHere (CallInformation call)
    {
        // TODO: IMPLEMENT THIS!
        return null;
    }

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        int solarSystemID = ItemFactory.GetStaticStation (parameters.ObjectID).SolarSystemID;

        if (SystemManager.SolarSystemBelongsToUs (solarSystemID))
            return BoundServiceManager.MachoNet.NodeID;

        return SystemManager.LoadSolarSystemOnCluster (solarSystemID);
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