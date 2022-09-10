using System.IO;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Types;

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
}