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
        public Dictionary<int, MachoClientTransport> ClientTransports { get; } = new Dictionary<int, MachoClientTransport>();
        /// <summary>
        /// The registered and validated node transports
        /// </summary>
        public Dictionary<long, MachoNodeTransport> NodeTransports { get; } = new Dictionary<long, MachoNodeTransport>();
        /// <summary>
        /// The registered and validated proxy transports
        /// </summary>
        public Dictionary<long, MachoProxyTransport> ProxyTransports { get; } = new Dictionary<long, MachoProxyTransport>();
        /// <summary>
        /// Full list of active transports for this node
        /// </summary>
        public List<MachoTransport> TransportList { get; } = new List<MachoTransport>();

        /// <summary>
        /// The unvalidated transports
        /// </summary>
        public List<MachoTransport> UnauthenticatedTransports { get; } = new List<MachoTransport>();
        public MachoNet MachoNet { get; }
        
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
            this.UnauthenticatedTransports.Add(new MachoUnauthenticatedTransport(this, clientSocket, Log.Logger));
            
            // begin accepting again
            this.BeginAccept(AcceptCallback);
        }

        /// <summary>
        /// Registers the given transport as a client's transport
        /// </summary>
        /// <param name="transport"></param>
        public void ResolveClientTransport(MachoUnauthenticatedTransport transport)
        {
            // first remove the transport from the unauthenticated list
            this.UnauthenticatedTransports.Remove(transport);
            
            // create the new client transport and store it somewhere
            MachoClientTransport newTransport = new MachoClientTransport(transport);
            
            if (this.ClientTransports.TryGetValue(newTransport.Session.UserID, out MachoClientTransport original) == true)
                original.AbortConnection();

            this.ClientTransports.Add(newTransport.Session.UserID, newTransport);
            this.TransportList.Add(newTransport);
        }

        public void ResolveNodeTransport(MachoUnauthenticatedTransport transport)
        {
            Log.Info($"Connection from server with nodeID {transport.Session.NodeID}");
            // first remove the transport from the unauthenticated list
            this.UnauthenticatedTransports.Remove(transport);

            // create the new client transport and store it somewhere
            MachoNodeTransport newTransport = new MachoNodeTransport(transport);
            
            if (this.NodeTransports.TryGetValue(newTransport.Session.NodeID, out MachoNodeTransport original) == true)
                original.AbortConnection();

            this.NodeTransports.Add(newTransport.Session.NodeID, newTransport);
            this.TransportList.Add(newTransport);
        }

        public void ResolveProxyTransport(MachoUnauthenticatedTransport transport)
        {
            Log.Info($"Connection from proxy with nodeID {transport.Session.NodeID}");
            // first remove the transport from the unauthenticated list
            this.UnauthenticatedTransports.Remove(transport);
            
            // create the new client transport and store it somewhere
            MachoProxyTransport newTransport = new MachoProxyTransport(transport);
            
            if (this.ProxyTransports.TryGetValue(newTransport.Session.NodeID, out MachoProxyTransport original) == true)
                original.AbortConnection();

            this.ProxyTransports.Add(newTransport.Session.NodeID, newTransport);
            this.TransportList.Add(newTransport);
        }
        
        public void OnTransportTerminated(MachoTransport transport)
        {
            if (transport is not MachoUnauthenticatedTransport)
                this.TransportList.Remove(transport);
            
            switch (transport)
            {
                case MachoUnauthenticatedTransport:
                    this.UnauthenticatedTransports.Remove(transport);
                    break;
                case MachoClientTransport:
                    this.ClientTransports.Remove(transport.Session.UserID);
                    break;
                case MachoNodeTransport:
                    this.NodeTransports.Remove(transport.Session.NodeID);
                    break;
                case MachoProxyTransport:
                    this.ProxyTransports.Remove(transport.Session.NodeID);
                    break;
            }
        }
    }
}