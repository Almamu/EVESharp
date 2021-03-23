using System.IO;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    /// <summary>
    /// Helper class to work with macho.MachoAddress objects in <seealso cref="PyPacket"/>
    /// </summary>
    public abstract class PyAddress
    {
        protected const string OBJECT_TYPE = "macho.MachoAddress";
        protected const string TYPE_ANY = "A";
        protected const string TYPE_NODE = "N";
        protected const string TYPE_BROADCAST = "B";
        protected const string TYPE_CLIENT = "C";

        protected PyString Type { get; }

        protected PyAddress(PyString type)
        {
            this.Type = type;
        }

        public static implicit operator PyAddress(PyObjectData value)
        {
            if (value.Name != OBJECT_TYPE)
                throw new InvalidDataException($"Expected {OBJECT_TYPE} for PyAddress object, got {value.Name}");

            PyTuple data = value.Arguments as PyTuple;
            PyString type = data[0] as PyString;

            switch (type)
            {
                case TYPE_ANY:
                    return (PyAddressAny) value;

                case TYPE_CLIENT:
                    return (PyAddressClient) value;

                case TYPE_NODE:
                    return (PyAddressNode) value;

                case TYPE_BROADCAST:
                    return (PyAddressBroadcast) value;

                default:
                    throw new InvalidDataException($"Unknown PyAddress type {type}");
            }
        }

        public static implicit operator PyDataType(PyAddress address)
        {
            if (address is PyAddressAny any)
                return any;
            if (address is PyAddressBroadcast broadcast)
                return broadcast;
            if (address is PyAddressClient client)
                return client;
            if (address is PyAddressNode node)
                return node;
            throw new InvalidDataException();
        }
    }
}