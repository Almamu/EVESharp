using System;
using System.Net.Sockets;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class MachoProxyTransport : MachoTransport
{
    public MachoProxyTransport(MachoServerTransport transport, Channel channel) :
        base(transport, new EVEClientSocket(channel), channel.Logger)
    {
        this.Socket.SetReceiveCallback(HandleProxyPacket);
    }
    
    public MachoProxyTransport(MachoTransport source) : base(source)
    {
        this.Socket.SetReceiveCallback(HandleProxyPacket);
    }

    private void HandleProxyPacket(PyDataType data)
    {
        Log.Debug(PrettyPrinter.FromDataType(data));
    }
}