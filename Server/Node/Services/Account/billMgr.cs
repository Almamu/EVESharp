using Common.Services;
using EVE.Packets.Complex;
using Node.Database;
using Node.Exceptions.corpRegistry;
using Node.Network;
using Node.StaticData.Corporation;
using PythonTypes.Types.Primitives;

namespace Node.Services.Account
{
    public class billMgr : IService
    {
        private CacheStorage CacheStorage { get; }
        private BillsDB DB { get; init; }
        public billMgr(CacheStorage cacheStorage, BillsDB db)
        {
            this.CacheStorage = cacheStorage;
            this.DB = db;
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

            return CachedMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetCorporationBillsReceivable(CallInformation call)
        {
            // make sure the player has the accountant role
            if (CorporationRole.Accountant.Is(call.Client.CorporationRole) == false &&
                CorporationRole.JuniorAccountant.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied("Only accountants can access the bills");
            
            return this.DB.GetCorporationBillsReceivable(call.Client.CorporationID);
        }

        public PyDataType GetCorporationBills(CallInformation call)
        {
            // make sure the player has the accountant role
            if (CorporationRole.Accountant.Is(call.Client.CorporationRole) == false &&
                CorporationRole.JuniorAccountant.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied("Only accountants can access the bills");
            
            return this.DB.GetCorporationBillsPayable(call.Client.CorporationID);
        }
    }
}