using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PyBuffer : PyDataType
    {
        public override int GetHashCode()
        {
            return (int) CRC32.Checksum(this.Value);
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

        public static implicit operator PyBuffer(byte[] value)
        {
            return new PyBuffer(value);
        }
    }
}