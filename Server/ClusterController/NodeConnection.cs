using System;
using System.Collections.Generic;
using Common.Logging;
using Common.Network;
using EVE.Packets;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace ClusterController
{
    public class NodeConnection : Connection
    {
        private SystemManager SystemManager { get; }
        private Channel Log { get; }
        public long NodeID { get; }
        public int SolarSystemLoadedCount { get; set; }

        public NodeConnection(EVEClientSocket socket, ConnectionManager connectionManager, SystemManager systemManager, Logger logger, long nodeID)
            : base(socket, connectionManager)
        {
            // TODO: FREE THE LOG CHANNEL ONCE THE NODE IS MARKED AS DISCONNECTED
            this.Log = logger.CreateLogChannel($"Node-{this.Socket.GetRemoteAddress()}");
            this.NodeID = nodeID;
            this.SystemManager = systemManager;
            // send node change notification
            this.SendNodeInitialState();
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

        private void SendNodeInitialState()
        {
            NodeInfo nodeInfo = new NodeInfo {NodeID = NodeID};

            Log.Debug($"Notifying node {nodeInfo.NodeID:X4} of it's new ID");

            this.Socket.Send(nodeInfo);
        }

        private static void HandlePingRsp(PyPacket packet)
        {
            // alter package to include the times the data
            PyTuple handleMessage = new PyTuple(3)
            {
                // this time should come from the stream packetizer or the socket itself
                // but there's no way we're adding time tracking for all the goddamned packets
                // so this should be sufficient
                [0] = DateTime.UtcNow.ToFileTime(),
                [1] = DateTime.UtcNow.ToFileTime(),
                [2] = "proxy::handle_message"
            };

            PyTuple writing = new PyTuple(3)
            {
                [0] = DateTime.UtcNow.ToFileTime(),
                [1] = DateTime.UtcNow.ToFileTime(),
                [2] = "proxy::writing"
            };
            
            (packet.Payload[0] as PyList)?.Add(handleMessage);
            (packet.Payload[0] as PyList)?.Add(writing);
        }
        
        private void HandleNotification(PyPacket packet)
        {
            if (packet.Destination is PyAddressBroadcast == false)
            {
                Log.Error("Received a notification that is not a broadcast...");
                return;
            }
            
            PyAddressBroadcast destination = packet.Destination as PyAddressBroadcast;

            if (packet.UserID != -1)
            {
                Log.Trace($"Relaying notification to client {packet.UserID}");

                this.ConnectionManager.NotifyClient((int) packet.UserID, packet);
                return;
            }
            
            // special situation, the ClusterController has to take care of fullfiling the proper information
            // in the packet, like UserID
            switch (destination.IDType)
            {
                case "solarsystemid2":
                    this.ConnectionManager.NotifyBySolarSystemID2(packet, destination);
                    break;
                case "constellationid":
                    this.ConnectionManager.NotifyByConstellationID(packet, destination);
                    break;
                case "corpid":
                    this.ConnectionManager.NotifyByCorporationID(packet, destination);
                    break;
                case "regionid":
                    this.ConnectionManager.NotifyByRegionID(packet, destination);
                    break;
                case "charid":
                    this.ConnectionManager.NotifyByCharacterID(packet, destination);
                    break;
                case "stationid":
                    this.ConnectionManager.NotifyByStationID(packet, destination);
                    break;
                case "allianceid":
                    this.ConnectionManager.NotifyByAllianceID(packet, destination);
                    break;
                case "nodeid":
                    this.ConnectionManager.NotifyByNodeID(packet, destination);
                    break;
                // TODO: IMPLEMENT OTHER NOTIFICATION ID TYPES BASED ON THE CLIENT CODE IF THEY'RE ACTUALLY USEFUL
                default:
                    Log.Error($"Unexpected broadcast with idtype {destination.IDType.Value} and negative userID (autofill by ClusterController)");
                    break;
            }
        }

        private void HandleSessionChangeNotification(PyPacket packet)
        {
            // session change notifications are always sent to the client
            // but interested nodes might want to hear about it too
            // (for example to add the character to the station he is in)
            // so this takes care of relaying the session change notification
            // from the originating node to all the interested
            if (packet.Destination is PyAddressClient == false)
            {
                Log.Error("Received a SessionChangeNotification not aimed to a client");
                return;
            }

            Log.Trace($"Sending SessionChangeNotification to client {packet.UserID}");

            ClientConnection client;
            
            lock (this.ConnectionManager.Clients)
                client = this.ConnectionManager.Clients[(int) packet.UserID];

            // send the sessionchangenotification to the client
            client.Socket.Send(packet);
            // update the local copy of the session too
            client.UpdateSession(packet);

            // check if the session change includes solar system change and ensure at least one node has the solar system loaded
            PyDataType newSolarsystemid2 = client.Session.GetCurrent("solarsystemid2");
            PyDataType oldSolarsystemid2 = client.Session.GetPrevious("solarsystemid2");
            int newSolarsystemid2int = 0;
            int oldSolarsystemid2int = 0;

            if (newSolarsystemid2 is PyInteger newSolarsystemid2integer)
                newSolarsystemid2int = newSolarsystemid2integer;
            if (oldSolarsystemid2 is PyInteger oldSolarsystemid2integer)
                oldSolarsystemid2int = oldSolarsystemid2integer;

            // the solar system changed, ensure it's loaded and if not ensure one node loads it at least
            if (newSolarsystemid2 != oldSolarsystemid2)
            {
                if (this.SystemManager.IsSolarSystemLoaded(newSolarsystemid2int) == false)
                {
                    long nodeID = this.SystemManager.LoadSolarSystem(newSolarsystemid2int);
                    
                    // send a notification to the node
                    this.ConnectionManager.NotifyNode(nodeID, "OnSolarSystemLoad", new PyTuple (1) { [0] = newSolarsystemid2int});
                    
                    // set the new nodeID for the client
                    client.NodeID = nodeID;
                }
            }

            // notify the nodes with the session changes now
            this.ConnectionManager.NotifyAllNodes(packet);
        }

        private void RelayPacket(PyPacket packet)
        {
            switch (packet.Destination)
            {
                case PyAddressClient _:
                    Log.Trace($"Sending packet to client {packet.UserID}");
                    this.ConnectionManager.NotifyClient((int) (packet.UserID), packet);
                    break;
                case PyAddressNode address:
                    Log.Trace($"Sending packet to node {address.NodeID}");
                    this.ConnectionManager.NotifyNode((int) (address.NodeID), packet);
                    break;
                case PyAddressBroadcast _:
                    Log.Error("Broadcast packets not supported yet");
                    break;
            }
        }

        private void ReceivePacketCallback(PyDataType input)
        {
            Log.Debug("Processing packet from node");

            PyPacket packet = input;
            
            // alter the ping responses from nodes to add the extra required information
            if (packet.Type == PyPacket.PacketType.PING_RSP)
                HandlePingRsp(packet);

            switch (packet.Type)
            {
                // relay packet if needed
                case PyPacket.PacketType.CALL_RSP:
                case PyPacket.PacketType.CALL_REQ:
                case PyPacket.PacketType.ERRORRESPONSE:
                case PyPacket.PacketType.PING_RSP:
                    this.RelayPacket(packet);
                    break;
                case PyPacket.PacketType.SESSIONCHANGENOTIFICATION:
                    this.HandleSessionChangeNotification(packet);
                    break;
                case PyPacket.PacketType.NOTIFICATION:
                    this.HandleNotification(packet);
                    break;
                default:
                    Log.Error($"Wrong packet name: {packet.TypeString}");
                    break;
            }
        }
    }
}