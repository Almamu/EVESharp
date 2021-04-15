using System;
using Common.Services;
using Node.Inventory;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class facWarMgr : IService
    {
        private CacheStorage CacheStorage { get; }
        private ItemFactory ItemFactory { get; }
        public facWarMgr(CacheStorage cacheStorage, ItemFactory itemFactory)
        {
            this.CacheStorage = cacheStorage;
            this.ItemFactory = itemFactory;
        }

        public PyDataType GetWarFactions(CallInformation call)
        {
            this.CacheStorage.Load(
                "facWarMgr",
                "GetWarFactions",
                "SELECT factionID, militiaCorporationID FROM chrFactions WHERE militiaCorporationID IS NOT NULL",
                CacheStorage.CacheObjectType.IntIntDict
            );

            return PyCacheMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("facWarMgr", "GetWarFactions")
            );
        }

        public PyDataType GetFacWarSystems(CallInformation call)
        {
            /*
             * The data is an integer dict (indicating solar system) with these as entries:
             * ["occupierID"] = Faction ID - I guess faction ID that controls the system
             * ["factionID"] = Faction ID - I guess original faction ID that controled the system?
             */
            if (this.CacheStorage.Exists("facWarMgr", "GetFacWarSystems") == false)
            {
                this.CacheStorage.StoreCall(
                    "facWarMgr",
                    "GetFacWarSystems",
                    new PyDictionary (),
                    DateTime.UtcNow.ToFileTimeUtc()
                );                
            }

            return PyCacheMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("facWarMgr", "GetFacWarSystems")
            );
        }

        public PyDataType GetCharacterRankOverview(PyInteger characterID, CallInformation call)
        {
            return new Rowset(new PyList(4)
                {
                    [0] = "currentRank",
                    [1] = "highestRank",
                    [2] = "factionID",
                    [3] = "lastModified"
                }
            );
        }

        public PyInteger GetFactionMilitiaCorporation(PyInteger factionID, CallInformation call)
        {
            return this.ItemFactory.GetStaticFaction(factionID).MilitiaCorporationId;
        }
    }
}