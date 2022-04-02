namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PyObjectData : PyDataType
    {
        public PyString Name { get; }
        public PyDataType Arguments { get; }

        public PyObjectData(PyString name, PyDataType arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
        }

        public override int GetHashCode()
        {
            return (this.Name?.GetHashCode() ?? 0) << 16 | (this.Arguments?.GetHashCode() ?? 0);
        }
    }
}