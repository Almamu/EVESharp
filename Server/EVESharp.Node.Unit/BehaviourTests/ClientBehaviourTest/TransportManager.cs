using System;
using System.Net.Http;
using EVESharp.Common.Configuration;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Transports;
using Serilog;

namespace EVESharp.Node.Unit.BehaviourTests.ClientBehaviourTest;

/// <summary>
/// Custom transport manager for client behaviour tests
/// </summary>
public class TransportManager : Server.Shared.Transports.TransportManager
{
    public TransportManager (HttpClient httpClient, ILogger logger) : base (httpClient, logger) { }
    public event Action <TestEveClientSocket> OnNewTransportOpen;

    public override MachoServerTransport OpenServerTransport (IMachoNet machoNet, MachoNet configuration)
    {
        return this.ServerTransport = new TestMachoServerTransport (configuration.Port, machoNet, this.Log.ForContext <TestMachoServerTransport> ());
    }

    public override IMachoTransport OpenNewTransport (IMachoNet machoNet, string ip, ushort port)
    {
        // tests only do this for node <-> proxy transports
        TestEveClientSocket socket = new TestEveClientSocket ();
        
        IMachoTransport transport = base.NewTransport (machoNet, socket);
        
        this.OnNewTransportOpen?.Invoke (socket);
        
        return transport;
    }
}