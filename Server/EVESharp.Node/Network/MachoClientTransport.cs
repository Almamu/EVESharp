using System;
using System.IO;
using System.Text.RegularExpressions;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.EVE;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Accounts;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public class MachoClientTransport : MachoTransport
    {
#if DEBUG
        private Channel CallLog { get; init; }
        private Channel ResultLog { get; init; }
#endif
        private SessionManager SessionManager => this.Server.MachoNet.SessionManager;
        
        public MachoClientTransport(MachoTransport source) : base(source)
        {
#if DEBUG
            this.CallLog = this.Log.Logger.CreateLogChannel("CallDebug", true);
            this.ResultLog = this.Log.Logger.CreateLogChannel("ResultDebug", true);
#endif
            // finally assign the correct packet handler
            this.Socket.SetReceiveCallback(ReceiveNormalPacket);
            this.Socket.SetExceptionHandler(HandleException);
            this.Socket.SetOnConnectionLostHandler(HandleConnectionLost);
        }

        private void HandleConnectionLost()
        {
            Log.Fatal($"Client {this.Session.UserID} lost connection to the server");
            
            // clean up ourselves
            this.Server.OnTransportTerminated(this);
            // remove the session
            this.SessionManager.FreeSession(this.Session);
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

        private void ReceiveNormalPacket(PyDataType packet)
        {
            if (packet is PyObject)
                throw new Exception("Got exception from client");

            PyPacket pyPacket = packet;

            // generate the proper PyAddressClient for the notification packets
            // this prevents more special cases for the whole packet flow
            if (pyPacket.Type == PyPacket.PacketType.NOTIFICATION)
            {
                pyPacket.UserID = this.Session.UserID;
                pyPacket.Source = new PyAddressClient(pyPacket.UserID);
            }
            
            if (pyPacket.UserID != this.Session.UserID)
                throw new Exception("Received a packet coming from a client trying to spoof It's userID");

            if (pyPacket.Type == PyPacket.PacketType.PING_REQ)
            {
                // alter package to include the times the data

                // this time should come from the stream packetizer or the socket itself
                // but there's no way we're adding time tracking for all the goddamned packets
                // so this should be sufficient
                PyTuple handleMessage = new PyTuple(3)
                {
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

                (pyPacket.Payload[0] as PyList)?.Add(handleMessage);
                (pyPacket.Payload[0] as PyList)?.Add(writing);
            }
            
            if (pyPacket.Destination is PyAddressNode dest)
            {
                // search for the node in the list
                if (pyPacket.Source is PyAddressClient == false)
                    throw new Exception("Received a packet coming from a client trying to spoof the address");
                
                // handle the package locally if the destination node is us
                if (dest.NodeID == this.Server.MachoNet.Container.NodeID)
                {
                    this.HandleNormalPacket(pyPacket);
                }
                else
                {
                    // update the outofbounds data with the session information
                    if (pyPacket.OutOfBounds is null)
                        pyPacket.OutOfBounds = new PyDictionary();
                    
                    pyPacket.OutOfBounds["Session"] = this.Session;
                    
                    // re-queue the packet
                    this.Server.MachoNet.QueuePacket(pyPacket);
                }
            }
            else if (pyPacket.Destination is PyAddressAny)
            {
                // request to any should be routed to whatever node we want
                // there might be a good algorithm for determining this
                // but for now use the node this player belongs to and call it a day
                // this.ConnectionManager.NotifyNode(this.NodeID, pyPacket);
                this.HandleNormalPacket(pyPacket);
            }
            else
            {
                throw new Exception($"Unexpected destination type {pyPacket.Destination.GetType().Name} for packet");
            }
        }

        private void HandleNormalPacket(PyPacket packet)
        {
            try
            {
                switch (packet.Type)
                {
                    case PyPacket.PacketType.CALL_REQ:
                        this.HandleCallReq(packet);
                        break;
                    case PyPacket.PacketType.SESSIONCHANGENOTIFICATION:
                        this.HandleSessionChangeNotification(packet);
                        break;
                    case PyPacket.PacketType.PING_REQ:
                        this.HandlePingReq(packet);
                        break;
                    case PyPacket.PacketType.CALL_RSP:
                        this.HandleCallRes(packet);
                        break;
                    case PyPacket.PacketType.NOTIFICATION:
                        this.HandleNotification(packet);
                        break;
                    default:
                        throw new CustomError("Packet type not allowed");
                }
            }
            catch (PyException ex)
            {
                this.Server.MachoNet.SendException(packet.Source, packet.Destination, packet.Type, ex);
            }
            catch (Exception ex)
            {
                int errorID = ++this.Server.MachoNet.ErrorCount;

                Log.Fatal($"Detected non-client exception on packet, registered as error {errorID}. Extra information: ");
                Log.Fatal(ex.Message);
                Log.Fatal(ex.StackTrace);
                
                // send client a proper notification about the error based on the roles
                if ((this.Session.Role & (int) Roles.ROLE_PROGRAMMER) == (int) Roles.ROLE_PROGRAMMER)
                {
                    this.Server.MachoNet.SendException(packet.Source, packet.Destination, packet.Type, new CustomError($"An internal server error occurred.<br><b>Reference</b>: {errorID}<br><b>Message</b>: {ex.Message}<br><b>Stack trace</b>:<br>{ex.StackTrace.Replace("\n", "<br>")}"));
                }
                else
                {
                    this.Server.MachoNet.SendException(packet.Source, packet.Destination, packet.Type, new CustomError($"An internal server error occurred. <b>Reference</b>: {errorID}"));
                }
            }
        }

        private void HandleSessionChangeNotification(PyPacket packet)
        {
            // get the session changes and update ourselves
            SessionChangeNotification scn = packet.Payload;
            
            Log.Debug($"Updating session for client {packet.UserID}");
            
            // load the delta into the current session
            this.Session.ApplyDelta(scn.Changes);
            
            // this also requires sending the data to the client AND nodes of interest
            // TODO: UPDATE THE SESSION THAT COMES FROM THE NODE
        }

        private void HandleNotification(PyPacket packet)
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
            foreach (PyTuple objectID in objectIDs.GetEnumerable<PyTuple>())
            {
                if (objectID[0] is PyString == false)
                {
                    Log.Fatal("Expected bound call with bound string, but got something different");
                    return;
                }

                string boundString = objectID[0] as PyString;

                // parse the bound string to get back proper node and bound ids
                Match regexMatch = Regex.Match(boundString, "N=([0-9]+):([0-9]+)");

                if (regexMatch.Groups.Count != 3)
                {
                    Log.Fatal($"Cannot find nodeID and boundID in the boundString {boundString}");
                    return;
                }

                int nodeID = int.Parse(regexMatch.Groups[1].Value);
                int boundID = int.Parse(regexMatch.Groups[2].Value);

                if (nodeID != this.Server.MachoNet.Container.NodeID)
                {
                    Log.Fatal("Got a ClientHasReleasedTheseObjects call for an object ID that doesn't belong to us");
                    // TODO: MIGHT BE A GOOD IDEA TO RELAY THIS CALL TO THE CORRECT NODE
                    // TODO: INSIDE THE NETWORK, AT LEAST THAT'S WHAT CCP IS DOING BASED
                    // TODO: ON THE CLIENT'S CODE... NEEDS MORE INVESTIGATION
                    return;
                }
                
                this.Server.MachoNet.BoundServiceManager.FreeBoundService(boundID);
            }
        }
        
        private void HandlePingReq(PyPacket packet)
        {
            // alter package to include the times the data
            PyAddressClient source = packet.Source as PyAddressClient;

            // this time should come from the stream packetizer or the socket itself
            // but there's no way we're adding time tracking for all the goddamned packets
            // so this should be sufficient
            PyTuple proxyHandleMessage = new PyTuple(3)
            {
                [0] = DateTime.UtcNow.ToFileTime(),
                [1] = DateTime.UtcNow.ToFileTime(),
                [2] = "proxy::handle_message"
            };

            PyTuple proxyWritting = new PyTuple(3)
            {
                [0] = DateTime.UtcNow.ToFileTime(),
                [1] = DateTime.UtcNow.ToFileTime(),
                [2] = "proxy::writing"
            };

            // this time should come from the stream packetizer or the socket itself
            // but there's no way we're adding time tracking for all the goddamned packets
            // so this should be sufficient
            PyTuple handleMessage = new PyTuple(3)
            {
                [0] = DateTime.UtcNow.ToFileTime(),
                [1] = DateTime.UtcNow.ToFileTime(),
                [2] = "server::handle_message"
            };

            PyTuple turnaround = new PyTuple(3)
            {
                [0] = DateTime.UtcNow.ToFileTime(),
                [1] = DateTime.UtcNow.ToFileTime(),
                [2] = "server::turnaround"
            };

            // TODO: SEND THESE TO A RANDOM NODE THAT IS NOT US!
            (packet.Payload[0] as PyList)?.Add(proxyHandleMessage);
            (packet.Payload[0] as PyList)?.Add(proxyWritting);
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

        private void HandleCallRes(PyPacket packet)
        {
            // ensure the response is directed to us
            if (packet.Destination is PyAddressNode == false)
            {
                Log.Error("Received a call response not directed to us");
                return;
            }

            PyAddressNode dest = packet.Destination as PyAddressNode;

            if (dest.NodeID != this.Server.MachoNet.Container.NodeID)
            {
                Log.Error($"Received a call response for node {dest.NodeID} but we are {this.Server.MachoNet.Container.NodeID}");
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
                
            this.Server.MachoNet.ServiceManager.ReceivedRemoteCallAnswer(dest.CallID, subStream.Stream);
        }
        
        private void HandleCallReq(PyPacket packet)
        {
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

                if (destNode.NodeID != this.Server.MachoNet.Container.NodeID)
                {
                    Log.Fatal(
                        "Received a call request for a node that is not us, did the ClusterController get confused or something?!"
                    );
                    return;
                }
            }

            callInformation = new CallInformation
            {
                CallID = source.CallID,
                Payload = args,
                NamedPayload = sub,
                Source = packet.Source,
                Destination = packet.Destination,
                MachoNet = this.Server.MachoNet,
                Session = this.Session,
                ResutOutOfBounds = new PyDictionary<PyString, PyDataType>()
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

                    if (nodeID != this.Server.MachoNet.Container.NodeID)
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

                    callResult = this.Server.MachoNet.BoundServiceManager.ServiceCall(
                        boundID, call, callInformation
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

                    callResult = this.Server.MachoNet.ServiceManager.ServiceCall(
                        destinationService, call, callInformation
                    );

#if DEBUG
                    ResultLog.Trace("Result");
                    ResultLog.Trace(PrettyPrinter.FromDataType(callResult));
#endif
                }

                this.Server.MachoNet.SendCallResult(callInformation, callResult, callInformation.ResutOutOfBounds);
            }
            catch (PyException e)
            {
                this.Server.MachoNet.SendException(callInformation, packet.Type, e);
            }
            catch (ProvisionalResponse provisional)
            {
                this.Server.MachoNet.SendProvisionalResponse(callInformation, provisional);
            }
            catch (Exception ex)
            {
                int errorID = ++this.Server.MachoNet.ErrorCount;

                Log.Fatal($"Detected non-client exception on call to {destinationService}::{call}, registered as error {errorID}. Extra information: ");
                Log.Fatal(ex.Message);
                Log.Fatal(ex.StackTrace);
                
                // send client a proper notification about the error based on the roles
                if ((callInformation.Session.Role & (int) Roles.ROLE_PROGRAMMER) == (int) Roles.ROLE_PROGRAMMER)
                {
                    this.Server.MachoNet.SendException(callInformation, packet.Type, new CustomError($"An internal server error occurred.<br><b>Reference</b>: {errorID}<br><b>Message</b>: {ex.Message}<br><b>Stack trace</b>:<br>{ex.StackTrace.Replace("\n", "<br>")}"));
                }
                else
                {
                    this.Server.MachoNet.SendException(callInformation, packet.Type, new CustomError($"An internal server error occurred. <b>Reference</b>: {errorID}"));
                }
            }
        }

        public void SendServiceCall(string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0)
        {
            // queue the call in the service manager and get the callID
            int callID = this.Server.MachoNet.ServiceManager.ExpectRemoteServiceResult(callback, this.Session, extraInfo, timeoutCallback, timeoutSeconds);
            
            // prepare the request packet
            PyPacket packet = new PyPacket(PyPacket.PacketType.CALL_REQ);

            packet.UserID = this.Session.UserID;
            packet.Destination = new PyAddressClient(packet.UserID, null, service);
            packet.Source = new PyAddressNode(this.Server.MachoNet.Container.NodeID, callID);
            packet.OutOfBounds = new PyDictionary();
            packet.OutOfBounds["role"] = (int) Roles.ROLE_SERVICE | (int) Roles.ROLE_REMOTESERVICE;
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
    }    
}