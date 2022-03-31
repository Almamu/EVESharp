using System;
using System.Collections.Generic;
using System.Net.Http;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.Node.Accounts;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;

namespace EVESharp.Node.Network
{
    public class MachoServerTransport : EVEServerSocket
    {
        public IMachoNet MachoNet { get; }
        public HttpClient HttpClient { get; }
        
        public MachoServerTransport(int port, HttpClient httpClient, IMachoNet machoNet, ILogger logger) : base(port, logger)
        {
            this.HttpClient = httpClient;
            this.MachoNet = machoNet;
        }

        public new void Listen()
        {
            base.Listen();
            this.BeginAccept(AcceptCallback);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            EVEServerSocket serverSocket = ar.AsyncState as EVEServerSocket;
            EVEClientSocket clientSocket = serverSocket.EndAccept(ar);
            
            // got a new transport, register it
            this.MachoNet.TransportManager.NewTransport(new MachoUnauthenticatedTransport(this.MachoNet, this.HttpClient, clientSocket, Log.ForContext<MachoUnauthenticatedTransport>(clientSocket.GetRemoteAddress())));
            
            // begin accepting again
            this.BeginAccept(AcceptCallback);
        }
    }
}