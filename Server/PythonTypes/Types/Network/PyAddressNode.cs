using System.IO;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    public class PyAddressNode : PyAddress
    {
        /// <summary>
        /// The related node ID
        /// </summary>
        public PyInteger NodeID { get; }
        /// <summary>
        /// The related callID if needed
        /// </summary>
        public PyInteger CallID { get; }
        /// <summary>
        /// The related service
        /// </summary>
        public PyString Service { get; }

        public PyAddressNode() : base(TYPE_NODE)
        {
        }

        public PyAddressNode(PyInteger nodeID) : base(TYPE_NODE)
        {
            this.NodeID = nodeID;
        }

        public PyAddressNode(PyInteger nodeID, PyInteger callID = null, PyString service = null) : this(nodeID)
        {
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
                throw new InvalidDataException($"Trying to cast a different PyAddress ({type}) to PyAddressAny");

            return new PyAddressNode(
                data[1] is PyNone ? null : data[1] as PyInteger,
                data[3] is PyNone ? null : data[3] as PyInteger,
                data[2] is PyNone ? null : data[2] as PyString
            );
        }
    }
}