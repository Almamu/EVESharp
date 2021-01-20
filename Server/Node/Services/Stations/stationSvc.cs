using Common.Database;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Stations
{
    public class stationSvc : Service
    {
        private ItemManager ItemManager { get; }
        public stationSvc(ItemManager itemManager)
        {
            this.ItemManager = itemManager;
        }

        public PyDataType GetStation(PyInteger stationID, CallInformation call)
        {
            return this.ItemManager.Stations[stationID].GetStationInfo();
        }
    }
}