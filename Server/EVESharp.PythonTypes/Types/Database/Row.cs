using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

public class Row
{
    /// <summary>
    /// Type of row
    /// </summary>
    private const string ROW_TYPE_NAME = "util.Row";
    /// <summary>
    /// The columns for this row
    /// </summary>
    public PyList<PyString> Header { get; }
    /// <summary>
    /// The values for each column
    /// </summary>
    public PyList Line { get; }

    public Row(PyList<PyString> header, PyList line)
    {
        this.Header = header;
        this.Line   = line;
    }
        
    public static implicit operator PyDataType(Row row)
    {
        PyDictionary data = new PyDictionary();

        data["header"] = row.Header;
        data["line"]   = row.Line;

        return new PyObjectData(ROW_TYPE_NAME, data);
    }

    /// <summary>
    /// Simple helper method that creates the correct Row data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="header"></param>
    /// <param name="fieldTypes"></param>
    /// <returns></returns>
    public static Row FromMySqlDataReader(MySqlDataReader reader, PyList<PyString> header, FieldType[] fieldTypes)
    {
        PyList row = new PyList(reader.FieldCount);

        for (int i = 0; i < reader.FieldCount; i++)
            row[i] = IDatabaseConnection.ObjectFromColumn(reader, fieldTypes[i], i);

        return new Row(header, row);
    }

    /// <summary>
    /// Simple helper method that creates the correct Row data off a result row and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static Row FromMySqlDataReader(IDatabaseConnection connection, MySqlDataReader reader)
    {
        PyList<PyString> header = new PyList<PyString>(reader.FieldCount);
        PyList           row    = new PyList(reader.FieldCount);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            header[i] = reader.GetName(i);
            row[i]    = IDatabaseConnection.ObjectFromColumn(reader, connection.GetFieldType(reader, i), i);
        }
            
        return new Row(header, row);
    }
}