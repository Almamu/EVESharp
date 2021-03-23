using Node.Database;
using Node.Exceptions.insuranceSvc;
using Node.Exceptions.jumpCloneSvc;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Inventory
{
    public class insuranceSvc : BoundService
    {
        private int mStationID = 0;
        private InsuranceDB DB { get; }
        private ItemManager ItemManager { get; }
        private MarketDB MarketDB { get; }
        private SystemManager SystemManager { get; }
        
        public insuranceSvc(ItemManager itemManager, InsuranceDB db, MarketDB marketDB, SystemManager systemManager, BoundServiceManager manager) : base(manager, null)
        {
            this.DB = db;
            this.ItemManager = itemManager;
            this.MarketDB = marketDB;
            this.SystemManager = systemManager;
        }

        protected insuranceSvc(ItemManager itemManager, InsuranceDB db, MarketDB marketDB, SystemManager systemManager, BoundServiceManager manager, int stationID, Client client) : base (manager, client)
        {
            this.mStationID = stationID;
            this.DB = db;
            this.ItemManager = itemManager;
            this.MarketDB = marketDB;
            this.SystemManager = systemManager;
        }

        public override PyInteger MachoResolveObject(PyInteger stationID, PyInteger zero, CallInformation call)
        {
            int solarSystemID = this.ItemManager.GetStaticStation(stationID).SolarSystemID;

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            if (this.MachoResolveObject(objectData as PyInteger, 0, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");
            
            return new insuranceSvc(this.ItemManager, this.DB, this.MarketDB, this.SystemManager, this.BoundServiceManager, objectData as PyInteger, call.Client);
        }

        public PyList<PyPackedRow> GetContracts(CallInformation call)
        {
            if (this.mStationID == 0)
            {
                int? shipID = call.Client.ShipID;
                
                if (shipID is null)
                    throw new CustomError($"The character is not onboard any ship");
                
                return this.DB.GetContractForShip(call.Client.EnsureCharacterIsSelected(), (int) shipID);
            }
            else
            {
                return this.DB.GetContractsForShipsOnStation(call.Client.EnsureCharacterIsSelected(), this.mStationID);
            }
        }

        public PyList<PyPackedRow> GetContractForShip(PyInteger itemID, CallInformation call)
        {
            return this.DB.GetContractForShip(call.Client.EnsureCharacterIsSelected(), itemID);
        }

        public PyList<PyPackedRow> GetContracts(PyInteger includeCorp, CallInformation call)
        {
            if (includeCorp == 0)
                return this.DB.GetContractsForShipsOnStation(call.Client.EnsureCharacterIsSelected(), this.mStationID);
            else
                return this.DB.GetContractsForShipsOnStationIncludingCorp(call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID, this.mStationID);
        }

        public PyBool InsureShip(PyInteger itemID, PyDecimal insuranceCost, PyInteger isCorpItem, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (this.ItemManager.TryGetItem(itemID, out Ship item) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Character character = this.ItemManager.GetItem<Character>(callerCharacterID);

            if (isCorpItem == 1 && item.OwnerID != call.Client.CorporationID && item.OwnerID != callerCharacterID)
                throw new MktNotOwner();

            if (item.Singleton == false)
                throw new InsureShipFailed("Only assembled ships can be insured");

            string ownerName = "";

            if (this.DB.IsShipInsured(item.ID, out ownerName) == true)
                throw new InsureShipFailedSingleContract(ownerName);
            
            // check the user has enough money
            character.EnsureEnoughBalance(insuranceCost);
            
            // subtract the money off the character
            character.Balance -= insuranceCost;
            
            double fraction = insuranceCost * 100 / item.Type.BasePrice;

            // create insurance record
            this.DB.InsureShip(item.ID, isCorpItem == 0 ? callerCharacterID : call.Client.CorporationID, fraction / 5);
            
            this.MarketDB.CreateJournalForCharacter(
                MarketReference.Insurance, character.ID, character.ID, this.ItemManager.SecureCommerceCommision.ID, -item.ID,
                -insuranceCost, character.Balance, $"Insurance fee for {item.Name}", 1000
            );
            
            // send the notification to the user
            call.Client.NotifyBalanceUpdate(character.Balance);
            
            // persist changes in the balance
            character.Persist();

            // TODO: CHECK IF THE INSURANCE SHOULD BE CHARGED TO THE CORP
            
            return true;
        }

        public PyDataType UnInsureShip(PyInteger itemID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (this.ItemManager.TryGetItem(itemID, out Ship item) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Character character = this.ItemManager.GetItem<Character>(callerCharacterID);

            if (item.OwnerID != call.Client.CorporationID && item.OwnerID != callerCharacterID)
                throw new MktNotOwner();

            // remove insurance record off the database
            this.DB.UnInsureShip(itemID);
            
            return null;
        }
    }
}