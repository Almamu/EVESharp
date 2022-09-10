namespace EVESharp.Types;

public class PyObjectData : PyDataType
{
    public PyString   Name      { get; }
    public PyDataType Arguments { get; }

    public PyObjectData (PyString name, PyDataType arguments)
    {
        this.Name      = name;
        this.Arguments = arguments;
    }

    public override int GetHashCode ()
    {
        return (this.Name?.GetHashCode () ?? PyNone.HASH_VALUE) ^ (this.Arguments?.GetHashCode () ?? PyNone.HASH_VALUE) ^ 0x69548514;
    }
}