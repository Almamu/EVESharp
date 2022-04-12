namespace EVESharp.PythonTypes.Types.Primitives;

public class PyObjectData : PyDataType
{
    public PyString   Name      { get; }
    public PyDataType Arguments { get; }

    public PyObjectData (PyString name, PyDataType arguments)
    {
        Name      = name;
        Arguments = arguments;
    }

    public override int GetHashCode ()
    {
        return (Name?.GetHashCode () ?? PyNone.HASH_VALUE) ^ (Arguments?.GetHashCode () ?? PyNone.HASH_VALUE) ^ 0x69548514;
    }
}