using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Services.Account
{
    public class billMgr : Service
    {
        public billMgr(ServiceManager manager) : base(manager)
        {
        }

        public PyDataType GetBillTypes(PyDictionary namedPayload, Client client)
        {
            this.ServiceManager.CacheStorage.Load(
                "billMgr",
                "GetBillTypes",
                "SELECT billTypeID, billTypeName, description FROM billTypes",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.ServiceManager.CacheStorage.GetHint("billMgr", "GetBillTypes");

            PyTuple args = new PyTuple(3);
            PyDictionary versionCheck = new PyDictionary();
            versionCheck["versionCheck"] = "run";

            args[0] = versionCheck;
            args[1] = cacheHint;
            args[2] = new PyNone();

            return new PyObjectData("objectCaching.CachedMethodCallResult", args);
        }
    }
}