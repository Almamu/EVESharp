using System;
using Common;
using Common.Network;
using Common.Packets;
using Marshal;

namespace ClusterControler
{
    public class NodeConnection: Connection
    {
        public long NodeID { get; private set; }
        
        public NodeConnection(EVEClientSocket socket, ConnectionManager connectionManager, long nodeID)
            : base(socket, connectionManager)
        {
            this.NodeID = nodeID;
            // send node change notification
            this.SendNodeChangeNotification();
            // assign callbacks to the socket
            this.Socket.SetReceiveCallback(ReceivePacketCallback);
        }

        protected override void OnConnectionLost()
        {
            // TODO: GET CLUSTER CONTROLLER TO MOVE THE RESOURCES HERE TO OTHER NODES
            // remove this node from the active list
            this.ConnectionManager.RemoveNodeConnection(this);
            // forcefully close the socket to free resources
            this.Socket.ForcefullyDisconnect();
        }

        private void SendNodeChangeNotification()
        {
            NodeInfo nodeInfo = new NodeInfo();

            nodeInfo.nodeID = NodeID;
            // solar systems to be loaded by the node
            nodeInfo.solarSystems.Items.Add(new PyNone());

            Log.Debug("Node", $"Notifying node {nodeInfo.nodeID} of it's new ID");
            
            this.Socket.Send(nodeInfo);
        }

        private void ReceivePacketCallback(PyObject packet)
        {
            Log.Debug("Node", "Processing packet from node");
            
            if (packet is PyObjectData == false)
                throw new Exception("Non-valid node packet. Ignoring...");

            PyObjectData item = packet as PyObjectData;

            if (item.Name == "macho.CallRsp")
            {
                PyPacket final = new PyPacket();

                final.Decode(item);

                if (final.dest.type == PyAddress.AddrType.Client)
                {
                    Log.Trace("Node", $"Sending packet to client {final.userID}");
                    this.ConnectionManager.NotifyClient((int)(final.userID), packet);
                }
                else if (final.dest.type == PyAddress.AddrType.Node)
                {
                    Log.Trace("Node", $"Sending packet to node {final.dest.typeID}");
                    this.ConnectionManager.NotifyNode((int)(final.dest.typeID), packet);
                }
                else if (final.dest.type == PyAddress.AddrType.Broadcast)
                {
                    Log.Error("Node", "Broadcast packets not supported yet");
                }
                // TODO: Handle Broadcast packets
            }
            else
            {
                Log.Error("Node", $"Wrong packet name: {item.Name}");
            }
        }
    }
}