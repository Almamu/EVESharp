using Common.Services;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.War
{
    public class facWarMgr : Service
    {
        private CacheStorage CacheStorage { get; }
        private ItemManager ItemManager { get; }
        public facWarMgr(CacheStorage cacheStorage, ItemManager itemManager)
        {
            this.CacheStorage = cacheStorage;
            this.ItemManager = itemManager;
        }

        public PyDataType GetWarFactions(CallInformation call)
        {
            this.CacheStorage.Load(
                "facWarMgr",
                "GetWarFactions",
                "SELECT factionID, militiaCorporationID FROM chrFactions WHERE militiaCorporationID IS NOT NULL",
                CacheStorage.CacheObjectType.IntIntDict
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("facWarMgr", "GetWarFactions");

            PyTuple args = new PyTuple(3);
            PyDictionary versionCheck = new PyDictionary();
            versionCheck["versionCheck"] = "run";

            args[0] = versionCheck;
            args[1] = cacheHint;
            args[2] = new PyNone();

            return new PyObjectData("objectCaching.CachedMethodCallResult", args);
        }

        public PyDataType GetCharacterRankOverview(PyInteger characterID, CallInformation call)
        {
            return new Rowset((PyList) new PyDataType[]
                {
                    "currentRank", "highestRank", "factionID", "lastModified"
                }
            );
        }

        public PyDataType GetFactionMilitiaCorporation(PyInteger factionID, CallInformation call)
        {
            return this.ItemManager.GetFaction(factionID).MilitiaCorporationId;
        }
    }
}