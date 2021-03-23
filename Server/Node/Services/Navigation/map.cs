using System.Collections.Generic;
using Common.Services;
using Node.Data;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Navigation
{
    public class map : IService
    {
        private ItemManager ItemManager { get; }
        private StationManager StationManager { get; }
        private CacheStorage CacheStorage { get; }
        public map(ItemManager itemManager, StationManager stationManager, CacheStorage cacheStorage)
        {
            this.ItemManager = itemManager;
            this.StationManager = stationManager;
            this.CacheStorage = cacheStorage;
        }

        public PyTuple GetStationExtraInfo(CallInformation call)
        {
            Rowset stations = new Rowset(new PyList(5)
            {
                [0] = "stationID",
                [1] = "solarSystemID",
                [2] = "operationID",
                [3] = "stationTypeID",
                [4] = "ownerID"
            });
            Rowset operationServices = new Rowset(new PyList(2)
            {
                [0] = "operationID",
                [1] = "serviceID"
            });
            Rowset services = new Rowset(new PyList(2)
            {
                [0] = "serviceID",
                [1] = "serviceName"
            });
            
            foreach ((int _, Station station) in this.ItemManager.Stations)
                stations.Rows.Add(new PyList(5)
                {
                    [0] = station.ID,
                    [1] = station.LocationID,
                    [2] = station.Operations.OperationID,
                    [3] = station.StationType.ID,
                    [4] = station.OwnerID
                });

            foreach ((int _, StationOperations operation) in this.StationManager.Operations)
                foreach (int serviceID in operation.Services)
                    operationServices.Rows.Add(new PyList(2)
                    {
                        [0] = operation.OperationID,
                        [1] = serviceID
                    });

            foreach ((int serviceID, string name) in this.StationManager.Services)
                services.Rows.Add(new PyList(2)
                {
                    [0] = serviceID,
                    [1] = name
                });
            
            return new PyTuple(3)
            {
                [0] = stations,
                [1] = operationServices,
                [2] = services
            };
        }

        public PyDataType GetSolarSystemPseudoSecurities(CallInformation call)
        {
            this.CacheStorage.Load(
                "map",
                "GetSolarSystemPseudoSecurities",
                "SELECT solarSystemID, security FROM mapSolarSystems",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("map", "GetSolarSystemPseudoSecurities");

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }

        // TODO: PROPERLY IMPLEMENT THIS ONE
        public PyDataType GetMyExtraMapInfoAgents(CallInformation call)
        {
            return new Rowset(
                new PyList(2)
                {
                    [0] = "fromID",
                    [1] = "rank"
                }
            );
        }

        public PyDictionary GetStuckSystems(CallInformation call)
        {
            return new PyDictionary();
        }
    }
}