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
using Serilog;

namespace EVESharp.Database;

public class Database : IDatabase
{
    private readonly string mConnectionString;

    private ILogger Log { get; }

    public Database (EVESharp.Common.Configuration.Database configuration, ILogger logger)
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

#region Connection handling
    public IDbConnection OpenConnection ()
    {
        MySqlConnection connection = new MySqlConnection (this.mConnectionString);

        connection.Open ();

        return connection;
    }
#endregion Connection handling
    
#region Database locking

    public DbLock GetLock (string lockName)
    {
        DbLock dbLock = new DbLock
        {
            Connection = new MySqlConnection (this.mConnectionString),
            Name = lockName,
            Creator = this
        };

        try
        {
            dbLock.Connection.Open ();

            MySqlCommand command = new MySqlCommand ("SELECT GET_LOCK (@lockName, 0xFFFFFFFF);", (MySqlConnection) dbLock.Connection);

            using (command)
            {
                command.Parameters.AddWithValue ("@lockName", lockName);

                command.ExecuteNonQuery ();
            }
        }
        catch (Exception e)
        {
            dbLock.Dispose ();
            throw;
        }

        return dbLock;
    }

    public void ReleaseLock (DbLock dbLock)
    {
        if (dbLock is null)
            throw new ArgumentNullException (nameof (dbLock), "A valid connection is required");

        MySqlCommand command = new MySqlCommand ("SELECT RELEASE_LOCK (@lockName);", (MySqlConnection) dbLock.Connection);

        using (command)
        {
            command.Parameters.AddWithValue ("@lockName", dbLock.Name);

            command.ExecuteNonQuery ();
        }
    }

#endregion Database locking
    
#region Querying
    public DbCommand Prepare (IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        MySqlCommand command = new MySqlCommand (query, (MySqlConnection) connection);

        // add values
        if (values is not null && values.Count > 0)
            this.AddNamedParameters (values, command);

        // run the prepared statement
        return command;
    }
    
    public ulong Insert (IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        MySqlCommand command = (MySqlCommand) this.Prepare (connection, query, values);

        using (command)
        {
            command.ExecuteNonQuery ();

            return (ulong) command.LastInsertedId;
        }
    }

    public void Query (IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        DbCommand command = this.Prepare (connection, query, values);
        
        using (command)
        {
            command.ExecuteNonQuery ();
        }
    }
    
    private void AddNamedParameters (Dictionary <string, object> parameters, MySqlCommand command)
    {
        foreach ((string parameterName, object value) in parameters)
            command.Parameters.AddWithValue (parameterName, value);
    }
#endregion Querying
    
#region Procedures
    /// <summary>
    /// Prepares things to perform a procedure call on the given connection
    /// </summary>
    /// <param name="connection">The connection to use (the function will create a new one if null)</param>
    /// <param name="procedureName">The procedure to call</param>
    /// <returns>A command ready to perform the call</returns>
    private MySqlCommand PrepareProcedureCall (IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        // setup the command in the correct mode to perform the stored procedure call
        MySqlCommand command = new MySqlCommand (procedureName, (MySqlConnection) connection);
        
        command.CommandType = CommandType.StoredProcedure;
        
        if (values is not null)
            this.AddNamedParameters (values, command);

        return command;
    }
    
    public void QueryProcedure (IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        MySqlCommand command = this.PrepareProcedureCall (connection, procedureName, values);

        using (command)
        {
            command.ExecuteNonQuery ();
        }
    }

    public DbDataReader SelectProcedure (IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        MySqlCommand command = this.PrepareProcedureCall (connection, procedureName, values);

        using (command)
        {
            return command.ExecuteReader ();
        }
    }
    
    public ulong InsertProcedure (IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        MySqlCommand command = this.PrepareProcedureCall (connection, procedureName, values);

        using (command)
        {
            command.ExecuteNonQuery ();
        }

        command = new MySqlCommand ("SELECT LAST_INSERT_ID()", (MySqlConnection) connection);

        using (command)
            return (ulong) command.ExecuteScalar ();
    }
#endregion Procedures
}