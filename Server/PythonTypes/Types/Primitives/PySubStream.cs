namespace PythonTypes.Types.Primitives
{
    public class PySubStream : PyDataType
    {
        public PyDataType Stream { get; }

        public PySubStream(PyDataType stream) : base(PyObjectType.SubStream)
        {
            this.Stream = stream;
        }
    }
}