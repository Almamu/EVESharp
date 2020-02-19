namespace PythonTypes.Types.Primitives
{
    public class PyNone : PyDataType
    {
        public PyNone() : base(PyObjectType.None)
        {
        }

        public static implicit operator string(PyNone ignored)
        {
            return null;
        }

        public static implicit operator byte[](PyNone ignored)
        {
            return null;
        }

        public static implicit operator double(PyNone ignored)
        {
            return 0.0;
        }

        public static implicit operator float(PyNone ignored)
        {
            return 0;
        }

        public static implicit operator long(PyNone ignored)
        {
            return 0;
        }

        public static implicit operator int(PyNone ignored)
        {
            return 0;
        }

        public static implicit operator short(PyNone ignored)
        {
            return 0;
        }

        public static implicit operator byte(PyNone ignored)
        {
            return 0;
        }

        public static implicit operator bool(PyNone ignored)
        {
            return false;
        }
    }
}