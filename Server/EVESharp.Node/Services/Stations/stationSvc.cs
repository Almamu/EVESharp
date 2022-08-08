using System;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.Node.Cache;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Stations;

public class stationSvc : Service
{
    public override AccessLevel  AccessLevel  => AccessLevel.None;
    private         IItems  Items  { get; }
    private         ICacheStorage CacheStorage { get; }

    public stationSvc (IItems items, ICacheStorage cacheStorage)
    {
        this.Items  = items;
        CacheStorage = cacheStorage;
    }

    public PyDataType GetStation (CallInformation call, PyInteger stationID)
    {
        // generate cache for this call, why is this being called for every item in the assets window
        // when a list is expanded?!

        if (CacheStorage.Exists ("stationSvc", $"GetStation_{stationID}") == false)
            CacheStorage.StoreCall (
                "stationSvc", $"GetStation_{stationID}",
                this.Items.Stations [stationID].GetStationInfo (),
                DateTime.UtcNow.ToFileTimeUtc ()
            );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("stationSvc", $"GetStation_{stationID}"));
    }

    public PyDataType GetSolarSystem (CallInformation call, PyInteger solarSystemID)
    {
        return this.Items.SolarSystems [solarSystemID].GetSolarSystemInfo ();
    }
}