using EVESharp.Types;
using EVESharp.Types.Collections;
using Org.BouncyCastle.Security;

namespace EVESharp.EVE.Types;

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

    public void AddRow (int index, PyList data)
    {
        if (data.Count != Headers.Count)
            throw new InvalidParameterException ("The row doesn't have the same amount of items as the header of the IndexRowset");

        Lines [index] = data;
    }
}