using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Common;
using Marshal;
using Common.Network;
using Common.Packets;
using System.Threading;

namespace Proxy
{
    public class Client
    {
        private StreamPacketizer packetizer = null;
        private TCPSocket socket = null;
        private Thread thr = null;
        private Session session = null;
        private int nodeID = 1; // Proxy nodeID

        public Client(StreamPacketizer p, TCPSocket s)
        {
            socket = s;
            packetizer = p;
            thr = new Thread(Run);
        }

        public void Start()
        {
            thr.Start();
        }

        public void InitialSession(Session from)
        {
            session = from;
        }

        private void Run()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(1);

                    byte[] data = new byte[socket.Available];
                    int bytes = 0;

                    try
                    {
                        bytes = socket.Recv(data);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode != 10035)
                            throw new DisconnectException();
                    }
                    catch (Exception)
                    {
                        throw new DisconnectException();
                    }

                    if (bytes == -1) // Client disconnected
                    {
                        throw new DisconnectException();
                    }
                    else if (bytes > 0)
                    {
                        packetizer.QueuePackets(data, bytes);
                        int p = packetizer.ProcessPackets();

                        byte[] actual = null;

                        for (int i = 0; i < p; i++)
                        {
                            actual = packetizer.PopItem();
                            PyObject obj = Unmarshal.Process<PyObject>(actual);

                            if (obj is PyObjectEx)
                            {
                                // PyException
                                Log.Error("Client", "Got exception from client");
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
                                    // Get the node ID to send
                                    if (packet.dest.type == PyAddress.AddrType.Node)
                                    {
                                        if (packet.dest.typeID == 1)
                                        {
                                            packet.dest.typeID = (ulong)nodeID; // We dont want to receive packets in the proxy
                                        }

                                        if (packet.source.type != PyAddress.AddrType.Client)
                                        {
                                            Log.Error("Client", string.Format("Wrong source data, expected client but got {0}", packet.source.type));
                                        }

                                        Log.Warning("Client", PrettyPrinter.Print(packet.Encode()));

                                        if (NodeManager.NotifyNode((int)packet.dest.typeID, obj) == false)
                                        {
                                            // We cant send the data to the node, what to do?
                                            Log.Error("Client", "Trying to send a packet to a non-existing node");
                                            throw new DisconnectException();
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (DisconnectException)
            {

            }
            catch (Exception ex)
            {
                Log.Error("Client", "Unhandled exception... " + ex.Message);
                Log.Error("ExceptionHandler", "Stack trace: " + ex.StackTrace);
            }

            // We should notify our node about this
            Log.Error("Client", "Client disconnected");
            socket.Close();
            ClientManager.RemoveClient(this);
        }

        public void Send(PyObject data)
        {
            Send(Marshal.Marshal.Process(data));
        }

        private void Send(byte[] data)
        {
            // The same error as in Proxy/Connection.cs -.-'
            byte[] packet = new byte[data.Length + 4];

            Array.Copy(data, 0, packet, 4, data.Length);
            Array.Copy(BitConverter.GetBytes(data.Length), packet, 4);

            socket.Send(packet);
        }

        public void ChangeToNode(int newNodeID)
        {
            nodeID = newNodeID;
            SendSessionChange();
        }

        public void SendSessionChange()
        {
            PyPacket sc = CreateEmptySessionChange();

            PyObject client = SetSessionChangeDestination(sc);
            PyObject node = SetSessionChangeDestination(sc, nodeID);

            if (sc != null)
            {
                Send(client);

                NodeManager.NotifyNode(nodeID, node);
            }
        }

        public PyPacket CreateEmptySessionChange()
        {
            // Fill all the packet data, except the dest/source
            SessionChangeNotification scn = new SessionChangeNotification();
            scn.changes = session.EncodeChanges();

            if (scn.changes.Dictionary.Count == 0)
            {
                // Nothing to do
                return null;
            }

            // Add all the nodeIDs
            foreach (int node in NodeManager.nodes.Keys)
            {
                if (node == 0)
                    continue;

                scn.nodesOfInterest.Items.Add(new PyInt(node));
            }

            PyPacket p = new PyPacket();

            p.type_string = "macho.SessionChangeNotification";
            p.type = Macho.MachoNetMsg_Type.SESSIONCHANGENOTIFICATION;

            p.userID = (uint)ClientManager.GetClientID(this);

            p.payload = scn.Encode().As<PyTuple>();

            p.named_payload = new PyDict();
            p.named_payload.Set("channel", new PyString("sessionchange"));

            return p;
        }

        public PyObject SetSessionChangeDestination(PyPacket p)
        {
            p.source.type = PyAddress.AddrType.Node;
            p.source.typeID = (ulong)nodeID;
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Client;
            p.dest.typeID = (ulong)ClientManager.GetClientID(this);
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
            p.source.typeID = (ulong)nodeID;
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Client;
            p.dest.typeID = (ulong)GetAccountID();
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

        public string GetLanguageID()
        {
            return session.GetCurrentString("languageID");
        }

        public int GetAccountID()
        {
            return session.GetCurrentInt("userid");
        }

        public int GetAccountRole()
        {
            return session.GetCurrentInt("role");
        }

        public string GetAddress()
        {
            return session.GetCurrentString("address");
        }

        public void SetNodeID(int newNodeID)
        {
            nodeID = newNodeID;
        }
    }
}
