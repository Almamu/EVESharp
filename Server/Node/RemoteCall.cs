using System;
using PythonTypes.Types.Primitives;

namespace Node
{
    public class RemoteCall
    {
        public Client Client { get; set; }
        public object ExtraInfo { get; set; }
        public Action<RemoteCall, PyDataType> Callback { get; set; } 
        public Action<RemoteCall> TimeoutCallback { get; set; }
    }
}