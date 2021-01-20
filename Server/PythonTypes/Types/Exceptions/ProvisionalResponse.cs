using System;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Types.Exceptions
{
    /// <summary>
    /// Placeholder exception to tell the client we're sending a ProvisionalResponse to the call
    /// </summary>
    public class ProvisionalResponse : Exception
    {
        public const int DEFAULT_TIMEOUT = 480;
        public PyString EventID { get; }
        public PyTuple Arguments { get; }
        public int Timeout { get; } = DEFAULT_TIMEOUT;

        public ProvisionalResponse(PyString eventID, PyTuple arguments, int timeout = DEFAULT_TIMEOUT) : base()
        {
            this.EventID = eventID;
            this.Arguments = arguments;
            this.Timeout = timeout;
        }
    }
}