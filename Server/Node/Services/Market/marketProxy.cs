using System;
using System.Collections.Generic;
using System.Dynamic;
using Common.Services;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.jumpCloneSvc;
using Node.Exceptions.marketProxy;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Inventory.Notifications;
using Node.Inventory.SystemEntities;
using Node.Market;
using Node.Market.Notifications;
using Node.Network;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Market
{
    public class marketProxy : Service
    {
        private static readonly int[] JumpsPerSkillLevel = new int[]
        {
            -1, 0, 5, 10, 20, 50
        };

        private MarketDB DB { get; }
        private CharacterDB CharacterDB { get; }
        private ItemDB ItemDB { get; }
        private CacheStorage CacheStorage { get; }
        private ItemManager ItemManager { get; }
        private TypeManager TypeManager { get; }
        private SolarSystemDB SolarSystemDB { get; }
        private NodeContainer NodeContainer { get; }
        private ClientManager ClientManager { get; }
        private SystemManager SystemManager { get; }
        
        public marketProxy(MarketDB db, CharacterDB characterDB, ItemDB itemDB, SolarSystemDB solarSystemDB, ItemManager itemManager, TypeManager typeManager, CacheStorage cacheStorage, NodeContainer nodeContainer, ClientManager clientManager, SystemManager systemManager)
        {
            this.DB = db;
            this.CharacterDB = characterDB;
            this.ItemDB = itemDB;
            this.SolarSystemDB = solarSystemDB;
            this.CacheStorage = cacheStorage;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
            this.NodeContainer = nodeContainer;
            this.ClientManager = clientManager;
            this.SystemManager = systemManager;
        }

        public PyDataType CharGetNewTransactions(PyInteger sellBuy, PyInteger typeID, PyNone clientID,
            PyInteger quantity, PyNone fromDate, PyNone maxPrice, PyInteger minPrice, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            TransactionType transactionType = TransactionType.Either;

            if (sellBuy is PyInteger)
            {
                switch ((int) (sellBuy as PyInteger))
                {
                    case 0:
                        transactionType = TransactionType.Sell;
                        break;
                    case 1:
                        transactionType = TransactionType.Buy;
                        break;
                }
            }
            
            return this.DB.CharGetNewTransactions(
                callerCharacterID, clientID, transactionType, typeID as PyInteger, quantity, minPrice
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

            return PyCacheMethodCallResult.FromCacheHint(
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

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
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
            double salesTax = (this.NodeContainer.Constants["mktTransactionTax"] / 100.0) * (1 - accountingLevel * 0.1);
            double beforeTax = price * quantity;

            tax = beforeTax * salesTax;
            profit = (price * quantity) - tax;
        }

        private void CalculateBrokerCost(long brokerLevel, int quantity, double price, out double brokerCost)
        {
            double brokerPercentage = ((double) this.NodeContainer.Constants["marketCommissionPercentage"] / 100) * (1 - brokerLevel * 0.05);

            // TODO: GET THE STANDINGS FOR THE CHARACTER
            double factionStanding = 0.0;
            double corpStanding = 0.0;

            double weightedStanding = (0.7 * factionStanding + 0.3 * corpStanding) / 10.0;

            brokerPercentage = brokerPercentage * Math.Pow(2.0, -2 * weightedStanding);
            brokerCost = price * quantity * brokerPercentage;

            if (brokerCost < this.NodeContainer.Constants["mktMinimumFee"])
                brokerCost = this.NodeContainer.Constants["mktMinimumFee"];
        }
        
        private void NotifyItemChange(ClusterConnection connection, long nodeID, int itemID, string key, PyDataType newValue)
        {
            connection.SendNodeNotification(nodeID, "OnItemUpdate",
                new PyTuple(2)
                {
                    [0] = itemID,
                    [1] = new PyDictionary() { [key] = newValue}
                }
            );
        }

        private void NotifyBalanceChange(ClusterConnection connection, long nodeID, int characterID, double newBalance)
        {
            connection.SendNodeNotification(nodeID, "OnBalanceUpdate",
                new PyTuple(2)
                {
                    [0] = characterID,
                    [1] = newBalance
                }
            );
        }

        private void CheckSellOrderDistancePermissions(Character character, int stationID)
        {
            Station station = this.ItemManager.GetStation(stationID);

            if (character.RegionID != station.RegionID)
                throw new MktInvalidRegion();
            
            int jumps = this.SolarSystemDB.GetJumpsBetweenSolarSystems(character.SolarSystemID, station.SolarSystemID);
            long marketingSkillLevel = character.GetSkillLevel(ItemTypes.Marketing);
            long maximumDistance = JumpsPerSkillLevel[marketingSkillLevel];

            if (maximumDistance == -1 && character.StationID != stationID)
                throw new MktCantSellItemOutsideStation(jumps);
            if (character.SolarSystemID != station.SolarSystemID && maximumDistance < jumps)
                throw new MktCantSellItem2(jumps, maximumDistance);
        }

        private void CheckBuyOrderDistancePermissions(Character character, int stationID)
        {
            Station station = this.ItemManager.GetStation(stationID);

            if (character.RegionID != station.RegionID)
                throw new MktInvalidRegion();
            
            int jumps = this.SolarSystemDB.GetJumpsBetweenSolarSystems(character.SolarSystemID, station.SolarSystemID);
            long procurementSkillLevel = character.GetSkillLevel(ItemTypes.Procurement);
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

        private void PlaceImmediateSellOrderChar(MySqlConnection connection, Character character, int itemID, int typeID, int stationID, int quantity, double price, Client client)
        {
            int solarSystemID = this.ItemManager.GetStation(stationID).SolarSystemID;
            
            // look for matching buy orders
            MarketOrder[] orders = this.DB.FindMatchingBuyOrders(connection, price, typeID, character.ID, solarSystemID);
            
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
                        double balance = this.CharacterDB.GetCharacterBalance(order.CharacterID) + escrowLeft;
                        this.DB.CreateJournalForCharacter(MarketReference.MarketEscrow, order.CharacterID, order.CharacterID, null, null, escrowLeft, balance, "", 1000);
                        this.CharacterDB.SetCharacterBalance(order.CharacterID, balance);
                    
                        // if the character is loaded in any node inform that node of the change in wallet
                        int characterNode = this.ItemDB.GetItemNode(order.CharacterID);
                    
                        if (characterNode > 0)
                            this.NotifyBalanceChange(client.ClusterConnection, characterNode, order.CharacterID, balance);
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
                    // first calculate the raw ISK and the tax on that
                    double profit, tax;
                    
                    this.CalculateSalesTax(character.GetSkillLevel(ItemTypes.Accounting), order.UnitsLeft, order.Price, out tax, out profit);

                    // create the required records for the wallet
                    this.DB.CreateJournalForCharacter(MarketReference.MarketTransaction, character.ID, order.CharacterID, character.ID, null, profit, character.Balance + profit + tax, "", 1000);
                    this.DB.CreateJournalForCharacter(MarketReference.TransactionTax, character.ID, character.ID, null, null, -tax, character.Balance + profit, "", 1000);
                    this.DB.CreateTransactionForCharacter(character.ID, order.CharacterID, TransactionType.Sell, typeID, quantityToSell, price, stationID);
                    this.DB.CreateTransactionForCharacter(order.CharacterID, character.ID, TransactionType.Buy, typeID, quantityToSell, price, stationID);

                    // update balance for this character
                    character.Balance += profit;
                    
                    // send notification for the wallet to be updated
                    client.NotifyBalanceUpdate(character.Balance);
                    
                    // create the new item that will be used by the player
                    ItemEntity item = this.ItemManager.CreateSimpleItem(
                        this.TypeManager[typeID], order.CharacterID, this.ItemManager.LocationMarket.ID, ItemFlags.Hangar, quantityToSell
                    );
                    // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
                    this.ItemManager.UnloadItem(item);

                    long stationNode = this.SystemManager.GetNodeStationBelongsTo(stationID);
                    
                    // notify the node about the item
                    this.NotifyItemChange(client.ClusterConnection, stationNode, item.ID, "locationID", stationID);
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
                items = this.DB.PrepareItemForOrder(connection, typeID, (int) client.StationID, (int) client.ShipID, quantity, (int) client.CharacterID);
            else
                items = this.DB.PrepareItemForOrder(connection, typeID, stationID, -1, quantity, (int) client.CharacterID);

            if (items == null)
                throw new NotEnoughQuantity(this.TypeManager[typeID]);

            // now notify the nodes about the changes on the other items (if required)
            foreach (KeyValuePair<int, MarketDB.ItemQuantityEntry> pair in items)
            {
                if (pair.Value.NodeID == 0)
                    continue;

                if (pair.Value.Quantity == 0)
                    this.NotifyItemChange(client.ClusterConnection, pair.Value.NodeID, pair.Key, "locationID", this.ItemManager.LocationMarket.ID);
                else
                    this.NotifyItemChange(client.ClusterConnection, pair.Value.NodeID, pair.Key, "quantity", pair.Value.Quantity);
            }
        }

        private void PlaceSellOrderChar(int itemID, Character character, int stationID, int quantity, int typeID, int duration, double price, int range, double brokerCost, CallInformation call)
        {
            // check distance for the order
            this.CheckSellOrderDistancePermissions(character, stationID);
            
            // TODO: ADD SUPPORT FOR CORPORATIONS!
            
            // everything is checked already, perform table locking and do all the job here
            using MySqlConnection connection = this.DB.AcquireMarketLock();
            try
            {
                // move the items to update
                this.PlaceSellOrderCharUpdateItems(connection, call.Client, stationID, typeID, quantity);
                
                if (duration == 0)
                {
                    // finally create the records in the market database
                    this.PlaceImmediateSellOrderChar(connection, character, itemID, typeID, stationID, quantity, price, call.Client);
                }
                else
                {
                    // create the new item that will be used by the market
                    ItemEntity item = this.ItemManager.CreateSimpleItem(
                        this.TypeManager[typeID], character.ID, this.ItemManager.LocationMarket.ID, ItemFlags.Hangar, quantity
                    );
                    // finally place the order
                    this.DB.PlaceSellOrder(connection, item.Type.ID, item.ID, item.OwnerID, stationID, range, price, quantity, 1000, duration, false);
                    // unload the item, we do not want it to be held by anyone
                    this.ItemManager.UnloadItem(item);
                    // update balance for this character
                    character.Balance -= brokerCost;
                    character.Persist();
                    // create record in the journal
                    this.DB.CreateJournalForCharacter(MarketReference.Brokerfee, character.ID, character.ID, null, null, -brokerCost, character.Balance - brokerCost, "", 1000);
                    // send notification for the wallet to be updated
                    call.Client.NotifyBalanceUpdate(character.Balance);
                }
                // send a OnOwnOrderChange notification
                call.Client.NotifyMultiEvent(new OnOwnOrderChanged(typeID, "Add"));
            }
            finally
            {
                // free the lock
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

        private void PlaceImmediateBuyOrderChar(MySqlConnection connection, int typeID, Character character, int stationID, int quantity, double price, int range, CallInformation call)
        {
            int solarSystemID = this.ItemManager.GetStation(stationID).SolarSystemID;

            // look for matching sell orders
            MarketOrder[] orders = this.DB.FindMatchingSellOrders(connection, price, typeID, character.ID, solarSystemID);
            
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
                    double profit, tax;
                    double balance = this.CharacterDB.GetCharacterBalance(order.CharacterID);

                    this.CalculateSalesTax(this.CharacterDB.GetSkillLevelForCharacter(ItemTypes.Accounting, order.CharacterID), quantityToBuy, price, out tax, out profit);

                    balance += profit;

                    character.Balance -= quantityToBuy * price;
                    character.Persist();
                    
                    this.DB.CreateJournalForCharacter(MarketReference.MarketEscrow, character.ID, character.ID, null, null, -quantityToBuy * price, character.Balance, "", 1000);
                    this.DB.CreateJournalForCharacter(MarketReference.MarketTransaction, order.CharacterID, character.ID, order.CharacterID, null, price * quantityToBuy, balance + tax, "", 1000);
                    this.DB.CreateJournalForCharacter(MarketReference.TransactionTax, order.CharacterID, order.CharacterID, null, null, -tax, balance, "", 1000);
                    this.DB.CreateTransactionForCharacter(character.ID, order.CharacterID, TransactionType.Buy, typeID, quantityToBuy, price, stationID);
                    this.DB.CreateTransactionForCharacter(order.CharacterID, character.ID, TransactionType.Sell, typeID, quantityToBuy, price, stationID);
                    
                    // save the balance of the character
                    this.CharacterDB.SetCharacterBalance(order.CharacterID, balance);
                    
                    // if the character is loaded in any node inform that node of the change in wallet
                    int characterNode = this.ItemDB.GetItemNode(order.CharacterID);
                    
                    if (characterNode > 0)
                        this.NotifyBalanceChange(call.Client.ClusterConnection, characterNode, order.CharacterID, balance);
                    
                    // notify this character of the balance update too
                    call.Client.NotifyBalanceUpdate(character.Balance);
                    
                    // now ensure the item is present where it should be
                    if (order.UnitsLeft == quantityToBuy)
                    {
                        // now notify the nodes about the change, if the item is loaded anywhere this should be enough
                        long stationNode = this.SystemManager.GetNodeStationBelongsTo(stationID);

                        if (stationNode > 0)
                        {
                            // send proper notifications for owner and location changes
                            this.NotifyItemChange(call.Client.ClusterConnection, stationNode, order.ItemID, "ownerID", character.ID);
                            this.NotifyItemChange(call.Client.ClusterConnection, stationNode, order.ItemID, "locationID", stationID);
                            this.NotifyItemChange(call.Client.ClusterConnection, stationNode, order.ItemID, "quantity", quantityToBuy);
                        }
                        else
                        {
                            this.ItemDB.UpdateItemLocation(order.ItemID, stationID);
                            this.ItemDB.UpdateItemOwner(order.ItemID, character.ID);
                            this.ItemDB.UpdateItemQuantity(order.ItemID, quantityToBuy);
                        }
                    }
                    else
                    {
                        // create the new item that will be used by the player
                        ItemEntity item = this.ItemManager.CreateSimpleItem(
                            this.TypeManager[typeID], character.ID, this.ItemManager.LocationMarket.ID, ItemFlags.Hangar, quantityToBuy
                        );
                        // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
                        this.ItemManager.UnloadItem(item);

                        long stationNode = this.SystemManager.GetNodeStationBelongsTo(stationID);

                        // notify the node about the item
                        this.NotifyItemChange(call.Client.ClusterConnection, stationNode, item.ID, "locationID", stationID);
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
            this.CheckBuyOrderDistancePermissions(character, stationID);
            // make sure the character has enough money
            character.EnsureEnoughBalance(quantity * price);
            
            using MySqlConnection connection = this.DB.AcquireMarketLock();
            try
            {
                if (duration == 0)
                {
                    this.PlaceImmediateBuyOrderChar(connection, typeID, character, stationID, quantity, price, range, call);
                }
                else
                {
                    // make sure the character can pay the escrow
                    character.EnsureEnoughBalance(quantity * price);
                    // place the buy order
                    this.DB.PlaceBuyOrder(connection, typeID, character.ID, stationID, range, price, quantity, minVolume, 1000, duration, false);
                    // update balance for this character
                    character.Balance -= brokerCost;
                    // create record in the journal for the brokers fee and the escrow
                    this.DB.CreateJournalForCharacter(MarketReference.Brokerfee, character.ID, character.ID, null, null, -brokerCost, character.Balance, "", 1000);
                    character.Balance -= quantity * price;
                    character.Persist();
                    this.DB.CreateJournalForCharacter(MarketReference.MarketEscrow, character.ID, character.ID, null, null, -quantity * price, character.Balance, "", 1000);
                    // finally notify the character of the balance change
                    call.Client.NotifyBalanceUpdate(character.Balance);
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
            Character character = this.ItemManager.GetItem(call.Client.EnsureCharacterIsSelected()) as Character;
            double brokerCost = 0.0;
            
            // if the order is not immediate check the amount of orders the character has
            if (duration != 0)
            {
                int maximumOrders = this.GetMaxOrderCountForCharacter(character);
                int currentOrders = this.DB.CountCharsOrders(character.ID);

                if (maximumOrders <= currentOrders)
                    throw new MarketExceededOrderCount(currentOrders, maximumOrders);
                
                // calculate broker costs for the order
                this.CalculateBrokerCost(character.GetSkillLevel(ItemTypes.BrokerRelations), quantity, price, out brokerCost);
                // make sure the character has enough balance for the broker costs
                character.EnsureEnoughBalance(brokerCost);
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

            Character character = this.ItemManager.GetItem(callerCharacterID) as Character;
            
            using MySqlConnection connection = this.DB.AcquireMarketLock();
            try
            {
                MarketOrder order =  this.DB.GetOrderById(connection, orderID);

                if (order.CharacterID != callerCharacterID)
                    throw new MktOrderDidNotMatch();

                long currentTime = DateTime.UtcNow.ToFileTimeUtc();
                // check for timers, no changes in less than 5 minutes
                if (currentTime < order.Issued + TimeSpan.TicksPerMinute * 5)
                    throw new MktOrderDelay((order.Issued + TimeSpan.TicksPerMinute * 5) - currentTime);

                // check for escrow
                if (order.Escrow > 0.0 && order.Bid == TransactionType.Buy)
                {
                    character.Balance += order.Escrow;
                    character.Persist();
                    this.DB.CreateJournalForCharacter(MarketReference.MarketEscrow, order.CharacterID, order.CharacterID, null, null, order.Escrow, character.Balance, "", 1000);

                    call.Client.NotifyBalanceUpdate(character.Balance);
                }

                if (order.Bid == TransactionType.Sell)
                {
                    // check what node this item should be loaded at
                    long stationNode = this.SystemManager.GetNodeStationBelongsTo(order.LocationID);

                    if (stationNode > 0)
                    {
                        this.NotifyItemChange(call.Client.ClusterConnection, stationNode, order.ItemID, "locationID", order.LocationID);
                        this.NotifyItemChange(call.Client.ClusterConnection, stationNode, order.ItemID, "quantity", order.UnitsLeft);
                    }
                    else
                    {
                        // move the item back to the player's inventory
                        this.ItemDB.UpdateItemLocation(order.ItemID, order.LocationID);
                        this.ItemDB.UpdateItemQuantity(order.ItemID, order.UnitsLeft);
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

            Character character = this.ItemManager.GetItem(callerCharacterID) as Character;
            
            using MySqlConnection connection = this.DB.AcquireMarketLock();
            try
            {
                MarketOrder order =  this.DB.GetOrderById(connection, orderID);

                if (order.CharacterID != callerCharacterID)
                    throw new MktOrderDidNotMatch();

                long currentTime = DateTime.UtcNow.ToFileTimeUtc();
                // check for timers, no changes in less than 5 minutes
                if (currentTime < order.Issued + TimeSpan.TicksPerMinute * 5)
                    throw new MktOrderDelay((order.Issued + TimeSpan.TicksPerMinute * 5) - currentTime);

                // ensure the order hasn't been modified since the user saw it on the screen
                if ((int) order.Bid != bid || order.LocationID != stationID || order.Price != price ||
                    order.UnitsLeft != volRemaining || order.Issued != issued)
                    throw new MktOrderDidNotMatch();
                
                // get the modification broker's fee
                double brokerCost = 0.0;
                double newEscrow = 0.0;

                this.CalculateBrokerCost(character.GetSkillLevel(ItemTypes.BrokerRelations), volRemaining, (newPrice - price), out brokerCost);
                
                if (order.Bid == TransactionType.Buy)
                {
                    // calculate the difference in escrow
                    newEscrow = volRemaining * newPrice;
                    double escrowDiff = order.Escrow - newEscrow;
                    // make sure the character has enough to pay for the difference + broker
                    character.EnsureEnoughBalance(escrowDiff + brokerCost);
                    // first add the difference
                    character.Balance -= escrowDiff;
                    // create the record for the journal
                    this.DB.CreateJournalForCharacter(MarketReference.MarketEscrow, order.CharacterID, order.CharacterID, null, null, escrowDiff, character.Balance, "", 1000);    
                }
                else
                {
                    // sell orders only have to take into account broker cost
                    character.EnsureEnoughBalance(brokerCost);
                }
                
                // now subtract the broker cost
                character.Balance -= brokerCost;
                // create journal entry for the broker fee
                this.DB.CreateJournalForCharacter(MarketReference.Brokerfee, character.ID, character.ID, null, null, -brokerCost, character.Balance, "", 1000);
                // persist the character
                character.Persist();
                // everything looks okay, update the price of the order
                this.DB.UpdatePrice(connection, order.OrderID, newPrice, newEscrow);
                // notify the balance change
                call.Client.NotifyBalanceUpdate(character.Balance);
                // send a OnOwnOrderChange notification
                call.Client.NotifyMultiEvent(new OnOwnOrderChanged(order.TypeID, "Modified"));
            }
            finally
            {
                this.DB.ReleaseMarketLock(connection);
            }
            
            return null;
        }

        private int GetMaxOrderCountForCharacter(Character character)
        {
            Dictionary<int, Skill> injectedSkills = character.InjectedSkillsByTypeID;

            int retailLevel = 0, tradeLevel = 0, wholeSaleLevel = 0, tycoonLevel = 0;

            if (injectedSkills.ContainsKey((int) ItemTypes.Retail) == true)
                retailLevel = (int) injectedSkills[(int) ItemTypes.Retail].Level;
            if (injectedSkills.ContainsKey((int) ItemTypes.Trade) == true)
                tradeLevel = (int) injectedSkills[(int) ItemTypes.Trade].Level;
            if (injectedSkills.ContainsKey((int) ItemTypes.Wholesale) == true)
                wholeSaleLevel = (int) injectedSkills[(int) ItemTypes.Wholesale].Level;
            if (injectedSkills.ContainsKey((int) ItemTypes.Tycoon) == true)
                tycoonLevel = (int) injectedSkills[(int) ItemTypes.Tycoon].Level;
            
            return 5 + tradeLevel * 4 + retailLevel * 8 + wholeSaleLevel * 16 + tycoonLevel * 32;
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