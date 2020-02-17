using Org.BouncyCastle.X509.Extension;

namespace PythonTypes.Types.Primitives
{
    public class PyChecksumedStream : PyDataType
    {
        public PyDataType Data { get; }

        public PyChecksumedStream(PyDataType data) : base(PyObjectType.ChecksumedStream)
        {
            this.Data = data;
        }
    }
}