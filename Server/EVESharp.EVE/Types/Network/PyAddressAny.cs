using System.IO;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Types.Network;

public class PyAddressAny : PyAddress
{
    public PyInteger ID { get; }
    /// <summary>
    /// The related service
    /// </summary>
    public PyString Service { get; }

    public PyAddressAny (PyInteger id, PyString service = null) : base (TYPE_ANY)
    {
        this.Service = service;
        this.ID      = id;
    }

    public static implicit operator PyDataType (PyAddressAny value)
    {
        return new PyObjectData (
            OBJECT_TYPE,
            new PyTuple (3)
            {
                [0] = value.Type,
                [1] = value.Service,
                [2] = value.ID
            }
        );
    }

    public static implicit operator PyAddressAny (PyObjectData value)
    {
        if (value.Name != OBJECT_TYPE)
            throw new InvalidDataException ($"Expected {OBJECT_TYPE} for PyAddress object, got {value.Name}");

        PyTuple  data = value.Arguments as PyTuple;
        PyString type = data [0] as PyString;

        if (type != TYPE_ANY)
            throw new InvalidDataException ("Trying to cast unknown object to PyAddressAny");

        return new PyAddressAny (
            data [2] as PyInteger,
            data [1] as PyString
        );
    }
}