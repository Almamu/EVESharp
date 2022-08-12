using System;
using System.Net.Http;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using Serilog;

namespace EVESharp.EVE.Network.Transports;

public class MachoServerTransport : EVEServerSocket
{
    public IMachoNet MachoNet { get; }

    public MachoServerTransport (int port, IMachoNet machoNet, ILogger logger) : base (port, logger)
    {
        this.MachoNet   = machoNet;
    }

    public new void Listen ()
    {
        base.Listen ();
        this.BeginAccept (this.AcceptCallback);
    }

    protected void AcceptCallback (IAsyncResult ar)
    {
        EVEServerSocket serverSocket = ar.AsyncState as EVEServerSocket;
        EVEClientSocket clientSocket = serverSocket.EndAccept (ar);

        // got a new transport, register it
        this.MachoNet.TransportManager.NewTransport (this.MachoNet, clientSocket);

        // begin accepting again
        this.BeginAccept (this.AcceptCallback);
    }
}