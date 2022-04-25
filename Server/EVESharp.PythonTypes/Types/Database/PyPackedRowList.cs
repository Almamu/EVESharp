using System.Data;
using System.Data.Common;
using EVESharp.PythonTypes.Types.Collections;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

/// <summary>
/// Helper class to work with PyPackedRow lists (which are just a PyList of PyPackedRows)
/// </summary>
public static class PyPackedRowList
{
    /// <summary>
    /// Simple helper method that creates the correct PackedRowList data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="reader"></param>
    public static PyList <PyPackedRow> FromDataReader (IDatabaseConnection connection, DbDataReader reader)
    {
        DBRowDescriptor      descriptor = DBRowDescriptor.FromDataReader (connection, reader);
        PyList <PyPackedRow> list       = new PyList <PyPackedRow> ();

        while (reader.Read ())
            list.Add (PyPackedRow.FromDataReader (reader, descriptor));

        return list;
    }
}