using System.Collections.Generic;
using System.Data;
using EVESharp.Database.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Extensions;

public static class LockExtensions
{
#region Procedure functions
    public static CRowset CRowset (this IDatabase Database, IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).CRowset ();
    }
    public static Rowset Rowset (this IDatabase Database, IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).Rowset ();
    }
    public static IndexRowset IndexRowset (this IDatabase Database, IDbConnection connection, int indexField, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).IndexRowset (indexField);
    }
    public static Row Row (this IDatabase Database, IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).Row ();
    }
    public static PyDataType KeyVal (this IDatabase Database, IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).KeyVal ();
    }
    public static PyList <PyPackedRow> PackedRowList (this IDatabase Database, IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).PackedRowList ();
    }
    public static PyDictionary <PyString, PyDataType> Dictionary (this IDatabase Database, IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).Dictionary <PyString, PyDataType> ();
    }
    public static T1 Scalar <T1> (this IDatabase Database, IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).Scalar <T1> ();
    }

    public static (T1, T2) Scalar <T1, T2> (this IDatabase Database, IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).Scalar <T1, T2> ();
    }
    public static (T1, T2, T3) Scalar <T1, T2, T3> (this IDatabase Database, IDbConnection connection, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (connection, procedureName, values).Scalar <T1, T2, T3> ();
    }
#endregion Procedure functions

#region Query functions
    public static Rowset PrepareRowset (this IDatabase Database, IDbConnection connection, string query, Dictionary <string, object> values = null)
    {
        return Database.Select (connection, query, values).Rowset ();
    }
#endregion Query functions
}