namespace EVESharp.PythonTypes.Types.Primitives
{
    public class PySubStruct : PyDataType
    {
        public PyDataType Definition { get; }

        public PySubStruct(PyDataType definition)
        {
            this.Definition = definition;
        }
    }
}