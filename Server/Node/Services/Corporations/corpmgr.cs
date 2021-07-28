﻿using System;
using Common.Services;
using EVE.Packets.Complex;
using Node.Database;
using Node.Exceptions.corpRegistry;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.Corporations
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