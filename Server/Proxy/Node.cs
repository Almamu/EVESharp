using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Common.Network;
using Common.Packets;
using Marshal;
using System.Threading;

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

        private void SendNodeInfo()
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
            // Send the node info
            SendNodeInfo();

            while (true)
            {
                Thread.Sleep(1);
            }
        }

        public void Notify(PyObject data)
        {
            Send(data);
        }
    }
}
