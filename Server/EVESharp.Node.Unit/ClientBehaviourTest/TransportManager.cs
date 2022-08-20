using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.Common.Network;
using EVESharp.Common.Network.Sockets;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Transports;
using Serilog;

namespace EVESharp.Node.Unit.ClientBehaviourTest;

/// <summary>
/// Custom transport manager for client behaviour tests
/// </summary>
public class TransportManager : Server.Shared.Transports.TransportManager
{
    public new MachoServerTransport ServerTransport { get; private set; }
    public TransportManager (HttpClient httpClient, ILogger logger) : base (httpClient, logger) { }

    public override IMachoTransport NewTransport (IMachoNet machoNet, IEVESocket socket)
    {
        return base.NewTransport (machoNet, socket);
    }

    public override MachoServerTransport OpenServerTransport (IMachoNet machoNet, MachoNet configuration)
    {
        return this.ServerTransport = new TestMachoServerTransport (configuration.Port, machoNet, Log.ForContext <TestMachoServerTransport> ());
    }
}