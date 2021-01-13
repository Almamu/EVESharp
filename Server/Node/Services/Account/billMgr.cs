using Common.Services;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Account
{
    public class billMgr : Service
    {
        private CacheStorage CacheStorage { get; }
        public billMgr(CacheStorage cacheStorage)
        {
            this.CacheStorage = cacheStorage;
        }

        public PyDataType GetBillTypes(PyDictionary namedPayload, Client client)
        {
            this.CacheStorage.Load(
                "billMgr",
                "GetBillTypes",
                "SELECT billTypeID, billTypeName, description FROM billTypes",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("billMgr", "GetBillTypes");

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }
    }
}