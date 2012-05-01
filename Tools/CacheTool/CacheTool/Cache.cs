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

namespace CacheTool
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
        private static Dictionary<int, Dictionary<string, PyTuple>> userCacheData = new Dictionary<int, Dictionary<string, PyTuple>>();

        /*
         *  User cache 
         */
        public static bool LoadUserCacheFor(int user, string name)
        {
            return LoadUserCacheFor(user, name, false);
        }

        public static bool UpdateUserCache(int user, string name)
        {
            return LoadUserCacheFor(user, name, true);
        }

        public static bool LoadUserCacheFor(int user, string name, bool force)
        {
            Dictionary<string, PyTuple> old = null;

            if (userCacheData.ContainsKey(user) == true)
            {
                if (userCacheData[user].ContainsKey(name) == true)
                {
                    if (force == false)
                    {
                        return true;
                    }

                    userCacheData[user].Remove(name);
                }

                if (force == false)
                {
                    return true;
                }

                // Store the actual cache data to reload only the required cacheName
                old = userCacheData[user];
                userCacheData.Remove(user);
            }

            // Get the cache data from the DB
            PyTuple data = Database.CacheDB.GetUserCacheData(user, name);

            if (data == null)
            {
                WindowLog.Error("Cache", "Cannot load cache data for user " + user + " of type " + name);
                return false;
            }

            // If the data wasnt loaded yet create an empty dictionary
            if (old == null)
            {
                old = new Dictionary<string, PyTuple>();
            }

            // Add the cache loaded into the dictionary
            old.Add(name, data);

            // Load the dictionary with all the cache data into the userCacheData Dictionary
            userCacheData.Add(user, old);

            return true;
        }

        public static PyObject GetUserCache(int user, string name)
        {
            if (LoadUserCacheFor(user, name) == false)
            {
                return null;
            }

            // We can assume this will not throw any exception as we've just checked if it exists
            PyTuple info = userCacheData[user][name];

            PyCachedObject obj = new PyCachedObject();
            obj.nodeID = info.Items[2].As<PyIntegerVar>().Value;
            obj.objectID = new PyString(name);
            obj.shared = 0;
            obj.compressed = 0;
            obj.cache = info.Items[0].As<PyBuffer>();
            obj.timestamp = info.Items[1].As<PyLongLong>().Value;
            obj.version = info.Items[3].As<PyIntegerVar>().Value;

            return obj.Encode();
        }

        public static PyObject GetUserCacheData(int user, string name)
        {
            if (LoadUserCacheFor(user, name) == false)
            {
                return null;
            }

            PyTuple cache = userCacheData[user][name];

            if (cache == null)
            {
                return null;
            }

            CacheInfo data = new CacheInfo();
            data.objectID = new PyString(name);
            data.cacheTime = cache.Items[1].As<PyLongLong>().Value;
            data.nodeID = cache.Items[2].As<PyIntegerVar>().Value;
            data.version = cache.Items[3].As<PyIntegerVar>().Value; // This is a CRC of the buffer

            return data.Encode();
        }

        public static bool SaveUserCacheFor(int user, string name, PyObject data, long timestamp, int ownerType, string ownerName, int fakeNodeID)
        {
            byte[] marshaled = Marshal.Marshal.Process(data);

            if (Database.CacheDB.SaveUserCacheData(user, name, marshaled, timestamp, ownerName, ownerType, fakeNodeID) == false)
            {
                WindowLog.Error("Cache", "Cannot save usercache data for " + name);
                return false;
            }

            WindowLog.Debug("Cache", "Saved usercache data for " + name);

            if (UpdateUserCache(user, name) == false)
            {
                WindowLog.Error("Cache", "Cannot update local usercache info");
                return false;
            }

            return true;
        }

        /*
         * General purpose server cache
         */
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
            WindowLog.Debug("Cache::LoadCacheFor", "Loading cache " + name);

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

        public static void UnloadCache(string name)
        {
            try
            {
                cacheData.Remove(name);
            }
            catch (Exception)
            {

            }
        }

        public static bool UpdateCacheName(string name, string newName)
        {
            WindowLog.Debug("Cache::UpdateCacheName", "Changin cache name...");

            if (Database.CacheDB.UpdateCacheName(name, newName) == false)
            {
                return false;
            }

            UnloadCache(name);
            return LoadCacheFor(name);
        }

        public static void DeleteCache(string name)
        {
            WindowLog.Debug("Cache::DeleteCache", "Deleting cache " + name);

            Database.CacheDB.DeleteCache(name);
        }

        public static PyObject GetCache(string name)
        {
            if (LoadCacheFor(name) == false)
            {
                return null;
            }

            PyTuple info = cacheData[name];

            PyCachedObject obj = new PyCachedObject();
            obj.nodeID = info.Items[2].As<PyIntegerVar>().Value;
            obj.objectID = new PyString(name);
            obj.shared = 0;
            obj.compressed = 0;
            obj.cache = info.Items[0].As<PyBuffer>();
            obj.timestamp = info.Items[1].As<PyLongLong>().Value;
            obj.version = info.Items[3].As<PyIntegerVar>().Value;

            return obj.Encode();
        }

        public static PyObject GetCacheData(string name)
        {
            if (LoadCacheFor(name) == false)
            {
                WindowLog.Error("Cache", "Cannot load cache data for cache " + name);
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
            data.nodeID = cache.Items[2].As<PyIntegerVar>().Value;
            data.version = cache.Items[3].As<PyIntegerVar>().Value; // This is a CRC of the buffer

            return data.Encode();
        }

        public static bool SaveCacheFor(string name, PyObject data, long timestamp, int fakeNodeID)
        {
            byte[] marshaled = Marshal.Marshal.Process(data);

            return SaveCacheFor(name, marshaled, timestamp, fakeNodeID);
        }

        public static bool SaveCacheFor(string name, byte[] data, long timestamp, int fakeNodeID)
        {
            if (Database.CacheDB.SaveCacheData(name, data, timestamp, fakeNodeID) == false)
            {
                WindowLog.Error("Cache", "Cannot save cache dat for " + name);
                return false;
            }

            WindowLog.Debug("Cache", "Saved cache data for " + name);

            if (UpdateCache(name) == false)
            {
                WindowLog.Error("Cache", "Cannot update local cache info");
                return false;
            }

            return true;
        }
    }
}