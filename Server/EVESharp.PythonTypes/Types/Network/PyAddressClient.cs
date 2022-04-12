using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Types.Network;

public class PyAddressClient : PyAddress
{
    /// <summary>
    /// Related clientID
    /// </summary>
    public PyInteger ClientID { get; set; }
    /// <summary>
    /// The callID for the request/response
    /// </summary>
    public PyInteger CallID { get; }
    /// <summary>
    /// The related service
    /// </summary>
    public PyString Service { get; }

    public PyAddressClient () : base (TYPE_CLIENT) { }

    public PyAddressClient (PyInteger clientID) : base (TYPE_CLIENT)
    {
        ClientID = clientID;
    }

    public PyAddressClient (PyInteger clientID, PyInteger callID = null, PyString service = null) : this (clientID)
    {
        CallID  = callID;
        Service = service;
    }

    public static implicit operator PyDataType (PyAddressClient value)
    {
        return new PyObjectData (
            OBJECT_TYPE,
            new PyTuple (4)
            {
                [0] = value.Type,
                [1] = value.ClientID,
                [2] = value.CallID,
                [3] = value.Service
            }
        );
    }

    public static implicit operator PyAddressClient (PyObjectData value)
    {
        if (value.Name != OBJECT_TYPE)
            throw new InvalidDataException ($"Expected {OBJECT_TYPE} for PyAddress object, got {value.Name}");

        PyTuple  data = value.Arguments as PyTuple;
        PyString type = data [0] as PyString;

        if (type != TYPE_CLIENT)
            throw new InvalidDataException ($"Trying to cast a different PyAddress ({type}) to PyAddressClient");

        return new PyAddressClient (
            data [1] as PyInteger,
            data [2] as PyInteger,
            data [3] as PyString
        );
    }
}