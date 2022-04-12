namespace EVESharp.PythonTypes.Types.Primitives;

public class PyBuffer : PyDataType
{
    public byte [] Value  { get; }
    public int     Length => Value.Length;

    public PyBuffer (byte [] value)
    {
        Value = value;
    }

    public override int GetHashCode ()
    {
        return (int) CRC32.Checksum (Value);
    }

    public static implicit operator byte [] (PyBuffer obj)
    {
        return obj.Value;
    }

    public static implicit operator PyBuffer (byte [] value)
    {
        return new PyBuffer (value);
    }
}