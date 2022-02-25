using System;
using System.Collections.Generic;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.Node.Accounts;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public class MachoServerTransport : EVEServerSocket
    {
        /// <summary>
        /// The registered and validated client transports
        /// </summary>
        public Dictionary<long, MachoClientTransport> ClientTransports { get; } = new Dictionary<long, MachoClientTransport>();
        /// <summary>
        /// The registered and validated node transports
        /// </summary>
        public Dictionary<int, MachoClientTransport> NodeTransports { get; } = new Dictionary<int, MachoClientTransport>();
        /// <summary>
        /// The unvalidated transports
        /// </summary>
        private List<MachoClientTransport> UnauthenticatedTransports { get; } = new List<MachoClientTransport>();
        public MachoNet MachoNet { get; init; }
        
        public MachoServerTransport(int port, MachoNet machoNet, Logger logger) : base(port, logger.CreateLogChannel("MachoServerTransport"))
        {
            this.MachoNet = machoNet;
        }

        public new void Listen()
        {
            Log.Info("Starting up MachoNet Listener...");
            base.Listen();
            this.BeginAccept(AcceptCallback);
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            EVEServerSocket serverSocket = ar.AsyncState as EVEServerSocket;
            EVEClientSocket clientSocket = serverSocket.EndAccept(ar);
            
            // got a new transport, register it
            this.UnauthenticatedTransports.Add(new MachoClientTransport(this, clientSocket, Log.Logger));
            
            // begin accepting again
            this.BeginAccept(AcceptCallback);
        }

        /// <summary>
        /// Registers the given transport as a client's transport
        /// </summary>
        /// <param name="clientTransport"></param>
        public void ResolveClientTransport(MachoClientTransport clientTransport)
        {
            // first remove the transport from the unauthenticated list
            this.UnauthenticatedTransports.Remove(clientTransport);
            // add it to the clients list
            this.ClientTransports.Add(clientTransport.Session["userid"] as PyInteger, clientTransport);
        }

        public void ResolveNodeTransport(MachoClientTransport nodeTransport)
        {
            // first remove the transport from the unauthenticated list
            this.UnauthenticatedTransports.Remove(nodeTransport);
            // add it to the nodes list
            this.NodeTransports.Add(nodeTransport.Session["nodeid"] as PyInteger, nodeTransport);
        }

        public void OnTransportTerminated(MachoClientTransport transport)
        {
            // check what kind of transport it is and remove it from the lists
            this.UnauthenticatedTransports.Remove(transport);

            if (transport.Client is not null)
                this.ClientTransports.Remove(transport.Client.AccountID);
            else
                this.NodeTransports.Remove(0);
        }
    }
}