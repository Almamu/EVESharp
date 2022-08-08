using System;
using System.Collections.Generic;
using System.Data;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Exceptions.inventory;
using EVESharp.EVE.Exceptions.marketProxy;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Cache;
using EVESharp.Node.Client.Notifications.Inventory;
using EVESharp.Node.Client.Notifications.Market;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Dogma;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Primitives;
using Character = EVESharp.Node.Inventory.Items.Types.Character;

namespace EVESharp.Node.Services.Market;

[MustBeCharacter]
public class marketProxy : Service
{
    private static readonly int []      JumpsPerSkillLevel = {-1, 0, 5, 10, 20, 50};
    public override         AccessLevel AccessLevel => AccessLevel.None;

    private MarketDB           DB            { get; }
    private CharacterDB        CharacterDB   { get; }
    private ItemDB             ItemDB        { get; }
    private CacheStorage       CacheStorage  { get; }
    private ItemFactory        ItemFactory   { get; }
    private TypeManager        TypeManager   => ItemFactory.TypeManager;
    private SolarSystemDB      SolarSystemDB { get; }
    private Constants          Constants     { get; }
    private SystemManager      SystemManager => ItemFactory.SystemManager;
    private NotificationSender Notifications { get; }
    private IWalletManager      WalletManager { get; }
    private DogmaUtils         DogmaUtils    { get; }

    public marketProxy (
        MarketDB  db,        CharacterDB        characterDB, ItemDB itemDB, SolarSystemDB solarSystemDB, ItemFactory itemFactory, CacheStorage cacheStorage,
        Constants constants, NotificationSender notificationSender, IWalletManager walletManager, DogmaUtils dogmaUtils, ClusterManager clusterManager
    )
    {
        DB            = db;
        CharacterDB   = characterDB;
        ItemDB        = itemDB;
        SolarSystemDB = solarSystemDB;
        CacheStorage  = cacheStorage;
        ItemFactory   = itemFactory;
        Constants     = constants;
        Notifications = notificationSender;
        WalletManager = walletManager;
        DogmaUtils    = dogmaUtils;

        clusterManager.OnClusterTimer += this.PerformTimedEvents;
    }

    private PyDataType GetNewTransactions (
        int       entityID, PyInteger  sellBuy,  PyInteger  typeID,   PyDataType clientID,
        PyInteger quantity, PyDataType fromDate, PyDataType maxPrice, PyInteger  minPrice, PyInteger accountKey
    )
    {
        TransactionType transactionType = TransactionType.Either;

        if (sellBuy is not null)
            switch (sellBuy.Value)
            {
                case 0:
                    transactionType = TransactionType.Sell;
                    break;

                case 1:
                    transactionType = TransactionType.Buy;
                    break;

            }

        return DB.GetNewTransactions (entityID, null, transactionType, typeID, quantity, minPrice, accountKey);
    }

    public PyDataType CharGetNewTransactions (
        CallInformation call, PyInteger sellBuy,  PyInteger  typeID,   PyDataType clientID,
        PyInteger quantity, PyDataType fromDate, PyDataType maxPrice, PyInteger minPrice
    )
    {
        int callerCharacterID = call.Session.CharacterID;

        return this.GetNewTransactions (
            callerCharacterID, sellBuy, typeID, clientID, quantity, fromDate, maxPrice, minPrice,
            WalletKeys.MAIN
        );
    }

    [MustHaveCorporationRole(MLS.UI_SHARED_WALLETHINT8, CorporationRole.Accountant, CorporationRole.JuniorAccountant)]
    public PyDataType CorpGetNewTransactions (
        CallInformation call, PyInteger       sellBuy,  PyInteger  typeID,   PyDataType clientID,
        PyInteger       quantity, PyDataType fromDate, PyDataType maxPrice, PyInteger minPrice, PyInteger accountKey, PyInteger who
    )
    {
        // TODO: SUPPORT THE "who" PARAMETER
        return this.GetNewTransactions (
            call.Session.CorporationID, sellBuy, typeID, clientID, quantity, fromDate, maxPrice, minPrice,
            accountKey
        );
    }

    public PyDataType GetMarketGroups (CallInformation call)
    {
        // check if the cache already exits
        if (CacheStorage.Exists ("marketProxy", "GetMarketGroups") == false)
            CacheStorage.StoreCall (
                "marketProxy",
                "GetMarketGroups",
                DB.GetMarketGroups (),
                DateTime.UtcNow.ToFileTimeUtc ()
            );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("marketProxy", "GetMarketGroups"));
    }

    public PyDataType GetCharOrders (CallInformation call)
    {
        return DB.GetOrdersForOwner (call.Session.CharacterID);
    }

    [MustBeInStation]
    public PyDataType GetStationAsks (CallInformation call)
    {
        return DB.GetStationAsks (call.Session.StationID);
    }

    public PyDataType GetSystemAsks (CallInformation call)
    {
        return DB.GetSystemAsks (call.Session.SolarSystemID2);
    }

    public PyDataType GetRegionBest (CallInformation call)
    {
        return DB.GetRegionBest (call.Session.RegionID);
    }

    public PyDataType GetOrders (CallInformation call, PyInteger typeID)
    {
        // dirty little hack, but should do the trick
        CacheStorage.StoreCall (
            "marketProxy",
            "GetOrders_" + typeID,
            DB.GetOrders (call.Session.RegionID, call.Session.SolarSystemID2, typeID),
            DateTime.UtcNow.ToFileTimeUtc ()
        );

        PyDataType cacheHint = CacheStorage.GetHint ("marketProxy", "GetOrders_" + typeID);

        return CachedMethodCallResult.FromCacheHint (cacheHint);
    }

    public PyDataType StartupCheck (CallInformation call)
    {
        // this function is called when "buy this" is pressed in the market
        // seems to do some specific check on the market proxy status
        // but we can roll with no return info for it :D
        return null;
    }

    public PyDataType GetOldPriceHistory (CallInformation call, PyInteger typeID)
    {
        return DB.GetOldPriceHistory (call.Session.RegionID, typeID);
    }

    public PyDataType GetNewPriceHistory (CallInformation call, PyInteger typeID)
    {
        return DB.GetNewPriceHistory (call.Session.RegionID, typeID);
    }

    private void CalculateSalesTax (long accountingLevel, int quantity, double price, out double tax, out double profit)
    {
        double salesTax  = Constants.MarketTransactionTax / 100.0 * (1 - accountingLevel * 0.1);
        double beforeTax = price * quantity;

        tax    = beforeTax * salesTax;
        profit = price * quantity - tax;
    }

    private void CalculateBrokerCost (long brokerLevel, int quantity, double price, out double brokerCost)
    {
        double brokerPercentage = (double) Constants.MarketCommissionPercentage / 100 * (1 - brokerLevel * 0.05);

        // TODO: GET THE STANDINGS FOR THE CHARACTER
        double factionStanding = 0.0;
        double corpStanding    = 0.0;

        double weightedStanding = (0.7 * factionStanding + 0.3 * corpStanding) / 10.0;

        brokerPercentage = brokerPercentage * Math.Pow (2.0, -2 * weightedStanding);
        brokerCost       = price * quantity * brokerPercentage;

        if (brokerCost < Constants.MarketMinimumFee)
            brokerCost = Constants.MarketMinimumFee;
    }

    private void CheckSellOrderDistancePermissions (Character character, int stationID)
    {
        Station station = ItemFactory.GetStaticStation (stationID);

        if (character.RegionID != station.RegionID)
            throw new MktInvalidRegion ();

        int  jumps               = SolarSystemDB.GetJumpsBetweenSolarSystems (character.SolarSystemID, station.SolarSystemID);
        long marketingSkillLevel = character.GetSkillLevel (Types.Marketing);
        long maximumDistance     = JumpsPerSkillLevel [marketingSkillLevel];

        if (maximumDistance == -1 && character.StationID != stationID)
            throw new MktCantSellItemOutsideStation (jumps);
        if (character.SolarSystemID != station.SolarSystemID && maximumDistance < jumps)
            throw new MktCantSellItem2 (jumps, maximumDistance);
    }

    private void CheckBuyOrderDistancePermissions (Character character, int stationID, int duration)
    {
        // immediate orders can be placed regardless of distance
        if (duration == 0)
            return;

        Station station = ItemFactory.GetStaticStation (stationID);

        if (character.RegionID != station.RegionID)
            throw new MktInvalidRegion ();

        int  jumps                 = SolarSystemDB.GetJumpsBetweenSolarSystems (character.SolarSystemID, station.SolarSystemID);
        long procurementSkillLevel = character.GetSkillLevel (Types.Procurement);
        long maximumDistance       = JumpsPerSkillLevel [procurementSkillLevel];

        if (maximumDistance == -1 && character.StationID != stationID)
            throw new MktCantSellItemOutsideStation (jumps);
        if (character.SolarSystemID != station.SolarSystemID && maximumDistance < jumps)
            throw new MktCantSellItem2 (jumps, maximumDistance);
    }

    private void CheckMatchingBuyOrders (MarketOrder [] orders, int quantity, int stationID)
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
                quantity -= Math.Min (order.UnitsLeft, quantity);

            if (quantity <= 0)
                break;
        }

        // if there's not enough of those here that means the order was not matched
        // being an immediate one there's no other option but to bail out
        if (quantity > 0)
            throw new MktOrderDidNotMatch ();
    }

    private void PlaceImmediateSellOrderChar (
        IDbConnection connection, IWallet  wallet, Character character, int itemID, int typeID, int stationID, int quantity,
        double        price,      Session session
    )
    {
        int solarSystemID = ItemFactory.GetStaticStation (stationID).SolarSystemID;

        // look for matching buy orders
        MarketOrder [] orders = DB.FindMatchingOrders (connection, price, typeID, character.ID, solarSystemID, TransactionType.Buy);

        // ensure there's at least some that match
        this.CheckMatchingBuyOrders (orders, quantity, stationID);

        // there's at least SOME orders that can be satisfied, let's start satisfying them one by one whenever possible
        foreach (MarketOrder order in orders)
        {
            int quantityToSell = 0;

            // ensure the order is in the range
            if (order.Range == -1 && order.LocationID != stationID)
                continue;
            if (order.Range != -1 && order.Range < order.Jumps)
                continue;

            int orderOwnerID = order.IsCorp ? order.CorporationID : order.CharacterID;

            if (order.UnitsLeft <= quantity)
            {
                // if there's any kind of escrow left ensure that the character receives it back
                double escrowLeft = order.Escrow - order.UnitsLeft * price;

                if (escrowLeft > 0.0)
                {
                    // give back the escrow for the character
                    // TODO: THERE IS A POTENTIAL DEADLOCK HERE IF WE BUY FROM OURSELVES
                    using IWallet escrowWallet = WalletManager.AcquireWallet (orderOwnerID, order.AccountID, order.IsCorp);
                    {
                        escrowWallet.CreateJournalRecord (MarketReference.MarketEscrow, null, null, escrowLeft);
                    }
                }

                // this order is fully satisfiable, so do that
                // remove the order off the database if it's fully satisfied
                DB.RemoveOrder (connection, order.OrderID);

                quantityToSell =  order.UnitsLeft;
                quantity       -= order.UnitsLeft;
            }
            else if (order.MinimumUnits <= quantity)
            {
                // we can satisfy SOME of the order
                DB.UpdateOrderRemainingQuantity (connection, order.OrderID, order.UnitsLeft - quantity, quantity * price);
                // the quantity we're selling is already depleted if the code got here
                quantityToSell = quantity;
                quantity       = 0;
            }

            if (quantityToSell > 0)
            {
                // calculate sales tax
                double profit, tax;

                this.CalculateSalesTax (character.GetSkillLevel (Types.Accounting), quantity, price, out tax, out profit);

                // create the required records for the wallet
                wallet.CreateJournalRecord (MarketReference.MarketTransaction, orderOwnerID, character.ID, null, profit);

                if (tax > 0)
                    wallet.CreateJournalRecord (MarketReference.TransactionTax, null, null, -tax);

                wallet.CreateTransactionRecord (TransactionType.Sell, character.ID, orderOwnerID, typeID, quantityToSell, price, stationID);
                WalletManager.CreateTransactionRecord (
                    orderOwnerID, TransactionType.Buy, order.CharacterID, character.ID, typeID, quantityToSell, price, stationID,
                    order.AccountID
                );

                // create the new item that will be used by the player
                ItemEntity item = ItemFactory.CreateSimpleItem (
                    TypeManager [typeID], orderOwnerID, stationID, order.IsCorp ? Flags.CorpMarket : Flags.Hangar, quantityToSell
                );

                // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
                ItemFactory.UnloadItem (item);

                // check if the station it's at is loaded and notify the node in question
                // if not take care of the item notification ourselves
                long stationNode = SystemManager.GetNodeStationBelongsTo (stationID);

                if (stationNode == 0 || SystemManager.StationBelongsToUs (stationID))
                    Notifications.NotifyCharacter (item.OwnerID, OnItemChange.BuildLocationChange (item, ItemFactory.LocationMarket.ID));
                else
                    Notifications.NotifyNode (
                        stationNode, Node.Notifications.Nodes.Inventory.OnItemChange.BuildLocationChange (itemID, ItemFactory.LocationMarket.ID, stationID)
                    );
            }

            // ensure we do not sell more than we have
            if (quantity == 0)
                break;
        }
    }

    private void PlaceSellOrderCharUpdateItems (IDbConnection connection, Session session, int stationID, int typeID, int quantity)
    {
        Dictionary <int, MarketDB.ItemQuantityEntry> items             = null;
        int                                          callerCharacterID = session.CharacterID;

        // depending on where the character that is placing the order, the way to detect the items should be different
        if (stationID == session.StationID)
            items = DB.PrepareItemForOrder (
                connection, typeID, stationID, session.ShipID ?? -1, quantity, session.CharacterID, session.CorporationID, session.CorporationRole
            );
        else
            items = DB.PrepareItemForOrder (
                connection, typeID, stationID, -1, quantity, session.CharacterID, session.CorporationID, session.CorporationRole
            );

        if (items is null)
            throw new NotEnoughQuantity (TypeManager [typeID]);

        long stationNode = SystemManager.GetNodeStationBelongsTo (stationID);

        if (SystemManager.StationBelongsToUs (stationID) || stationNode == 0)
        {
            // load the items here and send proper notifications
            foreach ((int _, MarketDB.ItemQuantityEntry entry) in items)
            {
                ItemEntity item = ItemFactory.LoadItem (entry.ItemID);

                if (entry.Quantity == 0)
                {
                    // item has to be destroyed
                    ItemFactory.DestroyItem (item);
                    // notify item destroyal
                    DogmaUtils.QueueMultiEvent (callerCharacterID, OnItemChange.BuildLocationChange (item, entry.LocationID));
                }
                else
                {
                    // just a quantity change
                    item.Quantity = entry.Quantity;
                    // notify the client
                    DogmaUtils.QueueMultiEvent (callerCharacterID, OnItemChange.BuildQuantityChange (item, entry.OriginalQuantity));
                    // unload the item if it's not needed
                    ItemFactory.UnloadItem (item);
                }
            }
        }
        else
        {
            // the item changes should be handled by a different node
            Notifications.Nodes.Inventory.OnItemChange changes = new Notifications.Nodes.Inventory.OnItemChange ();

            foreach ((int _, MarketDB.ItemQuantityEntry entry) in items)
                if (entry.Quantity == 0)
                    changes.AddChange (entry.ItemID, "locationID", entry.LocationID, ItemFactory.LocationMarket.ID);
                else
                    changes.AddChange (entry.ItemID, "quantity", entry.OriginalQuantity, entry.Quantity);
        }
    }

    private void PlaceSellOrder (
        CallInformation call, int itemID, Character character,  int stationID, int quantity,   int             typeID, int duration, double price,
        int range,  double    brokerCost, int ownerID,   int accountKey
    )
    {
        int callerCharacterID = call.Session.CharacterID;
        // check distance for the order
        this.CheckSellOrderDistancePermissions (character, stationID);

        // obtain wallet lock too
        // everything is checked already, perform table locking and do all the job here
        using IWallet          wallet     = WalletManager.AcquireWallet (ownerID, accountKey, ownerID == call.Session.CorporationID);
        using IDbConnection connection = DB.AcquireMarketLock ();

        try
        {
            // check if the item is singleton and throw a exception about it
            {
                bool singleton = false;

                DB.CheckRepackagedItem (connection, itemID, out singleton);

                if (singleton)
                    throw new RepackageBeforeSelling (TypeManager [typeID]);
            }

            if (duration == 0)
            {
                // move the items to update
                this.PlaceSellOrderCharUpdateItems (connection, call.Session, stationID, typeID, quantity);
                // finally create the records in the market database
                this.PlaceImmediateSellOrderChar (
                    connection, wallet, character, itemID, typeID, stationID, quantity, price,
                    call.Session
                );
            }
            else
            {
                // ensure the player can pay taxes and broker
                wallet.EnsureEnoughBalance (brokerCost);
                // do broker fee first
                wallet.CreateJournalRecord (MarketReference.Brokerfee, null, null, -brokerCost);
                // move the items to update
                this.PlaceSellOrderCharUpdateItems (connection, call.Session, stationID, typeID, quantity);
                // finally place the order
                DB.PlaceSellOrder (
                    connection, typeID, character.ID, call.Session.CorporationID, stationID, range, price, quantity,
                    accountKey, duration, ownerID == character.CorporationID
                );
            }

            // send a OnOwnOrderChange notification
            DogmaUtils.QueueMultiEvent (callerCharacterID, new OnOwnOrderChanged (typeID, "Add"));
        }
        finally
        {
            DB.ReleaseMarketLock (connection);
        }
    }

    private void CheckMatchingSellOrders (MarketOrder [] orders, int quantity, int stationID)
    {
        foreach (MarketOrder order in orders)
        {
            if (order.Range == -1 && order.LocationID != stationID)
                continue;
            if (order.Range != -1 && order.Range < order.Jumps)
                continue;

            quantity -= Math.Min (quantity, order.UnitsLeft);

            if (quantity == 0)
                break;
        }

        if (quantity > 0)
            throw new MktOrderDidNotMatch ();
    }

    private void PlaceImmediateBuyOrderChar (
        CallInformation call, IDbConnection connection, IWallet          wallet, int typeID, Character character, int stationID, int quantity, double price,
        int           range
    )
    {
        int solarSystemID = ItemFactory.GetStaticStation (stationID).SolarSystemID;

        // look for matching sell orders
        MarketOrder [] orders = DB.FindMatchingOrders (connection, price, typeID, character.ID, solarSystemID, TransactionType.Sell);

        // ensure there's at least some that match
        this.CheckMatchingSellOrders (orders, quantity, solarSystemID);

        foreach (MarketOrder order in orders)
        {
            int quantityToBuy = 0;

            if (order.Range == -1 && order.LocationID != stationID)
                continue;
            if (order.Range != -1 && order.Range < order.Jumps)
                continue;

            int orderOwnerID = order.IsCorp ? order.CorporationID : order.CharacterID;

            if (order.UnitsLeft <= quantity)
            {
                // the order was completed, remove it from the database
                DB.RemoveOrder (connection, order.OrderID);

                // increase the amount of bought items
                quantityToBuy =  order.UnitsLeft;
                quantity      -= order.UnitsLeft;
            }
            else
            {
                // part of the sell order was satisfied
                DB.UpdateOrderRemainingQuantity (connection, order.OrderID, order.UnitsLeft - quantity, 0);

                quantityToBuy = quantity;
                quantity      = 0;
            }

            if (quantityToBuy > 0)
            {
                // calculate sales tax
                double tax;

                this.CalculateSalesTax (CharacterDB.GetSkillLevelForCharacter (Types.Accounting, order.CharacterID), quantity, price, out tax, out _);

                // acquire wallet journal for seller so we can update their balance to add the funds that he got
                using IWallet sellerWallet = WalletManager.AcquireWallet (orderOwnerID, order.AccountID, order.IsCorp);
                {
                    sellerWallet.CreateJournalRecord (MarketReference.MarketTransaction, orderOwnerID, null, price * quantityToBuy);
                    // calculate sales tax for the seller
                    if (tax > 0)
                        sellerWallet.CreateJournalRecord (MarketReference.TransactionTax, ItemFactory.OwnerSCC.ID, null, -tax);

                    sellerWallet.CreateTransactionRecord (TransactionType.Sell, order.CharacterID, wallet.OwnerID, typeID, quantityToBuy, price, stationID);
                }

                // create the transaction records for both characters
                wallet.CreateTransactionRecord (TransactionType.Buy, character.ID, orderOwnerID, typeID, quantityToBuy, price, stationID);

                long stationNode = SystemManager.GetNodeStationBelongsTo (stationID);

                // create the new item that will be used by the player
                ItemEntity item = ItemFactory.CreateSimpleItem (
                    TypeManager [typeID], wallet.OwnerID, stationID, wallet.OwnerID == character.CorporationID ? Flags.CorpMarket : Flags.Hangar, quantityToBuy
                );
                // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
                ItemFactory.UnloadItem (item);

                if (stationNode == 0 || SystemManager.StationBelongsToUs (stationID))
                    Notifications.NotifyCharacter (character.ID, OnItemChange.BuildLocationChange (item, ItemFactory.LocationMarket.ID));
                else
                    Notifications.NotifyNode (
                        stationNode, Node.Notifications.Nodes.Inventory.OnItemChange.BuildLocationChange (item.ID, ItemFactory.LocationMarket.ID, stationID)
                    );
            }

            // ensure we do not buy more than we need
            if (quantity == 0)
                break;
        }
    }

    private void PlaceBuyOrder (
        CallInformation call, int typeID, Character character,  int stationID, int quantity,   double          price, int duration, int minVolume,
        int range,  double    brokerCost, int ownerID,   int accountKey
    )
    {
        // ensure the character can place the order where he's trying to
        this.CheckBuyOrderDistancePermissions (character, stationID, duration);

        using IWallet        wallet     = WalletManager.AcquireWallet (ownerID, accountKey, ownerID == call.Session.CorporationID);
        using IDbConnection connection = DB.AcquireMarketLock ();

        try
        {
            // make sure the character can pay the escrow and the broker
            wallet.EnsureEnoughBalance (quantity * price + brokerCost);
            // do the escrow after
            wallet.CreateJournalRecord (MarketReference.MarketEscrow, null, null, -quantity * price);

            if (duration == 0)
            {
                this.PlaceImmediateBuyOrderChar (
                    call, connection, wallet, typeID, character, stationID, quantity, price, range
                );
            }
            else
            {
                // do broker fee first
                wallet.CreateJournalRecord (MarketReference.Brokerfee, null, null, -brokerCost);
                // place the buy order
                DB.PlaceBuyOrder (
                    connection, typeID, character.ID, call.Session.CorporationID, stationID, range, price, quantity,
                    minVolume, accountKey, duration, ownerID == character.CorporationID
                );
            }

            // send a OnOwnOrderChange notification
            DogmaUtils.QueueMultiEvent (character.ID, new OnOwnOrderChanged (typeID, "Add"));
        }
        finally
        {
            DB.ReleaseMarketLock (connection);
        }
    }


    public PyDataType PlaceCharOrder (
        CallInformation call, PyInteger  stationID, PyInteger       typeID, PyDecimal  price,  PyInteger quantity,
        PyInteger  bid,       PyInteger       range,  PyDataType itemID, PyInteger minVolume, PyInteger duration, PyInteger useCorp,
        PyDataType located
    )
    {
        return this.PlaceCharOrder (
            call, stationID, typeID, price, quantity, bid, range, itemID, minVolume, duration, useCorp == 1, located
        );
    }

    public PyDataType PlaceCharOrder (
        CallInformation call, PyInteger  stationID, PyInteger       typeID, PyDecimal  price,  PyInteger quantity,
        PyInteger  bid,       PyInteger       range,  PyDataType itemID, PyInteger minVolume, PyInteger duration, PyBool useCorp,
        PyDataType located
    )
    {
        // get solarSystem for the station
        Character character  = ItemFactory.GetItem <Character> (call.Session.CharacterID);
        double    brokerCost = 0.0;

        // if the order is not immediate check the amount of orders the character has
        if (duration != 0)
        {
            int maximumOrders = this.GetMaxOrderCountForCharacter (character);
            int currentOrders = DB.CountCharsOrders (character.ID);

            if (maximumOrders <= currentOrders)
                throw new MarketExceededOrderCount (currentOrders, maximumOrders);

            // calculate broker costs for the order
            this.CalculateBrokerCost (character.GetSkillLevel (Types.BrokerRelations), quantity, price, out brokerCost);
        }

        int ownerID    = character.ID;
        int accountKey = WalletKeys.MAIN;

        // make sure the user has permissions on the wallet of the corporation
        // for sell orders just look into if the user can query that wallet
        if (useCorp == true)
        {
            if (WalletManager.IsTakeAllowed (call.Session, call.Session.CorpAccountKey, call.Session.CorporationID) == false)
                throw new CrpAccessDenied (MLS.UI_CORP_ACCESSTOWALLETDIVISIONDENIED);
            if (CorporationRole.Trader.Is (call.Session.CorporationRole) == false)
                throw new CrpAccessDenied (MLS.UI_SHARED_WALLETHINT11);

            ownerID    = call.Session.CorporationID;
            accountKey = call.Session.CorpAccountKey;
        }

        // check if the character has the Marketing skill and calculate distances
        if (bid == (int) TransactionType.Sell)
        {
            if (itemID is PyInteger == false)
                throw new CustomError ("Unexpected data!");

            this.PlaceSellOrder (
                call, itemID as PyInteger, character, stationID, quantity, typeID, duration, price, range,
                brokerCost, ownerID, accountKey
            );
        }
        else if (bid == (int) TransactionType.Buy)
        {
            this.PlaceBuyOrder (
                call, typeID, character, stationID, quantity, price, duration, minVolume, range,
                brokerCost, ownerID, accountKey
            );
        }

        return null;
    }

    public PyDataType CancelCharOrder (CallInformation call, PyInteger orderID, PyInteger regionID)
    {
        int callerCharacterID = call.Session.CharacterID;

        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

        using IDbConnection connection = DB.AcquireMarketLock ();

        try
        {
            MarketOrder order = DB.GetOrderById (connection, orderID);

            if (order.CharacterID != callerCharacterID)
                throw new MktOrderDidNotMatch ();

            long currentTime = DateTime.UtcNow.ToFileTimeUtc ();

            // check for timers, no changes in less than 5 minutes
            if (currentTime < order.Issued + TimeSpan.TicksPerSecond * Constants.MarketModificationDelay)
                throw new MktOrderDelay (order.Issued + TimeSpan.TicksPerSecond * Constants.MarketModificationDelay - currentTime);

            int orderOwnerID = order.IsCorp ? order.CorporationID : order.CharacterID;

            // check for escrow
            if (order.Escrow > 0.0 && order.Bid == TransactionType.Buy)
            {
                using IWallet wallet = WalletManager.AcquireWallet (orderOwnerID, order.AccountID, order.IsCorp);
                {
                    wallet.CreateJournalRecord (MarketReference.MarketEscrow, null, null, order.Escrow);
                }
            }

            if (order.Bid == TransactionType.Sell)
            {
                // create the new item that will be used by the player
                ItemEntity item = ItemFactory.CreateSimpleItem (
                    TypeManager [order.TypeID], orderOwnerID, order.LocationID, order.IsCorp ? Flags.CorpMarket : Flags.Hangar, order.UnitsLeft
                );
                // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
                ItemFactory.UnloadItem (item);

                // check what node this item should be loaded at
                long stationNode = SystemManager.GetNodeStationBelongsTo (order.LocationID);

                if (stationNode == 0 || SystemManager.StationBelongsToUs (order.LocationID))
                    Notifications.NotifyCharacter (character.ID, OnItemChange.BuildLocationChange (item, ItemFactory.LocationMarket.ID));
                else
                    Notifications.NotifyNode (
                        stationNode,
                        Node.Notifications.Nodes.Inventory.OnItemChange.BuildLocationChange (item.ID, ItemFactory.LocationMarket.ID, order.LocationID)
                    );
            }

            // finally remove the order
            DB.RemoveOrder (connection, order.OrderID);

            OnOwnOrderChanged notification = new OnOwnOrderChanged (order.TypeID, "Removed");

            if (order.IsCorp)
                // send a notification to the owner
                Notifications.NotifyCorporationByRole (call.Session.CorporationID, CorporationRole.Trader, notification);
            else
                // send a OnOwnOrderChange notification
                DogmaUtils.QueueMultiEvent (callerCharacterID, notification);
        }
        finally
        {
            DB.ReleaseMarketLock (connection);
        }

        return null;
    }

    public PyDataType ModifyCharOrder (
        CallInformation call, PyInteger orderID, PyDecimal       newPrice, PyInteger bid, PyInteger stationID, PyInteger solarSystemID, PyDecimal price, PyInteger volRemaining,
        PyInteger issued
    )
    {
        int callerCharacterID = call.Session.CharacterID;

        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

        using IDbConnection connection = DB.AcquireMarketLock ();

        try
        {
            MarketOrder order = DB.GetOrderById (connection, orderID);

            if (order.CharacterID != callerCharacterID)
                throw new MktOrderDidNotMatch ();

            long currentTime = DateTime.UtcNow.ToFileTimeUtc ();

            // check for timers, no changes in less than 5 minutes
            if (currentTime < order.Issued + TimeSpan.TicksPerSecond * Constants.MarketModificationDelay)
                throw new MktOrderDelay (order.Issued + TimeSpan.TicksPerSecond * Constants.MarketModificationDelay - currentTime);

            // ensure the order hasn't been modified since the user saw it on the screen
            if ((int) order.Bid != bid || order.LocationID != stationID || order.Price != price ||
                order.UnitsLeft != volRemaining || order.Issued != issued)
                throw new MktOrderDidNotMatch ();

            // get the modification broker's fee
            double brokerCost = 0.0;
            double newEscrow  = 0.0;

            this.CalculateBrokerCost (character.GetSkillLevel (Types.BrokerRelations), volRemaining, newPrice - price, out brokerCost);

            int orderOwnerID = order.IsCorp ? order.CorporationID : order.CharacterID;

            using IWallet wallet = WalletManager.AcquireWallet (orderOwnerID, order.AccountID, order.IsCorp);
            {
                if (order.Bid == TransactionType.Buy)
                {
                    // calculate the difference in escrow
                    newEscrow = volRemaining * newPrice;
                    double escrowDiff = order.Escrow - newEscrow;

                    // ensure enough balances
                    wallet.EnsureEnoughBalance (escrowDiff + brokerCost);
                    // take the difference in escrow
                    wallet.CreateJournalRecord (MarketReference.MarketEscrow, null, null, escrowDiff);
                }
                else
                {
                    wallet.EnsureEnoughBalance (brokerCost);
                }

                // pay the broker fee once again
                wallet.CreateJournalRecord (MarketReference.Brokerfee, null, null, -brokerCost);
            }

            // everything looks okay, update the price of the order
            DB.UpdatePrice (connection, order.OrderID, newPrice, newEscrow);

            OnOwnOrderChanged notification = new OnOwnOrderChanged (order.TypeID, "Modified");

            if (order.IsCorp)
                // send a notification to the owner
                Notifications.NotifyCorporationByRole (call.Session.CorporationID, CorporationRole.Trader, notification);
            else
                // send a OnOwnOrderChange notification
                DogmaUtils.QueueMultiEvent (callerCharacterID, notification);
        }
        finally
        {
            DB.ReleaseMarketLock (connection);
        }

        return null;
    }

    /// <returns>The maximum active order count for the given <paramref name="character"/></returns>
    private int GetMaxOrderCountForCharacter (Character character)
    {
        Dictionary <int, Skill> injectedSkills = character.InjectedSkillsByTypeID;

        int retailLevel = 0, tradeLevel = 0, wholeSaleLevel = 0, tycoonLevel = 0;

        if (injectedSkills.ContainsKey ((int) Types.Retail))
            retailLevel = (int) injectedSkills [(int) Types.Retail].Level;
        if (injectedSkills.ContainsKey ((int) Types.Trade))
            tradeLevel = (int) injectedSkills [(int) Types.Trade].Level;
        if (injectedSkills.ContainsKey ((int) Types.Wholesale))
            wholeSaleLevel = (int) injectedSkills [(int) Types.Wholesale].Level;
        if (injectedSkills.ContainsKey ((int) Types.Tycoon))
            tycoonLevel = (int) injectedSkills [(int) Types.Tycoon].Level;

        return 5 + tradeLevel * 4 + retailLevel * 8 + wholeSaleLevel * 16 + tycoonLevel * 32;
    }

    /// <summary>
    /// Removes an expired buy order from the database and returns the leftover escrow back into the player's wallet
    /// </summary>
    /// <param name="connection">The database connection that acquired the lock</param>
    /// <param name="order">The order to mark as expired</param>
    private void BuyOrderExpired (IDbConnection connection, MarketOrder order)
    {
        // remove order
        DB.RemoveOrder (connection, order.OrderID);

        // give back the escrow paid by the player
        using IWallet wallet = WalletManager.AcquireWallet (order.IsCorp ? order.CorporationID : order.CharacterID, order.AccountID, order.IsCorp);
        {
            wallet.CreateJournalRecord (MarketReference.MarketEscrow, null, null, order.Escrow);
        }

        // notify the character about the change in the order
        Notifications.NotifyCharacter (order.CharacterID, new OnOwnOrderChanged (order.TypeID, "Expiry", order.AccountID > WalletKeys.MAIN));
    }

    /// <summary>
    /// Removes an expired sell order from the database and returns the leftover items back to the player's hangar
    /// </summary>
    /// <param name="connection">The database connection that acquired the lock</param>
    /// <param name="order">The order to mark as expired</param>
    private void SellOrderExpired (IDbConnection connection, MarketOrder order)
    {
        // remove order
        DB.RemoveOrder (connection, order.OrderID);
        // create the item back into the player's hanger

        // create the new item that will be used by the player
        ItemEntity item = ItemFactory.CreateSimpleItem (
            TypeManager [order.TypeID], order.CharacterID, order.LocationID, order.IsCorp ? Flags.CorpMarket : Flags.Hangar, order.UnitsLeft
        );
        // immediately unload it, if it has to be loaded the OnItemUpdate notification will take care of that
        ItemFactory.UnloadItem (item);

        long stationNode = SystemManager.GetNodeStationBelongsTo (order.LocationID);

        if (stationNode == 0 || SystemManager.StationBelongsToUs (order.LocationID))
            Notifications.NotifyCharacter (order.CharacterID, OnItemChange.BuildLocationChange (item, ItemFactory.LocationMarket.ID));
        else
            Notifications.NotifyNode (
                stationNode, Node.Notifications.Nodes.Inventory.OnItemChange.BuildLocationChange (item.ID, ItemFactory.LocationMarket.ID, order.LocationID)
            );

        // finally notify the character about the order change
        Notifications.NotifyCharacter (order.CharacterID, new OnOwnOrderChanged (order.TypeID, "Expiry"));
        // TODO: SEND AN EVEMAIL TO THE PLAYER?
    }

    /// <summary>
    /// Checks orders that are expired, cancels them and returns the items to the hangar if required
    /// </summary>
    public void PerformTimedEvents (object sender, EventArgs args)
    {
        using IDbConnection connection = DB.AcquireMarketLock ();

        try
        {
            List <MarketOrder> orders = DB.GetExpiredOrders (connection);

            foreach (MarketOrder order in orders)
                switch (order.Bid)
                {
                    case TransactionType.Buy:
                        // buy orders need to return the escrow
                        this.BuyOrderExpired (connection, order);
                        break;

                    case TransactionType.Sell:
                        // sell orders are a bit harder, the items have to go back to the player's hangar
                        this.SellOrderExpired (connection, order);
                        break;

                }
        }
        finally
        {
            DB.ReleaseMarketLock (connection);
        }
    }

    [MustHaveCorporationRole(MLS.UI_SHARED_WALLETHINT1, CorporationRole.Accountant)]
    public PyDataType GetCorporationOrders (CallInformation call)
    {
        return DB.GetOrdersForOwner (call.Session.CorporationID, true);
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