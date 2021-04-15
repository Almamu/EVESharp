using System;
using Common.Services;
using Node.Inventory;
using Node.Network;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Primitives;

namespace Node.Services.Stations
{
    public class stationSvc : IService
    {
        private ItemFactory ItemFactory { get; }
        private CacheStorage CacheStorage { get; }
        
        public stationSvc(ItemFactory itemFactory, CacheStorage cacheStorage)
        {
            this.ItemFactory = itemFactory;
            this.CacheStorage = cacheStorage;
        }

        public PyDataType GetStation(PyInteger stationID, CallInformation call)
        {
            // generate cache for this call, why is this being called for every item in the assets window
            // when a list is expanded?!

            if (this.CacheStorage.Exists("stationSvc", $"GetStation_{stationID}") == false)
            {
                this.CacheStorage.StoreCall(
                    "stationSvc", $"GetStation_{stationID}", 
                    this.ItemFactory.Stations[stationID].GetStationInfo(), 
                    DateTime.UtcNow.ToFileTimeUtc()
                );
            }

            return PyCacheMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("stationSvc", $"GetStation_{stationID}")
            );
        }

        public PyDataType GetSolarSystem(PyInteger solarSystemID, CallInformation call)
        {
            return this.ItemFactory.SolarSystems[solarSystemID].GetSolarSystemInfo();
        }
    }
}