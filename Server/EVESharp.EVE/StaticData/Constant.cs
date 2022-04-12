namespace EVESharp.EVE.StaticData;

public class Constant
{
    public string Name  { get; }
    public long   Value { get; }

    public Constant (string name, long value)
    {
        Name  = name;
        Value = value;
    }

    public static implicit operator long (Constant constant)
    {
        return constant.Value;
    }

    public static implicit operator int (Constant constant)
    {
        return (int) constant.Value;
    }
}