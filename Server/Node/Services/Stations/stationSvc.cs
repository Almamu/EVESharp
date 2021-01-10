using Common.Database;
using Node.Database;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Stations
{
    public class stationSvc : Service
    {
        private StationDB mDB = null;
        
        public stationSvc(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new StationDB(db);
        }

        public PyDataType GetStation(PyInteger stationID, PyDictionary namedPayload, Client client)
        {
            return this.ServiceManager.Container.ItemFactory.ItemManager.Stations[stationID].GetStationInfo();
        }
    }
}