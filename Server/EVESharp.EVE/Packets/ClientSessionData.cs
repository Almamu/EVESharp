using System.IO;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Packets;

class ClientSessionData
{
    private const string       TYPE_NAME   = "macho.sessionInitialState";
    public        PyDictionary SessionData = new PyDictionary();
    public        int          ClientID    = 0;

    public static implicit operator PyDataType(ClientSessionData sessionData)
    {
        return new PyObjectData(TYPE_NAME, new PyTuple(2)
            {
                [0] = sessionData.SessionData,
                [1] = sessionData.ClientID
            }
        );
    }

    public static implicit operator ClientSessionData(PyDataType sessionData)
    {
        PyObjectData objectData = sessionData as PyObjectData;

        if (objectData.Name != TYPE_NAME)
            throw new InvalidDataException($"Expected ObjectData of type {TYPE_NAME} but got {objectData.Name}");

        PyTuple arguments = objectData.Arguments as PyTuple;

        if (arguments.Count != 2)
            throw new InvalidDataException($"Expected tuple with two elements");

        ClientSessionData result = new ClientSessionData();

        result.SessionData = arguments[0] as PyDictionary;
        result.ClientID    = arguments[1] as PyInteger;

        return result;
    }
}