using System.Configuration;

namespace PythonTypes.Types.Primitives
{
    public class PyBuffer : PyDataType
    {
        public byte[] Value { get; }
        public int Length { get { return this.Value.Length; } }

        public PyBuffer(byte [] value) : base(PyObjectType.Buffer)
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
    }
}