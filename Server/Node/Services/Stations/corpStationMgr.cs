using System.Collections.Generic;
using Common.Logging;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.corpStationMgr;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Stations
{
    public class corpStationMgr : BoundService
    {
        private const double CLONE_CONTRACT_COST = 5600.0;
        
        private ItemDB ItemDB { get; }
        private MarketDB MarketDB { get; }
        private ItemManager ItemManager { get; }
        public corpStationMgr(ItemDB itemDB, MarketDB marketDB, ItemManager itemManager, BoundServiceManager manager) : base(manager)
        {
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            return new corpStationMgr(this.ItemDB, this.MarketDB, this.ItemManager, this.BoundServiceManager);
        }

        public PyDataType GetCorporateStationOffice(CallInformation call)
        {
            // TODO: IMPLEMENT WHEN CORPORATION SUPPORT IS IN PLACE
            return new PyList();
        }

        public PyDataType DoStandingCheckForStationService(PyInteger stationServiceID, CallInformation call)
        {
            call.Client.EnsureCharacterIsSelected();

            if (call.Client.StationID == null)
                throw new CanOnlyDoInStations();
            
            // TODO: CHECK ACTUAL STANDING VALUE
            
            return null;
        }

        private List<Station> GetPotentialHomeStations(Client client)
        {
            if (client.StationID == null)
                throw new CanOnlyDoInStations();

            List<Station> availableStations = new List<Station>();
            
            Character character = this.ItemManager.LoadItem(client.EnsureCharacterIsSelected()) as Character;

            // TODO: CHECK STANDINGS TO ENSURE THIS STATION CAN BE USED
            availableStations.Add(this.ItemManager.Stations[(int) client.StationID]);
            availableStations.Add(this.ItemManager.Stations[character.Corporation.StationID]);

            return availableStations;
        }
        
        public PyDataType GetPotentialHomeStations(CallInformation call)
        {
            List<Station> availableStations = this.GetPotentialHomeStations(call.Client);
            Rowset result = new Rowset((PyList) new PyDataType[]
                {
                    "stationID", "typeID", "serviceMask", 
                }
            );
            
            // build the return
            foreach (Station station in availableStations)
            {
                result.Rows.Add((PyList) new PyDataType[]
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
            if (call.Client.StationID == null)
                throw new CanOnlyDoInStations();
            
            Character character = this.ItemManager.LoadItem(callerCharacterID) as Character;
            
            // ensure the station selected is in the list of available stations for this character
            Station station = this.GetPotentialHomeStations(call.Client).Find(x => x.ID == stationID);

            if (station == null)
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
            if (character.Balance < CLONE_CONTRACT_COST)
                throw new NotEnoughMoney(character.Balance, CLONE_CONTRACT_COST);
            
            // subtract the money off the character
            character.Balance -= CLONE_CONTRACT_COST;

            this.MarketDB.CreateJournalForCharacter(
                station.ID, MarketReference.CloneTransfer, character.ID, null, station.ID,
                -CLONE_CONTRACT_COST, character.Balance, $"Moved clone to {station.Name}", 1000
            );
            
            // send the notification to the user
            call.Client.NotifyBalanceUpdate(character.Balance);
            
            // set clone's station
            character.ActiveClone.LocationID = stationID;
            character.ActiveClone.Persist();
                
            // invalidate client's cache
            call.Client.NotifyCloneUpdate();
            
            return null;
        }

        public PyDataType DoesPlayersCorpHaveJunkAtStation(CallInformation call)
        {
            if (ItemManager.IsNPCCorporationID(call.Client.CorporationID) == true)
                return false;
            
            // TODO: PROPERLY IMPLEMENT THIS ONE
            return false;
        }

        public PyDataType GetCorporateStationInfo(CallInformation call)
        {
            return new PyTuple(3)
            {
                [0] = new PyNone(), // eveowners list
                [1] = new PyNone(), // corporations list
                [2] = new PyNone()  // offices list
            };
        }

        public PyDataType GetNumberOfUnrentedOffices(CallInformation call)
        {
            // TODO: PROPERLY IMPLEMENT THIS, NPC STATIONS HAVE A LIMIT OF 24 OFFICES
            return 0;
        }
    }
}