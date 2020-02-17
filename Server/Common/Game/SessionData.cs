using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PythonTypes;

namespace Common.Game
{
    public class SessionData
    {
        public enum SessionNames
        {
            userType,
            userid,
            address,
            role,
            laguageID,
            constellationid,
            corpid,
            regionid,
            stationid,
            solarsystemid,
            locationid,
            gangrole,
            hqID,
            solarsystemid2,
            shipid,
            charid,
            corprole,
            rolesAtAll,
            rolesAtBase,
            rolesAtHQ,
            rolesAtOther
        };

        static public string[] sessionData = new string[]
        {
            "userType", "userid", "address", "role",
            "laguageID", "constellationid", "corpid",
            "regionid", "stationid", "solarsystemid",
            "locationid", "gangrole", "hqID", "solarsystemid2",
            "shipid", "charid", "corprole", "rolesAtAll",
            "rolesAtBase", "rolesAtHQ", "rolesAtOther"
        };
    }
}
