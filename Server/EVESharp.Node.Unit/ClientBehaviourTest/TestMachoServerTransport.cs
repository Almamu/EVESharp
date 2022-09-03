using System;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Transports;
using Serilog;

namespace EVESharp.Node.Unit.ClientBehaviourTest;

public class TestMachoServerTransport : MachoServerTransport
{
    public TestMachoServerTransport (int port, IMachoNet machoNet, ILogger logger) : base (port, machoNet, logger) { }

    public new void Listen ()
    {
        // do not really listen
    }

    /// <summary>
    /// Simulates a new connection to the server to initiate the proper mechanisms
    /// </summary>
    public TestEveClientSocket SimulateNewConnection ()
    {
        TestEveClientSocket socket = new TestEveClientSocket ();
        
        this.OnConnectionAccepted (socket);

        return socket;
    }
}