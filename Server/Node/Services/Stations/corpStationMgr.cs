using System.Collections.Generic;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.corpStationMgr;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Stations
{
    public class corpStationMgr : BoundService
    {
        private const double CLONE_CONTRACT_COST = 5600.0;
        
        private ItemDB ItemDB { get; }
        private MarketDB MarketDB { get; }
        private ItemManager ItemManager { get; }
        private TypeManager TypeManager { get; }
        private SystemManager SystemManager { get; }
        
        public corpStationMgr(ItemDB itemDB, MarketDB marketDB, ItemManager itemManager, TypeManager typeManager, SystemManager systemManager, BoundServiceManager manager) : base(manager, null)
        {
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
            this.SystemManager = systemManager;
        }
        
        protected corpStationMgr(ItemDB itemDB, MarketDB marketDB, ItemManager itemManager, TypeManager typeManager, SystemManager systemManager, BoundServiceManager manager, Client client) : base(manager, client)
        {
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
            this.SystemManager = systemManager;
        }

        public override PyInteger MachoResolveObject(PyInteger stationID, PyInteger zero, CallInformation call)
        {
            int solarSystemID = this.ItemManager.GetStation(stationID).SolarSystemID;

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            if (this.MachoResolveObject(objectData as PyInteger, 0, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            return new corpStationMgr(this.ItemDB, this.MarketDB, this.ItemManager, this.TypeManager, this.SystemManager, this.BoundServiceManager, call.Client);
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
            
            Character character = this.ItemManager.GetItem<Character>(client.EnsureCharacterIsSelected());

            // TODO: CHECK STANDINGS TO ENSURE THIS STATION CAN BE USED
            List<Station> availableStations = new List<Station>
            {
                this.ItemManager.Stations[stationID],
                this.ItemManager.Stations[character.Corporation.StationID]
            };
            
            return availableStations;
        }
        
        public PyDataType GetPotentialHomeStations(CallInformation call)
        {
            List<Station> availableStations = this.GetPotentialHomeStations(call.Client);
            Rowset result = new Rowset(new PyDataType[]
                {
                    "stationID", "typeID", "serviceMask", 
                }
            );
            
            // build the return
            foreach (Station station in availableStations)
            {
                result.Rows.Add(new PyDataType[]
                    {
                        station.ID, station.Type.ID, station.Operations.ServiceMask
                    }
                );
            }
            
            return result;
        }

        public PyDataType SetHomeStation(PyInteger stationID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            call.Client.EnsureCharacterIsInStation();
            
            Character character = this.ItemManager.GetItem<Character>(callerCharacterID);
            
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
            
            // check the user has enough money
            character.EnsureEnoughBalance(CLONE_CONTRACT_COST);
            
            // subtract the money off the character
            character.Balance -= CLONE_CONTRACT_COST;

            this.MarketDB.CreateJournalForCharacter(
                MarketReference.CloneTransfer, character.ID, character.ID, null, station.ID,
                -CLONE_CONTRACT_COST, character.Balance, $"Moved clone to {station.Name}", 1000
            );
            
            // send the notification to the user
            call.Client.NotifyBalanceUpdate(character.Balance);
            
            // set clone's station
            character.ActiveClone.LocationID = stationID;
            character.ActiveClone.Persist();
            
            // persist character info
            character.Persist();
                
            // invalidate client's cache
            call.Client.NotifyCloneUpdate();
            
            return null;
        }

        public PyBool DoesPlayersCorpHaveJunkAtStation(CallInformation call)
        {
            if (ItemManager.IsNPCCorporationID(call.Client.CorporationID) == true)
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
            
            Character character = this.ItemManager.GetItem<Character>(callerCharacterID);
            ItemType newCloneType = this.TypeManager[cloneTypeID];

            if (newCloneType.Group.ID != (int) ItemGroups.Clone)
                throw new CustomError("Only clone types allowed!");
            if (character.ActiveClone.Type.BasePrice > newCloneType.BasePrice)
                throw new MedicalThisCloneIsWorse();

            Station station = this.ItemManager.GetStation(stationID);
            
            // ensure the character has enough money in the account for the upgrade
            character.EnsureEnoughBalance(newCloneType.BasePrice);
            character.Balance -= newCloneType.BasePrice;
            // notify the client
            call.Client.NotifyBalanceUpdate(character.Balance);
            // create the wallet entry
            this.MarketDB.CreateTransactionForCharacter(
                callerCharacterID, 1, TransactionType.Buy, newCloneType.ID, 1, newCloneType.BasePrice,
                station.ID 
            );

            // update active clone's information
            character.ActiveClone.Type = newCloneType;
            character.ActiveClone.Name = newCloneType.Name;
            character.ActiveClone.Persist();
            character.Persist();
            
            return null;
        }
    }
}