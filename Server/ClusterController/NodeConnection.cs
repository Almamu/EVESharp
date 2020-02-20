using Common.Logging;
using Common.Network;
using Common.Packets;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace ClusterControler
{
    public class NodeConnection : Connection
    {
        private Channel Log { get; set; }
        public long NodeID { get; private set; }

        public NodeConnection(EVEClientSocket socket, ConnectionManager connectionManager, Logger logger, long nodeID)
            : base(socket, connectionManager)
        {
            // TODO: FREE THE LOG CHANNEL ONCE THE NODE IS MARKED AS DISCONNECTED
            this.Log = logger.CreateLogChannel($"Node-{this.Socket.GetRemoteAddress()}");
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
            nodeInfo.solarSystems.Add(new PyNone());

            Log.Debug($"Notifying node {nodeInfo.nodeID:X4} of it's new ID");

            this.Socket.Send(nodeInfo);
        }

        private void ReceivePacketCallback(PyDataType input)
        {
            Log.Debug("Processing packet from node");

            PyPacket packet = input;

            if (packet.Type == MachoMessageType.CALL_RSP || packet.Type == MachoMessageType.ERRORRESPONSE)
            {
                if (packet.Destination is PyAddressClient)
                {
                    Log.Trace($"Sending packet to client {packet.UserID}");
                    this.ConnectionManager.NotifyClient((int) (packet.UserID), packet);
                }
                else if (packet.Destination is PyAddressNode)
                {
                    PyAddressNode address = packet.Destination as PyAddressNode;

                    Log.Trace($"Sending packet to node {address.NodeID}");
                    this.ConnectionManager.NotifyNode((int) (address.NodeID), packet);
                }
                else if (packet.Destination is PyAddressBroadcast)
                {
                    Log.Error("Broadcast packets not supported yet");
                }

                // TODO: Handle Broadcast packets
            }
            else
            {
                Log.Error($"Wrong packet name: {packet.type_string}");
            }
        }
    }
}