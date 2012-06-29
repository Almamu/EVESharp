using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Network;
using Common.Packets;
using Marshal;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Proxy
{
    public class Node
    {
        StreamPacketizer packetizer = null;
        TCPSocket socket = null;
        Thread thr = null;
        
        public Node(StreamPacketizer p, TCPSocket s)
        {
            socket = s;
            packetizer = p;
            thr = new Thread(Run);
            thr.Start();
        }

        private void SendNodeChangeNotification()
        {
            NodeInfo nodeInfo = new NodeInfo();

            nodeInfo.nodeID = NodeManager.GetNodeID(this);
            nodeInfo.solarSystems.Items.Add(new PyNone()); // None = All solar systems

            Send(nodeInfo.Encode());
        }

        private void Send(PyObject packet)
        {
            byte[] data = Marshal.Marshal.Process(packet);
            Send(data);
        }

        private void Send(byte[] data)
        {
            byte[] packet = new byte[data.Length + 4];

            Array.Copy(data, 0, packet, 4, data.Length);
            Array.Copy(BitConverter.GetBytes(data.Length), packet, 4);

            socket.Send(packet);
        }

        private void Send(PyPacket packet)
        {
            Send(packet.Encode());
        }

        public void Run()
        {
            while (true)
            {
                Thread.Sleep(1);

                try
                {
                    byte[] data = new byte[socket.Available];
                    int bytes = socket.Recv(data);

                    if (bytes == -1)
                    {
                        throw new DisconnectException();
                    }
                    else if (bytes > 0)
                    {
                        packetizer.QueuePackets(data, bytes);
                        int p = packetizer.ProcessPackets();

                        for (int i = 0; i < p; i++)
                        {
                            byte[] packet = packetizer.PopItem();
                            PyObject obj = Unmarshal.Process<PyObject>(packet);

                            if (obj.Type == PyObjectType.ObjectData)
                            {
                                Log.Warning("Node", PrettyPrinter.Print(obj));
                                PyObjectData item = obj as PyObjectData;

                                if (item.Name == "macho.CallRsp")
                                {
                                    PyPacket final = new PyPacket();

                                    if (final.Decode(item) == true)
                                    {
                                        if (final.dest.type == PyAddress.AddrType.Client)
                                        {
                                            try
                                            {
                                                ClientManager.NotifyClient((int)final.userID, obj);
                                            }
                                            catch (Exception)
                                            {
                                                Log.Error("Node", "Trying to send a packet to a non-existing client");
                                            }
                                        }
                                        else if (final.dest.type == PyAddress.AddrType.Node)
                                        {
                                            NodeManager.NotifyNode((int)final.dest.typeID, obj);
                                        }
                                        else if (final.dest.type == PyAddress.AddrType.Broadcast)
                                        {
                                            // This should not be coded like this here, but will do the trick for now
                                            // TODO: Add a ClientManager
                                            ClientManager.NotifyClients(obj);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Error("Node", "Unknown type");
                            }

                        }
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode != 10035)
                        break;
                }
                catch (DisconnectException)
                {
                    Log.Error("Node", "Node " + NodeManager.GetNodeID(this) + " disconnected");
                    break;
                }
                catch (Exception)
                {

                }
            }
        }

        public void Notify(PyObject data)
        {
            Send(data);
        }
    }
}
