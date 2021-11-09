using System;
using EVESharp.Common.Services;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.corpRegistry;
using EVESharp.Node.Network;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Corporations
{
    public class corpmgr : IService
    {
        private CorporationDB DB { get; }
        private CacheStorage CacheStorage { get; }
        
        public corpmgr(CorporationDB db, CacheStorage cacheStorage)
        {
            this.DB = db;
            this.CacheStorage = cacheStorage;
        }
        
        public PyDataType GetPublicInfo(PyInteger corporationID, CallInformation call)
        {
            return this.DB.GetPublicInfo(corporationID);
        }

        public PyDataType GetCorporationIDForCharacter(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetCorporationIDForCharacter(characterID);
        }

        public PyDataType GetCorporations(PyInteger corporationID, CallInformation call)
        {
            return this.DB.GetCorporationRow(corporationID);
        }
        
        public PyDataType GetAssetInventory(PyInteger corporationID, PyString which, CallInformation call)
        {
            // TODO: CHECK PROPER PERMISSIONS TOO!
            if (call.Client.CorporationID != corporationID)
                throw new CrpAccessDenied("You must belong to this corporation");

            if (which == "offices")
            {
                call.Client.EnsureCharacterIsSelected();
         
                // dirty little hack, but should do the trick
                this.CacheStorage.StoreCall(
                    "corpmgr",
                    "GetAssetInventoryForLocation_" + which + "_" + corporationID,
                    this.DB.GetOfficesLocation(corporationID),
                    DateTime.UtcNow.ToFileTimeUtc()
                );

                PyDataType cacheHint = this.CacheStorage.GetHint("corpmgr", "GetAssetInventoryForLocation_" + which + "_" + corporationID);

                return CachedMethodCallResult.FromCacheHint(cacheHint);
            }

            return new PyList();
        }

        public PyDataType GetAssetInventoryForLocation(PyInteger corporationID, PyInteger location, PyString which, CallInformation call)
        {
            // TODO: CHECK PROPER PERMISSIONS TOO!
            if (call.Client.CorporationID != corporationID)
                throw new CrpAccessDenied("You must belong to this corporation");

            if (which == "offices")
            {
                call.Client.EnsureCharacterIsSelected();
         
                // dirty little hack, but should do the trick
                this.CacheStorage.StoreCall(
                    "corpmgr",
                    "GetAssetInventoryForLocation_" + location + "_" + which + "_" + corporationID,
                    this.DB.GetAssetsInOfficesAtStation(corporationID, location),
                    DateTime.UtcNow.ToFileTimeUtc()
                );

                PyDataType cacheHint = this.CacheStorage.GetHint("corpmgr", "GetAssetInventoryForLocation_" + location + "_" + which + "_" + corporationID);

                return CachedMethodCallResult.FromCacheHint(cacheHint);
            }

            return new PyList();
        }

        public PyDataType GetItemsRented(CallInformation call)
        {
            return this.DB.GetItemsRented(call.Client.CorporationID);
        }
    }
}