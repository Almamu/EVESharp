using System;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace EVE.Packets.Exceptions
{
    /// <summary>
    /// Placeholder exception to tell the client we're sending a ProvisionalResponse to the call
    /// </summary>
    public class ProvisionalResponse : Exception
    {
        /// <summary>
        /// The default timeout for a provisional response
        /// </summary>
        public const int DEFAULT_TIMEOUT = 480;
        /// <summary>
        /// The eventID to receive the provisional response
        /// </summary>
        public PyString EventID { get; }
        /// <summary>
        /// The data to pass on to the eventID
        /// </summary>
        public PyTuple Arguments { get; }
        public int Timeout { get; }

        public ProvisionalResponse(PyString eventID, PyTuple arguments, int timeout = DEFAULT_TIMEOUT)
        {
            this.EventID = eventID;
            this.Arguments = arguments;
            this.Timeout = timeout;
        }
    }
}