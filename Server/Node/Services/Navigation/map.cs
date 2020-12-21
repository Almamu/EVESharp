using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Node.Data;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Navigation
{
    public class map : Service
    {
        public map(ServiceManager manager) : base(manager)
        {
        }

        public PyDataType GetStationExtraInfo(PyDictionary namedPayload, Client client)
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
            
            foreach (KeyValuePair<int, Station> pair in this.ServiceManager.Container.ItemFactory.ItemManager.Stations)
                stations.Rows.Add((PyList) new PyDataType []
                {
                    pair.Value.ID, pair.Value.LocationID, pair.Value.Operations.OperationID, pair.Value.StationType.ID, pair.Value.OwnerID
                });

            foreach (KeyValuePair<int, StationOperations> pair in this.ServiceManager.Container.ItemFactory.StationManager.Operations)
                foreach (int serviceID in pair.Value.Services)
                    operationServices.Rows.Add((PyList) new PyDataType[]
                    {
                        pair.Value.OperationID, serviceID
                    });

            foreach (KeyValuePair<int, string> pair in this.ServiceManager.Container.ItemFactory.StationManager.Services)
                services.Rows.Add((PyList) new PyDataType[]
                {
                    pair.Key, pair.Value
                });
            
            return new PyTuple(new PyDataType[]
            {
                stations, operationServices, services
            });
        }
    }
}