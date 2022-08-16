using System.Net.Sockets;
using EVESharp.Common.Network;
using Serilog;

namespace EVESharp.Node.Unit.ClientBehaviourTest;

public class TestEveClientSocket : EVEClientSocket
{
    public TestEveClientSocket (ILogger logChannel) : base (logChannel) { }
}