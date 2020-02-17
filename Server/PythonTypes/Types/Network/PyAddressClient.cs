using System.IO;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    public class PyAddressClient : PyAddress
    {
        public PyInteger ClientID { get; set; }
        public PyInteger CallID { get; set; }
        public PyString Service { get; set; }

        public PyAddressClient() : base(TYPE_CLIENT)
        {
        }

        public PyAddressClient(int clientID) : base(TYPE_CLIENT)
        {
            this.ClientID = clientID;
        }

        public PyAddressClient(PyInteger clientID, PyInteger callID = null, PyString service = null) : base(TYPE_CLIENT)
        {
            this.ClientID = clientID;
            this.CallID = callID;
            this.Service = service;
        }
        
        public static implicit operator PyDataType(PyAddressClient value)
        {
            return new PyObjectData(
                OBJECT_TYPE,
                new PyTuple (new PyDataType []
                {
                    value.Type,
                    value.ClientID,
                    value.CallID,
                    value.Service
                })
            );
        }

        public static implicit operator PyAddressClient(PyObjectData value)
        {
            if(value.Name != OBJECT_TYPE)
                throw new InvalidDataException($"Expected {OBJECT_TYPE} for PyAddress object, got {value.Name}");

            PyTuple data = value.Arguments as PyTuple;
            PyString type = data[0] as PyString;

            if (type != TYPE_CLIENT)
                throw new InvalidDataException($"Trying to cast unknown object to PyAddressAny");
            
            return new PyAddressClient(
                data[1] is PyNone ? null : data[1] as PyInteger,
                data[2] is PyNone ? null : data[2] as PyInteger,
                data[3] is PyNone ? null : data[3] as PyString
            );
        }
    }
}