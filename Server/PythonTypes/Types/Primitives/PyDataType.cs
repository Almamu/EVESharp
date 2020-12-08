using System.Collections.Generic;

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

        public static implicit operator PyDataType(sbyte value)
        {
            return new PyInteger(value);
        }

        public static implicit operator PyDataType(long? value)
        {
            if (value == null)
                return new PyNone();

            return new PyInteger((long) value);
        }

        public static implicit operator PyDataType(int? value)
        {
            if (value == null)
                return new PyNone();

            return new PyInteger((int) value);
        }

        public static implicit operator PyDataType(short? value)
        {
            if (value == null)
                return new PyNone();

            return new PyInteger((short) value);
        }

        public static implicit operator PyDataType(byte? value)
        {
            if (value == null)
                return new PyNone();

            return new PyInteger((byte) value);
        }

        public static implicit operator PyDataType(sbyte? value)
        {
            if (value == null)
                return new PyNone();

            return new PyInteger((sbyte) value);
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

        public static implicit operator PyDataType(float? value)
        {
            if (value == null)
                return new PyNone();

            return new PyDecimal((float) value);
        }

        public static implicit operator PyDataType(double? value)
        {
            if (value == null)
                return new PyNone();

            return new PyDecimal((double) value);
        }

        public static implicit operator PyDataType(bool? value)
        {
            if (value == null)
                return new PyNone();

            return new PyBool((bool) value);
        }

        public static implicit operator PyDataType(Dictionary<PyDataType, PyDataType> value)
        {
            return new PyDictionary(value);
        }
    }
}