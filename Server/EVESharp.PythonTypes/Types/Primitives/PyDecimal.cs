using System.Globalization;

namespace EVESharp.PythonTypes.Types.Primitives;

public class PyDecimal : PyDataType
{
    public enum DecimalTypeEnum
    {
        Double,
        Float
    }

    public double          Value       { get; }
    public DecimalTypeEnum DecimalType { get; }

    public PyDecimal (double value)
    {
        Value       = value;
        DecimalType = DecimalTypeEnum.Double;
    }

    public PyDecimal (float value)
    {
        Value       = value;
        DecimalType = DecimalTypeEnum.Float;
    }

    private bool Equals (PyDecimal other)
    {
        if (ReferenceEquals (null, other)) return false;

        return Value.Equals (other.Value);
    }

    public override int GetHashCode ()
    {
        return Value.GetHashCode ();
    }

    public static bool operator > (PyDecimal obj, PyDecimal value)
    {
        return obj.Value > value.Value;
    }

    public static bool operator >= (PyDecimal obj, PyDecimal value)
    {
        return obj.Value >= value.Value;
    }

    public static bool operator < (PyDecimal obj, PyDecimal value)
    {
        return obj.Value < value.Value;
    }

    public static bool operator <= (PyDecimal obj, PyDecimal value)
    {
        return obj.Value <= value.Value;
    }

    public static bool operator == (PyDecimal left, PyDecimal right)
    {
        if (ReferenceEquals (left, right)) return true;
        if (ReferenceEquals (null, left)) return false;

        return left.Equals (right);
    }

    public static bool operator != (PyDecimal obj, PyDecimal value)
    {
        return !(obj == value);
    }

    public static implicit operator double (PyDecimal obj)
    {
        return obj.Value;
    }

    public static implicit operator double? (PyDecimal obj)
    {
        return obj?.Value;
    }

    public static explicit operator float (PyDecimal obj)
    {
        return (float) obj.Value;
    }

    public static explicit operator float? (PyDecimal obj)
    {
        return (float?) obj?.Value;
    }

    public static implicit operator PyDecimal (double value)
    {
        return new PyDecimal (value);
    }

    public static explicit operator PyDecimal (float value)
    {
        return new PyDecimal (value);
    }

    public override string ToString ()
    {
        return Value.ToString (CultureInfo.InvariantCulture);
    }
}