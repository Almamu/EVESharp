using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Proxy.Database
{
    public static class GeneralDB
    {
        public static void ResetSolarSystemStatus()
        {
            Database.Query("UPDATE solarsystemsloaded SET nodeID=0");
        }

        public static void ResetSolarSystemStatus(int solarSystemID)
        {
            Database.Query("UPDATE solarsystemsloaded SET nodeID=0 WHERE solarSystemID=" + solarSystemID);
        }

        public static void ResetItemsStatus()
        {
            Database.Query("UPDATE entity SET nodeID=0");
        }
    }
}
