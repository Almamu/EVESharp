using System;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public class ClusterConnection : EVEClientSocket
    {
        public ClusterConnection(Logger logger) : base(logger.CreateLogChannel("ClusterConnection"))
        {
        }
    }
}