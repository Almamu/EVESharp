using EVESharp.Common.Checksum;

namespace EVESharp.Types;

public class PyBuffer : PyDataType
{
    public byte [] Value  { get; }
    public int     Length => this.Value.Length;

    public PyBuffer (byte [] value)
    {
        this.Value = value;
    }

    public override int GetHashCode ()
    {
        return (int) CRC32.Checksum (this.Value);
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