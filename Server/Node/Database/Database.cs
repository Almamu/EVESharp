/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using Common;

namespace EVESharp.Database
{
    public static class Database
    {
        private static MySqlConnection connection;
        private static Queue<string> queryQueue = new Queue<string>();
        public static string Username = "root";
        public static string Password = "root";
        public static string Host = "localhost";
        public static string DB = "eve";

        public static bool Init()
        {
            string connStr = String.Format("server={0};user id={1}; password={2}; database={3}; pooling=false",
                Host, Username, Password, DB);

            try
            {
                connection = new MySqlConnection(connStr);
                connection.Open();
            }
            catch (MySqlException)
            {
                Log.Error("Database", "Cannot connect to server database");
                return false;
            }

            return true;
        }

        public static string DoEscapeString(string input)
        {
            return MySqlHelper.EscapeString(input);
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
                Log.Error("Database", "MySQL Error: " + ex.Message);
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
                Log.Error("Database", "MySQL Error: " + ex.Message);
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
                Log.Error("Database", "MySQL Error: " + ex.Message);
                return false;
            }

            return true;
        }

        public static void QueueQuery(string query)
        {
            lock (queryQueue)
            {
                queryQueue.Enqueue(query);
            }
        }

        public static void Update()
        {
            Log.Debug("Database", "Saving information into the DB");

            lock (queryQueue)
            {
                // Start the querys
                while (queryQueue.Count > 0)
                {
                    string query = queryQueue.Dequeue();
                    Query(query);
                }
            }
        }

        public static void Stop()
        {
            connection.Close();
        }
    }
}
