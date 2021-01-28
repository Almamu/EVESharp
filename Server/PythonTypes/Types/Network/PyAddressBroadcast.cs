using System.IO;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Network
{
    public class PyAddressBroadcast : PyAddress
    {
        /// <summary>
        /// The service that originated the broadcast
        /// </summary>
        public PyString Service { get; set; }
        /// <summary>
        /// Narrowcast for the ids that should be notified
        /// </summary>
        public PyList IDsOfInterest { get; set; }
        /// <summary>
        /// The field by which the narrowcast will be performed
        /// </summary>
        public PyString IDType { get; set; }

        public PyAddressBroadcast(PyList idsOfInterest, PyString idType, PyString service = null) : base(TYPE_BROADCAST)
        {
            this.Service = service;
            this.IDsOfInterest = idsOfInterest;
            this.IDType = idType;
        }

        public static implicit operator PyAddressBroadcast(PyObjectData value)
        {
            if (value.Name != OBJECT_TYPE)
                throw new InvalidDataException($"Expected {OBJECT_TYPE} for PyAddress object, got {value.Name}");

            PyTuple data = value.Arguments as PyTuple;
            PyString type = data[0] as PyString;

            if (type != TYPE_BROADCAST)
                throw new InvalidDataException($"Trying to cast unknown object to PyAddressAny");

            return new PyAddressBroadcast(
                data[2] as PyList,
                data[3] as PyString,
                (data[1] is PyNone) ? null : data[1] as PyString
            );
        }

        public static implicit operator PyDataType(PyAddressBroadcast value)
        {
            return new PyObjectData(
                OBJECT_TYPE,
                new PyTuple(new PyDataType[]
                {
                    value.Type,
                    value.Service,
                    value.IDsOfInterest,
                    value.IDType,
                })
            );
        }
    }
}