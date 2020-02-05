/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Net.Sockets;

using Marshal;

using Common;
using Common.Constants;
using Common.Network;
using Marshal.Network;

namespace ClusterControler
{
    public class ConnectionManager
    {
        public LoginQueue LoginQueue { get; set; }
        public int ClientsCount
        {
            get { return Clients.Count; }
            private set { }
        }
        public int NodesCount
        {
            get { return Nodes.Count; }
            private set { }
        }
        public Dictionary<int, NodeConnection> Nodes
        {
            get { return this.mNodeConnections; }
            private set { }
        }
        public Dictionary<int, ClientConnection> Clients
        {
            get { return this.mClientConnections; }
            private set { }
        }

        private int mLastClientID = 0;
        private int mLastNodeID = 0;
        
        private List<UnauthenticatedConnection> mUnauthenticatedConnections = new List<UnauthenticatedConnection>();
        private Dictionary<int, ClientConnection> mClientConnections = new Dictionary<int, ClientConnection>();
        private Dictionary<int, NodeConnection> mNodeConnections = new Dictionary<int, NodeConnection>();
        
        public ConnectionManager(LoginQueue loginQueue)
        {
            this.LoginQueue = loginQueue;
        }

        public void AddUnauthenticatedConnection(EVEClientSocket socket)
        {
            this.mUnauthenticatedConnections.Add (
                new UnauthenticatedConnection(socket, this)
            );
        }

        public void RemoveUnauthenticatedConnection(UnauthenticatedConnection connection)
        {
            this.mUnauthenticatedConnections.Remove(connection);
        }

        public void AddClientConnection(EVEClientSocket socket)
        {
            this.mClientConnections.Add(++this.mLastClientID, new ClientConnection(socket, this, this.mLastClientID));
        }

        public void RemoveClientConnection(ClientConnection connection)
        {
            this.mClientConnections.Remove(connection.ClientID);
        }

        public void AddNodeConnection(EVEClientSocket socket)
        {
            if(this.mNodeConnections.Count >= 0xFFFF)
                throw new Exception("Cannot accept more nodes in the connection manager, reached maximum of 0xFFFF");
            
            if (this.mNodeConnections.ContainsKey(Network.PROXY_NODE_ID) == false)
                this.mNodeConnections.Add(Network.PROXY_NODE_ID, new NodeConnection(socket, this, Network.PROXY_NODE_ID));
            else
            {
                // find an unused nodeID
                while (this.mNodeConnections.ContainsKey(this.mLastNodeID) == true)
                {
                    this.mLastNodeID++;
                    // ensure that we do not exceed the limit
                    if (this.mLastNodeID > 0xFFFF)
                        this.mLastNodeID = 0;
                }
                
                // finally assign it to the new node
                this.mNodeConnections.Add(this.mLastNodeID, new NodeConnection(socket, this, this.mLastNodeID));
            }
        }

        public void RemoveNodeConnection(NodeConnection connection)
        {
            // TODO: CHECK FOR PROXY NODE BEING DELETED AND TRY TO ASSIGN ANY OTHER
            this.mNodeConnections.Remove(connection.NodeID);
        }
        
        public void NotifyClient(int clientID, PyObject packet)
        {
            if (this.Clients.ContainsKey(clientID) == false)
                throw new Exception($"Trying to notify a not connected client {clientID}");
            
            this.Clients[clientID].Socket.Send(packet);
        }

        public void NotifyNode(int nodeID, PyObject packet)
        {
            if(this.Nodes.ContainsKey(nodeID) == false)
                throw new Exception($"Trying to notify a non-existant node {nodeID}");
            
            this.Nodes[nodeID].Socket.Send(packet);
        }

        public void NotifyNode(int nodeID, Encodeable packet)
        {
            this.NotifyNode(nodeID, packet.Encode());
        }
    }
}
