using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Services;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Exceptions.contractMgr;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Inventory.Notifications;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Contracts
{
    public class contractMgr : IService
    {
        // TODO: THE TYPEID FOR THE BOX IS 24445
        private ContractDB DB { get; }
        private ItemDB ItemDB { get; }
        private MarketDB MarketDB { get; }
        private ItemManager ItemManager { get; }
        private TypeManager TypeManager { get; }
        private SystemManager SystemManager { get; }
        private NotificationManager NotificationManager { get; }

        public contractMgr(ContractDB db, ItemDB itemDB, MarketDB marketDB, ItemManager itemManager, TypeManager typeManager, SystemManager systemManager, NotificationManager notificationManager)
        {
            this.DB = db;
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
            this.SystemManager = systemManager;
            this.NotificationManager = notificationManager;
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

        public PyDataType GetContractListForOwner(PyInteger ownerID, PyInteger contractStatus, PyInteger contractType, PyBool issuedToUs, CallInformation call)
        {
            int? contractTypeInt = null;
            int? contractStatusInt = null;
            int? startContractID = null;
            PyDataType pyStartContractID = null;

            call.NamedPayload.TryGetValue("startContractID", out pyStartContractID);

            if (pyStartContractID is PyInteger)
                startContractID = pyStartContractID as PyInteger;
            
            int resultsPerPage = call.NamedPayload["num"] as PyInteger;

            if (contractStatus != null)
                contractStatusInt = contractStatus;
            if (contractType != null)
                contractTypeInt = contractType;
            
            return KeyVal.FromDictionary(new PyDictionary()
                {
                    ["contracts"] = this.DB.GetContractsForOwner(ownerID, contractTypeInt, contractStatusInt),
                    ["bids"] = this.DB.GetContractBidsForOwner(ownerID, contractTypeInt, contractStatusInt),
                    ["items"] = this.DB.GetContractItemsForOwner(ownerID, contractTypeInt, contractStatusInt)
                }
            );
        }

        public PyDataType GetItemsInStation(PyInteger stationID, PyInteger forCorp, CallInformation call)
        {
            // TODO: HANDLE CORPORATION!
            if (forCorp == 1)
                throw new CustomError("This call doesn't support forCorp parameter yet!");

            return this.DB.GetItemsInStationForPlayer(call.Client.EnsureCharacterIsSelected(), stationID);
        }
        
        private void NotifyItemChange(ClusterConnection connection, long nodeID, int itemID, string key, PyDataType newValue)
        {
            this.NotificationManager.NotifyNode(nodeID, "OnItemUpdate",
                new PyTuple(2)
                {
                    [0] = itemID,
                    [1] = new PyDictionary() { [key] = newValue}
                }
            );
        }

        private void PrepareItemsForCourierOrAuctionContract(MySqlConnection connection, ulong contractID,
            ClusterConnection clusterConnection, PyList<PyList> itemList, Station station, int ownerID, int shipID)
        {
            // create the container in the system to ensure it's not visible to the player
            Container container = this.ItemManager.CreateSimpleItem(this.TypeManager[ItemTypes.PlasticWrap],
                this.ItemManager.LocationSystem.ID, station.ID, ItemFlags.None) as Container;
            
            Dictionary<int, ContractDB.ItemQuantityEntry> items =
                this.DB.PrepareItemsForContract(connection, contractID, itemList, station, ownerID, container.ID, shipID);

            double volume = 0;
            int crateID = container.ID;
            
            // unload the container crate so the node can load it properly
            this.ItemManager.UnloadItem(container);
            
            // notify the changes in the items to the nodes
            foreach ((int itemID, ContractDB.ItemQuantityEntry item) in items)
            {
                // tell the correct node about the change
                this.NotifyItemChange(clusterConnection, item.NodeID, itemID, "locationID", crateID);

                // ensure the volume is taken into account
                volume += item.Volume;
            }
            
            // update the contract with the crate and the new volume
            this.DB.UpdateContractCrateAndVolume(ref connection, contractID, crateID, volume);
        }
        
        public PyDataType CreateContract(PyInteger contractType, PyInteger availability, PyInteger assigneeID,
            PyInteger expireTime, PyInteger courierContractDuration, PyInteger startStationID, PyInteger endStationID, PyInteger priceOrStartingBid,
            PyInteger reward, PyInteger collateralOrBuyoutPrice, PyString title, PyString description, CallInformation call)
        {
            if (assigneeID != null && (ItemManager.IsNPC(assigneeID) == true || ItemManager.IsNPCCorporationID(assigneeID) == true))
                throw new ConNPCNotAllowed();
            
            // check for limits on contract creation
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (expireTime < 1440 || (courierContractDuration < 1 && contractType == (int) ContractTypes.Courier))
                throw new ConDurationZero();
            if (startStationID == endStationID)
                throw new ConDestinationSame();

            if (call.NamedPayload.TryGetValue("forCorp", out PyBool forCorp) == false)
                forCorp = false;

            Character character = this.ItemManager.GetItem<Character>(callerCharacterID);
            
            if (forCorp == false)
            {
                // check limits for the character
                long maximumContracts = 1 + (4 * character.GetSkillLevel(ItemTypes.Contracting));

                if (maximumContracts <= this.DB.GetOutstandingContractsCountForPlayer(callerCharacterID))
                    throw new ConTooManyContractsMax(maximumContracts);
            }
            else
            {
                throw new CustomError("Not supported yet!");
            }
            
            Station station = this.ItemManager.GetStaticStation(startStationID);

            using MySqlConnection connection = this.MarketDB.AcquireMarketLock();
            try
            {
                // named payload contains itemList, flag, requestItemTypeList and forCorp
                ulong contractID = this.DB.CreateContract(connection, call.Client.EnsureCharacterIsSelected(),
                    call.Client.CorporationID, call.Client.AllianceID, (ContractTypes) (int) contractType, availability,
                    assigneeID, expireTime, courierContractDuration, startStationID, endStationID, priceOrStartingBid,
                    reward, collateralOrBuyoutPrice, title, description, 1000);

                switch ((int) contractType)
                {
                    case (int) ContractTypes.ItemExchange:
                    case (int) ContractTypes.Auction:
                    case (int) ContractTypes.Courier:
                        this.PrepareItemsForCourierOrAuctionContract(
                            connection,
                            contractID,
                            call.Client.ClusterConnection,
                            (call.NamedPayload["itemList"] as PyList).GetEnumerable<PyList>(),
                            station,
                            callerCharacterID,
                            (int) call.Client.ShipID
                        );
                        break;
                    case (int) ContractTypes.Loan:
                        break;
                    default:
                        throw new CustomError("Unknown contract type");
                }
                
                if (contractType == (int) ContractTypes.ItemExchange)
                    this.DB.PrepareRequestedItems(connection, contractID, (call.NamedPayload["requestItemTypeList"] as PyList).GetEnumerable<PyList>());
                
                return contractID;
            }
            finally
            {
                this.MarketDB.ReleaseMarketLock(connection);
            }
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
            using MySqlConnection connection = this.MarketDB.AcquireMarketLock();
            try
            {
                // get contract type and status
                
                // get the items back to where they belong (if any)
                
                // 
            }
            finally
            {
                this.MarketDB.ReleaseMarketLock(connection);
            }
            return null;
        }

        public PyDataType SplitStack(PyInteger stationID, PyInteger itemID, PyInteger newStack, PyInteger forCorp,
            PyInteger flag, CallInformation call)
        {
            return null;
        }

        public PyDataType GetItemsInContainer(PyInteger locationID, PyInteger containerID, PyInteger forCorp,
            PyInteger flag, CallInformation call)
        {
            return this.DB.GetItemsInContainer(call.Client.EnsureCharacterIsSelected(), containerID);
        }
    }
}