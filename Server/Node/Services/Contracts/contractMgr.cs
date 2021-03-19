using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Services;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Contracts
{
    public class contractMgr : Service
    {
        // TODO: THE TYPEID FOR THE BOX IS 24445
        private ContractDB DB { get; }
        private ItemDB ItemDB { get; }
        private MarketDB MarketDB { get; }
        private ItemManager ItemManager { get; }
        private TypeManager TypeManager { get; }

        public contractMgr(ContractDB db, ItemDB itemDB, MarketDB marketDB, ItemManager itemManager, TypeManager typeManager)
        {
            this.DB = db;
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
        }

        public PyDataType NumRequiringAttention(CallInformation call)
        {
            return this.DB.NumRequiringAttention(call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID);
        }

        public PyDataType NumOutstandingContracts(CallInformation call)
        {
            return this.DB.NumOutstandingContracts(call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID);
        }

        public PyDataType CollectMyPageInfo(PyDataType ignoreList, CallInformation call)
        {
            // TODO: TAKE INTO ACCOUNT THE IGNORE LIST
            
            return this.DB.CollectMyPageInfo(call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID);
        }

        public PyDataType GetContractListForOwner(PyInteger ownerID, PyInteger contractStatus, PyInteger contractType, PyInteger action, CallInformation call)
        {
            int? startContractID = null;
            PyDataType pyStartContractID = null;

            call.NamedPayload.TryGetValue("startContractID", out pyStartContractID);

            if (pyStartContractID is PyInteger)
                startContractID = pyStartContractID as PyInteger;
            
            int resultsPerPage = call.NamedPayload["num"] as PyInteger;
            int characterID = call.Client.EnsureCharacterIsSelected();
            
            return KeyVal.FromDictionary(new PyDictionary()
                {
                    ["contracts"] = this.DB.GetContractsForOwner(characterID, call.Client.CorporationID),
                    ["bids"] = this.DB.GetContractBidsForOwner(characterID, call.Client.CorporationID),
                    ["items"] = this.DB.GetContractItemsForOwner(characterID, call.Client.CorporationID)
                }
            );
        }

        public PyDataType GetItemsInStation(PyInteger stationID, PyInteger forCorp, CallInformation call)
        {
            // TODO: HANDLE CORPORATION!
            if (forCorp == 1)
                throw new UserError("This call doesn't support forCorp parameter yet!");

            return this.DB.GetItemsInStationForPlayer(call.Client.EnsureCharacterIsSelected(), stationID);
        }

        private void PrepareItemsForCourierContract(PyList<PyList<PyInteger>> itemList, Station station, int ownerID, int shipID)
        {
            using MySqlConnection connection = this.MarketDB.AcquireMarketLock();
            try
            {
                Crate crate = this.ItemManager.CreateSimpleItem(this.TypeManager[ItemTypes.PlasticWrap],
                    ownerID, station.ID, ItemFlags.None) as Crate;
                
                Dictionary<int, ContractDB.ItemQuantityEntry> items =
                    this.DB.PrepareItemsForOrder(connection, itemList, station, ownerID, crate.ID, shipID);
            }
            finally
            {
                this.MarketDB.ReleaseMarketLock(connection);
            }
        }
        
        public PyDataType CreateContract(PyInteger contractType, PyInteger availability, PyInteger assigneeID,
            PyInteger expireTime, PyInteger duration, PyInteger startStationID, PyInteger endStationID, PyInteger price,
            PyInteger reward, PyInteger collateral, PyString title, PyString description, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            int crateID = 0;
            Station station = this.ItemManager.GetStation(startStationID);
            
            switch ((int) contractType)
            {
                case (int) ContractTypes.Auction:
                    break;
                case (int) ContractTypes.Courier:
                    this.PrepareItemsForCourierContract(
                        (call.NamedPayload["itemList"] as PyList).GetEnumerable<PyList<PyInteger>>(),
                        station,
                        callerCharacterID,
                        (int) call.Client.ShipID
                    );
                    break;
                case (int) ContractTypes.Loan:
                    break;
                case (int) ContractTypes.ItemExchange:
                    break;
                default:
                    throw new UserError("Unknown contract type");
                    break;
            }
            
            // TODO: CALCULATE DIFFERENT THINGS BASED ON CONTRACT TYPE
            
            // named payload contains itemList, flag, requestItemTypeList and forCorp
            return this.DB.CreateContract(call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID, call.Client.AllianceID,
                (ContractTypes) (int) contractType, availability, assigneeID, expireTime, duration, startStationID,
                endStationID, price, reward, collateral, title, description, 0.0, 0, 1000
            );
        }

        public PyDataType GetContractList(PyObjectData filtersKeyval, CallInformation call)
        {
            PyDictionary<PyString, PyDataType> filters = KeyVal.ToDictionary(filtersKeyval).GetEnumerable<PyString, PyDataType>();

            return null;
        }

        public PyDataType GetContract(PyInteger contractID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            return KeyVal.FromDictionary(new PyDictionary()
                {
                    ["contract"] = this.DB.GetContractInformation(contractID, callerCharacterID, call.Client.CorporationID),
                    ["bids"] = this.DB.GetContractBids(contractID, callerCharacterID, call.Client.CorporationID),
                    ["items"] = this.DB.GetContractItems(contractID, callerCharacterID, call.Client.CorporationID)
                }
            );
        }

        public PyDataType DeleteContract(PyInteger contractID, PyObjectData keyVal, CallInformation call)
        {
            return null;
        }

        public PyDataType SplitStack(PyInteger stationID, PyInteger itemID, PyInteger newStack, PyInteger forCorp,
            PyInteger flag, CallInformation call)
        {
            return null;
        }
    }
}