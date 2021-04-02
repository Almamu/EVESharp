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
using PythonTypes.Types.Collections;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace ClusterController
{
    public class ConnectionManager
    {
        /// <summary>
        /// Lower ids are proxy nodes, this number is arbitrarily selected
        /// </summary>
        private const int GAME_NODE_IDS = 1000000;
        
        public LoginQueue LoginQueue { get; }
        private GeneralDB GeneralDB { get; }
        public int ClientsCount => Clients.Count;
        public int NodesCount => Nodes.Count;
        public Dictionary<long, NodeConnection> Nodes { get; } = new Dictionary<long, NodeConnection>();
        public Dictionary<long, ClientConnection> Clients { get; } = new Dictionary<long, ClientConnection>();
        public List<ClientConnection> UnauthenticatedClientConnections { get; } = new List<ClientConnection>();

        public List<UnauthenticatedConnection> UnauthenticatedConnections { get; } = new List<UnauthenticatedConnection>();

        private SystemManager SystemManager { get; }
        private Channel Log { get; set; }

        private long mLastNodeID = 1;

        public ConnectionManager(LoginQueue loginQueue, SystemManager systemManager, GeneralDB generalDB, Logger logger)
        {
            this.Log = logger.CreateLogChannel("ConnectionManager");
            this.LoginQueue = loginQueue;
            this.SystemManager = systemManager;
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
                if (this.Clients.TryGetValue(connection.AccountID, out ClientConnection con) == true)
                {
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
            long nodeID = this.mLastNodeID++;

            lock (this.Nodes)
                this.Nodes.Add(nodeID, new NodeConnection(socket, this, this.SystemManager, Log.Logger, nodeID));
        }

        public void RemoveNodeConnection(NodeConnection connection)
        {
            // TODO: CHECK FOR PROXY NODE BEING DELETED AND TRY TO ASSIGN ANY OTHER
            lock (this.Nodes)
                this.Nodes.Remove(connection.NodeID);
        }

        public void NotifyClient(long clientID, PyDataType packet)
        {
            lock (this.Clients)
            {
                if (this.Clients.ContainsKey(clientID) == false)
                    throw new Exception($"Trying to notify a not connected client {clientID}");

                this.Clients[clientID].Socket.Send(packet);
            }
        }

        public void NotifyNode(long nodeID, PyDataType packet)
        {
            lock (this.Nodes)
            {
                if (this.Nodes.ContainsKey(nodeID) == false)
                    throw new Exception($"Trying to notify a non-existant node {nodeID}");

                this.Nodes[nodeID].Socket.Send(packet);
            }
        }

        public void NotifyNode(long nodeID, string type, PyTuple data)
        {
            PyPacket notification = new PyPacket(PyPacket.PacketType.NOTIFICATION);
            
            notification.Source = new PyAddressAny(0);
            notification.Destination = new PyAddressNode(nodeID);
            notification.Payload = new PyTuple(2) {[0] = type, [1] = data };
            notification.OutOfBounds = new PyDictionary();
            notification.UserID = 0;
            
            lock (this.Nodes)
                this.Nodes[nodeID].Socket.Send(notification);
        }

        public void NotifyBySolarSystemID2(PyPacket packet, PyAddressBroadcast destination)
        {
            lock (this.Clients)
            {
                foreach (PyDataType idData in destination.IDsOfInterest)
                {
                    PyInteger id = idData as PyInteger;
                            
                    foreach ((long userID, ClientConnection connection) in this.Clients)
                    {
                        if (connection.SolarSystemID2 == id)
                        {
                            // use the key instead of AccountID as this should be faster
                            packet.UserID = userID;
                            // queue the packet for the user
                            connection.Socket.Send(packet);
                        }
                    }
                }
            }
        }

        public void NotifyByConstellationID(PyPacket packet, PyAddressBroadcast destination)
        {
            lock (this.Clients)
            {
                foreach (PyDataType idData in destination.IDsOfInterest)
                {
                    PyInteger id = idData as PyInteger;
                            
                    foreach ((long userID, ClientConnection connection) in this.Clients)
                    {
                        if (connection.ConstellationID == id)
                        {
                            // use the key instead of AccountID as this should be faster
                            packet.UserID = userID;
                            // queue the packet for the user
                            connection.Socket.Send(packet);
                        }
                    }
                }
            }
        }

        public void NotifyByCorporationID(PyPacket packet, PyAddressBroadcast destination)
        {
            lock (this.Clients)
            {
                foreach (PyDataType idData in destination.IDsOfInterest)
                {
                    PyInteger id = idData as PyInteger;
                            
                    foreach ((long userID, ClientConnection connection) in this.Clients)
                    {
                        if (connection.CorporationID == id)
                        {
                            // use the key instead of AccountID as this should be faster
                            packet.UserID = userID;
                            // queue the packet for the user
                            connection.Socket.Send(packet);
                        }
                    }
                }
            }
        }

        public void NotifyByRegionID(PyPacket packet, PyAddressBroadcast destination)
        {
            lock (this.Clients)
            {
                foreach (PyDataType idData in destination.IDsOfInterest)
                {
                    PyInteger id = idData as PyInteger;
                            
                    foreach ((long userID, ClientConnection connection) in this.Clients)
                    {
                        if (connection.RegionID == id)
                        {
                            // use the key instead of AccountID as this should be faster
                            packet.UserID = userID;
                            // queue the packet for the user
                            connection.Socket.Send(packet);
                        }
                    }
                }
            }
        }

        public void NotifyByCharacterID(PyPacket packet, PyAddressBroadcast destination)
        {
            lock (this.Clients)
            {
                foreach (PyInteger id in destination.IDsOfInterest.GetEnumerable<PyInteger>())
                {
                    foreach ((long userID, ClientConnection connection) in this.Clients)
                    {
                        if (connection.CharacterID == id)
                        {
                            // use the key instead of AccountID as this should be faster
                            packet.UserID = userID;
                            // change the ids of interest to hide the character's we've notified
                            destination.IDsOfInterest = new PyList(1) {[0] = id};
                            // queue the packet for the user
                            connection.Socket.Send(packet);
                        }
                    }
                }
            }
        }

        public void NotifyByStationID(PyPacket packet, PyAddressBroadcast destination)
        {
            lock (this.Clients)
            {
                foreach (PyInteger id in destination.IDsOfInterest.GetEnumerable<PyInteger>())
                {
                    foreach ((long userID, ClientConnection connection) in this.Clients)
                    {
                        if (connection.StationID == id)
                        {
                            // use the key instead of AccountID as this should be faster
                            packet.UserID = userID;
                            // queue the packet for the user
                            connection.Socket.Send(packet);
                        }
                    }
                }
            }
        }

        public void NotifyByAllianceID(PyPacket packet, PyAddressBroadcast destination)
        {
            lock (this.Clients)
            {
                foreach (PyInteger id in destination.IDsOfInterest.GetEnumerable<PyInteger>())
                {
                    foreach ((long userID, ClientConnection connection) in this.Clients)
                    {
                        if (connection.AllianceID == id)
                        {
                            // use the key instead of AccountID as this should be faster
                            packet.UserID = userID;
                            // queue the packet for the user
                            connection.Socket.Send(packet);
                        }
                    }
                }
            }
        }

        public void NotifyByNodeID(PyPacket packet, PyAddressBroadcast destination)
        {
            lock (this.Nodes)
            {
                foreach (PyInteger id in destination.IDsOfInterest.GetEnumerable<PyInteger>())
                {
                    if (this.Nodes.TryGetValue(id, out NodeConnection connection) == false)
                    {
                        Log.Warning("Trying to notify a node that is not connected anymore...");
                        continue;
                    }
                    
                    // use the key instead of AccountID as this should be faster
                    packet.UserID = id;
                    // queue the packet for the user
                    connection.Socket.Send(packet);
                }
            }
        }

        public void NotifyAllNodes(PyPacket packet)
        {
            lock (this.Nodes)
            {
                foreach ((long _, NodeConnection connection) in this.Nodes)
                    connection.Socket.Send(packet);
            }
        }

        public void NotifyAllNodes(string type, PyTuple data)
        {
            PyPacket notification = new PyPacket(PyPacket.PacketType.NOTIFICATION);
            
            notification.Source = new PyAddressAny(0);
            notification.Payload = new PyTuple(2) {[0] = type, [1] = data };
            notification.OutOfBounds = new PyDictionary();
            notification.UserID = 0;

            lock (this.Nodes)
            {
                foreach ((long nodeID, NodeConnection connection) in this.Nodes)
                {
                    // update destination and send
                    notification.Destination = new PyAddressNode(nodeID);

                    connection.Socket.Send(notification);
                }
            }
        }
    }
}