using Common.Services;
using Node.Network;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Services.Account
{
    public class billMgr : IService
    {
        private CacheStorage CacheStorage { get; }
        public billMgr(CacheStorage cacheStorage)
        {
            this.CacheStorage = cacheStorage;
        }

        public PyDataType GetBillTypes(CallInformation call)
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