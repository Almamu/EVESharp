using Common.Database;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Config
{
    public class config : Service
    {
        private ConfigDB DB { get; }
        
        public config(ConfigDB db)
        {
            this.DB = db;
        }

        public PyDataType GetMultiOwnersEx(PyList ids, CallInformation call)
        {
            // return item data from the entity table and call it a day
            return this.DB.GetMultiOwnersEx(ids);
        }

        public PyDataType GetMultiGraphicsEx(PyList ids, CallInformation call)
        {
            return this.DB.GetMultiGraphicsEx(ids);
        }

        public PyDataType GetMultiLocationsEx(PyList ids, CallInformation call)
        {
            return this.DB.GetMultiLocationsEx(ids);
        }

        public PyDataType GetMultiAllianceShortNamesEx(PyList ids, CallInformation call)
        {
            return this.DB.GetMultiAllianceShortNamesEx(ids);
        }

        public PyDataType GetMap(PyInteger solarSystemID, CallInformation call)
        {
            return this.DB.GetMap(solarSystemID);
        }

        // THESE PARAMETERS AREN'T REALLY USED ANYMORE, THIS FUNCTION IS USUALLY CALLED WITH LOCATIONID, 1
        public PyDataType GetMapObjects(PyInteger locationID, PyInteger ignored1, CallInformation call)
        {
            return this.DB.GetMapObjects(locationID);
        }

        // THESE PARAMETERS AREN'T REALLY USED ANYMORE THIS FUNCTION IS USUALLY CALLED WITH LOCATIONID, 0, 0, 0, 1, 0
        public PyDataType GetMapObjects(PyInteger locationID, PyInteger wantRegions, PyInteger wantConstellations,
            PyInteger wantSystems, PyInteger wantItems, PyInteger unknown)
        {
            return this.DB.GetMapObjects(locationID);
        }

        public PyDataType GetMapOffices(PyInteger solarSystemID, CallInformation call)
        {
            return this.DB.GetMapOffices(solarSystemID);
        }

        public PyDataType GetCelestialStatistic(PyInteger celestialID, CallInformation call)
        {
            if (ItemManager.IsCelestialID(celestialID) == false)
                throw new CustomError($"Unexpected celestialID {celestialID}");
            
            // TODO: CHECK FOR STATIC DATA TO FETCH IT OFF MEMORY INSTEAD OF DATABASE?
            return this.DB.GetCelestialStatistic(celestialID);
        }
    }
}