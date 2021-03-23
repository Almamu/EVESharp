using System.IO;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Common.Packets
{
    public class SessionChangeNotification
    {
        private int mClueless = 0;
        public PyDictionary Changes { get; init; }
        /// <summary>
        /// List of nodes interested in the session change
        ///
        /// This is used by LIVE to know what nodes need to know about the client
        /// All in all, EVESharp takes a different approach where all nodes know
        /// about all the clients so this is mainly useless for us
        /// </summary>
        public PyList NodesOfInterest { get; init; }

        public static implicit operator PyTuple(SessionChangeNotification notification)
        {
            return new PyTuple(2)
            {
                [0] = new PyTuple(2)
                {
                    [0] = notification.mClueless,
                    [1] = notification.Changes
                },
                [1] = notification.NodesOfInterest
            };
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

            SessionChangeNotification scn = new SessionChangeNotification
            {
                NodesOfInterest = origin[1] as PyList,
                mClueless = sessionData[0] as PyInteger,
                Changes = sessionData[1] as PyDictionary
            };

            return scn;
        }
    }
}