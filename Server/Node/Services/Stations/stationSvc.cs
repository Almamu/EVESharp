using Common.Services;
using Node.Inventory;
using Node.Network;
using PythonTypes.Types.Primitives;

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

        public PyDataType GetSolarSystem(PyInteger solarSystemID, CallInformation call)
        {
            return this.ItemManager.SolarSystems[solarSystemID].GetSolarSystemInfo();
        }
    }
}