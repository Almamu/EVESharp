namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PyObjectData : PyDataType
    {
        public PyToken Name { get; }
        public PyDataType Arguments { get; }

        public PyObjectData(PyToken name, PyDataType arguments)
        {
            this.Name = name;
            this.Arguments = arguments;
        }

        public override int GetHashCode()
        {
            return (this.Name?.GetHashCode() ?? 0) ^ (this.Arguments?.GetHashCode() ?? 0) ^ 0x69548514;
        }
    }
}