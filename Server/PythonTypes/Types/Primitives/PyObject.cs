using PythonTypes.Types.Collections;

namespace PythonTypes.Types.Primitives
{
    public class PyObject : PyDataType
    {
        public bool IsType2 { get; }
        public PyTuple Header { get; }
        public PyList List { get; }
        public PyDictionary Dictionary { get; }

        public PyObject(bool isType2, PyTuple header, PyList list = null, PyDictionary dict = null) : base()
        {
            this.IsType2 = isType2;
            this.Header = header;
            this.List = list ?? new PyList();
            this.Dictionary = dict ?? new PyDictionary();
        }
    }
}