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
        private ItemManager ItemManager { get; }
        private CacheStorage CacheStorage { get; }
        
        public stationSvc(ItemManager itemManager, CacheStorage cacheStorage)
        {
            this.ItemManager = itemManager;
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
                    this.ItemManager.Stations[stationID].GetStationInfo(), 
                    DateTime.UtcNow.ToFileTimeUtc()
                );
            }

            return PyCacheMethodCallResult.FromCacheHint(
                this.CacheStorage.GetHint("stationSvc", $"GetStation_{stationID}")
            );
        }

        public PyDataType GetSolarSystem(PyInteger solarSystemID, CallInformation call)
        {
            return this.ItemManager.SolarSystems[solarSystemID].GetSolarSystemInfo();
        }
    }
}