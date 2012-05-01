using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Common;

namespace CacheTool.Database
{
    public static class Database
    {
        private static MySqlConnection connection;
        private static Queue<string> queryQueue = new Queue<string>();
        public static bool Init()
        {
            string connStr = String.Format("server={0};user id={1}; password={2}; database=eve-node; pooling=false",
                "localhost", "Almamu", "966772320");

            try
            {
                connection = new MySqlConnection(connStr);
                connection.Open();
            }
            catch (MySqlException)
            {
                WindowLog.Error("Database", "Cannot connect to server database");
                return false;
            }

            return true;
        }

        public static bool QueryLID(ref ulong id, string query)
        {
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader reader = null;

            try
            {
                reader = cmd.ExecuteReader();
                id = (ulong)reader.FieldCount; // Maybe not the best to use, but should do the trick
            }
            catch (MySqlException ex)
            {
                WindowLog.Error("Database", "MySQL Error: " + ex.Message);
                return false;
            }
            finally
            {
                if (reader != null) reader.Close();
            }

            return true;
        }

        public static bool Query(string query)
        {
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (MySqlException ex)
            {
                WindowLog.Error("Database", "MySQL Error: " + ex.Message);
                return false;
            }
            finally
            {
                if (reader != null) reader.Close();
            }

            return true;
        }

        public static bool Query(ref MySqlDataReader res, string query)
        {
            MySqlCommand cmd = new MySqlCommand(query, connection);

            try
            {
                res = cmd.ExecuteReader();
            }
            catch (MySqlException ex)
            {
                WindowLog.Error("Database", "MySQL Error: " + ex.Message);
                return false;
            }

            return true;
        }

        public static void QueueQuery(string query)
        {
            queryQueue.Enqueue(query);
        }

        public static void Update()
        {
            WindowLog.Debug("Database", "Saving information into the DB");

            // Start the querys
            string query = "";
            while (queryQueue.Count > 0)
            {
                query = queryQueue.Dequeue();
                Query(query);
            }
        }

        public static void Stop()
        {
            connection.Close();
        }
    }
}
