using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using Marshal;

using MySql.Data.MySqlClient;
using MySql.Data.Types;

using Common.Utils;
using Common;

namespace EVESharp.Database
{
    public static class CacheDB
    {

        public static PyTuple GetUserCacheData(int user, string name)
        {
            MySqlDataReader res = null;

            if (Database.Query(ref res, "SELECT cacheData, cacheTime, nodeID, version FROM usercache WHERE cacheType='" + name + "' AND cacheOwner=" + user) == false)
            {
                return null;
            }

            if (res == null)
            {
                return null;
            }

            if (res.Read() == false)
            {
                return null;
            }

            byte[] outb = null;

            try
            {
                outb = (byte[])res.GetValue(0);
            }
            catch (Exception)
            {
                Log.Error("CacheDB", "Cannot get usercache data for cache " + name);
                res.Close();
                return null;
            }

            PyTuple tup = new PyTuple();
            tup.Items.Add(new PyBuffer(outb));
            tup.Items.Add(new PyLongLong(res.GetInt64(1)));
            tup.Items.Add(new PyIntegerVar(res.GetInt32(2)));
            tup.Items.Add(new PyIntegerVar(res.GetUInt32(3)));

            res.Close();

            return tup;
        }

        public static bool SaveUserCacheData(int user, string cacheName, byte[] data, long cacheTime, string username, int ownerType)
        {
            // First of all convert the cache data into a string
            char[] cData = new char[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                cData[i] = (char)data[i];
            }

            string cacheData = new string(cData);

            uint version = Crc32.Checksum(data);

            string query = "DELETE FROM usercache WHERE cacheType='" + cacheName + "'";
            Database.Query(query);
            query = "INSERT INTO usercache(cacheType, cacheOwner, cacheOwnerName, cacheOwnerType, cacheData, cacheTime, nodeID, version)VALUES('" + cacheName + "', " + user + ", '" + username + "', " + ownerType + ", '" + cacheData + "', " + cacheTime + ", " + Program.NodeID + ", " + version + ");";

            if (Database.Query(query) == false)
            {
                Log.Error("CacheDB", "Cannot insert cache data for cache " + cacheName);
                return false;
            }

            return true;
        }

        public static bool SaveCacheData(string cacheName, byte[] data, long cacheTime)
        {
            // First of all convert the cache data into a string
            char[] cData = Encoding.ASCII.GetChars(data);

            string cacheData = new string(cData);

            uint version = Crc32.Checksum(data);

            string query = "DELETE FROM cacheinfo WHERE cacheName='" + cacheName + "'";
            Database.Query(query);
            query = "INSERT INTO cacheinfo(cacheName, cacheData, cacheTime, nodeID, version)VALUES('" + cacheName + "', '" + cacheData + "', " + cacheTime + ", " + Program.NodeID + ", " + version + ");";
            
            if (Database.Query(query) == false)
            {
                Log.Error("CacheDB", "Cannot insert cache data for cache " + cacheName);
                return false;
            }

            return true;
        }

        public static PyTuple GetCacheData(string name)
        {
            MySqlDataReader res = null;

            if (Database.Query(ref res, "SELECT cacheData, cacheTime, nodeID, version FROM cacheinfo WHERE cacheName='" + name + "';") == false)
            {
                return null;
            }

            if (res == null)
            {
                return null;
            }

            if (res.Read() == false)
            {
                res.Close();
                return null;
            }

            byte[] outb = null;

            try
            {
                outb = (byte[])res.GetValue(0);
            }
            catch (Exception)
            {
                Log.Error("CacheDB", "Cannot get cache data for cache " + name);
                res.Close();
                return null;
            }
            
            PyTuple tup = new PyTuple();
            tup.Items.Add(new PyBuffer(outb));
            tup.Items.Add(new PyLongLong(res.GetInt64(1)));
            tup.Items.Add(new PyIntegerVar(res.GetInt32(2)));
            tup.Items.Add(new PyIntegerVar(res.GetUInt32(3)));

            res.Close();

            return tup;
        }
    }
}
