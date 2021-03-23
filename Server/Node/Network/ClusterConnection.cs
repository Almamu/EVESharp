using System;
using Common.Logging;
using Common.Network;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace Node.Network
{
    public class ClusterConnection : EVEClientSocket
    {
        public ClusterConnection(Logger logger) : base(logger.CreateLogChannel("ClusterConnection"))
        {
        }
    }
}