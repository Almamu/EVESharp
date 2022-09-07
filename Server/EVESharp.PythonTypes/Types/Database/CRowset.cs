using System.Data;
using System.Data.Common;
using System.IO;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.PythonTypes.Types.Database;

/// <summary>
/// Helper class to work with dbutil.CRowset types to be sent to the EVE Online client
/// </summary>
public class CRowset
{
    private const string               TYPE_NAME = "dbutil.CRowset";
    public        DBRowDescriptor      Header  { get; }
    private       PyList <PyString>    Columns { get; set; }
    private       PyList <PyPackedRow> Rows    { get; }

    public virtual PyPackedRow this [int index]
    {
        get => Rows [index];
        set => Rows [index] = value;
    }

    public CRowset (DBRowDescriptor descriptor)
    {
        Header = descriptor;
        Rows   = new PyList <PyPackedRow> ();

        this.PrepareColumnNames ();
    }

    public CRowset (DBRowDescriptor descriptor, PyList <PyPackedRow> rows)
    {
        Header = descriptor;
        Rows   = rows;

        this.PrepareColumnNames ();
    }

    /// <summary>
    /// Creates the columns list based off the DBRowDescriptor of the header
    /// </summary>
    private void PrepareColumnNames ()
    {
        Columns = new PyList <PyString> (Header.Columns.Count);

        int index = 0;

        foreach (DBRowDescriptor.Column column in Header.Columns)
            Columns [index++] = column.Name;
    }

    /// <summary>
    /// Adds a new <seealso cref="PyPackedRow" /> to the result data of the CRowset
    /// </summary>
    /// <param name="row">The new row to add</param>
    public void Add (PyPackedRow row)
    {
        Rows.Add (row);
    }

    public static implicit operator PyDataType (CRowset rowset)
    {
        PyDictionary keywords = new PyDictionary ();

        keywords ["header"]  = rowset.Header;
        keywords ["columns"] = rowset.Columns;

        return new PyObject (
            true,
            new PyTuple (2)
            {
                [0] = new PyTuple (1) {[0] = new PyToken (TYPE_NAME)},
                [1] = keywords
            },
            rowset.Rows
        );
    }

    public static implicit operator CRowset (PyObject data)
    {
        if (data.Header [0] is PyToken == false || data.Header [0] as PyToken != TYPE_NAME)
            throw new InvalidDataException ($"Expected PyObject of type {data}");

        DBRowDescriptor descriptor = (data.Header [1] as PyDictionary) ["header"];

        return new CRowset (descriptor, data.List.GetEnumerable <PyPackedRow> ());
    }

    /// <summary>
    /// Helper method to instantiate a dbutil.CRowset type from a MySqlDataReader, this consumes the result
    /// but does not close it, so calling code has to take care of this. Ideally, please use "using" statements
    /// </summary>
    /// <param name="connection">The connection used</param>
    /// <param name="reader">The reader to use as source of the information</param>
    /// <returns>The CRowset object ready to be used</returns>
    public static CRowset FromDataReader (IDatabaseConnection connection, DbDataReader reader)
    {
        DBRowDescriptor descriptor = DBRowDescriptor.FromDataReader (connection, reader);
        CRowset         rowset     = new CRowset (descriptor);

        while (reader.Read ())
            rowset.Add (PyPackedRow.FromDataReader (reader, descriptor));

        return rowset;
    }
}