using System.Net.Http;
using EVESharp.Common.Configuration;
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

    public override MachoServerTransport OpenServerTransport (IMachoNet machoNet, MachoNet configuration)
    {
        return this.ServerTransport = new TestMachoServerTransport (configuration.Port, machoNet, Log.ForContext <TestMachoServerTransport> ());
    }
}