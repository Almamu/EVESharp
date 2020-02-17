using System.IO;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    public class PyAddressAny : PyAddress
    {
        public PyInteger ID { get; }
        public PyString Service { get; }

        public PyAddressAny(PyInteger ID, PyString service = null) : base(TYPE_ANY)
        {
            this.Service = service;
            this.ID = ID;
        }

        public static implicit operator PyDataType(PyAddressAny value)
        {
            return new PyObjectData(
                OBJECT_TYPE,
                new PyTuple (new PyDataType []
                {
                    value.Type,
                    value.Service,
                    value.ID
                })
            );
        }

        public static implicit operator PyAddressAny(PyObjectData value)
        {
            if(value.Name != OBJECT_TYPE)
                throw new InvalidDataException($"Expected {OBJECT_TYPE} for PyAddress object, got {value.Name}");

            PyTuple data = value.Arguments as PyTuple;
            PyString type = data[0] as PyString;

            if (type != TYPE_ANY)
                throw new InvalidDataException($"Trying to cast unknown object to PyAddressAny");
            
            return new PyAddressAny(
                data[2] is PyNone ? null : data[2] as PyInteger,
                data[1] as PyString
            );
        }
    }
}