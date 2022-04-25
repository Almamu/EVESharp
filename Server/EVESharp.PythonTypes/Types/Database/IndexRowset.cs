using System.Data;
using System.Data.Common;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Security;

namespace EVESharp.PythonTypes.Types.Database;

/// <summary>
/// Helper class to work with util.Rowset types to be sent to the EVE Online client
/// </summary>
public class IndexRowset
{
    /// <summary>
    /// Type of the rowset
    /// </summary>
    private const string TYPE_NAME = "util.IndexRowset";
    /// <summary>
    /// Type of every row
    /// </summary>
    private const string ROW_TYPE_NAME = "util.Row";

    protected PyList <PyString>                Headers { get; }
    protected PyDictionary <PyInteger, PyList> Lines   { get; }
    /// <summary>
    /// The field used to index the Rowset
    /// </summary>
    public string IDName { get; set; }

    public IndexRowset (string idName, PyList <PyString> headers)
    {
        Headers = headers;
        Lines   = new PyDictionary <PyInteger, PyList> ();
        IDName  = idName;
    }

    public static implicit operator PyDataType (IndexRowset rowset)
    {
        PyDictionary container = new PyDictionary
        {
            {"header", rowset.Headers},
            {"RowClass", new PyToken (ROW_TYPE_NAME)},
            {"idName", rowset.IDName},
            {"items", rowset.Lines}
        };

        return new PyObjectData (TYPE_NAME, container);
    }

    protected void AddRow (int index, PyList data)
    {
        if (data.Count != Headers.Count)
            throw new InvalidParameterException ("The row doesn't have the same amount of items as the header of the IndexRowset");

        Lines [index] = data;
    }

    /// <summary>
    /// Simple helper method that creates a correct IndexRowset and returns
    /// it's PyDataType representation, ready to be sent to the EVE Online client
    /// 
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="reader">The MySqlDataReader to read the data from</param>
    /// <param name="indexField">The field to use as index for the rowset</param>
    /// <returns></returns>
    public static IndexRowset FromDataReader (IDatabaseConnection connection, DbDataReader reader, int indexField)
    {
        string indexFieldName = reader.GetName (indexField);

        connection.GetDatabaseHeaders (reader, out PyList <PyString> headers, out FieldType [] fieldTypes);

        IndexRowset rowset = new IndexRowset (indexFieldName, headers);

        while (reader.Read ())
        {
            PyList row = new PyList (reader.FieldCount);

            for (int i = 0; i < row.Count; i++)
                row [i] = IDatabaseConnection.ObjectFromColumn (reader, fieldTypes [i], i);

            rowset.AddRow (reader.GetInt32 (indexField), row);
        }

        return rowset;
    }
}