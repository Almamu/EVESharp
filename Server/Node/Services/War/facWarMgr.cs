using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.War
{
    public class facWarMgr : Service
    {
        public facWarMgr(ServiceManager manager) : base(manager)
        {
        }

        public PyDataType GetWarFactions(PyDictionary namedPayload, Client client)
        {
            this.ServiceManager.CacheStorage.Load(
                "facWarMgr",
                "GetWarFactions",
                "SELECT factionID, militiaCorporationID FROM chrFactions WHERE militiaCorporationID IS NOT NULL",
                CacheStorage.CacheObjectType.IntIntDict
            );

            PyDataType cacheHint = this.ServiceManager.CacheStorage.GetHint("facWarMgr", "GetWarFactions");

            PyTuple args = new PyTuple(3);
            PyDictionary versionCheck = new PyDictionary();
            versionCheck["versionCheck"] = "run";

            args[0] = versionCheck;
            args[1] = cacheHint;
            args[2] = new PyNone();

            return new PyObjectData("objectCaching.CachedMethodCallResult", args);
        }

        public PyDataType GetCharacterRankOverview(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return new Rowset((PyList) new PyDataType[]
                {
                    "currentRank", "highestRank", "factionID", "lastModified"
                }
            );
        }
    }
}