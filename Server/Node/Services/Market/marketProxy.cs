using System;
using System.Collections.Generic;
using Common.Services;
using EVE;
using EVE.Packets.Complex;
using EVE.Packets.Exceptions;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Exceptions.corpRegistry;
using Node.Exceptions.inventory;
using Node.Exceptions.marketProxy;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using Node.Notifications.Client.Market;
using Node.Notifications.Nodes.Inventory;
using Node.Services.Account;
using Node.StaticData;
using Node.StaticData.Corporation;
using Node.StaticData.Inventory;
using PythonTypes.Types.Primitives;

namespace Node.Services.Market
{
    public class marketProxy : IService
    {
        private static readonly int[] JumpsPerSkillLevel = new int[]
        {
            -1, 0, 5, 10, 20, 50
        };

        private MarketDB DB { get; }
        private CharacterDB CharacterDB { get; }
        private ItemDB ItemDB { get; }
        private CacheStorage CacheStorage { get; }
        private ItemFactory ItemFactory { get; }
        private TypeManager TypeManager => this.ItemFactory.TypeManager;
        private SolarSystemDB SolarSystemDB { get; }
        private NodeContainer NodeContainer { get; }
        private ClientManager ClientManager { get; }
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private NotificationManager NotificationManager { get; }
        private WalletManager WalletManager { get; }
        
        public marketProxy(MarketDB db, CharacterDB characterDB, ItemDB itemDB, SolarSystemDB solarSystemDB, ItemFactory itemFactory, CacheStorage cacheStorage, NodeContainer nodeContainer, ClientManager clientManager, NotificationManager notificationManager, WalletManager walletManager)
        {
            this.DB = db;
            this.CharacterDB = characterDB;
            this.ItemDB = itemDB;
            this.SolarSystemDB = solarSystemDB;
            this.CacheStorage = cacheStorage;
            this.ItemFactory = itemFactory;
            this.NodeContainer = nodeContainer;
            this.ClientManager = clientManager;
            this.NotificationManager = notificationManager;
            this.WalletManager = walletManager;
        }

        public PyDataType CharGetNewTransactions(PyInteger sellBuy, PyInteger typeID, PyDataType clientID,
            PyInteger quantity, PyDataType fromDate, PyDataType maxPrice, PyInteger minPrice, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            TransactionType transactionType = TransactionType.Either;

            if (sellBuy is not null)
            {
                switch (sellBuy.Value)
                {
                    case 0:
                        transactionType = TransactionType.Sell;
                        break;
                    case 1:
                        transactionType = TransactionType.Buy;
                        break;
                }
            }
            
            return this.DB.GetNewTransactions(
                callerCharacterID, null, transactionType, typeID, quantity, minPrice, 1000
            );
        }

        public PyDataType CorpGetNewTransactions(PyInteger sellBuy, PyInteger typeID, PyDataType clientID,
            PyInteger quantity, PyDataType fromDate, PyDataType maxPrice, PyInteger minPrice, PyInteger accountKey, PyInteger who,
            CallInformation call)
        {
            // TODO: SUPPORT THE "who" PARAMETER
            int corporationID = call.Client.CorporationID;
            
            TransactionType transactionType = TransactionType.Either;

            if (sellBuy is not null)
            {
                switch (sellBuy.Value)
                {
                    case 0:
                        transactionType = TransactionType.Sell;
                        break;
                    case 1:
                        transactionType = TransactionType.Buy;
                        break;
                }
            }
            
            // transactions requires accountant roles for corporation
            if (corporationID == call.Client.CorporationID && (CorporationRole.Accountant.Is(call.Client.CorporationRole) == false || CorporationRole.JuniorAccountant.Is(call.Client.CorporationRole) == false))
                throw new CrpAccessDenied(MLS.UI_SHARED_WALLETHINT8);
            
            return this.DB.GetNewTransactions(
                corporationID, null, transactionType, typeID, quantity, minPrice, accountKey
            );
        }

        public PyDataType GetMarketGroups(CallInformation call)
        {
            // check if the cache already exits
            if (this.CacheStorage.Exists("marketProxy", "GetMarketGroups") == false)
            {
                this.CacheStorage.StoreCall(
                    "marketProxy",
                    "GetMarketGroups",
                    this.DB.GetMarketGroups(),
                    DateTime.UtcNow.ToFileTimeUtc()
                );
            }

            return CachedMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("marketProxy", "GetMarketGroups")
            );
        }

        public PyDataType GetCharOrders(CallInformation call)
        {
            return this.DB.GetCharOrders(call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType GetStationAsks(CallInformation call)
        {
            return this.DB.GetStationAsks(call.Client.EnsureCharacterIsInStation());
        }

        public PyDataType GetSystemAsks(CallInformation call)
        {
            call.Client.EnsureCharacterIsSelected();

            return this.DB.GetSystemAsks(call.Client.SolarSystemID2);
        }

        public PyDataType GetRegionBest(CallInformation call)
        {
            call.Client.EnsureCharacterIsSelected();

            return this.DB.GetRegionBest(call.Client.RegionID);
        }
        
        public PyDataType GetOrders(PyInteger typeID, CallInformation call)
        {
            call.Client.EnsureCharacterIsSelected();
         
            // dirty little hack, but should do the trick
            this.CacheStorage.StoreCall(
                "marketProxy",
                "GetOrders_" + typeID,
                this.DB.GetOrders(call.Client.RegionID, call.Client.SolarSystemID2, typeID),
                DateTime.UtcNow.ToFileTimeUtc()
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("marketProxy", "GetOrders_" + typeID);

            return CachedMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType StartupCheck(CallInformation call)
        {
            // this function is called when "buy this" is pressed in the market
            // seems to do some specific check on the market proxy status
            // but we can roll with no return info for it :D
            return null;
        }

        public PyDataType GetOldPriceHistory(PyInteger typeID, CallInformation call)
        {
            return this.DB.GetOldPriceHistory(call.Client.RegionID, typeID);
        }

        public PyDataType GetNewPriceHistory(PyInteger typeID, CallInformation call)
        {
            return this.DB.GetNewPriceHistory(call.Client.RegionID, typeID);
        }

        private void CalculateSalesTax(long accountingLevel, int quantity, double price, out double tax, out double profit)
        {
            double salesTax = (this.NodeContainer.Constants[Constants.mktTransactionTax] / 100.0) * (1 - accountingLevel * 0.1);
            double beforeTax = price * quantity;

            tax = beforeTax * salesTax;
            profit = (price * quantity) - tax;
        }

        private void CalculateBrokerCost(long brokerLevel, int quantity, double price, out double brokerCost)
        {
            double brokerPercentage = ((double) this.NodeContainer.Constants[Constants.marketCommissionPercentage] / 100) * (1 - brokerLevel * 0.05);

            // TODO: GET THE STANDINGS FOR THE CHARACTER
            double factionStanding = 0.0;
            double corpStanding = 0.0;

            double weightedStanding = (0.7 * factionStanding + 0.3 * corpStanding) / 10.0;

            brokerPercentage = brokerPercentage * Math.Pow(2.0, -2 * weightedStanding);
            brokerCost = price * quantity * brokerPercentage;

            if (brokerCost < this.NodeContainer.Constants[Constants.mktMinimumFee])
                brokerCost = this.NodeContainer.Constants[Constants.mktMinimumFee];
        }

        private void CheckSellOrderDistancePermissions(Character character, int stationID)
        {
            Station station = this.ItemFactory.GetStaticStation(stationID);

            if (character.RegionID != station.RegionID)
                throw new MktInvalidRegion();
            
            int jumps = this.SolarSystemDB.GetJumpsBetweenSolarSystems(character.SolarSystemID, station.SolarSystemID);
            long marketingSkillLevel = character.GetSkillLevel(Types.Marketing);
            long maximumDistance = JumpsPerSkillLevel[marketingSkillLevel];

            if (maximumDistance == -1 && character.StationID != stationID)
                throw new MktCantSellItemOutsideStation(jumps);
            if (character.SolarSystemID != station.SolarSystemID && maximumDistance < jumps)
                throw new MktCantSellItem2(jumps, maximumDistance);
        }

        private void CheckBuyOrderDistancePermissions(Character character, int stationID, int duration)
        {
            // immediate orders can be placed regardless of distance
            if (duration == 0)
                return;
            
            Station station = this.ItemFactory.GetStaticStation(stationID);

            if (character.RegionID != station.RegionID)
                throw new MktInvalidRegion();
            
            int jumps = this.SolarSystemDB.GetJumpsBetweenSolarSystems(character.SolarSystemID, station.SolarSystemID);
            long procurementSkillLevel = character.GetSkillLevel(Types.Procurement);
            long maximumDistance = JumpsPerSkillLevel[procurementSkillLevel];

            if (maximumDistance == -1 && character.StationID != stationID)
                throw new MktCantSellItemOutsideStation(jumps);
            if (character.SolarSystemID != station.SolarSystemID && maximumDistance < jumps)
                throw new MktCantSellItem2(jumps, maximumDistance);
        }
        
        private void CheckMatchingBuyOrders(MarketOrder[] orders, int quantity, int stationID)
        {
            // ensure there's enough satisfiable orders for the player
            foreach (MarketOrder order in orders)
            {
                // ensure the order is in the range
                if (order.Range == -1 && order.LocationID != stationID)
                    continue;
                if (order.Range != -1 && order.Range < order.Jumps)
                    continue;
                if (order.UnitsLeft <= quantity)
                    quantity -= order.UnitsLeft;
                if ((order.UnitsLeft <= order.MinimumUnits && order.UnitsLeft <= quantity) || order.MinimumUnits <= quantity)
                    quantity -= Math.Min(order.UnitsLeft, quantity);
                if (quantity <= 0)
                    break;
            }

            // if there's not enough of those here that means the order was not matched
            // being an immediate one there's no other option but to bail out
            if (quantity > 0)
                throw new MktOrderDidNotMatch();
        }

        private void PlaceImmediateSellOrderChar(MySqlConnection connection, Wallet wallet, Character character, int itemID, int typeID, int stationID, int quantity, double price, Client client)
        {
            int solarSystemID = this.ItemFactory.GetStaticStation(stationID).SolarSystemID;
            
            // look for matching buy orders
            MarketOrder[] orders = this.DB.FindMatchingOrders(connection, price, typeID, character.ID, solarSystemID, TransactionType.Buy);
            
            // ensure there's at least some that match
            this.CheckMatchingBuyOrders(orders, quantity, stationID);

            // there's at least SOME orders that can be satisfied, let's start satisfying them one by one whenever possible
            foreach (MarketOrder order in orders)
            {
                int quantityToSell = 0;
                
                // ensure the order is in the range
                if (order.Range == -1 && order.LocationID != stationID)
                    continue;
                if (order.Range != -1 && order.Range < order.Jumps)
                    continue;
                
                if (order.UnitsLeft <= quantity)
                {
                    // if there's any kind of escrow left ensure that the character receives it back
                    double escrowLeft = order.Escrow - order.UnitsLeft * price;

                    if (escrowLeft > 0.0)
                    {
                        // give back the escrow for the character
                        // TODO: THERE IS A POTENTIAL DEADLOCK HERE IF WE BUY FROM OURSELVES
                        using Wallet escrowWallet = this.WalletManager.AcquireWallet(order.CharacterID, order.AccountID);
                        {
                            escrowWallet.CreateJournalRecord(
                                MarketReference.MarketEscrow, null, null, escrowLeft
                            );
                        }
                    }
                    
                    // this order is fully satisfiable, so do that
                    // remove the order off the database if it's fully satisfied
                    this.DB.RemoveOrder(connection, order.OrderID);

                    quantityToSell = order.UnitsLeft;
                    quantity -= order.UnitsLeft;
                }
                else if (order.MinimumUnits <= quantity)
                {
                    // we can satisfy SOME of the order
                    this.DB.UpdateOrderRemainingQuantity(connection, order.OrderID, order.UnitsLeft - quantity, quantity * price);
                    // the quantity we're selling is already depleted if the code got here
                    quantityToSell = quantity;
                    quantity = 0;
                }

                if (quantityToSell > 0)
                {
                    
                    // calculate sales tax
                    double profit, tax;
                            
                    this.CalculateSalesTax(character.GetSkillLevel(Types.Accounting), quantity, price, out tax, out profit);
            
                    // create the required records for the wallet
                    wallet.CreateJournalRecord(MarketReference.MarketTransaction, order.CharacterID, character.ID, null, profit);
                    wallet.CreateJournalRecord(MarketReference.TransactionTax, null, null, -tax);
                    this.WalletManager.CreateTransactionRecord(character.ID, TransactionType.Sell, order.CharacterID, typeID, quantityToSell, price, stationID, order.AccountID);
                    this.WalletManager.CreateTransactionRecord(order.CharacterID, TransactionType.Buy, character.ID, typeID, quantityToSell, price, stationID, order.AccountID);
                    
                    // create the new item that will be used by the player
                    ItemEntity item = this.ItemFactory.CreateSimpleItem(
                        this.TypeManager[typeID], order.CharacterID, stationID, Flags.Hangar, quantityToSell
                    );
                    
                    // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
                    this.ItemFactory.UnloadItem(item);
                    
                    // check if the station it's at is loaded and notify the node in question
                    // if not take care of the item notification ourselves
                    long stationNode = this.SystemManager.GetNodeStationBelongsTo(stationID);

                    if (stationNode == 0 || this.SystemManager.StationBelongsToUs(stationID) == true)
                    {
                        this.NotificationManager.NotifyCharacter(item.OwnerID, Notifications.Client.Inventory.OnItemChange.BuildLocationChange(item, this.ItemFactory.LocationMarket.ID));
                    }
                    else
                    {
                        this.NotificationManager.NotifyNode(stationNode, OnItemChange.BuildLocationChange(itemID, this.ItemFactory.LocationMarket.ID, stationID));
                    }
                }

                // ensure we do not sell more than we have
                if (quantity == 0)
                    break;
            }
        }

        private void PlaceSellOrderCharUpdateItems(MySqlConnection connection, Client client, int stationID, int typeID, int quantity)
        {
            Dictionary<int, MarketDB.ItemQuantityEntry> items = null;
            
            // depending on where the character that is placing the order, the way to detect the items should be different
            if (stationID == client.StationID)
                items = this.DB.PrepareItemForOrder(connection, typeID, stationID, client.ShipID ?? -1, quantity, (int) client.CharacterID);
            else
                items = this.DB.PrepareItemForOrder(connection, typeID, stationID, -1, quantity, (int) client.CharacterID);

            if (items is null)
                throw new NotEnoughQuantity(this.TypeManager[typeID]);

            long stationNode = this.SystemManager.GetNodeStationBelongsTo(stationID);
            
            if (this.SystemManager.StationBelongsToUs(stationID) == true || stationNode == 0)
            {
                // load the items here and send proper notifications
                foreach ((int _, MarketDB.ItemQuantityEntry entry) in items)
                {
                    ItemEntity item = this.ItemFactory.LoadItem(entry.ItemID);

                    if (entry.Quantity == 0)
                    {
                        // item has to be destroyed
                        this.ItemFactory.DestroyItem(item);
                        // notify item destroyal
                        client.NotifyMultiEvent(Notifications.Client.Inventory.OnItemChange.BuildLocationChange(item, stationID));
                    }
                    else
                    {
                        // just a quantity change
                        item.Quantity = entry.Quantity;
                        // notify the client
                        client.NotifyMultiEvent(Notifications.Client.Inventory.OnItemChange.BuildQuantityChange(item, entry.OriginalQuantity));
                        // unload the item if it's not needed
                        this.ItemFactory.UnloadItem(item);
                    }
                }
            }
            else
            {
                // the item changes should be handled by a different node
                OnItemChange changes = new OnItemChange();

                foreach ((int _, MarketDB.ItemQuantityEntry entry) in items)
                {
                    if (entry.Quantity == 0)
                        changes.AddChange(entry.ItemID, "locationID", stationID, this.ItemFactory.LocationMarket.ID);
                    else
                        changes.AddChange(entry.ItemID, "quantity", entry.OriginalQuantity, entry.Quantity);
                }
            }
        }

        private void PlaceSellOrderChar(int itemID, Character character, int stationID, int quantity, int typeID, int duration, double price, int range, double brokerCost, CallInformation call)
        {
            // check distance for the order
            this.CheckSellOrderDistancePermissions(character, stationID);
            
            // TODO: ADD SUPPORT FOR CORPORATIONS!

            // obtain wallet lock too
            // everything is checked already, perform table locking and do all the job here
            using Wallet wallet = this.WalletManager.AcquireWallet(character.ID, 1000);
            using MySqlConnection connection = this.DB.AcquireMarketLock();
            try
            {
                // check if the item is singleton and throw a exception about it
                {
                    bool singleton = false;

                    this.DB.CheckRepackagedItem(connection, itemID, out singleton);

                    if (singleton == true)
                        throw new RepackageBeforeSelling(this.TypeManager[typeID]);
                }

                // move the items to update
                this.PlaceSellOrderCharUpdateItems(connection, call.Client, stationID, typeID, quantity);

                if (duration == 0)
                {
                    // finally create the records in the market database
                    this.PlaceImmediateSellOrderChar(connection, wallet, character, itemID, typeID, stationID, quantity,
                        price, call.Client);
                }
                else
                {
                    // ensure the player can pay taxes and broker
                    wallet.EnsureEnoughBalance(brokerCost);
                    // do broker fee first
                    wallet.CreateJournalRecord(MarketReference.Brokerfee, null, null, -brokerCost);
                    // finally place the order
                    this.DB.PlaceSellOrder(connection, typeID, character.ID, stationID, range, price, quantity, 1000,
                        duration, false);
                }

                // send a OnOwnOrderChange notification
                call.Client.NotifyMultiEvent(new OnOwnOrderChanged(typeID, "Add"));
            }
            finally
            {
                this.DB.ReleaseMarketLock(connection);
            }
        }

        private void CheckMatchingSellOrders(MarketOrder[] orders, int quantity, int stationID)
        {
            foreach (MarketOrder order in orders)
            {
                if (order.Range == -1 && order.LocationID != stationID)
                    continue;
                if (order.Range != -1 && order.Range < order.Jumps)
                    continue;
                
                quantity -= Math.Min(quantity, order.UnitsLeft);

                if (quantity == 0)
                    break;
            }

            if (quantity > 0)
                throw new MktOrderDidNotMatch();
        }

        private void PlaceImmediateBuyOrderChar(MySqlConnection connection, Wallet wallet, int typeID, Character character, int stationID, int quantity, double price, int range, CallInformation call)
        {
            int solarSystemID = this.ItemFactory.GetStaticStation(stationID).SolarSystemID;

            // look for matching sell orders
            MarketOrder[] orders = this.DB.FindMatchingOrders(connection, price, typeID, character.ID, solarSystemID, TransactionType.Sell);
            
            // ensure there's at least some that match
            this.CheckMatchingSellOrders(orders, quantity, solarSystemID);

            foreach (MarketOrder order in orders)
            {
                int quantityToBuy = 0;

                if (order.Range == -1 && order.LocationID != stationID)
                    continue;
                if (order.Range != -1 && order.Range < order.Jumps)
                    continue;
                
                if (order.UnitsLeft <= quantity)
                {
                    // the order was completed, remove it from the database
                    this.DB.RemoveOrder(connection, order.OrderID);
                    
                    // increase the amount of bought items
                    quantityToBuy = order.UnitsLeft;
                    quantity -= order.UnitsLeft;
                }
                else
                {
                    // part of the sell order was satisfied
                    this.DB.UpdateOrderRemainingQuantity(connection, order.OrderID, order.UnitsLeft - quantity, 0);

                    quantityToBuy = quantity;
                    quantity = 0;
                }

                if (quantityToBuy > 0)
                {
                    // acquire wallet journal for seller so we can update their balance to add the funds that he got
                    using Wallet sellerWallet = this.WalletManager.AcquireWallet(order.CharacterID, order.AccountID);
                    {
                        sellerWallet.CreateJournalRecord(MarketReference.MarketTransaction, character.ID, order.CharacterID, null, price * quantityToBuy);
                    }
                    
                    // create the transaction records for both characters
                    this.WalletManager.CreateTransactionRecord(character.ID, TransactionType.Buy, order.CharacterID, typeID, quantityToBuy, price, stationID, order.AccountID);
                    this.WalletManager.CreateTransactionRecord(order.CharacterID, TransactionType.Sell, character.ID, typeID, quantityToBuy, price, stationID, order.AccountID);

                    long stationNode = this.SystemManager.GetNodeStationBelongsTo(stationID);
                        
                    // create the new item that will be used by the player
                    ItemEntity item = this.ItemFactory.CreateSimpleItem(
                        this.TypeManager[typeID], character.ID, stationID, Flags.Hangar, quantityToBuy
                    );
                    // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
                    this.ItemFactory.UnloadItem(item);

                    if (stationNode == 0 || this.SystemManager.StationBelongsToUs(stationID) == true)
                    {
                        this.NotificationManager.NotifyCharacter(character.ID, Notifications.Client.Inventory.OnItemChange.BuildLocationChange(item, this.ItemFactory.LocationMarket.ID));
                    }
                    else
                    {
                        this.NotificationManager.NotifyNode(stationNode, OnItemChange.BuildLocationChange(item.ID, this.ItemFactory.LocationMarket.ID, stationID));
                    }
                }

                // ensure we do not buy more than we need
                if (quantity == 0)
                    break;
            }
        }

        private void PlaceBuyOrderChar(int typeID, Character character, int stationID, int quantity, double price, int duration, int minVolume, int range, double brokerCost, CallInformation call)
        {
            // ensure the character can place the order where he's trying to
            this.CheckBuyOrderDistancePermissions(character, stationID, duration);

            using Wallet wallet = this.WalletManager.AcquireWallet(character.ID, 1000);
            using MySqlConnection connection = this.DB.AcquireMarketLock();
            try
            {
                // make sure the character can pay the escrow and the broker
                wallet.EnsureEnoughBalance(quantity * price + brokerCost);
                // do the escrow after
                wallet.CreateJournalRecord(MarketReference.MarketEscrow, null, null, -quantity * price);
                
                if (duration == 0)
                {
                    this.PlaceImmediateBuyOrderChar(connection, wallet, typeID, character, stationID, quantity, price, range, call);
                }
                else
                {
                    // do broker fee first
                    wallet.CreateJournalRecord(MarketReference.Brokerfee, null, null, -brokerCost);
                    // place the buy order
                    this.DB.PlaceBuyOrder(connection, typeID, character.ID, stationID, range, price, quantity, minVolume, 1000, duration, false);
                }
                
                // send a OnOwnOrderChange notification
                call.Client.NotifyMultiEvent(new OnOwnOrderChanged(typeID, "Add"));
            }
            finally
            {
                this.DB.ReleaseMarketLock(connection);
            }
        }

        public PyDataType PlaceCharOrder(PyInteger stationID, PyInteger typeID, PyDecimal price, PyInteger quantity,
            PyInteger bid, PyInteger range, PyDataType itemID, PyInteger minVolume, PyInteger duration, PyBool useCorp,
            PyDataType located, CallInformation call)
        {
            // get solarSystem for the station
            Character character = this.ItemFactory.GetItem<Character>(call.Client.EnsureCharacterIsSelected());
            double brokerCost = 0.0;
            
            // if the order is not immediate check the amount of orders the character has
            if (duration != 0)
            {
                int maximumOrders = this.GetMaxOrderCountForCharacter(character);
                int currentOrders = this.DB.CountCharsOrders(character.ID);

                if (maximumOrders <= currentOrders)
                    throw new MarketExceededOrderCount(currentOrders, maximumOrders);
                
                // calculate broker costs for the order
                this.CalculateBrokerCost(character.GetSkillLevel(Types.BrokerRelations), quantity, price, out brokerCost);
            }
            
            // check if the character has the Marketing skill and calculate distances
            if (bid == (int) TransactionType.Sell)
            {
                if (itemID is PyInteger == false)
                    throw new CustomError("Unexpected data!");

                this.PlaceSellOrderChar(itemID as PyInteger, character, stationID, quantity, typeID, duration, price, range, brokerCost, call);
            }
            else if (bid == (int) TransactionType.Buy)
            {
                this.PlaceBuyOrderChar(typeID, character, stationID, quantity, price, duration, minVolume, range, brokerCost, call);
            }
            
            return null;
        }

        public PyDataType CancelCharOrder(PyInteger orderID, PyInteger regionID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            
            using MySqlConnection connection = this.DB.AcquireMarketLock();
            try
            {
                MarketOrder order =  this.DB.GetOrderById(connection, orderID);

                if (order.CharacterID != callerCharacterID)
                    throw new MktOrderDidNotMatch();

                long currentTime = DateTime.UtcNow.ToFileTimeUtc();
                // check for timers, no changes in less than 5 minutes
                if (currentTime < order.Issued + TimeSpan.TicksPerSecond * this.NodeContainer.Constants[Constants.mktModificationDelay])
                    throw new MktOrderDelay((order.Issued + TimeSpan.TicksPerSecond * this.NodeContainer.Constants[Constants.mktModificationDelay]) - currentTime);

                // check for escrow
                if (order.Escrow > 0.0 && order.Bid == TransactionType.Buy)
                {
                    using Wallet wallet = this.WalletManager.AcquireWallet(character.ID, 1000);
                    {
                        wallet.CreateJournalRecord(MarketReference.MarketEscrow, null, null, order.Escrow);
                    }
                }

                if (order.Bid == TransactionType.Sell)
                {
                                            
                    // create the new item that will be used by the player
                    ItemEntity item = this.ItemFactory.CreateSimpleItem(
                        this.TypeManager[order.TypeID], character.ID, order.LocationID, Flags.Hangar, order.UnitsLeft
                    );
                    // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
                    this.ItemFactory.UnloadItem(item);

                    // check what node this item should be loaded at
                    long stationNode = this.SystemManager.GetNodeStationBelongsTo(order.LocationID);

                    if (stationNode == 0 || this.SystemManager.StationBelongsToUs(order.LocationID) == true)
                    {
                        this.NotificationManager.NotifyCharacter(character.ID, Notifications.Client.Inventory.OnItemChange.BuildLocationChange(item, this.ItemFactory.LocationMarket.ID));
                    }
                    else
                    {
                        this.NotificationManager.NotifyNode(stationNode, OnItemChange.BuildLocationChange(item.ID, this.ItemFactory.LocationMarket.ID, order.LocationID));
                    }
                }
                
                // finally remove the order
                this.DB.RemoveOrder(connection, order.OrderID);
                // send a OnOwnOrderChange notification
                call.Client.NotifyMultiEvent(new OnOwnOrderChanged(order.TypeID, "Removed"));
            }
            finally
            {
                this.DB.ReleaseMarketLock(connection);
            }
            
            return null;
        }

        public PyDataType ModifyCharOrder(PyInteger orderID, PyDecimal newPrice, PyInteger bid, PyInteger stationID, PyInteger solarSystemID, PyDecimal price, PyInteger volRemaining, PyInteger issued, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            
            using MySqlConnection connection = this.DB.AcquireMarketLock();
            try
            {
                MarketOrder order =  this.DB.GetOrderById(connection, orderID);

                if (order.CharacterID != callerCharacterID)
                    throw new MktOrderDidNotMatch();

                long currentTime = DateTime.UtcNow.ToFileTimeUtc();
                // check for timers, no changes in less than 5 minutes
                if (currentTime < order.Issued + TimeSpan.TicksPerSecond * this.NodeContainer.Constants[Constants.mktModificationDelay])
                    throw new MktOrderDelay((order.Issued + TimeSpan.TicksPerSecond * this.NodeContainer.Constants[Constants.mktModificationDelay]) - currentTime);

                // ensure the order hasn't been modified since the user saw it on the screen
                if ((int) order.Bid != bid || order.LocationID != stationID || order.Price != price ||
                    order.UnitsLeft != volRemaining || order.Issued != issued)
                    throw new MktOrderDidNotMatch();
                
                // get the modification broker's fee
                double brokerCost = 0.0;
                double newEscrow = 0.0;

                this.CalculateBrokerCost(character.GetSkillLevel(Types.BrokerRelations), volRemaining, (newPrice - price), out brokerCost);

                using Wallet wallet = this.WalletManager.AcquireWallet(order.CharacterID, order.AccountID);
                {
                    if (order.Bid == TransactionType.Buy)
                    {
                        // calculate the difference in escrow
                        newEscrow = volRemaining * newPrice;
                        double escrowDiff = order.Escrow - newEscrow;
                        
                        // ensure enough balances
                        wallet.EnsureEnoughBalance(escrowDiff + brokerCost);
                        // take the difference in escrow
                        wallet.CreateJournalRecord(MarketReference.MarketEscrow, null, null, escrowDiff);
                    }
                    else
                    {
                        wallet.EnsureEnoughBalance(brokerCost);
                    }
                    
                    // pay the broker fee once again
                    wallet.CreateJournalRecord(MarketReference.Brokerfee, null, null, -brokerCost);
                }
                
                // everything looks okay, update the price of the order
                this.DB.UpdatePrice(connection, order.OrderID, newPrice, newEscrow);
                
                // send a OnOwnOrderChange notification
                call.Client.NotifyMultiEvent(new OnOwnOrderChanged(order.TypeID, "Modified"));
            }
            finally
            {
                this.DB.ReleaseMarketLock(connection);
            }
            
            return null;
        }

        /// <returns>The maximum active order count for the given <paramref name="character"/></returns>
        private int GetMaxOrderCountForCharacter(Character character)
        {
            Dictionary<int, Skill> injectedSkills = character.InjectedSkillsByTypeID;

            int retailLevel = 0, tradeLevel = 0, wholeSaleLevel = 0, tycoonLevel = 0;

            if (injectedSkills.ContainsKey((int) Types.Retail) == true)
                retailLevel = (int) injectedSkills[(int) Types.Retail].Level;
            if (injectedSkills.ContainsKey((int) Types.Trade) == true)
                tradeLevel = (int) injectedSkills[(int) Types.Trade].Level;
            if (injectedSkills.ContainsKey((int) Types.Wholesale) == true)
                wholeSaleLevel = (int) injectedSkills[(int) Types.Wholesale].Level;
            if (injectedSkills.ContainsKey((int) Types.Tycoon) == true)
                tycoonLevel = (int) injectedSkills[(int) Types.Tycoon].Level;
            
            return 5 + tradeLevel * 4 + retailLevel * 8 + wholeSaleLevel * 16 + tycoonLevel * 32;
        }

        /// <summary>
        /// Removes an expired buy order from the database and returns the leftover escrow back into the player's wallet
        /// </summary>
        /// <param name="connection">The database connection that acquired the lock</param>
        /// <param name="order">The order to mark as expired</param>
        private void BuyOrderExpired(MySqlConnection connection, MarketOrder order)
        {
            // remove order
            this.DB.RemoveOrder(connection, order.OrderID);

            // give back the escrow paid by the player
            using Wallet wallet = this.WalletManager.AcquireWallet(order.CharacterID, order.AccountID);
            {
                wallet.CreateJournalRecord(MarketReference.MarketEscrow, null, null, order.Escrow);
            }
            
            // notify the character about the change in the order
            this.NotificationManager.NotifyCharacter(order.CharacterID, new OnOwnOrderChanged(order.TypeID, "Expiry", order.AccountID > 1000));
        }

        /// <summary>
        /// Removes an expired sell order from the database and returns the leftover items back to the player's hangar
        /// </summary>
        /// <param name="connection">The database connection that acquired the lock</param>
        /// <param name="order">The order to mark as expired</param>
        private void SellOrderExpired(MySqlConnection connection, MarketOrder order)
        {
            // remove order
            this.DB.RemoveOrder(connection, order.OrderID);
            // create the item back into the player's hanger
                                    
            // create the new item that will be used by the player
            ItemEntity item = this.ItemFactory.CreateSimpleItem(
                this.TypeManager[order.TypeID], order.CharacterID, order.LocationID, Flags.Hangar, order.UnitsLeft
            );
            // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
            this.ItemFactory.UnloadItem(item);

            long stationNode = this.SystemManager.GetNodeStationBelongsTo(order.LocationID);

            if (stationNode == 0 || this.SystemManager.StationBelongsToUs(order.LocationID) == true)
            {
                this.NotificationManager.NotifyCharacter(order.CharacterID, Notifications.Client.Inventory.OnItemChange.BuildLocationChange(item, this.ItemFactory.LocationMarket.ID));
            }
            else
            {
                this.NotificationManager.NotifyNode(stationNode, OnItemChange.BuildLocationChange(item.ID, this.ItemFactory.LocationMarket.ID, order.LocationID));
            }
            
            // finally notify the character about the order change
            this.NotificationManager.NotifyCharacter(order.CharacterID, new OnOwnOrderChanged(order.TypeID, "Expiry"));
            // TODO: SEND AN EVEMAIL TO THE PLAYER?
        }

        /// <summary>
        /// Checks orders that are expired, cancels them and returns the items to the hangar if required
        /// </summary>
        public void PerformTimedEvents()
        {
            using MySqlConnection connection = this.DB.AcquireMarketLock();
            try
            {
                List<MarketOrder> orders = this.DB.GetExpiredOrders(connection);

                foreach (MarketOrder order in orders)
                {
                    switch (order.Bid)
                    {
                        case TransactionType.Buy:
                            // buy orders need to return the escrow
                            this.BuyOrderExpired(connection, order);
                            break;
                        case TransactionType.Sell:
                            // sell orders are a bit harder, the items have to go back to the player's hangar
                            this.SellOrderExpired(connection, order);
                            break;
                    }
                }
            }
            finally
            {
                this.DB.ReleaseMarketLock(connection);
            }
        }
        
        // Marketing skill affects range of remote sell order placing
        
        /*
         These are the limits applied by the client on the market stuff
        limits = {}
        currentOpen = 0
        myskills = sm.GetService('skills').MySkillLevelsByID()
        retailLevel = myskills.get(const.typeRetail, 0)
        tradeLevel = myskills.get(const.typeTrade, 0)
        wholeSaleLevel = myskills.get(const.typeWholesale, 0)
        accountingLevel = myskills.get(const.typeAccounting, 0)
        brokerLevel = myskills.get(const.typeBrokerRelations, 0)
        tycoonLevel = myskills.get(const.typeTycoon, 0)
        marginTradingLevel = myskills.get(const.typeMarginTrading, 0)
        marketingLevel = myskills.get(const.typeMarketing, 0)
        procurementLevel = myskills.get(const.typeProcurement, 0)
        visibilityLevel = myskills.get(const.typeVisibility, 0)
        daytradingLevel = myskills.get(const.typeDaytrading, 0)
        I = 5 + tradeLevel * 4 + retailLevel * 8 + wholeSaleLevel * 16 + tycoonLevel * 32
        limits['cnt'] = maxOrderCount
        commissionPercentage = const.marketCommissionPercentage / 100.0
        commissionPercentage *= 1 - brokerLevel * 0.05
        transactionTax = const.mktTransactionTax / 100.0
        transactionTax *= 1 - accountingLevel * 0.1
        limits['fee'] = commissionPercentage
        limits['acc'] = transactionTax
        limits['ask'] = jumpsPerSkillLevel[marketingLevel]
        limits['bid'] = jumpsPerSkillLevel[procurementLevel]
        limits['vis'] = jumpsPerSkillLevel[visibilityLevel]
        limits['mod'] = jumpsPerSkillLevel[daytradingLevel]
        limits['esc'] = 0.75 ** marginTradingLevel
        
        This might help understand market skill levels better:
        https://eve-files.com/media/corp/thoraemond/eve-trading-skills-for-remote-orders-20101217.png
         */
    }
}