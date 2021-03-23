namespace PythonTypes.Types.Primitives
{
    public class PySubStream : PyDataType
    {
        public PyDataType Stream { get; }

        public PySubStream(PyDataType stream)
        {
            this.Stream = stream;
        }
    }
}