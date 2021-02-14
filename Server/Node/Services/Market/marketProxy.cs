using System;
using Common.Services;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Services.Market
{
    public class marketProxy : Service
    {
        private MarketDB DB { get; }
        private CacheStorage CacheStorage { get; }
        
        public marketProxy(MarketDB db, CacheStorage cacheStorage)
        {
            this.DB = db;
            this.CacheStorage = cacheStorage;
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
    }
}