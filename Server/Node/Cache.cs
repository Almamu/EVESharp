using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Marshal;
using Common;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Common.Packets;
using Common.Utils;

namespace EVESharp
{
    public static class Cache
    {
        public static string[] LoginCacheTable = new string[]
        {
        	"config.BulkData.ramactivities",
	        "config.BulkData.billtypes",
	        "config.Bloodlines",
	        "config.Units",
	        "config.BulkData.tickernames",
	        "config.BulkData.ramtyperequirements",
	        "config.BulkData.ramaltypesdetailpergroup",
	        "config.BulkData.ramaltypes",
	        "config.BulkData.allianceshortnames",
	        "config.BulkData.ramcompletedstatuses",
	        "config.BulkData.categories",
	        "config.BulkData.invtypereactions",
	        "config.BulkData.dgmtypeeffects",
	        "config.BulkData.metagroups",
	        "config.BulkData.ramtypematerials",
	        "config.BulkData.ramaltypesdetailpercategory",
	        "config.BulkData.owners",
	        "config.StaticOwners",
	        "config.Races",
	        "config.Attributes",
	        "config.BulkData.dgmtypeattribs",
	        "config.BulkData.locations",
	        "config.BulkData.locationwormholeclasses",
	        "config.BulkData.groups",
	        "config.BulkData.shiptypes",
	        "config.BulkData.dgmattribs",
	        "config.Flags",
	        "config.BulkData.bptypes",
	        "config.BulkData.graphics",
	        "config.BulkData.mapcelestialdescriptions",
	        "config.BulkData.certificates",
	        "config.StaticLocations",
	        "config.InvContrabandTypes",
	        "config.BulkData.certificaterelationships",
	        "config.BulkData.units",
	        "config.BulkData.dgmeffects",
	        "config.BulkData.types",
	        "config.BulkData.invmetatypes"
        };

        public static int LoginCacheTableSize = LoginCacheTable.Length;

        public static string[] CreateCharacterCacheTable = new string[] 
        {
          	"charCreationInfo.bl_eyebrows",
	        "charCreationInfo.bl_eyes",
	        "charCreationInfo.bl_decos",
	        "charCreationInfo.bloodlines",
	        "charCreationInfo.bl_hairs",
	        "charCreationInfo.bl_backgrounds",
	        "charCreationInfo.bl_accessories",
	        "charCreationInfo.bl_costumes",
	        "charCreationInfo.bl_lights",
	        "charCreationInfo.races",
	        "charCreationInfo.ancestries",
	        "charCreationInfo.schools",
	        "charCreationInfo.attributes",
	        "charCreationInfo.bl_beards",
	        "charCreationInfo.bl_skins",
	        "charCreationInfo.bl_lipsticks",
	        "charCreationInfo.bl_makeups"
        };

        public static int CreateCharacterCacheTableSize = CreateCharacterCacheTable.Length;

        public static string[] CharacterAppearanceCacheTable = new string[]
        {
           	"charCreationInfo.eyebrows",
	        "charCreationInfo.eyes",
	        "charCreationInfo.decos",
	        "charCreationInfo.hairs",
	        "charCreationInfo.backgrounds",
	        "charCreationInfo.accessories",
	        "charCreationInfo.costumes",
	        "charCreationInfo.lights",
	        "charCreationInfo.makeups",
	        "charCreationInfo.beards",
	        "charCreationInfo.skins",
	        "charCreationInfo.lipsticks"
        };

        public static int CharacterAppearanceCacheTableSize = CharacterAppearanceCacheTable.Length;

        private static Dictionary<string, PyTuple> cacheData = new Dictionary<string, PyTuple>();

        public static bool LoadCacheFor(string name)
        {
            return LoadCacheFor(name, false);
        }

        public static bool UpdateCache(string name)
        {
            return LoadCacheFor(name, true);
        }

        private static bool LoadCacheFor(string name, bool force)
        {
            // First search for the cache
            if (cacheData.ContainsKey(name) == true)
            {
                if (force == false)
                {
                    return true;
                }

                cacheData.Remove(name);
            }

            PyTuple data = Database.CacheDB.GetCacheData(name);

            if (data == null)
            {
                return false;
            }

            cacheData.Add(name, data);

            return true;
        }

        public static PyObject GetCache(string name)
        {
            if (LoadCacheFor(name) == false)
            {
                return null;
            }

            PyTuple info = cacheData[name];

            PyCachedObject obj = new PyCachedObject();
            obj.nodeID = info.Items[2].As<PyInt>().Value;
            obj.objectID = new PyString(name);
            obj.shared = 0;
            obj.compressed = 0;
            obj.cache = info.Items[0].As<PyBuffer>();
            obj.timestamp = info.Items[1].As<PyLongLong>().Value;
            obj.version = info.Items[3].As<PyInt>().Value;

            return obj.Encode();
        }

        public static PyObject GetCacheData(string name)
        {
            if (LoadCacheFor(name) == false)
            {
                Log.Error("Cache", "Cannot load cache data for cache " + name);
                return null;
            }

            PyTuple cache = cacheData[name];

            if (cache == null)
            {
                return null;
            }

            CacheInfo data = new CacheInfo();
            data.objectID = new PyString(name);
            data.cacheTime = cache.Items[1].As<PyLongLong>().Value;
            data.nodeID = cache.Items[2].As<PyInt>().Value;
            data.version = cache.Items[3].As<PyInt>().Value; // This is a CRC of the buffer

            return data.Encode();
        }

        public static bool SaveCacheFor(string name, PyObject data, long timestamp)
        {
            byte[] marshaled = Marshal.Marshal.Process(data);

            if (Database.CacheDB.SaveCacheData(name, marshaled, timestamp) == false)
            {
                Log.Error("Cache", "Cannot save cache dat for " + name);
                return false;
            }

            Log.Debug("Cache", "Saved cache data for " + name);

            if (UpdateCache(name) == false)
            {
                Log.Error("Cache", "Cannot update local cache info");
                return false;
            }

            return true;
        }

        public static bool GenerateCache()
        {
            // Here we will generate all the needed cache data
            MySqlDataReader reader = null;
            if (Database.Database.Query(ref reader, "SELECT activityID, activityName, iconNo, description, published FROM ramactivities") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramactivities");
                return false;
            }

            SaveCacheFor(LoginCacheTable[0], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT billTypeID, billTypeName, description FROM billtypes") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for billtypes");
                return false;
            }

            SaveCacheFor(LoginCacheTable[1], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT bloodlineID, bloodlineName, raceID, description, maleDescription, femaleDescription, shipTypeID, corporationID, perception, willpower, charisma, memory, intelligence, graphicID, shortDescription, shortMaleDescription, shortFemaleDescription, 0 AS dataID FROM chrBloodlines") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for bloodlines");
                return false;
            }

            SaveCacheFor(LoginCacheTable[2], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT unitID, unitName, displayName FROM eveUnits") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for units");
                return false;
            }

            SaveCacheFor(LoginCacheTable[3], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            // TODO: Change the table
            if (Database.Database.Query(ref reader, "SELECT corporationID, tickerName, shape1, shape2, shape3, color1, color2, color3 FROM corporation WHERE hasPlayerPersonnelManager = 0") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for tickernames");
                return false;
            }

            SaveCacheFor(LoginCacheTable[4], DBUtils.DBResultToTupleSet(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT typeID, activityID, requiredTypeID, quantity, damagePerJob, recycle FROM typeActivityMaterials WHERE damagePerJob != 1.0 OR recycle = 1") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramtyperequirements");
                return false;
            }

            SaveCacheFor(LoginCacheTable[5], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT a.assemblyLineTypeID, b.activityID, a.groupID, a.timeMultiplier, a.materialMultiplier FROM ramAssemblyLineTypeDetailPerGroup AS a LEFT JOIN ramAssemblyLineTypes AS b ON a.assemblyLineTypeID = b.assemblyLineTypeID") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramaltypesdetailpergroup");
                return false;
            }

            SaveCacheFor(LoginCacheTable[6], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT assemblyLineTypeID, assemblyLineTypeName, assemblyLineTypeName AS typeName, description, activityID, baseTimeMultiplier, baseMaterialMultiplier, volume FROM ramAssemblyLineTypes") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramaltypes");
                return false;
            }

            SaveCacheFor(LoginCacheTable[7], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT allianceID, shortName FROM allianceshortnames") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for alliance_shortnames");
                return false;
            }

            SaveCacheFor(LoginCacheTable[8], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT completedStatusID, completedStatusName, completedStatusText FROM ramCompletedStatuses") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramcompletedstatuses");
                return false;
            }

            SaveCacheFor(LoginCacheTable[8], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT categoryID, categoryName, description, graphicID, published, 0 AS dataID FROM invCategories") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for invCategories");
                return false;
            }

            SaveCacheFor(LoginCacheTable[9], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT reactionTypeID, input, typeID, quantity FROM invTypeReactions") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for invTypeReactions");
                return false;
            }
            SaveCacheFor(LoginCacheTable[10], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT typeID, effectID, isDefault FROM dgmTypeEffects") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for dgmTypeEffects");
                return false;
            }

            SaveCacheFor(LoginCacheTable[11], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT metaGroupID, metaGroupName, description, graphicID, 0 AS dataID FROM invMetaGroups") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for invMetaGroups");
                return false;
            }

            SaveCacheFor(LoginCacheTable[12], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT typeID, requiredTypeID AS materialTypeID, quantity FROM typeActivityMaterials WHERE activityID = 6 AND damagePerJob = 1.0 UNION SELECT productTypeID AS typeID, requiredTypeID AS materialTypeID, quantity FROM typeActivityMaterials JOIN invBlueprintTypes ON typeID = blueprintTypeID WHERE activityID = 1 AND damagePerJob = 1.0") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramTypeMaterials");
                return false;
            }

            SaveCacheFor(LoginCacheTable[13], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT a.assemblyLineTypeID, b.activityID, a.categoryID, a.timeMultiplier, a.materialMultiplier FROM ramAssemblyLineTypeDetailPerCategory AS a LEFT JOIN ramAssemblyLineTypes AS b ON a.assemblyLineTypeID = b.assemblyLineTypeID") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for ramaltypesdetailpercategory");
                return false;
            }

            SaveCacheFor(LoginCacheTable[14], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT cacheOwner AS ownerID, cacheOwnerName AS ownerName, cacheOwnerType AS typeID FROM usercache") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for cacheowner");
                return false;
            }

            SaveCacheFor(LoginCacheTable[15], DBUtils.DBResultToRowset(ref reader), DateTime.Now.ToFileTime());

            if (Database.Database.Query(ref reader, "SELECT ownerID, ownerName, typeID FROM eveStaticOwners") == false)
            {
                Log.Error("Cache", "Cannot generate cache data for staticowners");
                return false;
            }
            /*
	        "config.StaticOwners",
	        "config.Races",
	        "config.Attributes",
	        "config.BulkData.dgmtypeattribs",
	        "config.BulkData.locations",
	        "config.BulkData.locationwormholeclasses",
	        "config.BulkData.groups",
	        "config.BulkData.shiptypes",
	        "config.BulkData.dgmattribs",
	        "config.Flags",
	        "config.BulkData.bptypes",
	        "config.BulkData.graphics",
	        "config.BulkData.mapcelestialdescriptions",
	        "config.BulkData.certificates",
	        "config.StaticLocations",
	        "config.InvContrabandTypes",
	        "config.BulkData.certificaterelationships",
	        "config.BulkData.units",
	        "config.BulkData.dgmeffects",
	        "config.BulkData.types",
	        "config.BulkData.invmetatypes"*/
            return true;
        }
    }
}
