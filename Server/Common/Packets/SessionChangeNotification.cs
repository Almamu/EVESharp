using System.IO;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class SessionChangeNotification
    {
        public int clueless = 0;
        public PyDictionary changes = new PyDictionary();
        public PyList nodesOfInterest = new PyList();

        public static implicit operator PyTuple(SessionChangeNotification notification)
        {
            return new PyTuple(new PyDataType[]
                {
                    new PyTuple(new PyDataType[] {notification.clueless, notification.changes}),
                    notification.nodesOfInterest
                }
            );
        }

        public static implicit operator SessionChangeNotification(PyTuple origin)
        {
            if (origin.Count != 2)
                throw new InvalidDataException("Expected a tuple with two elements");

            if (origin[0] is PyTuple == false)
                throw new InvalidDataException("The first element must be a tuple with the session data");
            if (origin[1] is PyList == false)
                throw new InvalidDataException("The second element must be a list of nodes");

            PyTuple sessionData = origin[0] as PyTuple;

            if (sessionData[0] is PyInteger == false)
                throw new InvalidDataException("Session data doesn't contain a integer as first element");
            if (sessionData[1] is PyDictionary == false)
                throw new InvalidDataException("Session data doesn't contain a dictionary with the actual data");
            
            SessionChangeNotification scn = new SessionChangeNotification();

            scn.nodesOfInterest = origin[1] as PyList;
            scn.clueless = sessionData[0] as PyInteger;
            scn.changes = sessionData[1] as PyDictionary;

            return scn;
        }
    }
}