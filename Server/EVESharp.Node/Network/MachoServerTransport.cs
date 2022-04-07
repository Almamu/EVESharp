using System;
using System.Net.Http;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.Node.Server.Shared;
using Serilog;

namespace EVESharp.Node.Network;

public class MachoServerTransport : EVEServerSocket
{
    public IMachoNet  MachoNet   { get; }
    public HttpClient HttpClient { get; }

    public MachoServerTransport (int port, HttpClient httpClient, IMachoNet machoNet, ILogger logger) : base (port, logger)
    {
        HttpClient = httpClient;
        MachoNet   = machoNet;
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
        MachoNet.TransportManager.NewTransport (
            new MachoUnauthenticatedTransport (
                MachoNet, HttpClient, clientSocket, Log.ForContext <MachoUnauthenticatedTransport> (clientSocket.GetRemoteAddress ())
            )
        );

        // begin accepting again
        this.BeginAccept (this.AcceptCallback);
    }
}