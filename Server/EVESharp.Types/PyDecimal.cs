using System.Globalization;

namespace EVESharp.Types;

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
        this.Value       = value;
        this.DecimalType = DecimalTypeEnum.Double;
    }

    public PyDecimal (float value)
    {
        this.Value       = value;
        this.DecimalType = DecimalTypeEnum.Float;
    }

    private bool Equals (PyDecimal other)
    {
        if (ReferenceEquals (null, other)) return false;

        return this.Value.Equals (other.Value);
    }

    public override int GetHashCode ()
    {
        return this.Value.GetHashCode ();
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
        return this.Value.ToString (CultureInfo.InvariantCulture);
    }
}