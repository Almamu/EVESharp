using System.IO;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    class ClientSessionData
    {
        private const string TYPE_NAME = "macho.sessionInitialState";
        public PyDictionary session = new PyDictionary();
        public int clientID = 0;

        public static implicit operator PyDataType(ClientSessionData sessionData)
        {
            return new PyObjectData(TYPE_NAME, new PyTuple(new PyDataType[]
            {
                sessionData.session, sessionData.clientID
            }));
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

            result.session = arguments[0] as PyDictionary;
            result.clientID = arguments[1] as PyInteger;

            return result;
        }
    }
}