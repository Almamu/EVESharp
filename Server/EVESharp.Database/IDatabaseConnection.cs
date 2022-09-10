using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database;

public interface IDatabaseConnection
{
#region Prepared statements
    /// <summary>
    /// Runs one prepared query with the given value as parameters, ignoring the result data
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The number of rows affected</returns>
    public DbCommand Prepare (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public int Prepare (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.Prepare (ref con, query, values).ExecuteNonQuery ();
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a CRowset representing the result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The Rowset object representing the result</returns>
    public CRowset PrepareCRowset (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public CRowset PrepareCRowset (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareCRowset (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns an IndexRowset representing the result
    /// </summary>
    /// <param name="indexField">The position of the index field in the result</param>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The Rowset object representing the result</returns>
    public IndexRowset PrepareIndexRowset (ref IDbConnection connection, int indexField, string query, Dictionary <string, object> values = null);

    public IndexRowset PrepareIndexRowset (int indexField, string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareIndexRowset (ref con, indexField, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a PyPackedRow representing the first result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The Rowset object representing the result</returns>
    public PyPackedRow PreparePackedRow (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public PyPackedRow PreparePackedRow (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PreparePackedRow (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a Rowset representing the result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The Rowset object representing the result</returns>
    public PyList <PyPackedRow> PreparePackedRowList (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public PyList <PyPackedRow> PreparePackedRowList (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PreparePackedRowList (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a Rowset representing the result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The Rowset object representing the result</returns>
    public Rowset PrepareRowset (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public Rowset PrepareRowset (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareRowset (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a PyDictionary representing the result.
    /// this only holds ONE row
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PyDataType object representing the result</returns>
    public PyList <PyInteger> PrepareList (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public PyList <PyInteger> PrepareList (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareList (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a PyDictionary representing the result.
    /// this only holds ONE row
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PyDataType object representing the result</returns>
    public PyDictionary <PyString, PyDataType> PrepareDictionary (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public PyDictionary <PyString, PyDataType> PrepareDictionary (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareDictionary (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a Row representing the result.
    /// this only holds ONE row
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PyDataType object representing the result</returns>
    public Row PrepareRow (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public Row PrepareRow (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareRow (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a KeyVal representing the result.
    /// KeyVals only hold ONE row
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PyDataType object representing the result</returns>
    public PyDataType PrepareKeyVal (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public PyDataType PrepareKeyVal (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareKeyVal (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }


    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a RowList representing
    /// the result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The RowList object representing the result</returns>
    public PyDataType PrepareDictRowList (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public PyDataType PrepareDictRowList (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareDictRowList (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
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
    public PyDataType PrepareIntPackedRowListDictionary (ref IDbConnection connection, string query, int keyColumnIndex, Dictionary <string, object> values = null);

    public PyDataType PrepareIntPackedRowListDictionary (string query, int keyColumnIndex, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareIntPackedRowListDictionary (ref con, query, keyColumnIndex, values);
        }
        finally
        {
            con?.Dispose ();
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
    public PyDictionary PrepareIntRowDictionary (ref IDbConnection connection, string query, int keyColumnIndex, Dictionary <string, object> values = null);

    public PyDictionary PrepareIntRowDictionary (string query, int keyColumnIndex, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareIntRowDictionary (ref con, query, keyColumnIndex, values);
        }
        finally
        {
            con?.Dispose ();
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
    public PyDictionary <PyInteger, PyList <PyInteger>> PrepareIntIntListDictionary (ref IDbConnection connection, string query, Dictionary<string, object> values = null);

    public PyDictionary <PyInteger, PyList <PyInteger>> PrepareIntIntListDictionary (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareIntIntListDictionary (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one prepared query with the given values as parameters and returns a IntIntDictionary representing the result
    /// </summary>
    /// <param name="query">The prepared query</param>
    /// <returns>The Rowset object representing the result</returns>
    public PyDictionary <PyInteger, PyInteger> PrepareIntIntDictionary (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public PyDictionary <PyInteger, PyInteger> PrepareIntIntDictionary (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareIntIntDictionary (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    public ulong PrepareLID (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public ulong PrepareLID (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PrepareLID (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }
    #endregion
    
    public void Query (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public void Query (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            this.Query (ref con, query, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }
        
    public DbDataReader Select (ref IDbConnection connection, string query, Dictionary <string, object> values = null);

    public DbDataReader Select (string query, Dictionary <string, object> values = null)
    {
        IDbConnection con    = null;
        DbDataReader  reader = this.Select (ref con, query, values);

        return new WrappedDbDataReader (reader, con);
    }

    #region Datbase procedures
    /// <summary>
    /// Calls the given procedure
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    public void Procedure (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);

    public void Procedure (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            this.Procedure (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Calls the given procedure
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The last inserted if of the procedure call</returns>
    public ulong ProcedureLID (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);

    public ulong ProcedureLID (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.ProcedureLID (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Calls the given procedure and returns it's data as a normal CRowset
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The values to add to the call</param>
    /// <returns>The CRowset object representing the result</returns>
    public CRowset CRowset (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);

    public CRowset CRowset (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.CRowset (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Calls the given procedure and returns it's data as a normal Rowset
    /// </summary>
    /// <param name="connection">The connection to be used (if any)</param>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The values to add to the call</param>
    /// <returns>The Rowset object representing the result</returns>
    public Rowset Rowset (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);

    public Rowset Rowset (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.Rowset (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Calls the given procedure and returns it's data as a normal CRowset
    /// </summary>
    /// <param name="indexField">The column of the index</param>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The values to add to the call</param>
    /// <returns>The IndexRowset object representing the result</returns>
    public IndexRowset IndexRowset (ref IDbConnection connection, int indexField, string procedureName, Dictionary <string, object> values = null);

    public IndexRowset IndexRowset (int indexField, string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.IndexRowset (ref con, indexField, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Calls a procedure and returns a Row representing the result.
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PyDataType object representing the result</returns>
    public Row Row (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);

    public Row Row (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.Row (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Calls a procedure and returns a KeyVal representing the result.
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PyDataType object representing the result</returns>
    public PyDataType KeyVal (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);

    public PyDataType KeyVal (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.KeyVal (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Calls the given procedure and returns it's data as a normal PackedRowList
    /// </summary>
    /// <param name="procedureName">The procedure name</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PackedRowList object representing the result</returns>
    public PyList <PyPackedRow> PackedRowList (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);

    public PyList <PyPackedRow> PackedRowList (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.PackedRowList (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    /// <summary>
    /// Runs one procedure and returns an IntIntDictionary representing the result
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The IntIntDictionary object representing the result</returns>
    public PyDictionary <PyInteger, PyInteger> IntIntDictionary (ref IDbConnection connection, string procedureName, Dictionary<string, object> values = null);

    public PyDictionary <PyInteger, PyInteger> IntIntDictionary (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.IntIntDictionary (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
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
    public PyDictionary <PyInteger, PyList <PyInteger>> IntIntListDictionary (ref IDbConnection connection, string procedureName, Dictionary<string, object> values = null);
    public PyDictionary <PyInteger, PyList <PyInteger>> IntIntListDictionary (string procedureName, Dictionary<string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.IntIntListDictionary (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
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
    public PyDictionary IntRowDictionary (ref IDbConnection connection, int keyColumnIndex, string procedureName, Dictionary <string, object> values = null);
    public PyDictionary IntRowDictionary (int keyColumnIndex, string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.IntRowDictionary (ref con, keyColumnIndex, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }


    /// <summary>
    /// Runs one procedure with the given values as parameters and returns a DictRowList representing
    /// the result
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The RowList object representing the result</returns>
    public PyDataType DictRowList (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);
    public PyDataType DictRowList (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.DictRowList (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }


    public PyDictionary <PyString, PyDataType> Dictionary (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);
    public PyDictionary <PyString, PyDataType> Dictionary (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.Dictionary (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }


    /// <summary>
    /// Runs one procedure with the given values as parameters and returns a PyList representing the result.
    /// this only holds ONE row
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The key-value pair of values to use when running the query</param>
    /// <returns>The PyDataType object representing the result</returns>
    public PyList <T> List <T> (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null) where T : PyDataType;
    public PyList <T> List <T> (string procedureName, Dictionary <string, object> values = null) where T : PyDataType
    {
        IDbConnection con = null;

        try
        {
            return this.List <T> (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }

    
    /// <summary>
    /// Calls the given function and returns it's value casting it to the given type. If the result returns more than
    /// one column or row, only the topmost, leftmost value is returned
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The values to supply the MySQL function</param>
    /// <typeparam name="T">The type to cast the return value to</typeparam>
    /// <returns>The functions result</returns>
    public T Scalar <T> (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);
    public T Scalar <T> (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.Scalar <T> (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }


    /// <summary>
    /// Calls the given function and returns it's value casting it to the given type. If the result returns more than
    /// one column or row, only the topmost, leftmost value is returned
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The values to supply the MySQL function</param>
    /// <typeparam name="T1">The type to cast the return value to</typeparam>
    /// <typeparam name="T2">The type to cast the return value to</typeparam>
    /// <returns>The functions result</returns>
    public (T1, T2) Scalar <T1, T2> (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);
    public (T1, T2) Scalar <T1, T2> (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.Scalar <T1, T2> (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }


    /// <summary>
    /// Calls the given function and returns it's value casting it to the given type. If the result returns more than
    /// one column or row, only the topmost, leftmost value is returned
    /// </summary>
    /// <param name="procedureName">The procedure to call</param>
    /// <param name="values">The values to supply the MySQL function</param>
    /// <typeparam name="T1">The type to cast the return value to</typeparam>
    /// <typeparam name="T2">The type to cast the return value to</typeparam>
    /// <typeparam name="T3">The type to cast the return value to</typeparam>
    /// <returns>The functions result</returns>
    public (T1, T2, T3) Scalar <T1, T2, T3> (ref IDbConnection connection, string procedureName, Dictionary <string, object> values = null);
    public (T1, T2, T3) Scalar <T1, T2, T3> (string procedureName, Dictionary <string, object> values = null)
    {
        IDbConnection con = null;

        try
        {
            return this.Scalar <T1, T2, T3> (ref con, procedureName, values);
        }
        finally
        {
            con?.Dispose ();
        }
    }
    #endregion
    
    #region Database locking
    /// <summary>
    /// Acquires the specified lock on this (or a new) connection
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="lockName"></param>
    public void GetLock (ref IDbConnection connection, string lockName);

    /// <summary>
    /// Releases the specified lock on this connection
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="lockName"></param>
    public void ReleaseLock (IDbConnection connection, string lockName);
    #endregion Database locking
}