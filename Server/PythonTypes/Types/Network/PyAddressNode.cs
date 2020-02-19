using System.IO;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    public class PyAddressNode : PyAddress
    {
        public PyInteger NodeID { get; }
        public PyInteger CallID { get; }
        public PyString Service { get; }

        public PyAddressNode() : base(TYPE_NODE)
        {
        }

        public PyAddressNode(PyInteger nodeID) : base(TYPE_NODE)
        {
            this.NodeID = nodeID;
        }

        public PyAddressNode(PyInteger nodeID, PyInteger callID = null, PyString service = null) : base(TYPE_NODE)
        {
            this.NodeID = nodeID;
            this.CallID = callID;
            this.Service = service;
        }

        public static implicit operator PyDataType(PyAddressNode value)
        {
            return new PyObjectData(
                OBJECT_TYPE,
                new PyTuple(new PyDataType[]
                {
                    value.Type,
                    value.NodeID,
                    value.Service,
                    value.CallID
                })
            );
        }

        public static implicit operator PyAddressNode(PyObjectData value)
        {
            if (value.Name != OBJECT_TYPE)
                throw new InvalidDataException($"Expected {OBJECT_TYPE} for PyAddress object, got {value.Name}");

            PyTuple data = value.Arguments as PyTuple;
            PyString type = data[0] as PyString;

            if (type != TYPE_NODE)
                throw new InvalidDataException($"Trying to cast unknown object to PyAddressAny");

            return new PyAddressNode(
                data[1] is PyNone ? null : data[1] as PyInteger,
                data[3] is PyNone ? null : data[3] as PyInteger,
                data[2] is PyNone ? null : data[2] as PyString
            );
        }
    }
}