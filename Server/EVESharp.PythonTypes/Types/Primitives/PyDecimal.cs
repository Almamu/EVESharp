using System;
using System.Globalization;

namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PyDecimal : PyDataType
    {
        public enum DecimalTypeEnum
        {
            Double,
            Float
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public double Value { get; }
        public DecimalTypeEnum DecimalType { get; }

        public PyDecimal(double value)
        {
            this.Value = value;
            this.DecimalType = DecimalTypeEnum.Double;
        }

        public PyDecimal(float value)
        {
            this.Value = value;
            this.DecimalType = DecimalTypeEnum.Float;
        }
        
        public static bool operator >(PyDecimal obj, PyDecimal value)
        {
            return obj.Value > value.Value;
        }
        
        public static bool operator >=(PyDecimal obj, PyDecimal value)
        {
            return obj.Value >= value.Value;
        }

        public static bool operator <(PyDecimal obj, PyDecimal value)
        {
            return obj.Value < value.Value;
        }

        public static bool operator <=(PyDecimal obj, PyDecimal value)
        {
            return obj.Value <= value.Value;
        }

        public static bool operator ==(PyDecimal obj, PyDecimal value)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj.Value.Equals(value);
        }

        public static bool operator !=(PyDecimal obj, PyDecimal value)
        {
            return !(obj == value);
        }

        public static implicit operator double(PyDecimal obj)
        {
            return obj.Value;
        }

        public static implicit operator double?(PyDecimal obj)
        {
            return obj?.Value;
        }

        public static explicit operator float(PyDecimal obj)
        {
            return (float) obj.Value;
        }

        public static explicit operator float?(PyDecimal obj)
        {
            return (float?) obj?.Value;
        }

        public static implicit operator PyDecimal(double value)
        {
            return new PyDecimal(value);
        }

        public static explicit operator PyDecimal(float value)
        {
            return new PyDecimal(value);
        }

        public override string ToString()
        {
            return this.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}