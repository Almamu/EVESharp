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
using System.Data.Common;
using EVESharp.Database.MySql;
using EVESharp.EVE.Database;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Serilog;

namespace EVESharp.Database;

public class DatabaseConnection : IDatabaseConnection
{
    private readonly string                             mConnectionString;
    private          ILogger                            Log            { get; }

    public DatabaseConnection (EVESharp.Common.Configuration.Database configuration, ILogger logger)
    {
        Log = logger;
        Log.Debug ("Initializing database connection...");
        
        this.mConnectionString = new MySqlConnectionStringBuilder
        {
            Server          = configuration.Hostname,
            Port            = configuration.Port,
            Database        = configuration.Name,
            UserID          = configuration.Username,
            Password        = configuration.Password,
            MinimumPoolSize = 10
        }.ToString ();
        
        Log.Information ("Database connection initialized successfully");
    }

    public ulong PrepareLID (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            MySqlCommand command = (MySqlCommand) this.Prepare (ref connection, query, values);

            command.ExecuteNonQuery ();

            return (ulong) command.LastInsertedId;
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    public void Query (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbCommand command = this.Prepare (ref connection, query, values);
            
            using (command)
            {
                command.ExecuteNonQuery ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }
    
    public DbDataReader Select (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            return this.Prepare (ref connection, query, values).ExecuteReader ();
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    private void AddNamedParameters (Dictionary <string, object> parameters, MySqlCommand command)
    {
        foreach ((string parameterName, object value) in parameters)
            command.Parameters.AddWithValue (parameterName, value);
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters
    /// </summary>
    /// <param name="connection">where to store the MySql connection (has to be closed manually)</param>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The reader with the results of the query</returns>
    public DbCommand Prepare (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            // only open a connection if it's really needed
            if (connection == null)
            {
                connection = new MySqlConnection (this.mConnectionString);
                connection.Open ();
            }

            MySqlCommand command = new MySqlCommand (query, (MySqlConnection) connection);

            // add values
            if (values is not null)
                this.AddNamedParameters (values, command);

            // run the prepared statement
            return command;
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a CRowset representing the result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The Rowset object representing the result</returns>
    public CRowset PrepareCRowset (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.CRowset ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

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
    public IndexRowset PrepareIndexRowset (ref IDbConnection connection, int indexField, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.IndexRowset (indexField);
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }
    
    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a PyPackedRow representing the first result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The Rowset object representing the result</returns>
    public PyPackedRow PreparePackedRow (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                if (reader.Read () == false)
                    return null;

                // run the prepared statement
                return reader.PackedRow (reader.DBRowDescriptor ());
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a Rowset representing the result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The Rowset object representing the result</returns>
    public PyList <PyPackedRow> PreparePackedRowList (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.PackedRowList ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }
    
    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a Rowset representing the result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The Rowset object representing the result</returns>
    public Rowset PrepareRowset (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.Rowset ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }
    
    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a IntIntDictionary representing the result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <returns>The Rowset object representing the result</returns>
    public PyDictionary <PyInteger, PyInteger> PrepareIntIntDictionary (ref IDbConnection connection, string query, Dictionary<string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.IntIntDictionary ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

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
    public PyDictionary <PyInteger, PyList <PyInteger>> PrepareIntIntListDictionary (ref IDbConnection connection, string query, Dictionary<string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.IntIntListDictionary ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

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
    public PyDictionary PrepareIntRowDictionary (ref IDbConnection connection, string query, int keyColumnIndex, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.IntRowDictionary (keyColumnIndex);
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

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
    public PyDataType PrepareIntPackedRowListDictionary (ref IDbConnection connection, string query, int keyColumnIndex, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.IntPackedRowListDictionary (keyColumnIndex);
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

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
    public PyDataType PrepareDictRowList (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.DictRowList ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

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
    public PyDataType PrepareKeyVal (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                if (reader.Read () == false)
                    return null;

                // run the prepared statement
                return reader.KeyVal ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

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
    public Row PrepareRow (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                if (reader.Read () == false)
                    return null;

                // run the prepared statement
                return reader.Row ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

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
    public PyDictionary <PyString, PyDataType> PrepareDictionary (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                if (reader.Read () == false)
                    return null;

                // run the prepared statement
                return reader.Dictionary <PyString, PyDataType> ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

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
    public PyList <PyInteger> PrepareList (ref IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        try
        {
            DbDataReader  reader     = this.Select (ref connection, query, values);
            
            using (reader)
            {
                // run the prepared statement
                return reader.List <PyInteger> ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    public void GetLock (ref IDbConnection connection, string lockName)
    {
        try
        {
            if (connection == null)
            {
                connection = new MySqlConnection (this.mConnectionString);
                connection.Open ();
            }

            MySqlCommand command = new MySqlCommand ("SELECT GET_LOCK (@lockName, 0xFFFFFFFF);", (MySqlConnection) connection);

            using (command)
            {
                command.Parameters.AddWithValue ("@lockName", lockName);

                command.ExecuteNonQuery ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    public void ReleaseLock (IDbConnection connection, string lockName)
    {
        try
        {
            if (connection == null)
                throw new ArgumentNullException (nameof (connection), "A valid connection is required");

            MySqlCommand command = new MySqlCommand ("SELECT RELEASE_LOCK (@lockName);", (MySqlConnection) connection);

            using (command)
            {
                command.Parameters.AddWithValue ("@lockName", lockName);

                command.ExecuteNonQuery ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Prepares things to perform a procedure call on the given connection
    /// </summary>
    /// <param name="connection">The connection to use (the function will create a new one if null)</param>
    /// <param name="procedureName">The procedure to call</param>
    /// <returns>A command ready to perform the call</returns>
    private MySqlCommand PrepareProcedureCall (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            // open a new connection if one is not available already
            if (connection is null)
            {
                connection = new MySqlConnection (this.mConnectionString);
                connection.Open ();
            }

            // setup the command in the correct mode to perform the stored procedure call
            MySqlCommand command = new MySqlCommand (procedureName, (MySqlConnection) connection);
            
            command.CommandType = CommandType.StoredProcedure;
            
            if (values is not null)
                this.AddNamedParameters (values, command);

            return command;
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls the given procedure
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    public void Procedure (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);

            using (command)
            {
                command.ExecuteNonQuery ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls the given procedure
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The last inserted if of the procedure call</returns>
    public ulong ProcedureLID (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);

            using (command)
            {
                command.ExecuteNonQuery ();

                return (ulong) command.LastInsertedId;
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls the given procedure and returns it's data as a normal CRowset
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The values to add to the call</param>
    /// <returns>The CRowset object representing the result</returns>
    public CRowset CRowset (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            // initialize a command and a connection for this procedure call
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            {
                using (DbDataReader reader = command.ExecuteReader ())
                {
                    return reader.CRowset ();
                }
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls the given procedure and returns it's data as a normal CRowset
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The values to add to the call</param>
    /// <returns>The Rowset object representing the result</returns>
    public Rowset Rowset (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            // initialize a command and a connection for this procedure call
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            {
                using (DbDataReader reader = command.ExecuteReader ())
                {
                    return reader.Rowset ();
                }
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls the given procedure and returns it's data as a normal CRowset
    /// </summary>
    /// <param name="indexField">The column of the index</param>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The values to add to the call</param>
    /// <returns>The IndexRowset object representing the result</returns>
    public IndexRowset IndexRowset (ref IDbConnection connection, int indexField, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            // initialize a command and a connection for this procedure call
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            {
                using (DbDataReader reader = command.ExecuteReader ())
                {
                    return reader.IndexRowset (indexField);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls a procedure and returns a Row representing the result.
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PyDataType object representing the result</returns>
    public Row Row (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            // initialize a command and a connection for this procedure call
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            using (DbDataReader reader = command.ExecuteReader ())
            {
                if (reader.Read () == false)
                    return null;

                return reader.Row ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls the given procedure and returns it's data as a normal CRowset
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PackedRowList object representing the result</returns>
    public PyList <PyPackedRow> PackedRowList (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            // initialize a command and a connection for this procedure call
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            using (DbDataReader reader = command.ExecuteReader ())
            {
                // run the prepared statement
                return reader.PackedRowList ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Runs one procedure and returns an IntIntDictionary representing the result
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The IntIntDictionary object representing the result</returns>
    public PyDictionary <PyInteger, PyInteger> IntIntDictionary (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            // initialize a command and a connection for this procedure call
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName);

            using (command)
            using (DbDataReader reader = command.ExecuteReader ())
            {
                // run the prepared statement
                return reader.IntIntDictionary ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Runs one procedure with the given values as parameters and returns a IntIntListDictionary representing
    /// the result
    ///
    /// IMPORTANT: The first column must be ordered (direction doesn't matter) for this to properly work
    /// </summary>
    /// <param name="procedureName">The procedure to run</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The IntIntListDictionary object representing the result</returns>
    public PyDictionary <PyInteger, PyList <PyInteger>> IntIntListDictionary (ref IDbConnection connection, string procedureName, Dictionary<string, object> values = null)
    {
        try
        {
            // initialize a command and a connection for this procedure call
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            using (DbDataReader reader = command.ExecuteReader ())
            {
                // run the prepared statement
                return reader.IntIntListDictionary ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Runs one procedure with the given values as parameters and returns a IntRowDictionary representing
    /// the result
    /// </summary>
    /// <param name="keyColumnIndex">The column to use as index for the IntRowDictionary</param>
    /// <param name="procedureName">The procedure to run</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The IntRowDictionary object representing the result</returns>
    public PyDictionary IntRowDictionary (ref IDbConnection connection, int keyColumnIndex, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            // initialize a command and a connection for this procedure call
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            using (DbDataReader reader = command.ExecuteReader ())
            {
                // run the prepared statement
                return reader.IntRowDictionary (keyColumnIndex);
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Runs one procedure with the given values as parameters and returns a DictRowList representing
    /// the result
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The DictRowList object representing the result</returns>
    public PyDataType DictRowList (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            // initialize a command and a connection for this procedure call
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            using (DbDataReader reader = command.ExecuteReader ())
            {
                // run the prepared statement
                return reader.DictRowList ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    public PyDictionary <PyString, PyDataType> Dictionary (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        try
        {
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            using (DbDataReader reader = command.ExecuteReader ())
            {
                if (reader.Read () == false)
                    return null;

                return reader.Dictionary <PyString, PyDataType> ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Runs one procedure with the given values as parameters and returns a PyList representing the result.
    /// this only holds ONE row
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PyDataType object representing the result</returns>
    public PyList <T> List <T> (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null) where T : PyDataType
    {
        try
        {
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, procedureName, values);
            
            using (command)
            using (DbDataReader reader = command.ExecuteReader ())
            {
                // run the prepared statement
                return reader.List <T> ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls the given function and returns it's value casting it to the given type. If the result returns more than
    /// one column or row, only the topmost, leftmost value is returned
    /// </summary>
    /// <param name="functionName">The MySQL function to call</param>
    /// <param name="values">The values to supply the MySQL function</param>
    /// <typeparam name="T">The type to cast the return value to</typeparam>
    /// <returns>The functions result</returns>
    public T Scalar <T> (ref IDbConnection connection, string functionName, Dictionary <string, object> values = null)
    {
        try
        {
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, functionName, values);
            
            using (command)
            {
                return (T) command.ExecuteScalar ();
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls the given function and returns it's value casting it to the given type. If the result returns more than one row, only the topmost value is returned
    /// </summary>
    /// <param name="functionName">The MySQL function to call</param>
    /// <param name="values">The values to supply the MySQL function</param>
    /// <typeparam name="T1">The type to cast the first column's return value to</typeparam>
    /// <typeparam name="T2">The type to cast the second column's return value to</typeparam>
    /// <typeparam name="T3">The type to cast the third column's return value to</typeparam>
    /// <returns>The functions result</returns>
    public (T1, T2) Scalar <T1, T2> (ref IDbConnection connection, string functionName, Dictionary <string, object> values = null)
    {
        try
        {
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, functionName, values);
            
            using (command)
            {
                DbDataReader reader = command.ExecuteReader ();

                using (reader)
                {
                    if (reader.Read () == false || reader.HasRows == false)
                        throw new Exception ("Expected at least one row back, but couldn't get any");
                    if (reader.FieldCount != 2)
                        throw new Exception ($"Expected two columns but returned {reader.FieldCount}");

                    return (
                        (T1) reader.GetValue (0),
                        (T2) reader.GetValue (1)
                    );
                }
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }

    /// <summary>
    /// Calls the given function and returns it's value casting it to the given type. If the result returns more than one row, only the topmost value is returned
    /// </summary>
    /// <param name="functionName">The MySQL function to call</param>
    /// <param name="values">The values to supply the MySQL function</param>
    /// <typeparam name="T1">The type to cast the first column's return value to</typeparam>
    /// <typeparam name="T2">The type to cast the second column's return value to</typeparam>
    /// <typeparam name="T3">The type to cast the third column's return value to</typeparam>
    /// <returns>The functions result</returns>
    public (T1, T2, T3) Scalar <T1, T2, T3> (ref IDbConnection connection, string functionName, Dictionary <string, object> values = null)
    {
        try
        {
            MySqlCommand    command    = this.PrepareProcedureCall (ref connection, functionName, values);
            
            using (command)
            {
                DbDataReader reader = command.ExecuteReader ();

                using (reader)
                {
                    if (reader.Read () == false || reader.HasRows == false)
                        throw new Exception ("Expected at least one row back, but couldn't get any");
                    if (reader.FieldCount != 3)
                        throw new Exception ($"Expected three columns but returned {reader.FieldCount}");

                    return (
                        (T1) reader.GetValue (0),
                        (T2) reader.GetValue (1),
                        (T3) reader.GetValue (2)
                    );
                }
            }
        }
        catch (Exception e)
        {
            Log.Error ($"MySQL error: {e.Message}");

            throw;
        }
    }
}