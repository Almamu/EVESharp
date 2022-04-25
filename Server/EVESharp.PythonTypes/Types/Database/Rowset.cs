using System;
using System.Data;
using System.Data.Common;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

/// <summary>
/// Helper class to work with util.Rowset types to be sent to the EVE Online client
/// </summary>
public class Rowset
{
    /// <summary>
    /// Type of the rowser
    /// </summary>
    private const string TYPE_NAME = "util.Rowset";
    /// <summary>
    /// Type of every row
    /// </summary>
    private const string ROW_TYPE_NAME = "util.Row";

    /// <summary>
    /// Headers of the rowset
    /// </summary>
    public PyList <PyString> Header { get; }
    /// <summary>
    /// All the rows of the Rowset
    /// </summary>
    public PyList <PyList> Rows { get; }

    public Row this [int index]
    {
        get => new Row (Header, Rows [index]);
        set => Rows.Add (value.Line);
    }

    public Rowset (PyList <PyString> headers)
    {
        Header = headers;
        Rows   = new PyList <PyList> ();
    }

    public Rowset (PyList <PyString> headers, PyList <PyList> rows)
    {
        Header = headers;
        Rows   = rows;
    }

    /// <summary>
    /// Simple helper method that creates a correct Rowset ready to be sent
    /// to the EVE Online client based on the given MySqlDataReader
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static Rowset FromDataReader (IDatabaseConnection connection, DbDataReader reader)
    {
        connection.GetDatabaseHeaders (reader, out PyList <PyString> headers, out FieldType [] fieldTypes);
        Rowset result = new Rowset (headers);

        while (reader.Read ())
        {
            PyList row = new PyList (reader.FieldCount);

            for (int i = 0; i < reader.FieldCount; i++)
                row [i] = IDatabaseConnection.ObjectFromColumn (reader, fieldTypes [i], i);

            result.Rows.Add (row);
        }

        return result;
    }

    public static implicit operator PyDataType (Rowset rowset)
    {
        // create the main container for the util.Rowset
        PyDictionary arguments = new PyDictionary
        {
            // store the header and specify the type of rows the Rowset contains
            ["header"]   = rowset.Header,
            ["RowClass"] = new PyToken (ROW_TYPE_NAME),
            ["lines"]    = rowset.Rows
        };

        return new PyObjectData (TYPE_NAME, arguments);
    }

    public static implicit operator Rowset (PyDataType from)
    {
        if (from is not PyObjectData data || data.Name != TYPE_NAME)
            throw new Exception ($"Expected an object data of name {TYPE_NAME}");
        if (data.Arguments is not PyDictionary args)
            throw new Exception ("Expected object data with a dictionary");

        // ensure the dictionary has the required keys
        if (args.TryGetValue ("header", out PyList header) == false)
            throw new Exception ("Rowset header cannot be found");
        if (args.TryGetValue ("RowClass", out PyToken rowClass) == false || rowClass.Token != ROW_TYPE_NAME)
            throw new Exception ("Unknown row header");
        if (args.TryGetValue ("lines", out PyList lines) == false)
            throw new Exception ("Unknown row lines format");

        // return the new rowset with the correct values
        return new Rowset (header.GetEnumerable <PyString> (), lines.GetEnumerable <PyList> ());
    }
}