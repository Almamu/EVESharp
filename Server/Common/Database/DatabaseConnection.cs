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
using Common.Logging;
using MySql.Data.MySqlClient;

namespace Common.Database
{
    public class DatabaseConnection
    {
        private Channel Log { get; set; }
        private readonly string mConnectionString = "";
        private readonly Queue<string> mQueryQueue = new Queue<string>();

        public DatabaseConnection(string connectionString, Logger logger)
        {
            this.mConnectionString = connectionString;
            this.Log = logger.CreateLogChannel("Database");
        }

        ~DatabaseConnection()
        {
        }

        public ulong PrepareQueryLID(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                MySqlDataReader reader = this.PrepareQuery(ref connection, query, values);

                using (connection)
                using (reader)
                    return (ulong) reader.FieldCount;
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        public void Query(string query)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            try
            {
                connection = new MySqlConnection(this.mConnectionString);
                connection.Open();

                MySqlCommand command = new MySqlCommand(query, connection);

                reader = command.ExecuteReader();
            }
            catch (Exception e)
            {
                if (reader != null)
                    reader.Close();

                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }

        /// <summary>
        /// Creates a MySqlCommand with the given query to execute prepared queries
        /// </summary>
        /// <param name="connection">where to store the MySql connection (has to be closed manually)</param>
        /// <param name="query">The prepared query</param>
        /// <returns>The generated command to perform the queries agains the database</returns>
        public MySqlCommand PrepareQuery(ref MySqlConnection connection, string query)
        {
            try
            {
                connection = new MySqlConnection(this.mConnectionString);
                connection.Open();
                
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Prepare();

                return command;
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters
        /// </summary>
        /// <param name="connection">where to store the MySql connection (has to be closed manually)</param>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The reader with the results of the query</returns>
        public MySqlDataReader PrepareQuery(ref MySqlConnection connection, string query, Dictionary<string, object> values)
        {
            try
            {
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                foreach (KeyValuePair<string, object> pair in values)
                    command.Parameters.AddWithValue(pair.Key, pair.Value);
                
                // run the prepared statement
                return command.ExecuteReader();
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given value as parameters, ignoring the result data
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The number of rows affected</returns>
        public int PrepareQuery(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                using (connection)
                {
                    // add values
                    foreach (KeyValuePair<string, object> pair in values)
                        command.Parameters.AddWithValue(pair.Key, pair.Value);
                    // run the command
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        public MySqlDataReader Query(ref MySqlConnection connection, string query)
        {
            try
            {
                connection = new MySqlConnection(this.mConnectionString);
                connection.Open();

                MySqlCommand command = new MySqlCommand(query, connection);

                return command.ExecuteReader();
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        public void QueueQuery(string query)
        {
            lock (this.mQueryQueue)
            {
                this.mQueryQueue.Enqueue(query);
            }
        }

        public static DatabaseConnection FromConfiguration(Configuration.Database configuration, Logger logger)
        {
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder();

            stringBuilder.Server = configuration.Hostname;
            stringBuilder.Database = configuration.Name;
            stringBuilder.UserID = configuration.Username;
            stringBuilder.Password = configuration.Password;
            stringBuilder.MinimumPoolSize = 10;

            return new DatabaseConnection(stringBuilder.ToString(), logger);
        }
    }
}