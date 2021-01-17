using System;
using System.Collections.Generic;
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
            
            // alter the ping responses from nodes to add the extra required information
            if (packet.Type == PyPacket.PacketType.PING_RSP)
            {
                // alter package to include the times the data
                PyTuple handleMessage = new PyTuple(3);

                // this time should come from the stream packetizer or the socket itself
                // but there's no way we're adding time tracking for all the goddamned packets
                // so this should be sufficient
                handleMessage[0] = DateTime.UtcNow.ToFileTime();
                handleMessage[1] = DateTime.UtcNow.ToFileTime();
                handleMessage[2] = "proxy::handle_message";
                
                PyTuple writing = new PyTuple(3);

                writing[0] = DateTime.UtcNow.ToFileTime();
                writing[1] = DateTime.UtcNow.ToFileTime();
                writing[2] = "proxy::writing";
                
                (packet.Payload[0] as PyList)?.Add(handleMessage);
                (packet.Payload[0] as PyList)?.Add(writing);
            }

            if (packet.Type == PyPacket.PacketType.CALL_RSP || packet.Type == PyPacket.PacketType.ERRORRESPONSE ||
                packet.Type == PyPacket.PacketType.PING_RSP || packet.Type == PyPacket.PacketType.CALL_REQ)
            {
                if (packet.Destination is PyAddressClient)
                {
                    Log.Trace($"Sending packet to client {packet.UserID}");
                    this.ConnectionManager.NotifyClient((int) (packet.UserID), packet);
                }
                else if (packet.Destination is PyAddressNode address)
                {
                    Log.Trace($"Sending packet to node {address.NodeID}");
                    this.ConnectionManager.NotifyNode((int) (address.NodeID), packet);
                }
                else if (packet.Destination is PyAddressBroadcast)
                {
                    Log.Error("Broadcast packets not supported yet");
                }
            }
            else if (packet.Type == PyPacket.PacketType.SESSIONCHANGENOTIFICATION)
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

                ClientConnection client = this.ConnectionManager.Clients[(int) packet.UserID];

                // send the sessionchangenotification to the client
                client.Socket.Send(packet);
                // update the local copy of the session too
                client.UpdateSession(packet);

                // parse the notification, get the nodes of interest and tell them about this session change too
                SessionChangeNotification scn = packet.Payload;

                foreach (PyInteger nodeID in scn.nodesOfInterest)
                {
                    this.ConnectionManager.NotifyNode(nodeID, packet);
                }
            }
            else if (packet.Type == PyPacket.PacketType.NOTIFICATION)
            {
                if (packet.Destination is PyAddressBroadcast == false)
                {
                    Log.Error("Received a notification that is not a broadcast...");
                    return;
                }
                
                PyAddressBroadcast destination = packet.Destination as PyAddressBroadcast;

                if (packet.UserID == -1)
                {
                    // special situation, the ClusterController has to take care of fullfiling the proper information
                    // in the packet, like UserID
                    switch (destination.IDType)
                    {
                        case "solarsystemid2":
                            foreach (PyDataType idData in destination.IDsOfInterest)
                            {
                                PyInteger id = idData as PyInteger;
                                
                                foreach (KeyValuePair<long, ClientConnection> entry in this.ConnectionManager.Clients)
                                {
                                    if (entry.Value.SolarSystemID2 == id)
                                    {
                                        // use the key instead of AccountID as this should be faster
                                        packet.UserID = entry.Key;
                                        // queue the packet for the user
                                        entry.Value.Socket.Send(packet);
                                    }
                                }
                            }
                            break;
                        
                        case "constellationid":
                            foreach (PyDataType idData in destination.IDsOfInterest)
                            {
                                PyInteger id = idData as PyInteger;
                                
                                foreach (KeyValuePair<long, ClientConnection> entry in this.ConnectionManager.Clients)
                                {
                                    if (entry.Value.ConstellationID == id)
                                    {
                                        // use the key instead of AccountID as this should be faster
                                        packet.UserID = entry.Key;
                                        // queue the packet for the user
                                        entry.Value.Socket.Send(packet);
                                    }
                                }
                            }
                            break;
                        
                        case "corpid":
                            foreach (PyDataType idData in destination.IDsOfInterest)
                            {
                                PyInteger id = idData as PyInteger;
                                
                                foreach (KeyValuePair<long, ClientConnection> entry in this.ConnectionManager.Clients)
                                {
                                    if (entry.Value.CorporationID == id)
                                    {
                                        // use the key instead of AccountID as this should be faster
                                        packet.UserID = entry.Key;
                                        // queue the packet for the user
                                        entry.Value.Socket.Send(packet);
                                    }
                                }
                            }
                            break;
                        
                        case "regionid":
                            foreach (PyDataType idData in destination.IDsOfInterest)
                            {
                                PyInteger id = idData as PyInteger;
                                
                                foreach (KeyValuePair<long, ClientConnection> entry in this.ConnectionManager.Clients)
                                {
                                    if (entry.Value.RegionID == id)
                                    {
                                        // use the key instead of AccountID as this should be faster
                                        packet.UserID = entry.Key;
                                        // queue the packet for the user
                                        entry.Value.Socket.Send(packet);
                                    }
                                }
                            }
                            break;
                        case "charid":
                            PyList idlist = destination.IDsOfInterest;

                            foreach (PyDataType idData in idlist)
                            {
                                PyInteger id = idData as PyInteger;
                                
                                foreach (KeyValuePair<long, ClientConnection> entry in this.ConnectionManager.Clients)
                                {
                                    if (entry.Value.CharacterID == id)
                                    {
                                        // use the key instead of AccountID as this should be faster
                                        packet.UserID = entry.Key;
                                        // change the ids of interest to hide the character's we've notified
                                        destination.IDsOfInterest = (PyList) new PyDataType[] {id};
                                        // queue the packet for the user
                                        entry.Value.Socket.Send(packet);
                                    }
                                }
                            }
                            break;
                        case "stationid":
                            Log.Warning($"stationid based notifications not supported yet");
                            break;
                        case "allianceid":
                            Log.Warning($"allianceid based notifications not supported yet");
                            break;
                        // TODO: IMPLEMENT stationid AND allianceid (there seems to be more, look at the client's code)
                        case "nodeid":
                            Log.Warning($"Inter-node notifications not implemented yet! Perhaps you're interested in implementing it?");
                            break;
                        default:
                            Log.Error($"Unexpected broadcast with idtype {destination.IDType.Value} and negative userID (autofill by ClusterController)");
                            break;
                    }
                }
                else
                {
                    Log.Trace($"Relaying notification to client {packet.UserID}");

                    this.ConnectionManager.NotifyClient((int) packet.UserID, packet);
                }
            }
            else
            {
                Log.Error($"Wrong packet name: {packet.TypeString}");
            }
        }
    }
}