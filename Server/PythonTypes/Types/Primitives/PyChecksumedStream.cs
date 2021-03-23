namespace PythonTypes.Types.Primitives
{
    public class PyChecksumedStream : PyDataType
    {
        public PyDataType Data { get; }

        public PyChecksumedStream(PyDataType data)
        {
            this.Data = data;
        }
    }
}