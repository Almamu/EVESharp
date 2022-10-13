using System;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Types;

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
        get => new Row (this.Header, this.Rows [index]);
        set => this.Rows.Add (value.Line);
    }

    public Rowset (PyList <PyString> headers)
    {
        this.Header = headers;
        this.Rows   = new PyList <PyList> ();
    }

    public Rowset (PyList <PyString> headers, PyList <PyList> rows)
    {
        this.Header = headers;
        this.Rows   = rows;
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