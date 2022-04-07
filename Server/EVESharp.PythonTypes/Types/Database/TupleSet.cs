using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

/// <summary>
/// Helper class to work with TupleSet types to be sent to the EVE Online client
/// </summary>
public static class TupleSet
{
    /// <summary>
    /// Simple helper method that creates a correct tupleset and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <returns></returns>
    public static PyDataType FromMySqlDataReader(IDatabaseConnection connection, MySqlDataReader reader)
    {
        connection.GetDatabaseHeaders(reader, out PyList<PyString> columns, out FieldType[] fieldTypes);
        PyList rows = new PyList();
            
        while (reader.Read() == true)
        {
            PyList linedata = new PyList(columns.Count);

            for (int i = 0; i < columns.Count; i++)
                linedata[i] = IDatabaseConnection.ObjectFromColumn(reader, fieldTypes[i], i);

            rows.Add(linedata);
        }

        return new PyTuple(2)
        {
            [0] = columns,
            [1] = rows
        };
    }
}