namespace EVESharp.EVE.Data.Configuration;

public class Constant
{
    public string Name  { get; }
    public long   Value { get; }

    public Constant (string name, long value)
    {
        this.Name  = name;
        this.Value = value;
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