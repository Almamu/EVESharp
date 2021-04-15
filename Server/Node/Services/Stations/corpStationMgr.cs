using System;
using System.Collections.Generic;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.corpStationMgr;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using Node.Services.Account;
using Node.StaticData;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using Type = Node.StaticData.Inventory.Type;

namespace Node.Services.Stations
{
    public class corpStationMgr : BoundService
    {
        private ItemFactory ItemFactory { get; }
        private ItemDB ItemDB => this.ItemFactory.ItemDB;
        private MarketDB MarketDB { get; }
        private TypeManager TypeManager => this.ItemFactory.TypeManager;
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private WalletManager WalletManager { get; }
        private NodeContainer Container { get; }
        
        public corpStationMgr(MarketDB marketDB, ItemFactory itemFactory, NodeContainer container, BoundServiceManager manager, WalletManager walletManager) : base(manager, null)
        {
            this.MarketDB = marketDB;
            this.ItemFactory = itemFactory;
            this.Container = container;
            this.WalletManager = walletManager;
        }
        
        protected corpStationMgr(MarketDB marketDB, ItemFactory itemFactory, NodeContainer container, BoundServiceManager manager, WalletManager walletManager, Client client) : base(manager, client)
        {
            this.MarketDB = marketDB;
            this.ItemFactory = itemFactory;
            this.Container = container;
            this.WalletManager = walletManager;
        }

        public override PyInteger MachoResolveObject(PyInteger stationID, PyInteger zero, CallInformation call)
        {
            int solarSystemID = this.ItemFactory.GetStaticStation(stationID).SolarSystemID;

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            if (this.MachoResolveObject(objectData as PyInteger, 0, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            return new corpStationMgr(this.MarketDB, this.ItemFactory, this.Container, this.BoundServiceManager, this.WalletManager, call.Client);
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
            Rowset result = new Rowset(new PyList(3)
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
            return new PyTuple(3)
            {
                [0] = null, // eveowners list
                [1] = null, // corporations list
                [2] = null  // offices list
            };
        }

        public PyInteger GetNumberOfUnrentedOffices(CallInformation call)
        {
            // TODO: PROPERLY IMPLEMENT THIS, NPC STATIONS HAVE A LIMIT OF 24 OFFICES
            return 0;
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
                wallet.CreateTransactionRecord(TransactionType.Buy, this.ItemFactory.LocationSystem.ID, newCloneType.ID, 1, newCloneType.BasePrice, station.ID);
            }
            
            // update active clone's information
            character.ActiveClone.Type = newCloneType;
            character.ActiveClone.Name = newCloneType.Name;
            character.ActiveClone.Persist();
            character.Persist();
            
            return null;
        }
    }
}