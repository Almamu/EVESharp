/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
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
using System.Net.Sockets;
using ClusterController.Database;
using Common.Constants;
using Common.Database;
using Common.Logging;
using Common.Network;
using MySqlX.XDevAPI;
using PythonTypes.Types.Primitives;

namespace ClusterController
{
    public class ConnectionManager
    {
        private const int MAX_NODE_ID = 0xFFFF;
        
        public LoginQueue LoginQueue { get; }
        private GeneralDB GeneralDB { get; }
        public int ClientsCount => Clients.Count;
        public int NodesCount => Nodes.Count;
        public Dictionary<long, NodeConnection> Nodes { get; } = new Dictionary<long, NodeConnection>();
        public Dictionary<long, ClientConnection> Clients { get; } = new Dictionary<long, ClientConnection>();
        public List<ClientConnection> UnauthenticatedClientConnections { get; } = new List<ClientConnection>();

        public List<UnauthenticatedConnection> UnauthenticatedConnections { get; } = new List<UnauthenticatedConnection>();

        private Channel Log { get; set; }

        private long mLastNodeID = 0;

        public ConnectionManager(LoginQueue loginQueue, GeneralDB generalDB, Logger logger)
        {
            this.Log = logger.CreateLogChannel("ConnectionManager");
            this.LoginQueue = loginQueue;
            this.GeneralDB = generalDB;
        }

        public void AddUnauthenticatedConnection(EVEClientSocket socket)
        {
            lock (this.UnauthenticatedConnections)
                this.UnauthenticatedConnections.Add(
                    new UnauthenticatedConnection(socket, this, Log.Logger)
                );
        }

        public void RemoveUnauthenticatedConnection(UnauthenticatedConnection connection)
        {
            lock (this.UnauthenticatedConnections)
                this.UnauthenticatedConnections.Remove(connection);
        }

        public void AddUnauthenticatedClientConnection(EVEClientSocket socket)
        {
            lock (this.UnauthenticatedClientConnections)
                this.UnauthenticatedClientConnections.Add(new ClientConnection(socket, this, this.GeneralDB, Log.Logger));
        }

        public void RemoveUnauthenticatedClientConnection(ClientConnection connection)
        {
            lock (this.UnauthenticatedClientConnections)
                this.UnauthenticatedClientConnections.Remove(connection);
        }

        public void AddAuthenticatedClientConnection(ClientConnection connection)
        {
            lock (this.Clients)
            {
                // if there's already an user connected with this account inmediately log it off
                if (this.Clients.ContainsKey(connection.AccountID) == true)
                {
                    ClientConnection con = this.Clients[connection.AccountID];

                    this.Clients.Remove(connection.AccountID);

                    try
                    {
                        // try to disconnect the user
                        // there might be situations where the client is already disconnected
                        // and the connection is just hung in there
                        // so exceptions from this can be ignored
                        con.Socket.ForcefullyDisconnect();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                this.Clients.Add(connection.AccountID, connection);
            }
        }

        public void RemoveAuthenticatedClientConnection(ClientConnection connection)
        {
            lock (this.Clients)
            {
                // before removing it, ensure that the connection wasn't replaced by anyone already
                if (this.Clients[connection.AccountID] == connection)
                    this.Clients.Remove(connection.AccountID);
            }
        }

        public void AddNodeConnection(EVEClientSocket socket)
        {
            lock (this.Nodes)
            {
                if (this.Nodes.Count >= MAX_NODE_ID)
                    throw new Exception($"Cannot accept more nodes in the connection manager, reached maximum of {MAX_NODE_ID}");

                if (this.Nodes.ContainsKey(Network.PROXY_NODE_ID) == false)
                    this.Nodes.Add(Network.PROXY_NODE_ID, new NodeConnection(socket, this, Log.Logger, Network.PROXY_NODE_ID));
                else
                {
                    int iterations = 0;
                    
                    // find an unused nodeID
                    while (this.Nodes.ContainsKey(this.mLastNodeID) == true)
                    {
                        this.mLastNodeID++;
                        // ensure that we do not exceed the limit
                        if (this.mLastNodeID > MAX_NODE_ID)
                            this.mLastNodeID = 0;

                        iterations++;

                        if (iterations > MAX_NODE_ID)
                            throw new Exception("Cannot find a free node slot to assign to this node");
                    }

                    // finally assign it to the new node
                    this.Nodes.Add(this.mLastNodeID, new NodeConnection(socket, this, Log.Logger, this.mLastNodeID));
                }
            }
        }

        public void RemoveNodeConnection(NodeConnection connection)
        {
            // TODO: CHECK FOR PROXY NODE BEING DELETED AND TRY TO ASSIGN ANY OTHER
            lock (this.Nodes)
                this.Nodes.Remove(connection.NodeID);
        }

        public void NotifyClient(int clientID, PyDataType packet)
        {
            lock (this.Clients)
            {
                if (this.Clients.ContainsKey(clientID) == false)
                    throw new Exception($"Trying to notify a not connected client {clientID}");

                this.Clients[clientID].Socket.Send(packet);
            }
        }

        public void NotifyNode(int nodeID, PyDataType packet)
        {
            lock (this.Nodes)
            {
                if (this.Nodes.ContainsKey(nodeID) == false)
                    throw new Exception($"Trying to notify a non-existant node {nodeID}");

                this.Nodes[nodeID].Socket.Send(packet);
            }
        }
    }
}