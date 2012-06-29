using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVESharp.Database;

namespace EVESharp.Inventory.SystemEntities
{
    public struct SolarSystemInfo
    {
        public int regionID;
        public int constellationID;
        public double x, y, z, xMin, yMin, zMin, xMax, yMax, zMax;
        public double luminosity;
        public bool border;
        public bool fringe;
        public bool corridor;
        public bool hub;
        public bool international;
        public bool regional;
        public bool constellation;
        public double security;
        public int factionID;
        public double radius;
        public int sunTypeID;
        public string securityClass;
    }

    public class SolarSystemLoadException : Exception
    {
        public SolarSystemLoadException()
            : base("Cannot load solar system from database")
        {

        }
    }
    
    public class SolarSystem : Inventory
    {
        public SolarSystem(Entity from)
            : base(from)
        {
            if (typeID != 5) // SolarSystems typeID
            {
                throw new Exception("Trying to load a non-solar system item like one");
            }

            solarSystemInfo = ItemDB.GetSolarSystemInfo(itemID);
        }

        public SolarSystem(Entity from, SolarSystemInfo info) : base(from)
        {
            if (typeID != 5)
            {
                throw new Exception("Trying to load a non-solar system item like one");
            }

            solarSystemInfo = info;
        }

        public SolarSystemInfo solarSystemInfo { private set; get; }
    }
}
