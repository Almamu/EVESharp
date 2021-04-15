using System;
using Node.Database;
using Node.Exceptions.insuranceSvc;
using Node.Exceptions.jumpCloneSvc;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using Node.Services.Account;
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
        private ItemFactory ItemFactory { get; }
        private MarketDB MarketDB { get; }
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private WalletManager WalletManager { get; }
        
        public insuranceSvc(ItemFactory itemFactory, InsuranceDB db, MarketDB marketDB, WalletManager walletManager, BoundServiceManager manager) : base(manager, null)
        {
            this.DB = db;
            this.ItemFactory = itemFactory;
            this.MarketDB = marketDB;
            this.WalletManager = walletManager;
        }

        protected insuranceSvc(ItemFactory itemFactory, InsuranceDB db, MarketDB marketDB, WalletManager walletManager, BoundServiceManager manager, int stationID, Client client) : base (manager, client)
        {
            this.mStationID = stationID;
            this.DB = db;
            this.ItemFactory = itemFactory;
            this.MarketDB = marketDB;
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
            
            return new insuranceSvc(this.ItemFactory, this.DB, this.MarketDB, this.WalletManager, this.BoundServiceManager, objectData as PyInteger, call.Client);
        }

        public PyList<PyPackedRow> GetContracts(CallInformation call)
        {
            if (this.mStationID == 0)
            {
                int? shipID = call.Client.ShipID;
                
                if (shipID is null)
                    throw new CustomError($"The character is not onboard any ship");

                return new PyList<PyPackedRow>(1)
                {
                    [0] = this.DB.GetContractForShip(call.Client.EnsureCharacterIsSelected(), (int) shipID)
                };
            }
            else
            {
                return this.DB.GetContractsForShipsOnStation(call.Client.EnsureCharacterIsSelected(), this.mStationID);
            }
        }

        public PyPackedRow GetContractForShip(PyInteger itemID, CallInformation call)
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
            
            if (this.ItemFactory.TryGetItem(itemID, out Ship item) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);

            if (isCorpItem == 1 && item.OwnerID != call.Client.CorporationID && item.OwnerID != callerCharacterID)
                throw new MktNotOwner();

            if (item.Singleton == false)
                throw new InsureShipFailed("Only assembled ships can be insured");

            if (this.DB.IsShipInsured(item.ID, out string ownerName) == true)
                throw new InsureShipFailedSingleContract(ownerName);

            using Wallet wallet = this.WalletManager.AcquireWallet(character.ID, 1000);
            {
                wallet.EnsureEnoughBalance(insuranceCost);
                wallet.CreateJournalRecord(
                    MarketReference.Insurance, this.ItemFactory.SecureCommerceCommision.ID, -item.ID, -insuranceCost, $"Insurance fee for {item.Name}"
                );
            }
            
            double fraction = insuranceCost * 100 / item.Type.BasePrice;

            // create insurance record
            this.DB.InsureShip(item.ID, isCorpItem == 0 ? callerCharacterID : call.Client.CorporationID, fraction / 5);

            // TODO: CHECK IF THE INSURANCE SHOULD BE CHARGED TO THE CORP
            
            return true;
        }

        public PyDataType UnInsureShip(PyInteger itemID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (this.ItemFactory.TryGetItem(itemID, out Ship item) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);

            if (item.OwnerID != call.Client.CorporationID && item.OwnerID != callerCharacterID)
                throw new MktNotOwner();

            // remove insurance record off the database
            this.DB.UnInsureShip(itemID);
            
            return null;
        }
    }
}