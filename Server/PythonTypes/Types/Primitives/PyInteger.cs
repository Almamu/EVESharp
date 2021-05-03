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
            return Value.GetHashCode();
        }

        public enum IntegerTypeEnum
        {
            Long,
            Int,
            Short,
            Byte
        }

        public long Value { get; }

        public IntegerTypeEnum IntegerType { get; }

        public PyInteger(long value)
        {
            this.Value = value;
            this.IntegerType = IntegerTypeEnum.Long;
        }

        public PyInteger(int value)
        {
            this.Value = value;
            this.IntegerType = IntegerTypeEnum.Int;
        }

        public PyInteger(short value)
        {
            this.Value = value;
            this.IntegerType = IntegerTypeEnum.Short;
        }

        public PyInteger(byte value)
        {
            this.Value = value;
            if (value > sbyte.MaxValue)
                this.IntegerType = IntegerTypeEnum.Short;
            else
               this.IntegerType = IntegerTypeEnum.Byte;
        }

        public PyInteger(sbyte value)
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

        public static implicit operator double?(PyInteger obj)
        {
            return obj?.Value;
        }

        public static implicit operator ulong?(PyInteger obj)
        {
            return (ulong?) obj?.Value;
        }

        public static implicit operator long?(PyInteger obj)
        {
            return obj?.Value;
        }

        public static implicit operator uint?(PyInteger obj)
        {
            return (uint?) obj?.Value;
        }

        public static implicit operator int?(PyInteger obj)
        {
            return (int?) obj?.Value;
        }

        public static implicit operator ushort?(PyInteger obj)
        {
            return (ushort?) obj?.Value;
        }

        public static implicit operator short?(PyInteger obj)
        {
            return (short?) obj?.Value;
        }

        public static implicit operator byte?(PyInteger obj)
        {
            return (byte?) obj?.Value;
        }

        public static implicit operator sbyte?(PyInteger obj)
        {
            return (sbyte?) obj?.Value;
        }

        public static implicit operator ulong(PyInteger obj)
        {
            return (ulong) obj.Value;
        }

        public static implicit operator double(PyInteger obj)
        {
            return obj.Value;
        }

        public static implicit operator long(PyInteger obj)
        {
            return obj.Value;
        }

        public static implicit operator uint(PyInteger obj)
        {
            return (uint) obj.Value;
        }

        public static implicit operator int(PyInteger obj)
        {
            return (int) obj.Value;
        }

        public static implicit operator ushort(PyInteger obj)
        {
            return (ushort) obj.Value;
        }

        public static implicit operator short(PyInteger obj)
        {
            return (short) obj.Value;
        }

        public static implicit operator byte(PyInteger obj)
        {
            return (byte) obj.Value;
        }

        public static implicit operator sbyte(PyInteger obj)
        {
            return (sbyte) obj.Value;
        }
        
        public static implicit operator PyInteger(ulong value)
        {
            return new PyInteger((long) value);
        }
        
        public static implicit operator PyInteger(long value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyInteger(uint value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyInteger(int value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyInteger(ushort value)
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

        public static implicit operator PyInteger(sbyte value)
        {
            return new PyInteger(value);
        }

        public override string ToString()
        {
            return this.Value.ToString();
        }
    }
}