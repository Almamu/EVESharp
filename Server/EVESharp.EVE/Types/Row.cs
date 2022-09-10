using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Types;

public class Row
{
    /// <summary>
    /// Type of row
    /// </summary>
    private const string ROW_TYPE_NAME = "util.Row";
    /// <summary>
    /// The columns for this row
    /// </summary>
    public PyList <PyString> Header { get; }
    /// <summary>
    /// The values for each column
    /// </summary>
    public PyList Line { get; }

    public Row (PyList <PyString> header, PyList line)
    {
        Header = header;
        Line   = line;
    }

    public static implicit operator PyDataType (Row row)
    {
        PyDictionary data = new PyDictionary ();

        data ["header"] = row.Header;
        data ["line"]   = row.Line;

        return new PyObjectData (ROW_TYPE_NAME, data);
    }
}