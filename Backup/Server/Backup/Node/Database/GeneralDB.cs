using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using Common;

namespace EVESharp.Database
{
    public static class GeneralDB
    {
        public static List<int> GetUnloadedSolarSystems()
        {
            MySqlDataReader res = null;

            if (Database.Query(ref res, "SELECT solarSystemID FROM solarsystemsloaded WHERE nodeID=0") == false)
            {
                return new List<int>();
            }

            if (res == null)
            {
                return new List<int>();
            }

            List<int> result = new List<int>();

            while (res.Read())
            {
                result.Add(res.GetInt32(0));
            }

            res.Close();

            return result;
        }

        public static bool LoadSolarSystem(int solarSystemID)
        {
            if (Database.Query("UPDATE solarsystemsloaded SET nodeID = " + Program.GetNodeID() + " WHERE solarSystemID = " + solarSystemID) == false)
            {
                Log.Error("GeneralDB", "Cannot change solarSystem " + solarSystemID + " status to loaded");
                return false;
            }

            return true;
        }

        public static void UnloadSolarSystem(int solarSystemID)
        {
            Database.Query("UPDATE solarsystemsloaded SET nodeID=0 WHERE solarSystemID=" + solarSystemID);
        }
    }
}
