using System;

namespace PythonTypes.Types.Primitives
{
    public class PyObjectData : PyDataType
    {
        public PyString Name { get; }
        public PyDataType Arguments { get; }

        public PyObjectData(PyString name, PyDataType arguments) : base(PyObjectType.ObjectData)
        {
            this.Name = name;
            this.Arguments = arguments;
        }
    }
}