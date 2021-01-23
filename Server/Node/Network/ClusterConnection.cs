using System;
using System.Text.RegularExpressions;
using Common.Constants;
using Common.Logging;
using Common.Network;
using Common.Packets;
using Node.Database;
using Node.Inventory.Items.Types;
using Org.BouncyCastle.Bcpg;
using PythonTypes;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Network
{
    public class ClusterConnection
    {
        private Channel Log { get; }
#if DEBUG
        private Channel CallLog { get; }
        private Channel ResultLog { get; }
#endif
        public EVEClientSocket Socket { get; }
        private NodeContainer Container { get; }
        private SystemManager SystemManager { get; }
        private ServiceManager ServiceManager { get; }
        private ClientManager ClientManager { get; }
        private BoundServiceManager BoundServiceManager { get; }
        private Container DependencyInjector { get; }
        private AccountDB AccountDB { get; }

        public ClusterConnection(NodeContainer container, SystemManager systemManager, ServiceManager serviceManager,
            ClientManager clientManager, BoundServiceManager boundServiceManager, Logger logger, Container dependencyInjector,
            AccountDB accountDB)
        {
            this.Log = logger.CreateLogChannel("ClusterConnection");
#if DEBUG
            this.CallLog = logger.CreateLogChannel("CallDebug", true);
            this.ResultLog = logger.CreateLogChannel("ResultDebug", true);
#endif
            this.SystemManager = systemManager;
            this.ServiceManager = serviceManager;
            this.ClientManager = clientManager;
            this.BoundServiceManager = boundServiceManager;
            this.DependencyInjector = dependencyInjector;
            this.Container = container;
            this.AccountDB = accountDB;
            this.Socket = new EVEClientSocket(this.Log);
            this.Socket.SetReceiveCallback(ReceiveLowLevelVersionExchangeCallback);
            this.Socket.SetExceptionHandler(HandleException);
        }

        private void HandleException(Exception ex)
        {
            Log.Error("Exception detected: ");

            do
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            } while ((ex = ex.InnerException) != null);
        }

        private void ReceiveLowLevelVersionExchangeCallback(PyDataType ar)
        {
            try
            {
                LowLevelVersionExchange exchange = this.CheckLowLevelVersionExchange(ar);

                // reply with the node LowLevelVersionExchange
                LowLevelVersionExchange reply = new LowLevelVersionExchange();

                reply.codename = Common.Constants.Game.codename;
                reply.birthday = Common.Constants.Game.birthday;
                reply.build = Common.Constants.Game.build;
                reply.machoVersion = Common.Constants.Game.machoVersion;
                reply.version = Common.Constants.Game.version;
                reply.region = Common.Constants.Game.region;
                reply.isNode = true;
                reply.nodeIdentifier = "Node";

                this.Socket.Send(reply);

                // set the new handler
                this.Socket.SetReceiveCallback(ReceiveNodeInfoCallback);
            }
            catch (Exception e)
            {
                Log.Error($"Exception caught on LowLevelVersionExchange: {e.Message}");
                throw;
            }
        }

        private void ReceiveNodeInfoCallback(PyDataType ar)
        {
            if (ar is PyObjectData == false)
                throw new Exception($"Expected PyObjectData for machoNet.nodeInfo but got {ar.GetType()}");

            PyObjectData info = ar as PyObjectData;

            if (info.Name != "machoNet.nodeInfo")
                throw new Exception($"Expected PyObjectData of type machoNet.nodeInfo but got {info.Name}");

            // Update our local info
            NodeInfo nodeinfo = info;

            this.Container.NodeID = nodeinfo.nodeID;

            Log.Debug("Found machoNet.nodeInfo, our new node id is " + nodeinfo.nodeID.ToString("X4"));

            // load the specified solar systems
            this.SystemManager.LoadSolarSystems(nodeinfo.solarSystems);

            // finally set the new packet handler
            this.Socket.SetReceiveCallback(ReceiveNormalPacketCallback);
        }

        private void ReceiveNormalPacketCallback(PyDataType ar)
        {
            PyPacket packet = ar;
            Client client = null;
            
            if (this.ClientManager.Contains(packet.UserID))
                client = this.ClientManager.Get(packet.UserID);

            if (packet.Type == PyPacket.PacketType.CALL_REQ)
            {
                PyPacket result;
                PyTuple callInfo = ((packet.Payload[0] as PyTuple)[1] as PySubStream).Stream as PyTuple;
                
                string call = callInfo[1] as PyString;
                PyTuple args = callInfo[2] as PyTuple;
                PyDictionary sub = callInfo[3] as PyDictionary;
                PyDataType callResult = null;
                PyAddressClient source = packet.Source as PyAddressClient;
                string destinationService = null;
                CallInformation callInformation;
                
                if (packet.Destination is PyAddressAny destAny)
                {
                    destinationService = destAny.Service;
                }
                else if (packet.Destination is PyAddressNode destNode)
                {
                    destinationService = destNode.Service;

                    if (destNode.NodeID != Container.NodeID)
                    {
                        Log.Fatal(
                            "Received a call request for a node that is not us, did the ClusterController get confused or something?!"
                        );
                        return;
                    }
                }
                
                callInformation = new CallInformation
                {
                    Client = client,
                    CallID = source.CallID,
                    NamedPayload = sub,
                    PacketType = packet.Type,
                    Service = destinationService,
                    From = packet.Source,
                    To = packet.Destination
                };

                try
                {
                    if (destinationService == null)
                    {
                        if (callInfo[0] is PyString == false)
                        {
                            Log.Fatal("Expected bound call with bound string, but got something different");
                            return;
                        }

                        string boundString = callInfo[0] as PyString;

                        // parse the bound string to get back proper node and bound ids
                        Match regexMatch = Regex.Match(boundString, "N=([0-9]+):([0-9]+)");

                        if (regexMatch.Groups.Count != 3)
                        {
                            Log.Fatal($"Cannot find nodeID and boundID in the boundString {boundString}");
                            return;
                        }

                        int nodeID = int.Parse(regexMatch.Groups[1].Value);
                        int boundID = int.Parse(regexMatch.Groups[2].Value);

                        if (nodeID != this.Container.NodeID)
                        {
                            Log.Fatal("Got bound service call for a different node");
                            // TODO: MIGHT BE A GOOD IDEA TO RELAY THIS CALL TO THE CORRECT NODE
                            // TODO: INSIDE THE NETWORK, AT LEAST THAT'S WHAT CCP IS DOING BASED
                            // TODO: ON THE CLIENT'S CODE... NEEDS MORE INVESTIGATION
                            return;
                        }

#if DEBUG
                        CallLog.Trace("Payload");
                        CallLog.Trace(PrettyPrinter.FromDataType(args));
                        CallLog.Trace("Named payload");
                        CallLog.Trace(PrettyPrinter.FromDataType(sub));
#endif

                        callResult = this.BoundServiceManager.ServiceCall(
                            boundID, call, args, callInformation
                        );

#if DEBUG
                        ResultLog.Trace("Result");
                        ResultLog.Trace(PrettyPrinter.FromDataType(callResult));
#endif
                    }
                    else
                    {
                        Log.Trace($"Calling {destinationService}::{call}");

#if DEBUG
                        CallLog.Trace("Payload");
                        CallLog.Trace(PrettyPrinter.FromDataType(args));
                        CallLog.Trace("Named payload");
                        CallLog.Trace(PrettyPrinter.FromDataType(sub));
#endif

                        callResult = this.ServiceManager.ServiceCall(
                            destinationService, call, args, callInformation
                        );

#if DEBUG
                        ResultLog.Trace("Result");
                        ResultLog.Trace(PrettyPrinter.FromDataType(callResult));
#endif
                    }

                    this.SendCallResult(callInformation, callResult);
                }
                catch (PyException e)
                {
                    this.SendException(callInformation, e);
                }
                catch (ProvisionalResponse provisional)
                {
                    this.SendProvisionalResponse(callInformation, provisional);
                }
            }
            else if (packet.Type == PyPacket.PacketType.SESSIONCHANGENOTIFICATION)
            {
                Log.Debug($"Updating session for client {packet.UserID}");

                // ensure the client is registered in the node and store his session
                if (this.ClientManager.Contains(packet.UserID) == false)
                    this.ClientManager.Add(packet.UserID, this.DependencyInjector.GetInstance<Client>());

                this.ClientManager.Get(packet.UserID).UpdateSession(packet);
                return;
            }
            else if (packet.Type == PyPacket.PacketType.PING_REQ)
            {
                // alter package to include the times the data
                PyTuple handleMessage = new PyTuple(3);
                PyAddressClient source = packet.Source as PyAddressClient;

                // this time should come from the stream packetizer or the socket itself
                // but there's no way we're adding time tracking for all the goddamned packets
                // so this should be sufficient
                handleMessage[0] = DateTime.UtcNow.ToFileTime();
                handleMessage[1] = DateTime.UtcNow.ToFileTime();
                handleMessage[2] = "server::handle_message";
            
                PyTuple turnaround = new PyTuple(3);

                turnaround[0] = DateTime.UtcNow.ToFileTime();
                turnaround[1] = DateTime.UtcNow.ToFileTime();
                turnaround[2] = "server::turnaround";
                
                (packet.Payload[0] as PyList)?.Add(handleMessage);
                (packet.Payload[0] as PyList)?.Add(turnaround);

                // change to a response
                packet.Type = PyPacket.PacketType.PING_RSP;
                
                // switch source and destination
                packet.Source = packet.Destination;
                packet.Destination = source;
                
                // queue the packet back
                this.Socket.Send(packet);
            }
            else if (packet.Type == PyPacket.PacketType.CALL_RSP)
            {
                // ensure the response is directed to us
                if (packet.Destination is PyAddressNode == false)
                {
                    Log.Error("Received a call response not directed to us");
                    return;
                }

                PyAddressNode dest = packet.Destination as PyAddressNode;

                if (dest.NodeID != Container.NodeID)
                {
                    Log.Error($"Received a call response for node {dest.NodeID} but we are {Container.NodeID}");
                    return;
                }
                
                // handle call response
                if (packet.Payload.Count != 1)
                {
                    Log.Error("Received a call response without proper response data");
                    return;
                }

                PyDataType first = packet.Payload[0];

                if (first is PySubStream == false)
                {
                    Log.Error("Received a call response without proper response data");
                    return;
                }
                
                PySubStream subStream = packet.Payload[0] as PySubStream;
                
                this.ServiceManager.ReceivedRemoteCallAnswer(dest.CallID, subStream.Stream);
            }
            else if (packet.Type == PyPacket.PacketType.NOTIFICATION)
            {
                PyTuple callInfo = ((packet.Payload[0] as PyTuple)[1] as PySubStream).Stream as PyTuple;

                PyList objectIDs = callInfo[0] as PyList;
                string call = callInfo[1] as PyString;

                if (call != "ClientHasReleasedTheseObjects")
                {
                    Log.Error($"Received notification from client with unknown method {call}");
                    return;
                }
                
                // search for the given objects in the bound service
                // and sure they're freed
                foreach (PyDataType objectID in objectIDs)
                {
                    if (objectID is PyTuple == false)
                    {
                        Log.Fatal("Got a ClientHasReleasedTheseObjects for an unknown object id");
                        continue;
                    }
                    
                    PyTuple tuple = objectID as PyTuple;
                    
                    if (tuple[0] is PyString == false)
                    {
                        Log.Fatal("Expected bound call with bound string, but got something different");
                        return;
                    }

                    string boundString = tuple[0] as PyString;

                    // parse the bound string to get back proper node and bound ids
                    Match regexMatch = Regex.Match(boundString, "N=([0-9]+):([0-9]+)");

                    if (regexMatch.Groups.Count != 3)
                    {
                        Log.Fatal($"Cannot find nodeID and boundID in the boundString {boundString}");
                        return;
                    }

                    int nodeID = int.Parse(regexMatch.Groups[1].Value);
                    int boundID = int.Parse(regexMatch.Groups[2].Value);

                    if (nodeID != this.Container.NodeID)
                    {
                        Log.Fatal("Got a ClientHasReleasedTheseObjects call for an object ID that doesn't belong to us");
                        // TODO: MIGHT BE A GOOD IDEA TO RELAY THIS CALL TO THE CORRECT NODE
                        // TODO: INSIDE THE NETWORK, AT LEAST THAT'S WHAT CCP IS DOING BASED
                        // TODO: ON THE CLIENT'S CODE... NEEDS MORE INVESTIGATION
                        return;
                    }
                    
                    this.BoundServiceManager.FreeBoundService(boundID);
                }
            }
            
            // send any notification that might be pending
            client?.SendPendingNotifications();
        }

        protected LowLevelVersionExchange CheckLowLevelVersionExchange(PyDataType exchange)
        {
            LowLevelVersionExchange data = exchange;

            if (data.birthday != Common.Constants.Game.birthday)
                throw new Exception("Wrong birthday in LowLevelVersionExchange");

            if (data.build != Common.Constants.Game.build)
                throw new Exception("Wrong build in LowLevelVersionExchange");

            if (data.codename != Common.Constants.Game.codename + "@" + Common.Constants.Game.region)
                throw new Exception("Wrong codename in LowLevelVersionExchange");

            if (data.machoVersion != Common.Constants.Game.machoVersion)
                throw new Exception("Wrong machoVersion in LowLevelVersionExchange");

            if (data.version != Common.Constants.Game.version)
                throw new Exception("Wrong version in LowLevelVersionExchange");

            if (data.isNode == true)
            {
                if (data.nodeIdentifier != "Node")
                    throw new Exception("Wrong node string in LowLevelVersionExchange");
            }
            
            return data;
        }

        public void SendNotification(string notificationType, string idType, PyList idsOfInterest, Client client, PyDataType data)
        {
            PyTuple dataContainer = new PyTuple(new PyDataType []
                {
                    1, data
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    0, dataContainer
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    0, new PySubStream(dataContainer)
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    dataContainer, null
                }
            );
            
            PyPacket packet = new PyPacket(PyPacket.PacketType.NOTIFICATION);

            packet.Destination = new PyAddressBroadcast(idsOfInterest, idType, notificationType);
            packet.Source = new PyAddressNode(this.Container.NodeID);

            packet.UserID = client.AccountID;
            packet.Payload = dataContainer;

            this.Socket.Send(packet);
        }

        public void SendNotification(string notificationType, string idType, int id, Client client, PyDataType data)
        {
            PyTuple dataContainer = new PyTuple(new PyDataType []
                {
                    1, data
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    0, dataContainer
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    0, new PySubStream(dataContainer)
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    dataContainer, null
                }
            );
            
            PyPacket packet = new PyPacket(PyPacket.PacketType.NOTIFICATION);

            packet.Destination = new PyAddressBroadcast((PyList) new PyDataType[] { id }, idType, notificationType);
            packet.Source = new PyAddressNode(this.Container.NodeID);

            packet.UserID = client.AccountID;
            packet.Payload = dataContainer;

            this.Socket.Send(packet);
        }

        public void SendNotification(string notificationType, string idType, int id, PyDataType data)
        {
            PyTuple dataContainer = new PyTuple(new PyDataType []
                {
                    1, data
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    0, dataContainer
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    0, new PySubStream(dataContainer)
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    dataContainer, null
                }
            );
            
            PyPacket packet = new PyPacket(PyPacket.PacketType.NOTIFICATION);

            packet.Destination = new PyAddressBroadcast((PyList) new PyDataType[] { id }, idType, notificationType);
            packet.Source = new PyAddressNode(this.Container.NodeID);

            // set the userID to -1, this will indicate the cluster controller to fill it in
            packet.UserID = -1;
            packet.Payload = dataContainer;

            this.Socket.Send(packet);
        }
        
        public void SendNotification(string notificationType, string idType, PyList ids, PyDataType data)
        {
            PyTuple dataContainer = new PyTuple(new PyDataType []
                {
                    1, data
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    0, dataContainer
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    0, new PySubStream(dataContainer)
                }
            );

            dataContainer = new PyTuple(new PyDataType[]
                {
                    dataContainer, null
                }
            );
            
            PyPacket packet = new PyPacket(PyPacket.PacketType.NOTIFICATION);

            packet.Destination = new PyAddressBroadcast(ids, idType, notificationType);
            packet.Source = new PyAddressNode(this.Container.NodeID);

            // set the userID to -1, this will indicate the cluster controller to fill it in
            packet.UserID = -1;
            packet.Payload = dataContainer;

            this.Socket.Send(packet);
        }

        public void SendProvisionalResponse(CallInformation answerTo, ProvisionalResponse response)
        {
            PyPacket result = new PyPacket(PyPacket.PacketType.CALL_RSP);
            
            // ensure destination has clientID in it
            PyAddressClient source = answerTo.From as PyAddressClient;

            source.ClientID = answerTo.Client.AccountID;
            // switch source and dest
            result.Source = answerTo.To;
            result.Destination = source;

            result.UserID = source.ClientID;
            result.Payload = new PyTuple(0);
            result.NamedPayload = new PyDictionary
            {
                ["provisional"] = new PyTuple(3)
                {
                    [0] = response.Timeout, // macho timeout in seconds
                    [1] = response.EventID,
                    [2] = response.Arguments
                }
            };

            this.Socket.Send(result);
        }
        
        public void SendException(CallInformation answerTo, PyDataType content)
        {
            // build a new packet with the correct information
            PyPacket result = new PyPacket(PyPacket.PacketType.ERRORRESPONSE);
            
            // ensure destination has clientID in it
            PyAddressClient source = answerTo.From as PyAddressClient;

            source.ClientID = answerTo.Client.AccountID;
            // switch source and dest
            result.Source = answerTo.To;
            result.Destination = source;

            result.UserID = source.ClientID;
            result.Payload = new PyTuple(new PyDataType[]
            {
                (int) answerTo.PacketType, (int) MachoErrorType.WrappedException, new PyTuple (new PyDataType[] { new PySubStream(content) }), 
            });

            this.Socket.Send(result);
        }

        public void SendCallResult(CallInformation answerTo, PyDataType content)
        {
            PyPacket result = new PyPacket(PyPacket.PacketType.CALL_RSP);
            
            // ensure destination has clientID in it
            PyAddressClient source = answerTo.From as PyAddressClient;

            source.ClientID = answerTo.Client.AccountID;
            // switch source and dest
            result.Source = answerTo.To;
            result.Destination = source;

            result.UserID = source.ClientID;
            result.Payload = new PyTuple(new PyDataType[] {new PySubStream(content)});

            this.Socket.Send(result);
        }

        private void ClientSendServiceCall(int clientID, string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback, object extraInfo = null, int timeoutSeconds = 0)
        {
            // queue the call in the service manager and get the callID
            int callID = this.ServiceManager.ExpectRemoteServiceResult(callback, extraInfo, timeoutCallback, timeoutSeconds);
            
            // prepare the request packet
            PyPacket packet = new PyPacket(PyPacket.PacketType.CALL_REQ);

            packet.UserID = clientID;
            packet.Destination = new PyAddressClient(clientID, null, service);
            packet.Source = new PyAddressNode(this.Container.NodeID, callID);
            packet.NamedPayload = new PyDictionary();
            packet.NamedPayload["role"] = (int) Roles.ROLE_SERVICE | (int) Roles.ROLE_REMOTESERVICE;
            packet.Payload = new PyTuple(2)
            {
                [0] = new PyTuple (2)
                {
                    [0] = 0,
                    [1] = new PySubStream(new PyTuple(4)
                    {
                        [0] = 1,
                        [1] = call,
                        [2] = args,
                        [3] = namedPayload
                    })
                },
                [1] = null
            };
            
            // everything is ready, send the packet to the client
            this.Socket.Send(packet);
        }

        public void SendServiceCall(Client client, string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0)
        {
            this.ClientSendServiceCall(client.AccountID, service, call, args, namedPayload, callback, timeoutCallback, extraInfo, timeoutSeconds);
        }

        public void SendServiceCall(int characterID, string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0)
        {
            this.ClientSendServiceCall(
                this.AccountDB.GetAccountIDFromCharacterID(characterID),
                service, call, args, namedPayload, callback, timeoutCallback, extraInfo, timeoutSeconds
            );
        }

        public void SendServiceCall(Character character, string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0)
        {
            this.SendServiceCall(character.ID, service, call, args, namedPayload, callback,timeoutCallback, extraInfo, timeoutSeconds);
        }
    }
}