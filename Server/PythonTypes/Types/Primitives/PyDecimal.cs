namespace PythonTypes.Types.Primitives
{
    public class PyDecimal : PyDataType
    {
        public enum DecimalTypeEnum
        {
            Double,
            Float
        };

        protected bool Equals(PyDecimal other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PyDecimal) obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public double Value { get; }
        public DecimalTypeEnum DecimalType { get; }

        public PyDecimal(double value) : base(PyObjectType.Decimal)
        {
            this.Value = value;
            this.DecimalType = DecimalTypeEnum.Double;
        }

        public PyDecimal(float value) : base(PyObjectType.Decimal)
        {
            this.Value = value;
            this.DecimalType = DecimalTypeEnum.Float;
        }

        public static bool operator ==(PyDecimal obj1, PyDecimal obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (ReferenceEquals(null, obj1)) return false;
            if (ReferenceEquals(null, obj2)) return false;

            return obj1.Value == obj2.Value;
        }

        public static bool operator !=(PyDecimal obj1, PyDecimal obj2)
        {
            return !(obj1 == obj2);
        }


        public static bool operator >(PyDecimal obj, double value)
        {
            return obj.Value > value;
        }

        public static bool operator <(PyDecimal obj, double value)
        {
            return obj.Value < value;
        }

        public static bool operator ==(PyDecimal obj, double value)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj.Value == value;
        }

        public static bool operator !=(PyDecimal obj, double value)
        {
            return !(obj == value);
        }

        public static bool operator >(PyDecimal obj, float value)
        {
            return obj.Value > value;
        }

        public static bool operator <(PyDecimal obj, float value)
        {
            return obj.Value < value;
        }

        public static bool operator ==(PyDecimal obj, float value)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj.Value == value;
        }

        public static bool operator !=(PyDecimal obj, float value)
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

        public static implicit operator PyDecimal(double value)
        {
            return new PyDecimal(value);
        }

        public static explicit operator PyDecimal(float value)
        {
            return new PyDecimal(value);
        }
    }
}