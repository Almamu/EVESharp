using System.Collections.Generic;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Sessions
{
    public class Session : PyDictionary<PyString,PyDataType>
    {
        /// <summary>
        /// List of nodes that are "interested" in this session
        /// (nodes where a bound instance is for this client, etc)
        /// </summary>
        public PyList<PyInteger> NodesOfInterest { get; } = new PyList<PyInteger>();

        /// <summary>
        /// List of bound objects that are assigned to this session
        /// </summary>
        public Dictionary<int, long> BoundObjects { get; } = new Dictionary<int, long>();

        public static Session FromPyDictionary(PyDictionary origin)
        {
            Session session = new Session();
            
            foreach((PyDataType key, PyDataType value) in origin)
                if (key is PyString)
                    session[key] = value;

            return session;
        }
        
        public static Session FromPyDictionary(PyDictionary<PyString,PyDataType> origin)
        {
            Session session = new Session();
            
            foreach((PyString key, PyDataType value) in origin)
                    session[key] = value;

            return session;
        }

        public void ApplyDelta(PyDictionary<PyString, PyTuple> delta)
        {
            foreach ((PyString key, PyTuple value) in delta)
                this[key] = value[1];
        }

        public const string LANGUAGEID = "languageID";
        public const string USER_TYPE = "userType";
        public const string USERID = "userid";
        public const string ROLE = "role";
        public const string ADDRESS = "address";
        public const string CHAR_ID = "charid";
        public const string CORP_ID = "corpid";
        public const string CORP_ACCOUNT_KEY = "corpAccountKey";
        public const string SOLAR_SYSTEM_ID2 = "solarsystemid2";
        public const string CONSTELLATION_ID = "constellationid";
        public const string REGION_ID = "regionid";
        public const string HQ_ID = "hqID";
        public const string CORP_ROLE = "corprole";
        public const string ROLES_AT_ALL = "rolesAtAll";
        public const string ROLES_AT_BASE = "rolesAtBase";
        public const string ROLES_AT_HQ = "rolesAtHQ";
        public const string ROLES_AT_OTHER = "rolesAtOther";
        public const string SHIP_ID = "shipid";
        public const string STATION_ID = "stationid";
        public const string SOLAR_SYSTEM_ID = "solarsystemid";
        public const string LOCATION_ID = "locationid";
        public const string ALLIANCE_ID = "allianceid";
        public const string WARFACTION_ID = "warfactionid";
        public const string RACE_ID = "raceID";
        public const string NODE_ID = "nodeid";
        public const string LOAD_METRIC = "loadMetric";

        public long NodeID { get => this[NODE_ID] as PyInteger ?? 0; set => this[NODE_ID] = value; }
        public long LoadMetric { get => this[LOAD_METRIC] as PyInteger ?? 0; set => this[LOAD_METRIC] = value; }
        public string LanguageID { get => this[LANGUAGEID] as PyString; set => this[LANGUAGEID] = value; }
        public int UserID { get => this[USERID] as PyInteger; set => this[USERID] = value; }
        public int UserType { get => this[USER_TYPE] as PyInteger; set => this[USER_TYPE] = value; }
        public ulong Role { get => this[ROLE] as PyInteger; set => this[ROLE] = value; }
        public string Address { get => this[ADDRESS] as PyString; set => this[ADDRESS] = value; }
        public int? CharacterID { get => this[CHAR_ID] as PyInteger; set => this[CHAR_ID] = value; }
        public int CorporationID { get => this[CORP_ID] as PyInteger; set => this[CORP_ID] = value; }
        public int CorpAccountKey { get => this[CORP_ACCOUNT_KEY] as PyInteger ?? WalletKeys.MAIN_WALLET; set => this[CORP_ACCOUNT_KEY] = value; }
        public int SolarSystemID2 { get => this[SOLAR_SYSTEM_ID2] as PyInteger; set => this[SOLAR_SYSTEM_ID2] = value; }
        public int ConstellationID { get => this[CONSTELLATION_ID] as PyInteger; set => this[CONSTELLATION_ID] = value; }
        public int RegionID { get => this[REGION_ID] as PyInteger; set => this[REGION_ID] = value; }
        public int HQID { get => this[HQ_ID] as PyInteger; set => this[HQ_ID] = value; }
        public long CorporationRole { get => this[CORP_ROLE] as PyInteger; set => this[CORP_ROLE] = value; }
        public long RolesAtAll { get => this[ROLES_AT_ALL] as PyInteger; set => this[ROLES_AT_ALL] = value; }
        public long RolesAtBase { get => this[ROLES_AT_BASE] as PyInteger; set => this[ROLES_AT_BASE] = value; }
        public long RolesAtHQ { get => this[ROLES_AT_HQ] as PyInteger; set => this[ROLES_AT_HQ] = value; }
        public long RolesAtOther { get => this[ROLES_AT_OTHER] as PyInteger; set => this[ROLES_AT_OTHER] = value; }
        public int? ShipID { get => this[SHIP_ID] as PyInteger; set => this[SHIP_ID] = value; }

        public int? StationID
        {
            get => this[STATION_ID] as PyInteger;
            set
            {
                this[STATION_ID] = value;
                this[SOLAR_SYSTEM_ID] = null;
                this[LOCATION_ID] = value;
            }
        }

        public int? SolarSystemID
        {
            get => this[SOLAR_SYSTEM_ID] as PyInteger;
            set
            {
                this[SOLAR_SYSTEM_ID] = value;
                this[STATION_ID] = null;
                this[LOCATION_ID] = value;
            }
        }

        public int LocationID {get => this[LOCATION_ID] as PyInteger; set => this[LOCATION_ID] = value; }
        public int? AllianceID { get => this[ALLIANCE_ID] as PyInteger; set =>  this[ALLIANCE_ID] = value; }
        public int? WarFactionID { get => this[WARFACTION_ID] as PyInteger; set =>  this[WARFACTION_ID] = value; }
        public int? RaceID { get => this[RACE_ID] as PyInteger; set =>  this[RACE_ID] = value; }

        public PyDataType this[PyString index]
        {
            get => this.TryGetValue(index, out PyDataType result) == false ? null : result;
            set => base[index] = value;
        }
    }
}