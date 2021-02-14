using System.Collections.Generic;
using Common.Services;
using Node.Data;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Navigation
{
    public class map : Service
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

        public PyDataType GetStationExtraInfo(CallInformation call)
        {
            Rowset stations = new Rowset(new PyDataType []
            {
                "stationID", "solarSystemID", "operationID", "stationTypeID", "ownerID"
            });
            Rowset operationServices = new Rowset(new PyDataType[]
            {
                "operationID", "serviceID"
            });
            Rowset services = new Rowset(new PyDataType[]
            {
                "serviceID", "serviceName"
            });
            
            foreach (KeyValuePair<int, Station> pair in this.ItemManager.Stations)
                stations.Rows.Add((PyList) new PyDataType []
                {
                    pair.Value.ID, pair.Value.LocationID, pair.Value.Operations.OperationID, pair.Value.StationType.ID, pair.Value.OwnerID
                });

            foreach (KeyValuePair<int, StationOperations> pair in this.StationManager.Operations)
                foreach (int serviceID in pair.Value.Services)
                    operationServices.Rows.Add((PyList) new PyDataType[]
                    {
                        pair.Value.OperationID, serviceID
                    });

            foreach (KeyValuePair<int, string> pair in this.StationManager.Services)
                services.Rows.Add((PyList) new PyDataType[]
                {
                    pair.Key, pair.Value
                });
            
            return new PyTuple(new PyDataType[]
            {
                stations, operationServices, services
            });
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
                new PyDataType [] { "fromID", "rank" }
            );
        }

        public PyDataType GetStuckSystems(CallInformation call)
        {
            return new PyDictionary();
        }
    }
}