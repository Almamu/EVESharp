using System;
using System.Text.RegularExpressions;
using Common.Logging;
using Common.Network;
using Common.Packets;
using PythonTypes;
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
        private SystemManager SystemManager { get; set; }
        private ServiceManager ServiceManager { get; set; }
        private ClientManager ClientManager { get; set; }
        private BoundServiceManager BoundServiceManager { get; set; }
        private Container DependencyInjector { get; set; }

        public ClusterConnection(NodeContainer container, SystemManager systemManager, ServiceManager serviceManager,
            ClientManager clientManager, BoundServiceManager boundServiceManager, Logger logger, Container dependencyInjector)
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

            if (packet.Type == PyPacket.PacketType.CALL_REQ)
            {
                PyPacket result;
                PyTuple callInfo = ((packet.Payload[0] as PyTuple)[1] as PySubStream).Stream as PyTuple;
                
                string call = callInfo[1] as PyString;
                PyTuple args = callInfo[2] as PyTuple;
                PyDictionary sub = callInfo[3] as PyDictionary;
                PyDataType callResult = null;
                PyAddressClient source = packet.Source as PyAddressClient;

                try
                {
                    if (packet.Destination is PyAddressAny destAny)
                    {
                        if (destAny.Service.Length == 0)
                        {
                            if (callInfo[0] is PyString == false)
                            {
                                Log.Fatal("Expected bound call with bound string, but got something different");
                                return;
                            }

                            string boundString = callInfo[0] as PyString;
                            
                            // parse the bound string to get back proper node and bound ids
                            Match regexMatch = Regex.Match(boundString, "N=([0-9]+):([0-9]+)");

                            if (regexMatch.Captures.Count != 2)
                            {
                                Log.Fatal("Cannot find nodeID and boundID in the boundString");
                                return;
                            }

                            int nodeID = int.Parse(regexMatch.Captures[0].Value);
                            int boundID = int.Parse(regexMatch.Captures[1].Value);

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
                                boundID, call, args, sub, this.ClientManager.Get(packet.UserID)
                            );

#if DEBUG
                            ResultLog.Trace("Result");
                            ResultLog.Trace(PrettyPrinter.FromDataType(callResult));
#endif
                        }
                        else
                        {
                            Log.Trace($"Calling {destAny.Service.Value}::{call}");
                            
#if DEBUG
                            CallLog.Trace("Payload");
                            CallLog.Trace(PrettyPrinter.FromDataType(args));
                            CallLog.Trace("Named payload");
                            CallLog.Trace(PrettyPrinter.FromDataType(sub));
#endif
                            
                            callResult = this.ServiceManager.ServiceCall(
                                destAny.Service, call, args, sub, this.ClientManager.Get(packet.UserID)
                            );    

#if DEBUG
                            ResultLog.Trace("Result");
                            ResultLog.Trace(PrettyPrinter.FromDataType(callResult));
#endif
                        }
                    }
                    else if (packet.Destination is PyAddressNode destNode)
                    {
                        if (destNode.Service == null)
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
                                Log.Fatal("Cannot find nodeID and boundID in the boundString");
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
                                boundID, call, args, sub, this.ClientManager.Get(packet.UserID)
                            );

#if DEBUG
                            ResultLog.Trace("Result");
                            ResultLog.Trace(PrettyPrinter.FromDataType(callResult));
#endif
                        }
                        else
                        {
                            Log.Trace($"Calling {destNode.Service.Value}::{call}");
                            
#if DEBUG
                            CallLog.Trace("Payload");
                            CallLog.Trace(PrettyPrinter.FromDataType(args));
                            CallLog.Trace("Named payload");
                            CallLog.Trace(PrettyPrinter.FromDataType(sub));
#endif

                            callResult = this.ServiceManager.ServiceCall(
                                destNode.Service, call, args, sub, this.ClientManager.Get(packet.UserID)
                            );

#if DEBUG
                            ResultLog.Trace("Result");
                            ResultLog.Trace(PrettyPrinter.FromDataType(callResult));
#endif
                        }
                    }
                    else
                    {
                        Log.Error($"Received packet that wasn't directed to us");
                    }
                    
                    result = new PyPacket(PyPacket.PacketType.CALL_RSP);

                    // ensure destination has clientID in it
                    source.ClientID = (int) packet.UserID;
                    // switch source and dest
                    result.Source = packet.Destination;
                    result.Destination = source;

                    result.UserID = packet.UserID;

                    result.Payload = new PyTuple(new PyDataType[] {new PySubStream(callResult)});
                }
                catch (PyException e)
                {
                    // PyExceptions have to be relayed to the client
                    // this way it's easy to notify the user without needing any special code
                    result = new PyPacket(PyPacket.PacketType.ERRORRESPONSE);

                    source.ClientID = (int) packet.UserID;
                    result.Source = packet.Destination;
                    result.Destination = source;

                    result.UserID = packet.UserID;
                    result.Payload = new PyTuple(new PyDataType[]
                    {
                        (int) packet.Type, (int) MachoErrorType.WrappedException, new PyTuple (new PyDataType[] { new PySubStream(e) }), 
                    });
                }

                // any of the paths that lead to the send here must have the PyPacket initialized
                // so a null check is not needed
                this.Socket.Send(result);
            }
            else if (packet.Type == PyPacket.PacketType.SESSIONCHANGENOTIFICATION)
            {
                Log.Debug($"Updating session for client {packet.UserID}");

                // ensure the client is registered in the node and store his session
                if (this.ClientManager.Contains(packet.UserID) == false)
                    this.ClientManager.Add(packet.UserID, this.DependencyInjector.GetInstance<Client>());

                this.ClientManager.Get(packet.UserID).UpdateSession(packet);
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
    }
}