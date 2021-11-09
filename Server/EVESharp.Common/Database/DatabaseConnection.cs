/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
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
using System.Data;
using System.IO;
using EVESharp.Common.Logging;
using MySql.Data.MySqlClient;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Common.Database
{
    public class DatabaseConnection : IDatabaseConnection
    {
        public Dictionary<string, ColumnCharset> ColumnCharsets { get; init; } = new Dictionary<string, ColumnCharset>();
        private Channel Log { get; }
        private readonly string mConnectionString;

        public DatabaseConnection(Configuration.Database configuration, Logger logger)
        {
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = configuration.Hostname,
                Database = configuration.Name,
                UserID = configuration.Username,
                Password = configuration.Password,
                MinimumPoolSize = 10
            };
            
            this.mConnectionString = stringBuilder.ToString();
            this.Log = logger.CreateLogChannel("Database");
            this.FetchDatabaseColumnCharsets(configuration);
        }

        private void FetchDatabaseColumnCharsets(Configuration.Database configuration)
        {
            this.Log.Debug("Populating column information from database");
            
            // perform a query to the information_schema database
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = configuration.Hostname,
                Database = "information_schema",
                UserID = configuration.Username,
                Password = configuration.Password,
                MinimumPoolSize = 10
            };

            // establish a connection to the information_schema database
            MySqlConnection connection = new MySqlConnection(stringBuilder.ToString());

            connection.Open();

            using (connection)
            {
                MySqlCommand command = new MySqlCommand($"SELECT TABLE_NAME, COLUMN_NAME, CHARACTER_SET_NAME FROM COLUMNS WHERE TABLE_SCHEMA LIKE '{configuration.Name}' AND CHARACTER_SET_NAME IS NOT NULL", connection);
                MySqlDataReader reader = command.ExecuteReader();

                using (reader)
                {
                    // column information was fetched, store it somewhere so the Database Utils can use it
                    while (reader.Read() == true)
                    {
                        string tableName = reader.GetString(0);
                        string columnName = reader.GetString(1);
                        string charset = reader.GetString(2);
                        ColumnCharset value;

                        switch (charset)
                        {
                            default:
                                this.Log.Warning($"Unknown encoding {charset} for column {columnName} on table {tableName}, defaulting to utf8");
                                value = ColumnCharset.Wide;
                                break;
                            case "utf8":
                                value = ColumnCharset.Wide;
                                break;
                            case "ascii":
                            case "latin1":
                                value = ColumnCharset.Byte;
                                break;
                        }
                        
                        this.ColumnCharsets[$"{tableName}.{columnName}"] = value;
                    }
                }
            }

            this.Log.Debug("Column information populated properly");
        }

        public ulong PrepareQueryLID(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                using (connection)
                using (command)
                {
                    command.ExecuteNonQuery();

                    return (ulong) command.LastInsertedId;
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        public ulong PrepareQueryLID(ref MySqlConnection connection, string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                command.ExecuteNonQuery();

                return (ulong) command.LastInsertedId;
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        public void Query(string query)
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(this.mConnectionString);
                connection.Open();

                using (connection)
                {
                    MySqlCommand command = new MySqlCommand(query, connection);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        private void AddNamedParameters(Dictionary<string, object> parameters, MySqlCommand command)
        {
            foreach ((string parameterName, object value) in parameters)
                command.Parameters.AddWithValue(parameterName, value);
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
                // only open a connection if it's really needed
                if (connection == null)
                {
                    connection = new MySqlConnection(this.mConnectionString);
                    connection.Open();                    
                }
                
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
                this.AddNamedParameters(values, command);
                
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
        /// Runs one prepared query with the given values as parameters and returns a CRowset representing the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The Rowset object representing the result</returns>
        public CRowset PrepareCRowsetQuery(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return CRowset.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a CRowset representing the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <returns>The Rowset object representing the result</returns>
        public CRowset PrepareCRowsetQuery(string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return CRowset.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns an IndexRowset representing the result
        /// </summary>
        /// <param name="indexField">The position of the index field in the result</param>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The Rowset object representing the result</returns>
        public IndexRowset PrepareIndexRowsetQuery(int indexField, string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return IndexRowset.FromMySqlDataReader (this, reader, indexField);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns an IndexRowset representing the result
        /// </summary>
        /// <param name="indexField">The position of the index field in the result</param>
        /// <param name="query">The prepared query</param>
        /// <returns>The Rowset object representing the result</returns>
        public IndexRowset PrepareIndexRowsetQuery(int indexField, string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return IndexRowset.FromMySqlDataReader (this, reader, indexField);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a PyPackedRow representing the first result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The Rowset object representing the result</returns>
        public PyPackedRow PreparePackedRowQuery(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    if (reader.Read() == false)
                        return null;
                    
                    // run the prepared statement
                    return PyPackedRow.FromMySqlDataReader(reader, DBRowDescriptor.FromMySqlReader(this, reader));
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a Rowset representing the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The Rowset object representing the result</returns>
        public PyList<PyPackedRow> PreparePackedRowListQuery(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return PyPackedRowList.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a CRowset representing the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <returns>The Rowset object representing the result</returns>
        public PyDataType PreparePackedRowListQuery(string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return PyPackedRowList.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a Rowset representing the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The Rowset object representing the result</returns>
        public Rowset PrepareRowsetQuery(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return Rowset.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a CRowset representing the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <returns>The Rowset object representing the result</returns>
        public Rowset PrepareRowsetQuery(string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return Rowset.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a IntIntDictionary representing the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <returns>The Rowset object representing the result</returns>
        public PyDictionary<PyInteger,PyInteger> PrepareIntIntDictionary(string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return IntIntDictionary.FromMySqlDataReader(reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a IntIntListDictionary representing
        /// the result
        ///
        /// IMPORTANT: The first column must be ordered (direction doesn't matter) for this to properly work
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <returns>The Rowset object representing the result</returns>
        public PyDictionary<PyInteger,PyList<PyInteger>> PrepareIntIntListDictionary(string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return IntIntListDictionary.FromMySqlDataReader(reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a IntRowDictionary representing
        /// the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="keyColumnIndex">The column to use as index for the IntRowDictionary</param>
        /// <returns>The IntRowDictionary object representing the result</returns>
        public PyDictionary PrepareIntRowDictionary(string query, int keyColumnIndex)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return IntRowDictionary.FromMySqlDataReader(this, reader, keyColumnIndex);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a IntRowDictionary representing
        /// the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="keyColumnIndex">The column to use as key for the IntRowDictionary</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The IntRowDictionary object representing the result</returns>
        public PyDictionary PrepareIntRowDictionary(string query, int keyColumnIndex, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return IntRowDictionary.FromMySqlDataReader(this, reader, keyColumnIndex);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns an IntPackedRowListDictionary representing
        /// the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="keyColumnIndex">The column to use as key for the IntPackedRowListDictionary</param>
        /// <returns>The IntRowDictionary object representing the result</returns>
        public PyDataType PrepareIntPackedRowListDictionary(string query, int keyColumnIndex)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return IntPackedRowListDictionary.FromMySqlDataReader(this, reader, keyColumnIndex);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns an IntPackedRowListDictionary representing
        /// the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="keyColumnIndex">The column to use as key for the IntPackedRowListDictionary</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The IntRowDictionary object representing the result</returns>
        public PyDataType PrepareIntPackedRowListDictionary(string query, int keyColumnIndex, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return IntPackedRowListDictionary.FromMySqlDataReader(this, reader, keyColumnIndex);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a RowList representing
        /// the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <returns>The RowList object representing the result</returns>
        public PyDataType PrepareDictRowListQuery(string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return DictRowlist.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a RowList representing
        /// the result
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The RowList object representing the result</returns>
        public PyDataType PrepareDictRowListQuery(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);
                
                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return DictRowlist.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a KeyVal representing the result.
        /// KeyVals only hold ONE row
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <returns>The PyDataType object representing the result</returns>
        public PyDataType PrepareKeyValQuery(string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    if (reader.Read() == false)
                        return null;
                    
                    // run the prepared statement
                    return KeyVal.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a KeyVal representing the result.
        /// KeyVals only hold ONE row
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The PyDataType object representing the result</returns>
        public PyDataType PrepareKeyValQuery(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    if (reader.Read() == false)
                        return null;
                    
                    // run the prepared statement
                    return KeyVal.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a Row representing the result.
        /// this only holds ONE row
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <returns>The PyDataType object representing the result</returns>
        public Row PrepareRowQuery(string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    if (reader.Read() == false)
                        return null;
                    
                    // run the prepared statement
                    return Row.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a Row representing the result.
        /// this only holds ONE row
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The PyDataType object representing the result</returns>
        public Row PrepareRowQuery(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    if (reader.Read() == false)
                        return null;
                    
                    // run the prepared statement
                    return Row.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }
        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a PyDictionary representing the result.
        /// this only holds ONE row
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <returns>The PyDataType object representing the result</returns>
        public PyDictionary<PyString, PyDataType> PrepareDictionaryQuery(string query)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    if (reader.Read() == false)
                        return null;
                    
                    // run the prepared statement
                    return PyDictionary<PyString, PyDataType>.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a PyDictionary representing the result.
        /// this only holds ONE row
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The PyDataType object representing the result</returns>
        public PyDictionary<PyString, PyDataType> PrepareDictionaryQuery(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    if (reader.Read() == false)
                        return null;
                    
                    // run the prepared statement
                    return PyDictionary<PyString, PyDataType>.FromMySqlDataReader(this, reader);
                }
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Runs one prepared query with the given values as parameters and returns a PyDictionary representing the result.
        /// this only holds ONE row
        /// </summary>
        /// <param name="query">The prepared query</param>
        /// <param name="values">The key-value pair of values to use when running the query</param>
        /// <returns>The PyDataType object representing the result</returns>
        public PyList<PyInteger> PrepareList(string query, Dictionary<string, object> values)
        {
            try
            {
                MySqlConnection connection = null;
                // create the correct command
                MySqlCommand command = this.PrepareQuery(ref connection, query);

                // add values
                this.AddNamedParameters(values, command);

                MySqlDataReader reader = command.ExecuteReader();
                
                using (connection)
                using (reader)
                {
                    // run the prepared statement
                    return PyList<PyInteger>.FromMySqlDataReader(this, reader);
                }
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
                    this.AddNamedParameters(values, command);
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

        public void GetLock(ref MySqlConnection connection, string lockName)
        {
            try
            {
                if (connection == null)
                {
                    connection = new MySqlConnection(this.mConnectionString);
                    connection.Open();
                }

                MySqlCommand command = new MySqlCommand("SELECT GET_LOCK (@lockName, 0xFFFFFFFF);", connection);

                command.Parameters.AddWithValue("@lockName", lockName);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }

        public void ReleaseLock(MySqlConnection connection, string lockName)
        {
            try
            {
                if (connection == null)
                    throw new ArgumentNullException(nameof(connection), "A valid connection is required");

                MySqlCommand command = new MySqlCommand($"SELECT RELEASE_LOCK (@lockName);", connection);

                command.Parameters.AddWithValue("@lockName", lockName);
                
                command.ExecuteNonQuery();
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
                // only open a connection if it's really needed
                if (connection == null)
                {
                    connection = new MySqlConnection(this.mConnectionString);
                    connection.Open();                    
                }

                MySqlCommand command = new MySqlCommand(query, connection);

                return command.ExecuteReader();
            }
            catch (Exception e)
            {
                Log.Error($"MySQL error: {e.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Obtains important metadata used in the database functions
        /// </summary>
        /// <param name="reader">The reader to use</param>
        /// <param name="headers">Where to put the headers</param>
        /// <param name="fieldTypes">Where to put the field types</param>
        public void GetDatabaseHeaders(MySqlDataReader reader, out PyList<PyString> headers, out FieldType[] fieldTypes)
        {
            headers = new PyList<PyString>(reader.FieldCount);
            fieldTypes = new FieldType[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
            {
                headers[i] = reader.GetName(i);
                fieldTypes[i] = GetFieldType(reader, i);
            }
        }
        
        /// <summary>
        /// Obtains the list of types for all the columns in this MySqlDataReader
        /// </summary>
        /// <param name="reader">The reader to use</param>
        /// <returns></returns>
        public FieldType[] GetFieldTypes(MySqlDataReader reader)
        {
            FieldType[] result = new FieldType[reader.FieldCount];

            for (int i = 0; i < result.Length; i++)
                result[i] = GetFieldType(reader, i);

            return result;
        }
        
        /// <summary>
        /// Obtains the current field type off a MySqlDataReader for the given column
        /// </summary>
        /// <param name="reader">The data reader to use</param>
        /// <param name="index">The column to get the type from</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">If the type is not supported</exception>
        public FieldType GetFieldType(MySqlDataReader reader, int index)
        {
            Type type = reader.GetFieldType(index);

            if (type == typeof(string))
            {
                DataTable schema = reader.GetSchemaTable();
                string tableName = (string) schema.Rows[index][schema.Columns["BaseTableName"]];
                string columnName = (string) schema.Rows[index][schema.Columns["BaseColumnName"]];
                
                // default the charset to wide string if the column is not found
                // this typically happens when a UNION query is used
                if (this.ColumnCharsets.TryGetValue($"{tableName}.{columnName}", out ColumnCharset charset) == false)
                {
                    #if DEBUG
                    Log.Warning("Defaulting column type to wide string because the table name cannot be determined. This usually happens when writing UNION queries.");
                    #endif
                    return FieldType.WStr;
                }

                return charset == ColumnCharset.Byte ? FieldType.Str : FieldType.WStr;
            }
            
            if (type == typeof(ulong)) return FieldType.UI8;
            if (type == typeof(long)) return FieldType.I8;
            if (type == typeof(uint)) return FieldType.UI4;
            if (type == typeof(int)) return FieldType.I4;
            if (type == typeof(ushort)) return FieldType.UI2;
            if (type == typeof(short)) return FieldType.I2;
            if (type == typeof(sbyte)) return FieldType.I1;
            if (type == typeof(byte)) return FieldType.UI1;
            if (type == typeof(byte[])) return FieldType.Bytes;
            if (type == typeof(double) || type == typeof(decimal)) return FieldType.R8;
            if (type == typeof(float)) return FieldType.R4;
            if (type == typeof(bool)) return FieldType.Bool;

            throw new InvalidDataException($"Unknown field type {type}");
        }
    }
}