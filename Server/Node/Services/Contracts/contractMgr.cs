using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Services;
using EVE.Packets.Exceptions;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Exceptions.contractMgr;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using Node.Notifications.Client.Contracts;
using Node.Notifications.Nodes.Inventory;
using Node.Services.Account;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using Container = Node.Inventory.Items.Types.Container;

namespace Node.Services.Contracts
{
    public class contractMgr : IService
    {
        // TODO: THE TYPEID FOR THE BOX IS 24445
        private ContractDB DB { get; }
        private ItemDB ItemDB { get; }
        private MarketDB MarketDB { get; }
        private CharacterDB CharacterDB { get; }
        private ItemFactory ItemFactory { get; }
        private TypeManager TypeManager => this.ItemFactory.TypeManager;
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private NotificationManager NotificationManager { get; }
        private WalletManager WalletManager { get; }

        public contractMgr(ContractDB db, ItemDB itemDB, MarketDB marketDB, CharacterDB characterDB, ItemFactory itemFactory, NotificationManager notificationManager, WalletManager walletManager)
        {
            this.DB = db;
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.CharacterDB = characterDB;
            this.ItemFactory = itemFactory;
            this.NotificationManager = notificationManager;
            this.WalletManager = walletManager;
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

        private void PrepareItemsForCourierOrAuctionContract(MySqlConnection connection, ulong contractID,
            ClusterConnection clusterConnection, PyList<PyList> itemList, Station station, int ownerID, int shipID)
        {
            // create the container in the system to ensure it's not visible to the player
            Container container = this.ItemFactory.CreateSimpleItem(this.TypeManager[Types.PlasticWrap],
                this.ItemFactory.LocationSystem.ID, station.ID, Flags.None) as Container;
            
            Dictionary<int, ContractDB.ItemQuantityEntry> items =
                this.DB.PrepareItemsForContract(connection, contractID, itemList, station, ownerID, container.ID, shipID);

            double volume = 0;
            
            // build notification for item changes
            OnItemChange changes = new OnItemChange();
            long stationNode = this.SystemManager.GetNodeStationBelongsTo(station.ID);
            bool stationBelongsToUs = this.SystemManager.StationBelongsToUs(station.ID);
            
            // notify the changes in the items to the nodes
            foreach ((int _, ContractDB.ItemQuantityEntry item) in items)
            {
                if (stationNode == 0 || stationBelongsToUs == true)
                {
                    ItemEntity entity = this.ItemFactory.LoadItem(item.ItemID);

                    entity.LocationID = container.ID;
                    entity.Persist();
                    
                    // notify the character
                    this.NotificationManager.NotifyCharacter(ownerID, Notifications.Client.Inventory.OnItemChange.BuildLocationChange(entity, station.ID));
                }
                else
                {
                    // queue the notification
                    changes.AddChange(item.ItemID, "locationID", station.ID, container.ID);
                }

                // ensure the volume is taken into account
                volume += item.Volume;
            }
            
            // notify the proper node if needed
            if (changes.Updates.Count > 0)
                this.NotificationManager.NotifyNode(stationNode, changes);
            
            // update the contract with the crate and the new volume
            this.DB.UpdateContractCrateAndVolume(ref connection, contractID, container.ID, volume);
        }
        
        public PyDataType CreateContract(PyInteger contractType, PyInteger availability, PyInteger assigneeID,
            PyInteger expireTime, PyInteger courierContractDuration, PyInteger startStationID, PyInteger endStationID, PyInteger priceOrStartingBid,
            PyInteger reward, PyInteger collateralOrBuyoutPrice, PyString title, PyString description, CallInformation call)
        {
            if (assigneeID != null && (ItemFactory.IsNPC(assigneeID) == true || ItemFactory.IsNPCCorporationID(assigneeID) == true))
                throw new ConNPCNotAllowed();
            
            // check for limits on contract creation
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (expireTime < 1440 || (courierContractDuration < 1 && contractType == (int) ContractTypes.Courier))
                throw new ConDurationZero();
            if (startStationID == endStationID)
                throw new ConDestinationSame();

            if (call.NamedPayload.TryGetValue("forCorp", out PyBool forCorp) == false)
                forCorp = false;

            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            
            if (forCorp == false)
            {
                // check limits for the character
                long maximumContracts = 1 + (4 * character.GetSkillLevel(Types.Contracting));

                if (maximumContracts <= this.DB.GetOutstandingContractsCountForPlayer(callerCharacterID))
                    throw new ConTooManyContractsMax(maximumContracts);
            }
            else
            {
                throw new CustomError("Not supported yet!");
            }
            
            Station station = this.ItemFactory.GetStaticStation(startStationID);

            using MySqlConnection connection = this.MarketDB.AcquireMarketLock();
            try
            {
                // take reward from the character
                if (reward > 0)
                {
                    using Wallet wallet = this.WalletManager.AcquireWallet(callerCharacterID, 1000);
                    {
                        wallet.EnsureEnoughBalance(reward);
                        wallet.CreateJournalRecord(
                            MarketReference.ContractRewardAdded, null, null, -reward, ""
                        );
                    }
                }
                
                // named payload contains itemList, flag, requestItemTypeList and forCorp
                ulong contractID = this.DB.CreateContract(connection, call.Client.EnsureCharacterIsSelected(),
                    call.Client.CorporationID, call.Client.AllianceID, (ContractTypes) (int) contractType, availability,
                    assigneeID ?? 0, expireTime, courierContractDuration, startStationID, endStationID, priceOrStartingBid,
                    reward, collateralOrBuyoutPrice, title, description, 1000);

                // TODO: take broker's tax, deposit and sales tax
                
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

                ContractDB.Contract contract = this.DB.GetContract(connection, contractID);

                // ensure the contract is still in progress
                if (contract.Status != ContractStatus.Outstanding)
                    throw new ConAuctionAlreadyClaimed();

                this.DB.GetMaximumBid(connection, contractID, out int maximumBidderID, out int maximumBid);
            
                // calculate next bid slot
                int nextMinimumBid = maximumBid + (int) Math.Max(0.1 * (double) contract.Price, 1000);

                if (quantity < nextMinimumBid)
                    throw new ConBidTooLow(quantity, nextMinimumBid);

                // take the bid's money off the wallet
                using Wallet bidderWallet = this.WalletManager.AcquireWallet(bidderID, 1000);
                {
                    bidderWallet.EnsureEnoughBalance(quantity);
                    bidderWallet.CreateJournalRecord(
                        MarketReference.ContractAuctionBid, null, null, -quantity
                    );
                }
            
                // check who we'd outbid and notify them
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
                
                // return the money for the player that was the highest bidder
                using Wallet maximumBidderWallet = this.WalletManager.AcquireWallet(maximumBidderID, 1000);
                {
                    maximumBidderWallet.CreateJournalRecord(
                        MarketReference.ContractAuctionBidRefund, null, null, maximumBid, ""
                    );
                }
                return bidID;
            }
            finally
            {
                this.MarketDB.ReleaseMarketLock(connection);
            }
        }

        private void AcceptItemExchangeContract(MySqlConnection connection, Client client, ContractDB.Contract contract, Station station, int ownerID, Flags flag = Flags.Hangar)
        {
            List<ContractDB.ItemQuantityEntry> offeredItems = this.DB.GetOfferedItems(connection, contract.ID);
            Dictionary<int, int> itemsToCheck = this.DB.GetRequiredItemTypeIDs(connection, contract.ID);
            List<ContractDB.ItemQuantityEntry> changedItems = this.DB.CheckRequiredItemsAtStation(connection, station, ownerID, contract.IssuerID, flag, itemsToCheck);

            // extract the crate
            this.DB.ExtractCrate(connection, contract.CrateID, station.ID, ownerID);
            
            long stationNode = this.SystemManager.GetNodeStationBelongsTo(station.ID);

            if (stationNode == 0 || this.SystemManager.StationBelongsToUs(station.ID) == true)
            {
                foreach (ContractDB.ItemQuantityEntry change in changedItems)
                {
                    ItemEntity item = this.ItemFactory.LoadItem(change.ItemID);

                    if (change.Quantity == 0)
                    {
                        // remove item from the meta inventories
                        this.ItemFactory.MetaInventoryManager.OnItemDestroyed(item);
                        // temporarily move the item to the recycler, let the current owner know
                        item.LocationID = this.ItemFactory.LocationRecycler.ID;
                        client.NotifyMultiEvent(Notifications.Client.Inventory.OnItemChange.BuildLocationChange(item, station.ID));
                        // now set the item to the correct owner and place and notify it's new owner
                        // TODO: TAKE forCorp INTO ACCOUNT
                        item.LocationID = station.ID;
                        item.OwnerID = contract.IssuerID;
                        this.NotificationManager.NotifyCharacter(contract.IssuerID, Notifications.Client.Inventory.OnItemChange.BuildNewItemChange(item));
                        // add the item back to meta inventories if required
                        this.ItemFactory.MetaInventoryManager.OnItemLoaded(item);
                    }
                    else
                    {
                        int oldQuantity = item.Quantity;
                        item.Quantity = change.Quantity;
                        client.NotifyMultiEvent(Notifications.Client.Inventory.OnItemChange.BuildQuantityChange(item, oldQuantity));
                        
                        item.Persist();

                        // unload the item if required
                        this.ItemFactory.UnloadItem(item);
                    }
                }

                // move the offered items
                foreach (ContractDB.ItemQuantityEntry entry in offeredItems)
                {
                    ItemEntity item = this.ItemFactory.LoadItem(entry.ItemID);

                    item.LocationID = station.ID;
                    item.OwnerID = ownerID;
                    
                    client.NotifyMultiEvent(Notifications.Client.Inventory.OnItemChange.BuildLocationChange(item, contract.CrateID));
                    
                    item.Persist();

                    // unload the item if possible
                    this.ItemFactory.UnloadItem(item);
                }
            }
            else
            {
                OnItemChange changes = new OnItemChange();
            
                foreach (ContractDB.ItemQuantityEntry change in changedItems)
                {
                    if (change.Quantity == 0)
                    {
                        changes
                            .AddChange(change.ItemID, "locationID", contract.CrateID, station.ID)
                            .AddChange(change.ItemID, "ownerID", contract.IssuerID, ownerID);
                    }
                    else
                    {
                        // change the item quantity
                        changes.AddChange(change.ItemID, "quantity", change.OldQuantity, change.Quantity);
                        // create a new item and notify the new node about it
                        // TODO: HANDLE BLUEPRINTS TOO! RIGHT NOW NO DATA IS COPIED FOR THEM
                        ItemEntity item = this.ItemFactory.CreateSimpleItem(
                            this.TypeManager[change.TypeID], contract.IssuerID, station.ID, Flags.Hangar, change.OldQuantity - change.Quantity, false, false
                        );
                        // unload the created item
                        this.ItemFactory.UnloadItem(item);
                        changes.AddChange(item.ID, "location", 0, station.ID);
                    }
                }

                // move the offered items
                foreach (ContractDB.ItemQuantityEntry entry in offeredItems)
                {
                    // TODO: TAKE INTO ACCOUNT forCorp
                    changes
                        .AddChange(entry.ItemID, "locationID", contract.CrateID, station.ID)
                        .AddChange(entry.ItemID, "ownerID", contract.IssuerID, station.ID);
                }
            }
            
            // the contract was properly accepted, update it's status
            this.DB.UpdateContractStatus(ref connection, contract.ID, ContractStatus.Finished);
            this.DB.UpdateAcceptorID(ref connection, contract.ID, ownerID);
            this.DB.UpdateAcceptedDate(ref connection, contract.ID);
            this.DB.UpdateCompletedDate(ref connection, contract.ID);
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

                Station station = this.ItemFactory.GetStaticStation(contract.StationID);
                    
                // TODO: CHECK REWARD/PRICE
                switch (contract.Type)
                {
                    case ContractTypes.ItemExchange:
                        this.AcceptItemExchangeContract(connection, call.Client, contract, station, callerCharacterID);
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