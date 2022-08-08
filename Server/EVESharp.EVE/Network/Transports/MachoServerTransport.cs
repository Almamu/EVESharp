using System;
using System.Net.Http;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using Serilog;

namespace EVESharp.EVE.Network.Transports;

public class MachoServerTransport : EVEServerSocket
{
    public IMachoNet  MachoNet   { get; }
    public HttpClient HttpClient { get; }

    public MachoServerTransport (int port, HttpClient httpClient, IMachoNet machoNet, ILogger logger) : base (port, logger)
    {
        this.HttpClient = httpClient;
        this.MachoNet   = machoNet;
    }

    public new void Listen ()
    {
        base.Listen ();
        this.BeginAccept (this.AcceptCallback);
    }

    private void AcceptCallback (IAsyncResult ar)
    {
        EVEServerSocket serverSocket = ar.AsyncState as EVEServerSocket;
        EVEClientSocket clientSocket = serverSocket.EndAccept (ar);

        // got a new transport, register it
        this.MachoNet.TransportManager.NewTransport (
            new MachoUnauthenticatedTransport (
                this.MachoNet, this.HttpClient, clientSocket, this.Log.ForContext <MachoUnauthenticatedTransport> (clientSocket.GetRemoteAddress ())
            )
        );

        // begin accepting again
        this.BeginAccept (this.AcceptCallback);
    }
}