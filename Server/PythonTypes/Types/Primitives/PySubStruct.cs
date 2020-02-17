namespace PythonTypes.Types.Primitives
{
    public class PySubStruct : PyDataType
    {
        public PyDataType Definition { get; set; }

        public PySubStruct(PyDataType definition) : base(PyObjectType.SubStruct)
        {
            this.Definition = definition;
        }
    }
}