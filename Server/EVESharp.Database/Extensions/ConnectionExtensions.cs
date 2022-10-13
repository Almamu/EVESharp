using System.Collections.Generic;
using EVESharp.Database.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Extensions;

public static class ConnectionExtensions
{
#region Procedure functions
    public static CRowset CRowset (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).CRowset ();
    }
    public static Rowset Rowset (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).Rowset ();
    }
    public static IndexRowset IndexRowset (this IDatabase Database, int indexField, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).IndexRowset (indexField);
    }
    public static Row Row (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).Row ();
    }
    public static PyDataType KeyVal (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).KeyVal ();
    }
    public static PyList <PyPackedRow> PackedRowList (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).PackedRowList ();
    }
    public static PyDictionary <PyString, PyDataType> Dictionary (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).Dictionary <PyString, PyDataType> ();
    }
    public static PyList List (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).List<PyDataType> ();
    }
    public static PyList <T> List <T> (this IDatabase Database, string procedureName, Dictionary <string, object> values = null) where T : PyDataType
    {
        return Database.SelectProcedure (procedureName, values).List <T> ();
    }
    public static PyDataType DictRowList (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).DictRowList ();
    }
    public static PyDataType IntPackedRowListDictionary(this IDatabase Database, string procedureName, int keyColumnIndex, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).IntPackedRowListDictionary (keyColumnIndex);
    }
    public static PyDictionary<PyInteger, PyInteger> IntIntDictionary(this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).IntIntDictionary ();
    }
    public static PyDictionary<PyInteger, PyList<PyInteger>> IntIntListDictionary(this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).IntIntListDictionary ();
    }
    public static PyDictionary IntRowDictionary(this IDatabase Database, int keyColumnIndex, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).IntRowDictionary (keyColumnIndex);
    }
    public static T1 Scalar <T1> (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).Scalar <T1> ();
    }
    public static (T1, T2) Scalar <T1, T2> (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).Scalar <T1, T2> ();
    }
    public static (T1, T2, T3) Scalar <T1, T2, T3> (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.SelectProcedure (procedureName, values).Scalar <T1, T2, T3> ();
    }
#endregion Procedure functions

#region Query functions
    public static CRowset PrepareCRowset (this IDatabase Database, string query, Dictionary <string, object> values = null)
    {
        return Database.Select (query, values).CRowset ();
    }
    public static Rowset PrepareRowset (this IDatabase Database, string query, Dictionary <string, object> values = null)
    {
        return Database.Select (query, values).Rowset ();
    }
    public static IndexRowset PrepareIndexRowset (this IDatabase Database, int keyColumnIndex, string query, Dictionary <string, object> values = null)
    {
        return Database.Select (query, values).IndexRowset (keyColumnIndex);
    }
    public static Row PrepareRow (this IDatabase Database, string query, Dictionary <string, object> values = null)
    {
        return Database.Select (query, values).Row ();
    }
    public static PyList <PyPackedRow> PreparePackedRowList (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.Select (procedureName, values).PackedRowList ();
    }
    public static PyDataType PrepareKeyVal (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.Select (procedureName, values).KeyVal ();
    }
    public static PyPackedRow PreparePackedRow (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.Select (procedureName, values).PackedRow ();
    }
    public static PyDictionary PrepareIntRowDictionary(this IDatabase Database, string procedureName, int keyColumnIndex, Dictionary <string, object> values = null)
    {
        return Database.Select (procedureName, values).IntRowDictionary (keyColumnIndex);
    }
    public static PyDataType PrepareIntPackedRowListDictionary(this IDatabase Database, string procedureName, int keyColumnIndex, Dictionary <string, object> values = null)
    {
        return Database.Select (procedureName, values).IntPackedRowListDictionary (keyColumnIndex);
    }
    public static PyDataType PrepareDictRowList (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.Select (procedureName, values).DictRowList ();
    }
    public static PyList PrepareList (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.Select (procedureName, values).List <PyDataType> ();
    }
    public static PyDictionary PrepareDictionary (this IDatabase Database, string procedureName, Dictionary <string, object> values = null)
    {
        return Database.Select (procedureName, values).Dictionary<PyDataType, PyDataType> ();
    }
    public static PyList PrepareList<T> (this IDatabase Database, string procedureName, Dictionary <string, object> values = null) where T : PyDataType
    {
        return Database.Select (procedureName, values).List <T> ();
    }
#endregion Query functions
}