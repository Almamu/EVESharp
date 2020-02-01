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
using System.Linq;
using System.Text;
using System.Net.Sockets;

using Marshal;

using Common;

namespace ClusterControler
{
    public class ConnectionManager
    {
        private List<Connection> mConnections = new List<Connection>(); // This list contains ALL the connections, both nodes and clients
        private Dictionary<long, Connection> mClients = new Dictionary<long, Connection>(); // Contains the nodes list
        private Dictionary<int, Connection> mNodes = new Dictionary<int, Connection>(); // Contains the clients list
        public LoginQueue LoginQueue { get; set; }

        public ConnectionManager(LoginQueue loginQueue)
        {
            this.LoginQueue = loginQueue;
        }
        
        public int AddConnection(Socket sock)
        {
            Connection connection = new Connection(sock, this);

            // Priority
            connection.ClusterConnectionID = mConnections.Count;
            connection.Type = ConnectionType.Undefined;

            mConnections.Add(connection);

            return mConnections.Count;
        }

        public void UpdateConnection(Connection connection)
        {
            if (connection.Type == ConnectionType.Node)
            {
                // This will let us add a node as a proxy
                if (mNodes.Count == 0)
                {
                    connection.ClusterConnectionID = connection.NodeID = 0xFFAA;
                }
                else
                {
                    connection.NodeID = connection.ClusterConnectionID;
                }

                if (mNodes.ContainsKey(connection.NodeID))
                {
                    mNodes[connection.NodeID] = connection;
                }
                else
                {
                    mNodes.Add(connection.NodeID, connection);
                }
            }
            else if (connection.Type == ConnectionType.Client)
            {
                if (mClients.ContainsKey(connection.AccountID))
                {
                    mClients[connection.AccountID] = connection;
                }
                else
                {
                    mClients.Add(connection.AccountID, connection);
                }
            }
        }

        public void RemoveConnection(int index)
        {
            try
            {
                Connection connection = mConnections[index];

                if (connection == null)
                {
                    mConnections.RemoveAt(index);
                    return;
                }

                RemoveConnection(connection);
            }
            catch
            {

            }
        }

        public void RemoveConnection(Connection connection)
        {
            try
            {
                if (connection.Type == ConnectionType.Client)
                {
                    mClients.Remove(connection.AccountID);
                    mConnections.Remove(connection);
                }
                else if (connection.Type == ConnectionType.Node)
                {
                    Log.Warning("ConnectionManager", "Node " + connection.NodeID.ToString("X4") + " disconnected");

                    mNodes.Remove(connection.NodeID);
                    mConnections.Remove(connection);

                    if (connection.NodeID == 0xFFAA)
                    {
                        Log.Warning("ConnectionManager", "Proxy node has disconnected, searching for new nodes");

                        if (mNodes.Count == 0)
                        {
                            Log.Error("ConnectionManager", "No more nodes available, closing public cluster connections");

                            foreach (var client in mClients)
                            {
                                // Close the client connection
                                client.Value.EndConnection();
                            }
                        }
                        else
                        {
                            Connection newProxyNode = mNodes.First().Value;

                            // Remove the node from nodes list to update it
                            mNodes.Remove(newProxyNode.NodeID);

                            newProxyNode.ClusterConnectionID = newProxyNode.NodeID = 0xFFAA;

                            // And add it back as the proxy node
                            mNodes.Add(newProxyNode.ClusterConnectionID, newProxyNode);

                            // Send the node change notification
                            newProxyNode.SendNodeChangeNotification();
                        }
                    }
                }
                else
                {
                    mConnections.Remove(connection);
                }
            }
            catch
            {

            }
        }

        public Dictionary<int, Connection> Nodes
        {
            get
            {
                return mNodes;
            }

            set
            {

            }
        }

        public Dictionary<long, Connection> Clients
        {
            get
            {
                return mClients;
            }

            set
            {

            }
        }

        public void NotifyConnection(int clusterConnectionID, PyObject packet)
        {
            try
            {
                mConnections[clusterConnectionID].Send(packet); // This will notify the connection
            }
            catch
            {

            }
        }

        public void NotifyClient(int accountID, PyObject packet)
        {
            try
            {
                mClients[accountID].Send(packet); // This will notify the client
            }
            catch
            {

            }
        }

        public void NotifyNode(int nodeID, PyObject packet)
        {
            try
            {
                mNodes[nodeID].Send(packet);
            }
            catch
            {

            }
        }

        public int RandomNode
        {
            get
            {
                foreach (KeyValuePair<int, Connection> node in mNodes)
                {
                    return node.Key;
                }

                return 1;
            }

            private set
            {

            }
        }

        public int ClientsCount
        {
            get
            {
                return mClients.Count;
            }

            private set
            {

            }
        }

        public int NodesCount
        {
            get
            {
                return mNodes.Count;
            }

            private set
            {

            }
        }
    }
}
