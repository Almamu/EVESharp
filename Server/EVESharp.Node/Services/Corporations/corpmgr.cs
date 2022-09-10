using System;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.Node.Cache;
using EVESharp.Node.Database;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Corporations;

[MustBeCharacter]
public class corpmgr : Service
{
    public override AccessLevel   AccessLevel  => AccessLevel.None;
    private         CorporationDB DB           { get; }
    private         ICacheStorage CacheStorage { get; }

    public corpmgr (CorporationDB db, ICacheStorage cacheStorage)
    {
        DB           = db;
        CacheStorage = cacheStorage;
    }

    public PyDataType GetPublicInfo (CallInformation call, PyInteger corporationID)
    {
        return DB.GetPublicInfo (corporationID);
    }

    public PyDataType GetCorporationIDForCharacter (CallInformation call, PyInteger characterID)
    {
        return DB.GetCorporationIDForCharacter (characterID);
    }

    public PyDataType GetCorporations (CallInformation call, PyInteger corporationID)
    {
        return DB.GetCorporationRow (corporationID);
    }

    public PyDataType GetAssetInventory (CallInformation call, PyInteger corporationID, PyString which)
    {
        // TODO: CHECK PROPER PERMISSIONS TOO!
        if (call.Session.CorporationID != corporationID)
            throw new CrpAccessDenied ("You must belong to this corporation");

        if (which == "offices")
        {
            // dirty little hack, but should do the trick
            CacheStorage.StoreCall (
                "corpmgr",
                "GetAssetInventoryForLocation_" + which + "_" + corporationID,
                DB.GetOfficesLocation (corporationID),
                DateTime.UtcNow.ToFileTimeUtc ()
            );

            PyDataType cacheHint = CacheStorage.GetHint ("corpmgr", "GetAssetInventoryForLocation_" + which + "_" + corporationID);

            return CachedMethodCallResult.FromCacheHint (cacheHint);
        }

        throw new Exception ("This asset inventory is not supported yet!");

        return new PyList ();
    }

    public PyDataType GetAssetInventoryForLocation (CallInformation call, PyInteger corporationID, PyInteger location, PyString which)
    {
        // TODO: CHECK PROPER PERMISSIONS TOO!
        if (call.Session.CorporationID != corporationID)
            throw new CrpAccessDenied ("You must belong to this corporation");

        if (which == "offices")
        {
            // dirty little hack, but should do the trick
            CacheStorage.StoreCall (
                "corpmgr",
                "GetAssetInventoryForLocation_" + location + "_" + which + "_" + corporationID,
                DB.GetAssetsInOfficesAtStation (corporationID, location),
                DateTime.UtcNow.ToFileTimeUtc ()
            );

            PyDataType cacheHint = CacheStorage.GetHint ("corpmgr", "GetAssetInventoryForLocation_" + location + "_" + which + "_" + corporationID);

            return CachedMethodCallResult.FromCacheHint (cacheHint);
        }

        return new PyList ();
    }

    public PyDataType GetItemsRented (CallInformation call)
    {
        return DB.GetItemsRented (call.Session.CorporationID);
    }
}