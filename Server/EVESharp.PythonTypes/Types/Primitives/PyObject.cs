using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.PythonTypes.Types.Primitives;

public class PyObject : PyDataType
{
    public bool         IsType2    { get; }
    public PyTuple      Header     { get; }
    public PyList       List       { get; }
    public PyDictionary Dictionary { get; }

    public PyObject (bool isType2, PyTuple header, PyList list = null, PyDictionary dict = null)
    {
        IsType2    = isType2;
        Header     = header;
        List       = list ?? new PyList ();
        Dictionary = dict ?? new PyDictionary ();
    }

    public override int GetHashCode ()
    {
        return (IsType2 ? 1 : 0) ^ Header.GetHashCode () ^ List.GetHashCode () ^ Dictionary.GetHashCode () ^ 0x36120485;
    }
}