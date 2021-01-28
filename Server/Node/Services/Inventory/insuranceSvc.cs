using Node.Database;
using Node.Exceptions.insuranceSvc;
using Node.Exceptions.jumpCloneSvc;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
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
        
        public insuranceSvc(ItemManager itemManager, InsuranceDB db, MarketDB marketDB, BoundServiceManager manager) : base(manager)
        {
            this.DB = db;
            this.ItemManager = itemManager;
            this.MarketDB = marketDB;
        }

        protected insuranceSvc(ItemManager itemManager, InsuranceDB db, MarketDB marketDB, int stationID, BoundServiceManager manager) : base (manager)
        {
            this.mStationID = stationID;
            this.DB = db;
            this.ItemManager = itemManager;
            this.MarketDB = marketDB;
        }
        
        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            return new insuranceSvc(this.ItemManager, this.DB, this.MarketDB, objectData as PyInteger, this.BoundServiceManager);
        }

        public PyDataType GetContracts(CallInformation call)
        {
            if (this.mStationID == 0)
            {
                if (call.Client.ShipID == null)
                    throw new CustomError($"The character is not onboard any ship");
                
                return this.DB.GetContractForShip(call.Client.EnsureCharacterIsSelected(), (int) call.Client.ShipID);
            }
            else
            {
                return this.DB.GetContractsForShipsOnStation(call.Client.EnsureCharacterIsSelected(), this.mStationID);
            }
        }

        public PyDataType GetContractForShip(PyInteger itemID, CallInformation call)
        {
            return this.DB.GetContractForShip(call.Client.EnsureCharacterIsSelected(), itemID);
        }

        public PyDataType GetContracts(PyInteger includeCorp, CallInformation call)
        {
            if (includeCorp == 0)
                return this.DB.GetContractsForShipsOnStation(call.Client.EnsureCharacterIsSelected(), this.mStationID);
            else
                return this.DB.GetContractsForShipsOnStationIncludingCorp(call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID, this.mStationID);
        }

        public PyDataType InsureShip(PyInteger itemID, PyDecimal insuranceCost, PyInteger isCorpItem, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (this.ItemManager.IsItemLoaded(itemID) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Ship item = this.ItemManager.GetItem(itemID) as Ship;
            Character character = this.ItemManager.GetItem(callerCharacterID) as Character;

            if ((isCorpItem == 1 && item.OwnerID != call.Client.CorporationID) && item.OwnerID != callerCharacterID)
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
                MarketReference.Insurance, character.ID, this.ItemManager.SecureCommerceCommision.ID, -item.ID,
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
            
            if (this.ItemManager.IsItemLoaded(itemID) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Ship item = this.ItemManager.GetItem(itemID) as Ship;
            Character character = this.ItemManager.GetItem(callerCharacterID) as Character;

            if (item.OwnerID != call.Client.CorporationID && item.OwnerID != callerCharacterID)
                throw new MktNotOwner();

            // remove insurance record off the database
            this.DB.UnInsureShip(itemID);
            
            return null;
        }
    }
}