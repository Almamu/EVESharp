using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PythonTypes.Types.Primitives
{
    public class PyBuffer : PyDataType
    {
        protected bool Equals(PyBuffer other)
        {
            return this.Value.SequenceEqual(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            
            return Equals((PyBuffer) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Value);
        }

        public byte[] Value { get; }
        public int Length => this.Value.Length;

        public PyBuffer(byte[] value)
        {
            this.Value = value;
        }

        public static implicit operator byte[](PyBuffer obj)
        {
            return obj.Value;
        }

        public static explicit operator PyBuffer(byte[] value)
        {
            return new PyBuffer(value);
        }

        public static bool operator ==(PyBuffer left, PyBuffer right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(right, null)) return false;
            
            return left.Equals(right);
        }

        public static bool operator !=(PyBuffer left, PyBuffer right)
        {
            return !(left == right);
        }
    }
}