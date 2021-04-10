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
using Node.Market;
using Node.Network;
using Node.Notifications.Contracts;
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
        private CharacterDB CharacterDB { get; }
        private ItemManager ItemManager { get; }
        private TypeManager TypeManager { get; }
        private SystemManager SystemManager { get; }
        private NotificationManager NotificationManager { get; }

        public contractMgr(ContractDB db, ItemDB itemDB, MarketDB marketDB, CharacterDB characterDB, ItemManager itemManager, TypeManager typeManager, SystemManager systemManager, NotificationManager notificationManager)
        {
            this.DB = db;
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.CharacterDB = characterDB;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
            this.SystemManager = systemManager;
            this.NotificationManager = notificationManager;
        }

        private void NotifyBalanceChange(long nodeID, int characterID, double newBalance)
        {
            this.NotificationManager.NotifyNode(nodeID, "OnBalanceUpdate",
                new PyTuple(3)
                {
                    [0] = characterID,
                    [1] = 1000,
                    [2] = newBalance
                }
            );
        }

        public PyDataType NumRequiringAttention(CallInformation call)
        {
            // check for contracts that we've been outbid at and send notifications
            // TODO: HANDLE CORPORATION CONTRACTS TOO!
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            List<int> outbidContracts = this.DB.FetchLoginCharacterContractBids(callerCharacterID);
            List<int> assignedContracts = this.DB.FetchLoginCharacterContractAssigned(callerCharacterID);
            
            foreach (int contractID in outbidContracts)
                call.Client.NotifyMultiEvent(new OnContractOutbid(contractID));
            foreach (int contractID in assignedContracts)
                call.Client.NotifyMultiEvent(new OnContractAssigned(contractID));
            
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
            call.NamedPayload.TryGetValue("startContractID", out PyInteger startContractID);
            int resultsPerPage = call.NamedPayload["num"] as PyInteger;

            // limit the number of results to 100
            if (resultsPerPage > 100)
                resultsPerPage = 100;

            PyList<PyInteger> issuedByIDs = null;
            
            if (issuedToUs == false)
                issuedByIDs = new PyList<PyInteger>(1) {[0] = ownerID};

            List<int> contractList = this.DB.GetContractList(
                startContractID, resultsPerPage, null, null, issuedByIDs, issuedToUs == true ? ownerID : null,
                null, null,null, 0, 0, contractType, null,
                call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID, ownerID,
                contractStatus, true
            );

            return KeyVal.FromDictionary(new PyDictionary()
                {
                    ["contracts"] = this.DB.GetInformationForContractList(contractList),
                    ["bids"] = this.DB.GetBidsForContractList(contractList),
                    ["items"] = this.DB.GetItemsForContractList(contractList)
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
            PyList<PyInteger> notIssuedByIDs = null;
            PyList<PyInteger> issuedByIDs = null;

            call.NamedPayload.TryGetValue("startContractID", out PyInteger startContractID);
            int resultsPerPage = call.NamedPayload["num"] as PyInteger;

            filters.TryGetValue("regionID", out PyInteger regionID);
            filters.TryGetValue("stationID", out PyInteger stationID);
            filters.TryGetValue("solarSystemID", out PyInteger solarSystemID);
            filters.TryGetValue("itemTypeID", out PyInteger itemTypeID);
            filters.TryGetValue("assigneeID", out PyInteger assigneeID);
            filters.TryGetValue("itemGroupID", out PyInteger itemGroupID);
            filters.TryGetValue("itemCategoryID", out PyInteger itemCategoryID);
            filters.TryGetValue("priceMax", out PyInteger priceMax);
            filters.TryGetValue("priceMin", out PyInteger priceMin);
            filters.TryGetValue("type", out PyInteger type);
            filters.TryGetValue("description", out PyString description);

            if (priceMax < 0 || priceMin < 0 || priceMax < priceMin)
                throw new ConMinMaxPriceError();

            if (filters.TryGetValue("issuedByIDs", out PyList issuedIDs) == true && issuedIDs is not null)
                issuedByIDs = issuedIDs.GetEnumerable<PyInteger>();
            if (filters.TryGetValue("notIssuedByIDs", out PyList notIssuedIDs) == true && notIssuedIDs is not null)
                notIssuedByIDs = notIssuedIDs.GetEnumerable<PyInteger>();
            
            // limit the number of results to 100
            if (resultsPerPage > 100)
                resultsPerPage = 100;
            
            int? locationID = null;

            if (stationID is not null)
                locationID = stationID;
            else if (solarSystemID is not null)
                locationID = solarSystemID;
            else if (regionID is not null)
                locationID = regionID;

            List<int> contractList = this.DB.GetContractList(
                startContractID, resultsPerPage, itemTypeID, notIssuedByIDs, issuedByIDs, assigneeID,
                locationID, itemGroupID, itemCategoryID, priceMax ?? 0, priceMin ?? 0, type, description,
                call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID
            );

            return KeyVal.FromDictionary(new PyDictionary()
                {
                    ["contracts"] = this.DB.GetInformationForContractList(contractList),
                    ["bids"] = this.DB.GetBidsForContractList(contractList),
                    ["items"] = this.DB.GetItemsForContractList(contractList)
                }
            );
        }

        public PyDataType GetContract(PyInteger contractID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            // TODO: Check for regionID ConWrongRegion
            
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

        public PyDataType GetMyExpiredContractList(PyBool isCorp, CallInformation call)
        {
            int ownerID = 0;

            if (isCorp == true)
                ownerID = call.Client.CorporationID;
            else
                ownerID = call.Client.EnsureCharacterIsSelected();

            List<int> contractList = this.DB.GetContractList(
                null, 0, null, null, null, null, null, null, null, 0, 0,
                null, null, call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID, ownerID, null, true, true
            );
            
            return KeyVal.FromDictionary(new PyDictionary()
                {
                    ["contracts"] = this.DB.GetInformationForContractList(contractList),
                    ["bids"] = this.DB.GetBidsForContractList(contractList),
                    ["items"] = this.DB.GetItemsForContractList(contractList)
                }
            );
        }

        public PyDataType GetMyBids(PyInteger isCorp, CallInformation call)
        {
            return this.GetMyBids(isCorp == 1, call);
        }

        public PyDataType GetMyBids(PyBool isCorp, CallInformation call)
        {
            int ownerID = 0;

            if (isCorp == true)
                ownerID = call.Client.CorporationID;
            else
                ownerID = call.Client.EnsureCharacterIsSelected();

            List<int> contractList = this.DB.GetContractListByOwnerBids(ownerID);

            return KeyVal.FromDictionary(new PyDictionary()
                {
                    ["contracts"] = this.DB.GetInformationForContractList(contractList),
                    ["bids"] = this.DB.GetBidsForContractList(contractList),
                    ["items"] = this.DB.GetItemsForContractList(contractList)
                }
            );
        }

        public PyDataType GetMyCurrentContractList(PyBool acceptedByMe, PyBool isCorp, CallInformation call)
        {
            int ownerID = 0;

            if (isCorp == true)
                ownerID = call.Client.CorporationID;
            else
                ownerID = call.Client.EnsureCharacterIsSelected();

            List<int> contractList = null;

            if (acceptedByMe == true)
            {
                contractList = this.DB.GetContractListByAcceptor(ownerID);
            }
            else
            {
                contractList = this.DB.GetContractList(
                    null, 0, null, null, new PyList<PyInteger>(1){[0] = ownerID},
                    null, null, null, null, 0, 0, null,
                    null, call.Client.EnsureCharacterIsSelected(), call.Client.CorporationID,
                    ownerID, (int) ContractStatus.InProgress, true, true
                );
            }

            return KeyVal.FromDictionary(new PyDictionary()
                {
                    ["contracts"] = this.DB.GetInformationForContractList(contractList),
                    ["bids"] = this.DB.GetBidsForContractList(contractList),
                    ["items"] = this.DB.GetItemsForContractList(contractList)
                }
            );
        }

        public PyDataType PlaceBid(PyInteger contractID, PyInteger quantity, PyBool forCorp, PyObjectData locationData, CallInformation call)
        {
            using MySqlConnection connection = this.MarketDB.AcquireMarketLock();
            try
            {
                // TODO: SUPPORT PROPER CORP WALLET
                int bidderID = call.Client.EnsureCharacterIsSelected();

                if (forCorp == true)
                {
                    bidderID = call.Client.CorporationID;
                    throw new UserError("Corp bidding is not supported for now!");
                }
                
                // ensure there's enough balance left
                Character character = this.ItemManager.GetItem<Character>(bidderID);
                
                character.EnsureEnoughBalance(quantity);
                
                // change the character's balance
                character.Balance -= quantity;
                character.Persist();
                
                // create the journal entry
                this.MarketDB.CreateJournalForCharacter(MarketReference.ContractAuctionBid, character.ID, character.ID, null, null, -quantity, character.Balance, "", 1000);
                
                // update the player's balance
                call.Client.NotifyBalanceUpdate(character.Balance);

                ContractDB.Contract contract = this.DB.GetContract(connection, contractID);

                // ensure the contract is still in progress
                if (contract.Status != ContractStatus.Outstanding)
                    throw new ConAuctionAlreadyClaimed();

                this.DB.GetMaximumBid(connection, contractID, out int maximumBidderID, out int maximumBid);
            
                // calculate next bid slot
                int nextMinimumBid = maximumBid + (int) Math.Max((0.1 * contract.Price), 1000);

                if (quantity < nextMinimumBid)
                    throw new ConBidTooLow(quantity, nextMinimumBid);
            
                // check who we'd outbid first and notify them
                this.DB.GetOutbids(connection, contractID, quantity, out List<int> characterIDs, out List<int> corporationIDs);

                OnContractOutbid notification = new OnContractOutbid(contractID);

                foreach (int corporationID in corporationIDs)
                    if (corporationID != bidderID)
                        this.NotificationManager.NotifyCorporation(corporationID, notification);

                foreach (int characterID in characterIDs)
                    if (characterID != bidderID)
                        this.NotificationManager.NotifyCharacter(characterID, notification);

                // finally place the bid
                ulong bidID = this.DB.PlaceBid(connection, contractID, quantity, bidderID, forCorp);
                
                // now return the money for the player that was the highest bidder
                double balance = this.CharacterDB.GetCharacterBalance(maximumBidderID) + maximumBid;

                // create the journal entry
                this.MarketDB.CreateJournalForCharacter(MarketReference.ContractAuctionBidRefund, maximumBidderID, maximumBidderID, null, null, maximumBid, balance, "", 1000);
                
                // save the balance of the character
                this.CharacterDB.SetCharacterBalance(maximumBidderID, balance);
                    
                // if the character is loaded in any node inform that node of the change in wallet
                int characterNode = this.ItemDB.GetItemNode(maximumBidderID);
                    
                if (characterNode > 0)
                    this.NotifyBalanceChange(characterNode, maximumBidderID, balance);

                return bidID;
            }
            finally
            {
                this.MarketDB.ReleaseMarketLock(connection);
            }
        }

        private void AcceptItemExchangeContract(MySqlConnection connection, ClusterConnection clusterConnection, ContractDB.Contract contract, Station station, int ownerID, ItemFlags flag = ItemFlags.Hangar)
        {
            Dictionary<int, int> itemsToCheck = this.DB.GetRequiredItemTypeIDs(connection, contract.ID);
            List<ContractDB.ItemQuantityEntry> changedItems =
                this.DB.CheckRequiredItemsAtStation(connection, station, ownerID, flag, itemsToCheck);

            foreach (ContractDB.ItemQuantityEntry change in changedItems)
            {
                // ignore items that are not in a node
                if (change.NodeID == 0)
                    continue;
                
                if (change.Quantity == 0)
                    this.NotifyItemChange(clusterConnection, change.NodeID, change.ItemID, "locationID", this.ItemManager.LocationRecycler.ID);
                else
                    this.NotifyItemChange(clusterConnection, change.NodeID, change.ItemID, "quantity", change.Quantity);
            }
            
            // move items attached to the contract to the new owner
            
            // the contract was properly accepted, update it's status
            this.DB.UpdateContractStatus(ref connection, contract.ID, ContractStatus.Finished);
            this.DB.UpdateAcceptorID(ref connection, contract.ID, ownerID);
            // notify the contract as being accepted
            if(contract.ForCorp == false)
                this.NotificationManager.NotifyCharacter(contract.IssuerID, new OnContractAccepted(contract.ID));
            else
                this.NotificationManager.NotifyCorporation(contract.IssuerCorpID, new OnContractAccepted(contract.ID));
        }

        public PyDataType AcceptContract(PyInteger contractID, PyBool forCorp, CallInformation call)
        {
            if (forCorp == true)
                throw new UserError("Cannot accept contracts for corporation yet");
            
            using MySqlConnection connection = this.MarketDB.AcquireMarketLock();
            try
            {
                int callerCharacterID = call.Client.EnsureCharacterIsSelected();

                // TODO: SUPPORT forCorp
                ContractDB.Contract contract = this.DB.GetContract(connection, contractID);

                if (contract.Status != ContractStatus.Outstanding)
                    throw new ConContractNotOutstanding();
                if (contract.ExpireTime < DateTime.UtcNow.ToFileTimeUtc())
                    throw new ConContractExpired();

                Station station = this.ItemManager.GetStaticStation(contract.StationID);
                    
                switch (contract.Type)
                {
                    case ContractTypes.ItemExchange:
                        this.AcceptItemExchangeContract(connection, call.Client.ClusterConnection, contract, station, callerCharacterID);
                        break;
                    case ContractTypes.Auction:
                        throw new CustomError("Auctions cannot be accepted!");
                    case ContractTypes.Courier:
                        throw new CustomError("Courier contracts not supported yet!");
                    case ContractTypes.Loan:
                        throw new CustomError("Loan contracts not supported yet!");
                    case ContractTypes.Freeform:
                        throw new CustomError("Freeform contracts not supported yet!");
                    default:
                        throw new CustomError("Unknown contract type to accept!");
                }
            }
            finally
            {
                this.MarketDB.ReleaseMarketLock(connection);
            }
            
            return null;
        }

        public PyDataType FinishAuction(PyInteger contractID, PyBool forCorp, CallInformation call)
        {
            return null;
        }

        public PyDataType HasFittedCharges(PyInteger stationID, PyInteger itemID, PyInteger forCorp, PyInteger flag, CallInformation call)
        {
            // TODO: IMPLEMENT THIS!
            return null;
        }
    }
}