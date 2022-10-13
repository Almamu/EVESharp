using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace EVESharp.Database;

public interface IDatabase
{
#region Connection handling
    /// <summary>
    /// Opens a new connection to the database
    /// </summary>
    /// <returns></returns>
    public IDbConnection OpenConnection ();
#endregion
    
#region Database locking
    /// <summary>
    /// Acquires the specified lock on this (or a new) connection
    /// </summary>
    /// <param name="lockName"></param>
    public DbLock GetLock (string lockName);

    /// <summary>
    /// Releases the specified lock on this connection
    /// </summary>
    /// <param name="dbLock"></param>
    public void ReleaseLock (DbLock dbLock);
#endregion Database locking
    
#region Querying

    /// <summary>
    /// Runs one prepared query with the given value as parameters, ignoring the result data
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The number of rows affected</returns>
    public DbCommand Prepare (IDbConnection connection, string query, Dictionary <string, object> values = null);

    public DbCommand Prepare (DbLock dbLock, string query, Dictionary <string, object> values = null) => this.Prepare (dbLock.Connection, query, values);
    
    public int Prepare (string query, Dictionary <string, object> values = null)
    {
        using (IDbConnection connection = this.OpenConnection ())
            return this.Prepare (connection, query, values).ExecuteNonQuery ();
    }

    public ulong Insert (IDbConnection connection, string query, Dictionary <string, object> values = null);
    public ulong Insert (DbLock        dbLock,     string query, Dictionary <string, object> values = null) => this.Insert (dbLock.Connection, query, values);

    public ulong Insert (string query, Dictionary <string, object> values = null)
    {
        using (IDbConnection connection = this.OpenConnection ())
            return this.Insert (connection, query, values);
    }
    
    /// <summary>
    /// Performs a query on the database disregarding any possible result
    /// </summary>
    /// <param name="connection">The database connection</param>
    /// <param name="query">The query to run</param>
    /// <param name="values">Any of the values for the query</param>
    public void Query (IDbConnection connection, string query, Dictionary <string, object> values = null);
    public void Query (DbLock        dbLock,     string query, Dictionary <string, object> values = null) => this.Query (dbLock.Connection, query, values);

    public void Query (string query, Dictionary <string, object> values = null)
    {
        using (IDbConnection connection = this.OpenConnection ())
            this.Query (connection, query, values);
    }

    public DbDataReader Select (IDbConnection connection, string query, Dictionary <string, object> values = null) =>
        this.Prepare (connection, query, values).ExecuteReader ();
    public DbDataReader Select (DbLock dbLock, string query, Dictionary <string, object> values = null) => this.Select (dbLock.Connection, query, values);

    public DbDataReader Select (string query, Dictionary <string, object> values = null)
    {
        IDbConnection connection = this.OpenConnection ();

        return new WrappedDbDataReader (
            this.Select (connection, query, values),
            connection
        );
    }
#endregion Querying
    
#region Procedures

    /// <summary>
    /// Calls the given procedure
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    public void QueryProcedure (IDbConnection connection, string procedureName, Dictionary <string, object> values = null);
    public void QueryProcedure (DbLock dbLock, string procedureName, Dictionary <string, object> values = null) => this.QueryProcedure (dbLock.Connection, procedureName, values);

    public void QueryProcedure (string procedureName, Dictionary <string, object> values = null)
    {
        using (IDbConnection connection = this.OpenConnection ())
            this.QueryProcedure (connection, procedureName, values);
    }

    /// <summary>
    /// Calls the given procedure
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    public DbDataReader SelectProcedure (IDbConnection connection, string procedureName, Dictionary <string, object> values = null);
    public DbDataReader SelectProcedure (DbLock dbLock, string procedureName, Dictionary <string, object> values = null) => this.SelectProcedure (dbLock.Connection, procedureName, values);

    public DbDataReader SelectProcedure (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection connection = this.OpenConnection ();

        return new WrappedDbDataReader (
            this.SelectProcedure (connection, procedureName, values),
            connection
        );
    }

    /// <summary>
    /// Calls the given procedure
    /// </summary>
    /// <param name="connection">Database connection</param>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The last inserted if of the procedure call</returns>
    public ulong InsertProcedure (IDbConnection connection, string procedureName, Dictionary <string, object> values = null);
    public ulong InsertProcedure (DbLock dbLock, string procedureName, Dictionary <string, object> values = null) => this.InsertProcedure (dbLock.Connection, procedureName, values);

    public ulong InsertProcedure (string procedureName, Dictionary <string, object> values = null)
    {
        using (IDbConnection connection = this.OpenConnection ())
            return this.InsertProcedure (connection, procedureName, values);
    }
#endregion Procedures
}