using System;

namespace PythonTypes.Types.Primitives
{
    public class PyInteger : PyDataType
    {
        protected bool Equals(PyInteger other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PyInteger) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, (int) IntegerType);
        }

        public enum IntegerTypeEnum
        {
            Long,
            Int,
            Short,
            Byte
        };

        public long Value { get; }

        public IntegerTypeEnum IntegerType { get; }

        public PyInteger(long value) : base(PyObjectType.Integer)
        {
            this.Value = value;
            this.IntegerType = IntegerTypeEnum.Long;
        }

        public PyInteger(int value) : base(PyObjectType.Integer)
        {
            this.Value = value;
            this.IntegerType = IntegerTypeEnum.Int;
        }

        public PyInteger(short value) : base(PyObjectType.Integer)
        {
            this.Value = value;
            this.IntegerType = IntegerTypeEnum.Short;
        }

        public PyInteger(byte value) : base(PyObjectType.Integer)
        {
            this.Value = value;
            this.IntegerType = IntegerTypeEnum.Byte;
        }

        public static bool operator ==(PyInteger obj1, PyInteger obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (ReferenceEquals(null, obj1)) return false;
            if (ReferenceEquals(null, obj2)) return false;

            return obj1.Value == obj2.Value;
        }

        public static bool operator !=(PyInteger obj1, PyInteger obj2)
        {
            return !(obj1 == obj2);
        }
        
        public static bool operator >(PyInteger obj, long value)
        {
            return obj.Value > value;
        }

        public static bool operator <(PyInteger obj, long value)
        {
            return obj.Value < value;
        }

        public static bool operator ==(PyInteger obj, long value)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj.Value == value;
        }

        public static bool operator !=(PyInteger obj, long value)
        {
            return !(obj == value);
        }

        public static bool operator >(PyInteger obj, int value)
        {
            return obj.Value > value;
        }

        public static bool operator <(PyInteger obj, int value)
        {
            return obj.Value < value;
        }

        public static bool operator ==(PyInteger obj, int value)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj.Value == value;
        }

        public static bool operator !=(PyInteger obj, int value)
        {
            return !(obj == value);
        }

        public static bool operator >(PyInteger obj, short value)
        {
            return obj.Value > value;
        }

        public static bool operator <(PyInteger obj, short value)
        {
            return obj.Value < value;
        }

        public static bool operator ==(PyInteger obj, short value)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj.Value == value;
        }

        public static bool operator !=(PyInteger obj, short value)
        {
            return !(obj == value);
        }

        public static bool operator >(PyInteger obj, byte value)
        {
            return obj.Value > value;
        }

        public static bool operator <(PyInteger obj, byte value)
        {
            return obj.Value < value;
        }

        public static bool operator ==(PyInteger obj, byte value)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj.Value == value;
        }

        public static bool operator !=(PyInteger obj, byte value)
        {
            return !(obj == value);
        }

        public static implicit operator long?(PyInteger obj)
        {
            return obj?.Value;
        }

        public static implicit operator int?(PyInteger obj)
        {
            return (int?) obj?.Value;
        }

        public static implicit operator short?(PyInteger obj)
        {
            return (short?) obj?.Value;
        }

        public static implicit operator byte?(PyInteger obj)
        {
            return (byte?) obj?.Value;
        }

        public static implicit operator long(PyInteger obj)
        {
            return obj.Value;
        }

        public static implicit operator int(PyInteger obj)
        {
            return (int) obj.Value;
        }

        public static implicit operator short(PyInteger obj)
        {
            return (short) obj.Value;
        }

        public static implicit operator byte(PyInteger obj)
        {
            return (byte) obj.Value;
        }

        public static implicit operator PyInteger(long value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyInteger(int value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyInteger(short value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyInteger(byte value)
        {
            return new PyInteger(value);
        }
    }
}