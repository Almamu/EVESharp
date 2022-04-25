using System.Data.Common;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

public class RowList
{
    /// <summary>
    /// Simple helper method that creates a correct RowList and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// 
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <returns></returns>
    public static PyDataType FromDataReader (IDatabaseConnection connection, DbDataReader reader)
    {
        connection.GetDatabaseHeaders (reader, out PyList <PyString> headers, out FieldType [] fieldTypes);
        PyList lines = new PyList ();

        while (reader.Read ())
            lines.Add (Row.FromDataReader (reader, headers, fieldTypes));

        return new PyTuple (2)
        {
            [0] = headers,
            [1] = lines
        };
    }
}