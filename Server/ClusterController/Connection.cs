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
using System.Data;
using System.Linq;
using System.Text;
using System.Net.Sockets;

using Common.Packets;
using Common.Network;
using Common.Game;
using Common;

using Marshal;

namespace ClusterControler
{
    public class Connection
    {
        private AsyncCallback recvAsync;
        private AsyncCallback sendAsync;
        private StreamPacketizer packetizer = new StreamPacketizer();
        public ConnectionManager ConnectionManager { get; private set; }

        public Connection(Socket sock, ConnectionManager connectionManager)
        {
            this.ConnectionManager = connectionManager;
            Socket = new TCPSocket(sock, false);

            // Declare handlers
            recvAsync = new AsyncCallback(ReceiveAuthAsync);
            sendAsync = new AsyncCallback(SendAsync);

            // Session data
            Session = new Session();

            StageEnded = false;

            // Send LowLevel version exchange
            SendLowLevelVersionExchange();

            AsyncState state = new AsyncState();

            // Start receiving
            Socket.Socket.BeginReceive(state.buffer, 0, 8192, SocketFlags.None, recvAsync, state);
        }

        public void ReceiveAuthAsync(IAsyncResult ar)
        {
            try
            {
                AsyncState state = (AsyncState)(ar.AsyncState);

                int bytes = Socket.Socket.EndReceive(ar);

                packetizer.QueuePackets(state.buffer, bytes);
                int p = packetizer.ProcessPackets();

                for (int i = 0; i < p; i++)
                {
                    try
                    {
                        byte[] packet = packetizer.PopItem();

                        PyObject obj = Unmarshal.Process<PyObject>(packet);

                        if (obj != null)
                        {
                            PyObject result = TCPHandler.ProcessAuth(obj, this);

                            if (result != null)
                            {
                                Send(result);
                            }

                            if (StageEnded == true)
                            {
                                if (Type == ConnectionType.Node)
                                {
                                    recvAsync = new AsyncCallback(ReceiveNodeAsync);
                                }
                                else if (Type == ConnectionType.Client)
                                {
                                    recvAsync = new AsyncCallback(ReceiveClientAsync);
                                }

                                // Exit from the loop to keep the packets in the list ;)
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Connection", ex.ToString());
                    }
                }

                // Continue receiving data
                Socket.Socket.BeginReceive(state.buffer, 0, 8192, SocketFlags.None, recvAsync, state);
            }
            catch (ObjectDisposedException)
            {
                Log.Debug("Connection", "Disconnected");
                this.ConnectionManager.RemoveConnection(this);
            }
            catch (SocketException)
            {
                Log.Debug("Connection", "Disconnected");
                this.ConnectionManager.RemoveConnection(this);
            }
            catch (Exception ex)
            {
                Log.Error("Connection", "Caught unhandled exception: " + ex.ToString());
            }
        }

        public void ReceiveNodeAsync(IAsyncResult ar)
        {
            try
            {
                AsyncState state = (AsyncState)(ar.AsyncState);

                int bytes = Socket.Socket.EndReceive(ar);

                packetizer.QueuePackets(state.buffer, bytes);
                int p = packetizer.ProcessPackets();

                for (int i = 0; i < p; i++)
                {
                    byte[] packet = packetizer.PopItem();

                    PyObject obj = Unmarshal.Process<PyObject>(packet);

                    if (obj == null)
                    {
                        Log.Debug("Node", $"Null packet received ({packet.Length})");
                        continue;
                    }

                    if ((obj is PyObjectData) == false)
                    {
                        Log.Debug("Node", "Non-valid node packet. Dropping");
                        continue;
                    }

                    PyObjectData item = obj as PyObjectData;

                    if (item.Name == "macho.CallRsp")
                    {
                        PyPacket final = new PyPacket();

                        if (final.Decode(item) == false)
                        {
                            Log.Error("Node", "Cannot decode packet");
                            continue;
                        }

                        if (final.dest.type == PyAddress.AddrType.Client)
                        {
                            Log.Trace("Node", $"Sending packet to client {final.userID}");
                            this.ConnectionManager.NotifyClient((int)(final.userID), obj);
                        }
                        else if (final.dest.type == PyAddress.AddrType.Node)
                        {
                            Log.Trace("Node", $"Sending packet to node {final.dest.typeID}");
                            this.ConnectionManager.NotifyNode((int)(final.dest.typeID), obj);
                        }
                        else if (final.dest.type == PyAddress.AddrType.Broadcast)
                        {
                            Log.Error("Node", "Broadcast packets not supported yet");
                        }
                        // TODO: Handle Broadcast packets
                    }
                    else
                    {
                        Log.Error("Node", string.Format("Wrong packet name: {0}", item.Name));
                    }
                }

                Socket.Socket.BeginReceive(state.buffer, 0, 8192, SocketFlags.None, recvAsync, state);
            }
            catch (ObjectDisposedException)
            {
                Log.Debug("Node", "Disconnected");
                this.ConnectionManager.RemoveConnection(this);
            }
            catch (SocketException)
            {
                Log.Debug("Node", "Disconnected");
                this.ConnectionManager.RemoveConnection(this);
            }
            catch (Exception ex)
            {
                Log.Error("Node", "Caught unhandled exception: " + ex.ToString());
            }
        }

        public void ReceiveClientAsync(IAsyncResult ar)
        {
            try
            {
                AsyncState state = (AsyncState)(ar.AsyncState);

                int bytes = Socket.Socket.EndReceive(ar);

                packetizer.QueuePackets(state.buffer, bytes);
                int p = packetizer.ProcessPackets();

                for (int i = 0; i < p; i++)
                {
                    byte[] actual = packetizer.PopItem();
                    PyObject obj = Unmarshal.Process<PyObject>(actual);

                    if (obj == null)
                    {
                        continue;
                    }

                    if (obj is PyObjectEx)
                    {
                        // PyException
                        Log.Error("Client", "Got exception from client");
                        Log.Info("Client", PrettyPrinter.Print(obj));
                    }
                    else
                    {
                        PyPacket packet = new PyPacket();

                        if (packet.Decode(obj) == false)
                        {
                            Log.Error("Client", "Error decoding PyPacket");
                        }
                        else
                        {
                            if (packet.dest.type == PyAddress.AddrType.Node)
                            {
                                if (packet.source.type != PyAddress.AddrType.Client)
                                {
                                    Log.Error("Client", string.Format("Wrong source data, expected client but got {0}", packet.source.type));
                                }

                                // Notify the node, be careful here, the client will be able to send packets to game clients
                                if (packet.dest.typeID == 0xFFAA)
                                {
                                    Log.Trace("Client", "Sending packet to proxy");
                                }

                                this.ConnectionManager.NotifyNode((int)(packet.dest.typeID), obj);
                            }
                        }
                    }
                }

                Socket.Socket.BeginReceive(state.buffer, 0, 8192, SocketFlags.None, recvAsync, state);
            }
            catch (ObjectDisposedException)
            {
                Log.Debug("Client", "Disconnected");
                this.ConnectionManager.RemoveConnection(this);
            }
            catch (SocketException)
            {
                Log.Debug("Client", "Disconnected");
                this.ConnectionManager.RemoveConnection(this);
            }
            catch (Exception ex)
            {
                Log.Error("Client", "Caught unhandled exception: " + ex.ToString());
            }
        }

        public void SendAsync(IAsyncResult ar)
        {
            int bytes = Socket.Socket.EndSend(ar);
        }

        public void Send(PyObject packet)
        {
            Send(Marshal.Marshal.Process(packet));
        }

        public void Send(byte[] data)
        {
            byte[] packet = new byte[data.Length + 4];

            Array.Copy(BitConverter.GetBytes(data.Length), packet, 4);
            Array.Copy(data, 0, packet, 4, data.Length);

            // Send data
            // Socket.Socket.BeginSend(packet, 0, packet.Length, SocketFlags.None, sendAsync, null);
            int sent = 0;

            while (sent != packet.Length)
            {
                try
                {
                    sent += Socket.Socket.Send(packet, sent, packet.Length - sent, SocketFlags.None);
                    // TEMPORAL SOLUTION UNTIL THE TCPSocket CLASS IS REWRITTEN
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock)
                        continue;
                    throw;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public void SendLowLevelVersionExchange()
        {
            Log.Debug("Client", "Sending LowLevelVersionExchange...");

            LowLevelVersionExchange data = new LowLevelVersionExchange();

            data.codename = Common.Constants.Game.codename;
            data.birthday = Common.Constants.Game.birthday;
            data.build = Common.Constants.Game.build;
            data.machoVersion = Common.Constants.Game.machoVersion;
            data.version = Common.Constants.Game.version;
            data.usercount = this.ConnectionManager.ClientsCount;
            data.region = Common.Constants.Game.region;

            Send(data.Encode(false));
        }

        public bool CheckLowLevelVersionExchange(PyTuple packet)
        {
            LowLevelVersionExchange data = new LowLevelVersionExchange();

            if (data.Decode(packet) == false)
            {
                Log.Error("Client", "Wrong LowLevelVersionExchange packet");
                return false;
            }

            if (data.birthday != Common.Constants.Game.birthday)
            {
                Log.Error("Client", "Wrong birthday in LowLevelVersionExchange");
                return false;
            }

            if (data.build != Common.Constants.Game.build)
            {
                Log.Error("Client", "Wrong build in LowLevelVersionExchange");
                return false;
            }

            if (data.codename != Common.Constants.Game.codename + "@" + Common.Constants.Game.region)
            {
                Log.Error("Client", "Wrong codename in LowLevelVersionExchange");
                return false;
            }

            if (data.machoVersion != Common.Constants.Game.machoVersion)
            {
                Log.Error("Client", "Wrong machoVersion in LowLevelVersionExchange");
                return false;
            }

            if (data.version != Common.Constants.Game.version)
            {
                Log.Error("Client", "Wrong version in LowLevelVersionExchange");
                return false;
            }

            if (data.isNode == true)
            {
                if (data.nodeIdentifier != "Node")
                {
                    Log.Error("Client", "Wrong node string in LowLevelVersionExchange");
                    return false;
                }

                Type = ConnectionType.Node;
            }
            else
            {
                Type = ConnectionType.Client;
            }

            return true;
        }

        public void EndConnection()
        {
            try
            {
                Socket.Socket.Shutdown(SocketShutdown.Both);
                Socket.Socket.Close();
            }
            catch (ObjectDisposedException)
            {
                Log.Debug("Client", "Trying to close a connection which is already closed");
            }
            catch (SocketException)
            {
                Log.Debug("Client", "Trying to close a connection which is already closed");
            }
            catch
            {

            }

            this.ConnectionManager.RemoveConnection(ClusterConnectionID);
        }

        public void SendSessionChange()
        {
            PyPacket sc = CreateEmptySessionChange();

            PyObject client = SetSessionChangeDestination(sc);
            PyObject node = SetSessionChangeDestination(sc, NodeID);

            if (sc != null)
            {
                Send(client);
                this.ConnectionManager.NotifyConnection(NodeID, node);
            }
        }

        public PyPacket CreateEmptySessionChange()
        {
            // Fill all the packet data, except the dest/source
            SessionChangeNotification scn = new SessionChangeNotification();
            scn.changes = Session.EncodeChanges();

            if (scn.changes.Dictionary.Count == 0)
            {
                // Nothing to do
                return null;
            }

            Dictionary<int, Connection> nodes = this.ConnectionManager.Nodes;

            // Add all the nodeIDs
            foreach (KeyValuePair<int, Connection> node in nodes)
            {
                scn.nodesOfInterest.Items.Add(new PyInt(node.Key));
            }

            PyPacket p = new PyPacket();

            p.type_string = "macho.SessionChangeNotification";
            p.type = Macho.MachoNetMsg_Type.SESSIONCHANGENOTIFICATION;

            p.userID = (uint)AccountID;

            p.payload = scn.Encode().As<PyTuple>();

            p.named_payload = new PyDict();
            p.named_payload.Set("channel", new PyString("sessionchange"));

            return p;
        }

        public PyObject SetSessionChangeDestination(PyPacket p)
        {
            p.source.type = PyAddress.AddrType.Node;
            p.source.typeID = (ulong)NodeID;
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Client;
            p.dest.typeID = (ulong)AccountID;
            p.dest.callID = 0;

            return p.Encode();
        }

        public PyObject SetSessionChangeDestination(PyPacket p, int node)
        {
            // The session change info should never come from the client
            p.source.type = PyAddress.AddrType.Node;
            p.source.typeID = (ulong)1;
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Node;
            p.dest.typeID = (ulong)node;
            p.dest.callID = 0;

            return p.Encode();
        }

        public PyObject CreateSessionChange()
        {
            PyPacket p = CreateEmptySessionChange();

            if (p == null)
            {
                return null;
            }

            p.source.type = PyAddress.AddrType.Node;
            p.source.typeID = (ulong)NodeID;
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Client;
            p.dest.typeID = (ulong)AccountID;
            p.dest.callID = 0;

            return p.Encode();
        }

        public PyObject CreateSessionChange(int nodeid)
        {
            PyPacket p = CreateEmptySessionChange();

            if (p == null)
            {
                return null;
            }

            // The session change info should never come from the client
            p.source.type = PyAddress.AddrType.Node;
            p.source.typeID = (ulong)1;
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Node;
            p.dest.typeID = (ulong)nodeid;
            p.dest.callID = 0;

            return p.Encode();
        }

        public void SendNodeChangeNotification()
        {
            if (Type != ConnectionType.Node)
            {
                return;
            }

            NodeInfo nodeInfo = new NodeInfo();

            nodeInfo.nodeID = NodeID;
            nodeInfo.solarSystems.Items.Add(new PyNone()); // None = All solar systems

            Send(nodeInfo.Encode());
        }

        public void SendLoginNotification(LoginStatus loginStatus)
        {
            TCPHandler.SendLoginNotification(loginStatus, this, this.ConnectionManager);
        }

        public bool StageEnded
        {
            get;
            set;
        }

        public bool Banned
        {
            get;
            set;
        }

        public long Role
        {
            get;
            set;
        }

        public long AccountID
        {
            get;
            set;
        }

        public string Address
        {
            get
            {
                return Socket.GetAddress();
            }

            private set
            {

            }
        }

        public string LanguageID
        {
            get;
            set;
        }

        // This can have two meanings
        // When connection is a Node this is the nodeID(the same as ClusterConnectionID)
        // When connection is a Client, the node in which it is now
        public int NodeID
        {
            get;
            set;
        }

        public TCPSocket Socket
        {
            get;
            set;
        }

        public Session Session
        {
            get;
            set;
        }

        public ConnectionType Type
        {
            get;
            set;
        }

        // ID in the ConnectionManager list
        public int ClusterConnectionID
        {
            get;
            set;
        }
    }
}
