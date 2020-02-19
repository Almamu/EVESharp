namespace PythonTypes.Types.Primitives
{
    public class PyDataType
    {
        private PyObjectType Type { get; }

        protected PyDataType(PyObjectType type)
        {
            this.Type = type;
        }

        public static implicit operator PyDataType(string str)
        {
            if (str == null)
                return new PyNone();

            return new PyString(str);
        }

        public static implicit operator PyDataType(long value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(int value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(short value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(byte value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(byte[] value)
        {
            if (value == null)
                return new PyNone();

            return new PyBuffer(value);
        }

        public static implicit operator PyDataType(float value)
        {
            return new PyDecimal(value);
        }

        public static implicit operator PyDataType(double value)
        {
            return new PyDecimal(value);
        }

        public static implicit operator PyDataType(bool value)
        {
            return new PyBool(value);
        }
    }
}