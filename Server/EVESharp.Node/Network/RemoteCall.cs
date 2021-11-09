using System;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    /// <summary>
    /// Stores information for the calls that are waiting an answer from a client/node
    /// </summary>
    public class RemoteCall
    {
        /// <summary>
        /// Related client
        /// </summary>
        public Client Client { get; set; }
        /// <summary>
        /// Any extra information for this call, this can store anything useful
        /// </summary>
        public object ExtraInfo { get; set; }
        /// <summary>
        /// The function to call when the call result is received
        /// </summary>
        public Action<RemoteCall, PyDataType> Callback { get; set; }
        /// <summary>
        /// The function to call when the timeout is reached
        /// </summary>
        public Action<RemoteCall> TimeoutCallback { get; set; }
        /// <summary>
        /// Related node
        /// </summary>
        public int NodeID { get; set; }
    }
}