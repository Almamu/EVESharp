using System;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.Node.Cache;
using EVESharp.Node.Inventory;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Stations;

public class stationSvc : Service
{
    public override AccessLevel  AccessLevel  => AccessLevel.None;
    private         ItemFactory  ItemFactory  { get; }
    private         CacheStorage CacheStorage { get; }

    public stationSvc (ItemFactory itemFactory, CacheStorage cacheStorage)
    {
        ItemFactory  = itemFactory;
        CacheStorage = cacheStorage;
    }

    public PyDataType GetStation (PyInteger stationID, CallInformation call)
    {
        // generate cache for this call, why is this being called for every item in the assets window
        // when a list is expanded?!

        if (CacheStorage.Exists ("stationSvc", $"GetStation_{stationID}") == false)
            CacheStorage.StoreCall (
                "stationSvc", $"GetStation_{stationID}",
                ItemFactory.Stations [stationID].GetStationInfo (),
                DateTime.UtcNow.ToFileTimeUtc ()
            );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("stationSvc", $"GetStation_{stationID}"));
    }

    public PyDataType GetSolarSystem (PyInteger solarSystemID, CallInformation call)
    {
        return ItemFactory.SolarSystems [solarSystemID].GetSolarSystemInfo ();
    }
}