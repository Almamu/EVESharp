using System;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Old;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Node.Cache;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Corporations;

[MustBeCharacter]
public class corpmgr : Service
{
    public override AccessLevel         AccessLevel  => AccessLevel.None;
    private         CorporationDB       DB           { get; }
    private         ICacheStorage       CacheStorage { get; }
    private         IDatabase Database     { get; }

    public corpmgr (CorporationDB db, ICacheStorage cacheStorage, IDatabase database)
    {
        DB           = db;
        CacheStorage = cacheStorage;
        Database     = database;
    }

    public PyDataType GetPublicInfo (ServiceCall call, PyInteger corporationID)
    {
        return DB.GetPublicInfo (corporationID);
    }

    public PyDataType GetCorporationIDForCharacter (ServiceCall call, PyInteger characterID)
    {
        return DB.GetCorporationIDForCharacter (characterID);
    }

    public PyDataType GetCorporations (ServiceCall call, PyInteger corporationID)
    {
        return DB.GetCorporationRow (corporationID);
    }

    public PyDataType GetAssetInventory (ServiceCall call, PyInteger corporationID, PyString which)
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
    }

    public PyDataType GetAssetInventoryForLocation (ServiceCall call, PyInteger corporationID, PyInteger location, PyString which)
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

    public PyDataType GetItemsRented (ServiceCall call)
    {
        return DB.GetItemsRented (call.Session.CorporationID);
    }

    public PyTuple AuditMember (ServiceCall call, PyInteger memberID, PyInteger fromDate, PyInteger toDate, PyInteger rowsPerPage)
    {
        if (toDate % TimeSpan.TicksPerDay == 0)
            toDate += TimeSpan.TicksPerDay;
        
        return new PyTuple (2)
        {
            [0] = Database.CrpGetAuditLog (call.Session.CorporationID, memberID, fromDate, toDate, rowsPerPage),
            [1] = Database.CrpGetAuditRole (call.Session.CorporationID, memberID, fromDate, toDate, rowsPerPage)
        };
    }
}