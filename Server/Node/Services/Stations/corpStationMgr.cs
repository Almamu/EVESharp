using System;
using System.Collections.Generic;
using System.ComponentModel;
using EVE;
using EVE.Packets.Exceptions;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.corpStationMgr;
using Node.Exceptions.Internal;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using Node.Notifications.Client.Corporations;
using Node.Notifications.Client.Wallet;
using Node.Services.Account;
using Node.StaticData;
using Node.StaticData.Corporation;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using Type = Node.StaticData.Inventory.Type;

namespace Node.Services.Stations
{
    public class corpStationMgr : ClientBoundService
    {
        private ItemFactory ItemFactory { get; }
        private ItemDB ItemDB => this.ItemFactory.ItemDB;
        private MarketDB MarketDB { get; }
        private StationDB StationDB { get; init; }
        private BillsDB BillsDB { get; init; }
        private TypeManager TypeManager => this.ItemFactory.TypeManager;
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private WalletManager WalletManager { get; }
        private NodeContainer Container { get; }
        private NotificationManager NotificationManager { get; init; }
        
        public corpStationMgr(MarketDB marketDB, StationDB stationDb, BillsDB billsDb, NotificationManager notificationManager, ItemFactory itemFactory, NodeContainer container, BoundServiceManager manager, WalletManager walletManager) : base(manager)
        {
            this.MarketDB = marketDB;
            this.StationDB = stationDb;
            this.BillsDB = billsDb;
            this.NotificationManager = notificationManager;
            this.ItemFactory = itemFactory;
            this.Container = container;
            this.WalletManager = walletManager;
        }
        
        // TODO: PROVIDE OBJECTID PROPERLY
        protected corpStationMgr(MarketDB marketDB, StationDB stationDb, BillsDB billsDb, NotificationManager notificationManager, ItemFactory itemFactory, NodeContainer container, BoundServiceManager manager, WalletManager walletManager, Client client) : base(manager, client, 0)
        {
            this.MarketDB = marketDB;
            this.StationDB = stationDb;
            this.BillsDB = billsDb;
            this.NotificationManager = notificationManager;
            this.ItemFactory = itemFactory;
            this.Container = container;
            this.WalletManager = walletManager;
        }

        public PyList GetCorporateStationOffice(CallInformation call)
        {
            // TODO: IMPLEMENT WHEN CORPORATION SUPPORT IS IN PLACE
            return new PyList();
        }

        public PyDataType DoStandingCheckForStationService(PyInteger stationServiceID, CallInformation call)
        {
            call.Client.EnsureCharacterIsSelected();
            call.Client.EnsureCharacterIsInStation();
            
            // TODO: CHECK ACTUAL STANDING VALUE
            
            return null;
        }

        private List<Station> GetPotentialHomeStations(Client client)
        {
            int stationID = client.EnsureCharacterIsInStation();
            
            Character character = this.ItemFactory.GetItem<Character>(client.EnsureCharacterIsSelected());

            // TODO: CHECK STANDINGS TO ENSURE THIS STATION CAN BE USED
            List<Station> availableStations = new List<Station>
            {
                this.ItemFactory.Stations[stationID],
                this.ItemFactory.Stations[character.Corporation.StationID]
            };
            
            return availableStations;
        }
        
        public PyDataType GetPotentialHomeStations(CallInformation call)
        {
            List<Station> availableStations = this.GetPotentialHomeStations(call.Client);
            Rowset result = new Rowset(new PyList<PyString>(3)
                {
                    [0] = "stationID",
                    [1] = "typeID",
                    [2] = "serviceMask", 
                }
            );
            
            // build the return
            foreach (Station station in availableStations)
            {
                result.Rows.Add(new PyList(3)
                    {
                        [0] = station.ID,
                        [1] = station.Type.ID,
                        [2] = station.Operations.ServiceMask
                    }
                );
            }
            
            return result;
        }

        public PyDataType SetHomeStation(PyInteger stationID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            call.Client.EnsureCharacterIsInStation();
            
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            
            // ensure the station selected is in the list of available stations for this character
            Station station = this.GetPotentialHomeStations(call.Client).Find(x => x.ID == stationID);

            if (station is null)
                throw new CustomError("The selected station is not in your allowed list...");

            // we could check if the current station is the same as the new one
            // but in reality it doesn't matter much, if the user wants to pay twice for it, sure, why not
            // in practice it doesn't make much difference
            // it also simplifies code that needs to communicate between nodes
            
            // what we need to do tho is ensure there's no other clone in here in the first place
            Rowset clones = this.ItemDB.GetClonesForCharacter(character.ID, (int) character.ActiveCloneID);

            foreach (PyList entry in clones.Rows)
            {
                int locationID = entry[2] as PyInteger;

                // if a clone is already there, refuse to have the medical in there
                if (locationID == stationID)
                    throw new MedicalYouAlreadyHaveACloneContractAtThatStation();
            }

            using Wallet wallet = this.WalletManager.AcquireWallet(character.ID, 1000);
            {
                double contractCost = this.Container.Constants[Constants.costCloneContract];
                
                wallet.EnsureEnoughBalance(contractCost);
                wallet.CreateJournalRecord(MarketReference.CloneTransfer, null, station.ID, -contractCost, $"Moved clone to {station.Name}");
            }
            
            // set clone's station
            character.ActiveClone.LocationID = stationID;
            character.ActiveClone.Persist();
            
            // persist character info
            character.Persist();
                
            // invalidate client's cache
            this.Client.ServiceManager.jumpCloneSvc.OnCloneUpdate(character.ID);
            
            return null;
        }

        public PyBool DoesPlayersCorpHaveJunkAtStation(CallInformation call)
        {
            if (ItemFactory.IsNPCCorporationID(call.Client.CorporationID) == true)
                return false;
            
            // TODO: PROPERLY IMPLEMENT THIS ONE
            return false;
        }

        public PyTuple GetCorporateStationInfo(CallInformation call)
        {
            int stationID = call.Client.EnsureCharacterIsInStation();
            
            return new PyTuple(3)
            {
                [0] = this.StationDB.GetOfficesOwners(stationID), // eveowners list
                [1] = this.StationDB.GetCorporations(stationID), // corporations list
                [2] = this.StationDB.GetOfficesList(stationID)  // offices list
            };
        }

        public PyInteger GetNumberOfUnrentedOffices(CallInformation call)
        {
            int stationID = call.Client.EnsureCharacterIsInStation();

            // if no amount of office slots are indicated in the station type return 24 as a default value
            int maximumOffices = this.ItemFactory.GetItem<Station>(stationID).StationType.OfficeSlots ?? 24;

            return maximumOffices - this.StationDB.CountRentedOffices(stationID);
        }

        public PyDataType SetCloneTypeID(PyInteger cloneTypeID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            int stationID = call.Client.EnsureCharacterIsInStation();
            
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            Type newCloneType = this.TypeManager[cloneTypeID];

            if (newCloneType.Group.ID != (int) Groups.Clone)
                throw new CustomError("Only clone types allowed!");
            if (character.ActiveClone.Type.BasePrice > newCloneType.BasePrice)
                throw new MedicalThisCloneIsWorse();

            Station station = this.ItemFactory.GetStaticStation(stationID);

            using Wallet wallet = this.WalletManager.AcquireWallet(character.ID, 1000);
            {
                wallet.EnsureEnoughBalance(newCloneType.BasePrice);
                wallet.CreateTransactionRecord(TransactionType.Buy, character.ID, this.ItemFactory.LocationSystem.ID, newCloneType.ID, 1, newCloneType.BasePrice, station.ID);
            }
            
            // update active clone's information
            character.ActiveClone.Type = newCloneType;
            character.ActiveClone.Name = newCloneType.Name;
            character.ActiveClone.Persist();
            character.Persist();
            
            return null;
        }

        public PyInteger GetQuoteForRentingAnOffice(CallInformation call)
        {
            int stationID = call.Client.EnsureCharacterIsInStation();

            // make sure the user is director or allowed to rent
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false && CorporationRole.CanRentOffice.Is(call.Client.CorporationRole) == false)
                throw new RentingOfficeQuotesOnlyGivenToActiveCEOsOrEquivale();
            
            return this.ItemFactory.Stations[stationID].OfficeRentalCost;
        }

        public PyDataType RentOffice(PyInteger cost, CallInformation call)
        {
            int rentalCost = this.GetQuoteForRentingAnOffice(call);
            int stationID = call.Client.EnsureCharacterIsInStation();
            int characterID = call.Client.EnsureCharacterIsSelected();
            
            // double check to ensure the amout we're paying is what we require now
            if (rentalCost != cost)
                throw new RentingAnOfficeCostsMore(rentalCost);
            // check that there's enoug offices left
            if (this.GetNumberOfUnrentedOffices(call) <= 0)
                throw new NoOfficesAreAvailableForRenting();
            // check if there's any office rented by us already
            if (this.StationDB.CorporationHasOfficeRentedAt(call.Client.CorporationID, stationID) == true)
                throw new RentingYouHaveAnOfficeHere();
            if (CorporationRole.CanRentOffice.Is(call.Client.CorporationRole) == false && CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new RentingOfficeQuotesOnlyGivenToActiveCEOsOrEquivale();
            // ensure the character has the required skill to manage offices
            this.ItemFactory.GetItem<Character>(characterID).EnsureSkillLevel(Types.PublicRelations);
            // RentingOfficeRequestDenied
            int ownerCorporationID = this.ItemFactory.Stations[stationID].OwnerID;
            // perform the transaction
            using (Wallet corpWallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, call.Client.CorpAccountKey, true))
            {
                corpWallet.EnsureEnoughBalance(rentalCost);
                corpWallet.CreateJournalRecord(MarketReference.OfficeRentalFee, ownerCorporationID, null, -rentalCost);
            }
            // create the office folder
            ItemEntity item = this.ItemFactory.CreateSimpleItem(
                this.TypeManager[Types.OfficeFolder], call.Client.CorporationID,
                stationID, Flags.Office, 1, false, true
            );
            long dueDate = DateTime.UtcNow.AddDays(30).ToFileTimeUtc();
            // create the bill record for the renewal
            int billID = (int) this.BillsDB.CreateBill(
                BillTypes.RentalBill, call.Client.CorporationID, ownerCorporationID,
                rentalCost, dueDate, 0, (int) Types.OfficeFolder, stationID
            );
            // create the record in the database
            this.StationDB.RentOffice(call.Client.CorporationID, stationID, item.ID, dueDate, rentalCost, billID);
            // notify all characters in the station about the office change
            this.NotificationManager.NotifyStation(stationID, new OnOfficeRentalChanged(call.Client.CorporationID, item.ID, item.ID));
            // notify all the characters about the bill received
            this.NotificationManager.NotifyCorporation(call.Client.CorporationID, new OnBillReceived());
            // return the new officeID
            return item.ID;
            // TODO: NOTIFY THE CORPREGISTRY SERVICE TO UPDATE THIS LIST OF OFFICES
        }
        
        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            int solarSystemID = this.ItemFactory.GetStaticStation(parameters.ObjectID).SolarSystemID;

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            if (this.MachoResolveObject(bindParams, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");
            
            return new corpStationMgr(this.MarketDB, this.StationDB, this.BillsDB, this.NotificationManager, this.ItemFactory, this.Container, this.BoundServiceManager, this.WalletManager, call.Client);
        }
    }
}