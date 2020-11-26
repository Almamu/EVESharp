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

        public static implicit operator double?(PyNone ignored)
        {
            return null;
        }

        public static implicit operator float?(PyNone ignored)
        {
            return null;
        }

        public static implicit operator long?(PyNone ignored)
        {
            return null;
        }

        public static implicit operator int?(PyNone ignored)
        {
            return null;
        }

        public static implicit operator short?(PyNone ignored)
        {
            return null;
        }

        public static implicit operator byte?(PyNone ignored)
        {
            return null;
        }

        public static implicit operator sbyte?(PyNone ignored)
        {
            return null;
        }
        public static implicit operator bool?(PyNone ignored)
        {
            return null;
        }
    }
}