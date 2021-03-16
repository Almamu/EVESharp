using System;
using System.Collections.Generic;
using Common.Services;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Exceptions;
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
            
            return this.DB.GetOrders(call.Client.RegionID, call.Client.SolarSystemID2, typeID);
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
        
        public PyDataType PlaceCharOrder(PyInteger stationID, PyInteger typeID, PyDecimal price, PyInteger quantity,
            PyInteger bid, PyInteger range, PyDataType itemID, PyInteger minVolume, PyInteger duration, PyBool useCorp,
            PyDataType located, CallInformation call)
        {
            // TODO: PROPERLY HANDLE THE RANGE PARAMETER
            /*
             * {1: 'station', 3: 'solarsystem', 4: 'constellation', 5: 'region'}
             */
            int? callerStationID = call.Client.StationID;
            int callerSolarSystem = call.Client.SolarSystemID2;
            // get solarSystem for the station
            Station station = this.ItemManager.GetStation(stationID);
            SolarSystem solarSystem = this.ItemManager.GetSolarSystem(station.LocationID);
            Character character = this.ItemManager.GetItem(call.Client.EnsureCharacterIsSelected()) as Character;
            
            // if the order is not immediate check the amount of orders the character has
            if (duration != 0)
            {
                int maximumOrders = this.GetMaxOrderCountForCharacter(character);
                int currentOrders = this.DB.CountCharsOrders(character.ID);

                if (maximumOrders <= currentOrders)
                    throw new MarketExceededOrderCount(currentOrders, maximumOrders);
            }
            
            int jumps = 0;

            if (callerSolarSystem != solarSystem.ID)
                jumps = this.SolarSystemDB.GetJumpsBetweenSolarSystems(callerSolarSystem, solarSystem.ID);
            
            // check if the character has the Marketing skill and calculate distances
            if (bid == (int) TransactionType.Sell)
            {
                if (itemID is PyInteger == false)
                    throw new CustomError("Unexpected data!");
                
                long skillLevel = 0;
                Skill marketingSkill = null;
                int itemIDint = itemID as PyInteger;

                if (character.InjectedSkillsByTypeID.TryGetValue((int) ItemTypes.Marketing, out marketingSkill) == true)
                {
                    skillLevel = marketingSkill.Level;
                }

                long maximumDistance = JumpsPerSkillLevel[skillLevel];

                // -1 distance means that only from the same station can be done
                if (maximumDistance == -1)
                {
                    // ensure the player is in that station
                    if (callerStationID != stationID)
                        throw new MktCantSellItemOutsideStation(jumps);
                }
                else
                {
                    // player is out of range to place an order here
                    if (callerSolarSystem != solarSystem.ID && maximumDistance < jumps)
                        throw new MktCantSellItem2(jumps, maximumDistance);
                }

                // TODO: ADD SUPPORT FOR CORPORATIONS!
                
                // sell orders should also validate quantities, ensure they are correct
                int availableUnits = this.DB.GetAvailableItemQuantity((int) call.Client.ShipID,
                    (int) call.Client.StationID, (int) call.Client.CharacterID, typeID);

                if (availableUnits < quantity)
                    throw new NotEnoughQuantity(this.TypeManager[typeID]);
                
                // everything is checked already, perform table locking and do all the job here
                using MySqlConnection connection = this.DB.AcquireMarketLock();
                try
                {
                    // first ensure that there's enough items to sell of that in the same location
                    Dictionary<int, MarketDB.ItemQuantityEntry> items = this.DB.PrepareItemForOrder(connection, typeID,
                        (int) call.Client.StationID, (int) call.Client.ShipID, quantity,
                        (int) call.Client.CharacterID);

                    if (items == null)
                    {
                        throw new MktAsksMustSpecifyItems();
                    }
                            
                    // now notify the nodes about the changes on the other items (if required)
                    foreach (KeyValuePair<int, MarketDB.ItemQuantityEntry> pair in items)
                    {
                        if (pair.Value.NodeID == 0)
                            continue;

                        if (pair.Value.Quantity == 0)
                        {
                            this.NotifyItemChange(call.Client.ClusterConnection, pair.Value.NodeID, pair.Key, "locationID", this.ItemManager.LocationMarket.ID);
                        }
                        else
                        {
                            this.NotifyItemChange(call.Client.ClusterConnection, pair.Value.NodeID, pair.Key, "quantity", pair.Value.Quantity);
                        }
                    }
                    
                    if (duration == 0)
                    {
                        int quantityAvailable = quantity;
                            
                        // look for matching buy orders
                        MarketOrder[] orders = this.DB.FindMatchingOrders(connection, TransactionType.Buy, price, typeID, quantity);

                        // ensure there's enough satisfiable orders for the player
                        foreach (MarketOrder order in orders)
                        {
                            if (order.UnitsLeft <= quantityAvailable)
                            {
                                quantityAvailable -= order.UnitsLeft;
                            }
                            if ((order.UnitsLeft <= order.MinimumUnits && order.UnitsLeft <= quantityAvailable) || order.MinimumUnits <= quantityAvailable)
                                quantityAvailable -= order.UnitsLeft;

                            if (quantityAvailable <= 0)
                                break;
                        }

                        // if there's not enough of those here that means the order was not matched
                        // being an immediate one there's no other option but to bail out
                        if (quantityAvailable > 0)
                            throw new MktOrderDidNotMatch();
                        
                        quantityAvailable = quantity;
                        
                        // there's at least SOME orders that can be satisfied, let's start satisfying them one by one whenever possible
                        foreach (MarketOrder order in orders)
                        {
                            int quantityToSell = 0;
                            
                            if (order.UnitsLeft <= quantityAvailable)
                            {
                                // this order is fully satisfiable, so do that
                                // remove the order off the database if it's fully satisfied
                                this.DB.RemoveOrder(connection, order.OrderID);

                                quantityToSell = order.UnitsLeft;
                                quantityAvailable -= order.UnitsLeft;
                            }
                            else if (order.MinimumUnits <= quantityAvailable)
                            {
                                // we can satisfy SOME of the order
                                this.DB.UpdateOrderRemainingQuantity(connection, order.OrderID, order.UnitsLeft - quantityAvailable);
                                // the quantity we're selling is already depleted if the code got here
                                quantityToSell = order.UnitsLeft - quantityAvailable;
                                quantityAvailable = 0;
                            }

                            if (quantityToSell > 0)
                            {
                                // first calculate the raw ISK and the tax on that
                                double profit, tax;
                                
                                this.CalculateSalesTax(character.GetSkillLevel(ItemTypes.Accounting), order.UnitsLeft, order.Price, out tax, out profit);

                                // create the required records for the wallet
                                this.DB.CreateJournalForCharacter(MarketReference.MarketTransaction, character.ID, order.CharacterID, null, profit, character.Balance + profit + tax, null, 1000);
                                this.DB.CreateJournalForCharacter(MarketReference.TransactionTax, character.ID, null, null, tax, character.Balance + profit, null, 1000);
                                
                                // update balance for this character
                                character.Balance += profit;
                                
                                // send notification for the wallet to be updated
                                call.Client.NotifyBalanceUpdate(character.Balance);
                                
                                // accounting has been taken into account, start moving items
                                if (quantityAvailable == 0)
                                {
                                    // easier to move the item to it's new forever home
                                    this.ItemDB.UpdateItemOwner(itemIDint, order.CharacterID);
                                    
                                    // now notify the nodes about the change, if the item is loaded anywhere this should be enough
                                    int itemNode = this.ItemDB.GetItemNode(itemIDint);

                                    if (itemNode > 0)
                                        this.NotifyItemChange(call.Client.ClusterConnection, itemNode, itemIDint, "ownerID", order.CharacterID);
                                }
                                else
                                {
                                    // create the new item that will be used by the player
                                    ItemEntity item = this.ItemManager.CreateSimpleItem(
                                        this.TypeManager[typeID], order.CharacterID, stationID, ItemFlags.Hangar, quantity
                                    );
                                    // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
                                    this.ItemManager.UnloadItem(item);

                                    long stationNode = this.SystemManager.GetNodeStationBelongsTo(stationID);
                                    
                                    // notify the node about the item
                                    this.NotifyItemChange(call.Client.ClusterConnection, stationNode, item.ID, "locationID", stationID);
                                }
                            }
                        }
                    }
                    else
                    {
                        // TODO: ensure that the character has enough money to pay for the broker's fee
                        // formula should be something along these lines: 5%-(0.3%*BrokerRelationsLevel)-(0.03%*FactionStanding)-(0.02%*CorpStanding)
                        // source: https://support.eveonline.com/hc/en-us/articles/203218962-Broker-Fee-and-Sales-Tax
                        // there's a minimum of 100 ISK
                        
                        // create the new item that will be used by the market
                        ItemEntity item = this.ItemManager.CreateSimpleItem(
                            this.TypeManager[typeID], (int) call.Client.CharacterID, this.ItemManager.LocationMarket.ID, ItemFlags.None, quantity
                        );
                        
                        // finally place the order
                        this.DB.PlaceSellOrder(connection, item.Type.ID, item.ID, item.OwnerID, station.ID, range, price, quantity, quantity, minVolume, 1000, duration, false);
                            
                        // unload the item, we do not want it to be held by anyone
                        this.ItemManager.UnloadItem(item);
                            
                        // send a OnOwnOrderChange notification
                        call.Client.NotifyMultiEvent(new OnOwnOrderChanged(typeID, "Add"));
                    }
                }
                finally
                {
                    // free the lock
                    this.DB.ReleaseMarketLock(connection);
                }

                // everything looks fine, the character should be able to place an order
                // now the actual issue comes here
                // players can place orders far away from the stations the items are at
                // so there has to be some internal communication about this
                // luckily looks like the client can be notified of these changes after the fact
                // so a small delay of a second or two should not really be noticeable

                // as an extra, the table needs to be locked so no other orders come at the same time
                // this way we can check for immediate order placing and other things without any race conditions
                // which is critical in such a system

                // to achieve this please ensure that the market service does not access any of the inventory manager
                // functions as these MUST be handled by the required node whenever possible
                // that applies even to orders performed on the same node as the market call being made

                // in the future this would allow us to separate the market stuff from the game nodes as CCP does
                // in case the market is actually taking a lot of time of the server's processing power
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