using System;
using System.IO;
using System.Text.RegularExpressions;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.EVE;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Accounts;
using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network
{
    public class MachoClientTransport
    {
        /// <summary>
        /// The session associated with this transport
        /// </summary>
        public Session Session { get; }
        /// <summary>
        /// The server transport that created it
        /// </summary>
        private MachoServerTransport Server { get; init; }
        /// <summary>
        /// The underlying socket to send/receive data
        /// </summary>
        public EVEClientSocket Socket { get; init; }

#if DEBUG
        private Channel CallLog { get; init; }
        private Channel ResultLog { get; init; }
#endif
        
        private Channel Log { get; init; }
        /// <summary>
        /// The client related to this transport (if any)
        /// </summary>
        public Client Client { get; private set; }
        
        public MachoClientTransport(MachoServerTransport transport, EVEClientSocket socket, Logger logger)
        {
            this.Session = new Session();
            this.Server = transport;
            this.Socket = socket;
            this.Log = logger.CreateLogChannel(socket.GetRemoteAddress());
#if DEBUG
            this.CallLog = logger.CreateLogChannel("CallDebug", true);
            this.ResultLog = logger.CreateLogChannel("ResultDebug", true);
#endif
            // send low level version exchange to start authorization chain
            this.SendLowLevelVersionExchange();
            // setup receive callbacks and exception handler
            this.Socket.SetReceiveCallback(ReceiveFirstMessageCallback);
            this.Socket.SetExceptionHandler(HandleException);
            // setup basic session data
            this.Session["address"] = this.Socket.GetRemoteAddress();
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

        private void ReceiveFirstMessageCallback(PyDataType ar)
        {
            try
            {
                // depending on the type of data we're receiving, this has to be treated differently
                if (ar is PyObjectData)
                {
                    this.HandleIdentificationReq(ar);
                }
                else
                {
                    this.HandleLowLevelVersionExchange(ar);
                }
            }
            catch (Exception e)
            {
                Log.Error($"Exception caught on IdentificationReq/LowLevelVersionExchange: {e.Message}");
                throw;
            }
        }

        private void HandleIdentificationReq(IdentificationReq req)
        {
            Log.Error("Cannot handle identification req yet!");
        }

        private void HandleLowLevelVersionExchange(LowLevelVersionExchange ex)
        {
            Log.Debug("Handling low level version exchange");
            
            // assign the new packet handler to wait for commands again
            this.Socket.SetReceiveCallback(ReceiveCommandCallback);
        }

        private void ReceiveLowLevelVersionExchangeCallback(PyDataType ar)
        {
            try
            {
                this.HandleLowLevelVersionExchange(ar);
            }
            catch (Exception e)
            {
                Log.Error($"Exception caught on LowLevelVersionExchange: {e.Message}");
                throw;
            }
        }

        private void ReceiveCommandCallback(PyDataType packet)
        {
            ClientCommand command = packet;

            if (command.Command == "QC")
            {
                Log.Debug("Received QueueCheck command");
                // send player position on the queue
                this.Socket.Send(new PyInteger(this.Server.MachoNet.LoginQueue.Count()));
                // send low level version exchange required
                this.SendLowLevelVersionExchange();
                // wait for a new low level version exchange again
                this.Socket.SetReceiveCallback(ReceiveLowLevelVersionExchangeCallback);
            }
            else if (command.Command == "VK")
            {
                Log.Debug("Received VipKey command");
                // next is the placebo challenge
                this.Socket.SetReceiveCallback(ReceiveCryptoRequestCallback);
            }
            else
            {
                throw new Exception("Received unknown data!");
            }
        }

        private void ReceiveCryptoRequestCallback(PyDataType packet)
        {
            PlaceboRequest request = packet;

            if (request.Command != "placebo")
                throw new InvalidDataException($"Unknown command {request.Command}, expected 'placebo'");

            if (request.Arguments.Length > 0)
                Log.Warning("Received PlaceboRequest with extra arguments, this is not supported");

            Log.Debug("Received correct Crypto request");
            // answer the client with a correct crypto challenge
            this.Socket.Send(new PyString("OK CC"));
            // next is the first login attempt
            this.Socket.SetReceiveCallback(ReceiveAuthenticationRequestCallback);
        }

        private void ReceiveAuthenticationRequestCallback(PyDataType packet)
        {
            AuthenticationReq request = packet;

            if (request.user_password is null)
            {
                Log.Trace("Rejected by server; requesting plain password");
                // request the user a plain password
                this.Socket.Send(new PyInteger(1)); // 1 => plain, 2 => hashed
                return;
            }

            // TODO: DINAMICALLY FETCH THIS SO WE SUPPORT TRANSLATIONS
            if (request.user_languageid != "EN" && request.user_languageid != "RU" && request.user_languageid != "DE")
                // default to english language
                this.Session["languageID"] = "EN";
            else
                // set languageid in the session to the one requested as we have translations for that one
                this.Session["languageID"] = request.user_languageid;

            // add the user to the authentication queue
            this.Server.MachoNet.LoginQueue.Enqueue(this, request);
        }

        private void ReceiveLoginResultResponse(PyDataType packet)
        {
            PyTuple data = packet as PyTuple;

            if (data.Count != 3)
                throw new Exception($"Expected tuple to have 3 items but got {data.Count}");

            // Handshake sent when we are mostly in
            HandshakeAck ack = new HandshakeAck
            {
                LiveUpdates = this.Server.MachoNet.GeneralDB.FetchLiveUpdates(),
                JIT = this.Session["languageID"] as PyString,
                UserID = this.Session["userid"] as PyInteger,
                MaxSessionTime = null,
                UserType = AccountType.USER,
                Role = this.Session["role"] as PyInteger,
                Address = this.Session["address"] as PyString,
                InDetention = null,
                ClientHashes = new PyList(),
                UserClientID = this.Session["userid"] as PyInteger
            };
            
            // send the response first
            this.Socket.Send(ack);
            // send the session change
            this.SendSessionChange();
            // set the client class
            this.Client = new Client(
                this.Server.MachoNet.Container, this, this.Server.MachoNet.ServiceManager, this.Server.MachoNet.TimerManager,
                this.Server.MachoNet.ItemFactory, this.Server.MachoNet.SystemManager, this.Server.MachoNet.NotificationManager, this.Server.MachoNet.ClientManager, this.Server.MachoNet
            );
            
            // finally assign the correct packet handler
            this.Socket.SetReceiveCallback(ReceivePacketResponse);
        }

        private void ReceivePacketResponse(PyDataType packet)
        {
            if (packet is PyObject)
                throw new Exception("Got exception from client");

            PyPacket pyPacket = packet;

            // generate the proper PyAddressClient for the notification packets
            // this prevents more special cases for the whole packet flow
            if (pyPacket.Type == PyPacket.PacketType.NOTIFICATION)
            {
                pyPacket.UserID = this.Session["userid"] as PyInteger;
                pyPacket.Source = new PyAddressClient(pyPacket.UserID);
            }
            
            if (pyPacket.UserID != this.Session["userid"] as PyInteger)
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

                long destNodeID = dest.NodeID;
                
                // change destination node if the node it looks for is us (node 0)
                if (dest.NodeID == this.Server.MachoNet.Container.NodeID)
                {
                    this.HandleNormalPacket(pyPacket);
                }
                else
                {
                    // TODO: RELAY THE PACKET TO THE PROPER NODE
                    /*
                     pyPacket.Destination = new PyAddressNode(this.NodeID, dest.CallID, dest.Service);
                     destNodeID = this.NodeID;
                    */
                }

                // send the packet to the correct node
                // this.ConnectionManager.NotifyNode(destNodeID, pyPacket);
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
            }
            
            // send any notification that might be pending
            this.Client?.SendPendingNotifications();
        }

        private void HandleSessionChangeNotification(PyPacket packet)
        {
            /*
            Log.Debug($"Updating session for client {packet.UserID}");

            // ensure the client is registered in the node and store his session
            if (client == null)
                this.ClientManager.Add(
                    packet.UserID,
                    client = new Client(
                        this.Container, this.MachoServerTransport, this.ServiceManager, this.TimerManager,
                        this.ItemFactory, this.SystemManager, this.NotificationManager, this.ClientManager, this
                    )
                );

            client.UpdateSession(packet);
            */
            // TODO: UPDATE THE SESSION THAT COMES FROM THE NODE
        }

        private void HandleNotification(PyPacket packet)
        {
            if (packet.Source is PyAddressAny)
            {
                this.Server.MachoNet.HandleBroadcastNotification(packet);
                return;
            }
            
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
                Client = this.Client,
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

                    callResult = this.Server.MachoNet.ServiceManager.ServiceCall(
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
            catch (Exception ex)
            {
                int errorID = ++this.Server.MachoNet.ErrorCount;

                Log.Fatal($"Detected non-client exception on call to {callInformation.Service}::{call}, registered as error {errorID}. Extra information: ");
                Log.Fatal(ex.Message);
                Log.Fatal(ex.StackTrace);
                
                // send client a proper notification about the error based on the roles
                if ((callInformation.Client.Role & (int) Roles.ROLE_PROGRAMMER) == (int) Roles.ROLE_PROGRAMMER)
                {
                    this.SendException(callInformation, new CustomError($"An internal server error occurred.<br><b>Reference</b>: {errorID}<br><b>Message</b>: {ex.Message}<br><b>Stack trace</b>:<br>{ex.StackTrace.Replace("\n", "<br>")}"));
                }
                else
                {
                    this.SendException(callInformation, new CustomError($"An internal server error occurred. <b>Reference</b>: {errorID}"));
                }
            }
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
            result.OutOfBounds = new PyDictionary
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


        public void SendCallResult(CallInformation answerTo, PyDataType content)
        {
            // ensure destination has clientID in it
            PyAddressClient originalSource = answerTo.From as PyAddressClient;
            originalSource.ClientID = answerTo.Client.AccountID;

            this.Socket.Send(
                new PyPacket(PyPacket.PacketType.CALL_RSP)
                {
                    // switch source and dest
                    Source = answerTo.To,
                    Destination = originalSource,
                    UserID = originalSource.ClientID,
                    Payload = new PyTuple(1) {[0] = new PySubStream(content)}
                }
            );
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
            result.Payload = new PyTuple(3)
            {
                [0] = (int) answerTo.PacketType,
                [1] = (int) MachoErrorType.WrappedException,
                [2] = new PyTuple (1) { [0] = new PySubStream(content) }, 
            };

            this.Socket.Send(result);
        }

        protected void SendLowLevelVersionExchange()
        {
            Log.Debug("Sending LowLevelVersionExchange...");

            LowLevelVersionExchange data = new LowLevelVersionExchange
            {
                Codename = Game.CODENAME,
                Birthday = Game.BIRTHDAY,
                Build = Game.BUILD,
                MachoVersion = Game.MACHO_VERSION,
                Version = Game.VERSION,
                UserCount = this.Server.ClientTransports.Count,
                Region = Game.REGION
            };

            this.Socket.Send(data);
        }

        public void SendLoginNotification(LoginStatus loginStatus, long accountID, long role)
        {
            if (loginStatus == LoginStatus.Success)
            {
                // We should check for a exact number of nodes here when we have the needed infraestructure
                if (true)
                {
                    AuthenticationRsp rsp = new AuthenticationRsp();

                    // String "None" marshaled
                    byte[] func_marshaled_code = new byte[] {0x74, 0x04, 0x00, 0x00, 0x00, 0x4E, 0x6F, 0x6E, 0x65};

                    rsp.serverChallenge = "";
                    rsp.func_marshaled_code = func_marshaled_code;
                    rsp.verification = false;
                    rsp.cluster_usercount = this.Server.ClientTransports.Count;
                    rsp.proxy_nodeid = 0; // ProxyNodeID is 0
                    rsp.user_logonqueueposition = 1;
                    rsp.challenge_responsehash = "55087";

                    rsp.macho_version = Game.MACHO_VERSION;
                    rsp.boot_version = Game.VERSION;
                    rsp.boot_build = Game.BUILD;
                    rsp.boot_codename = Game.CODENAME;
                    rsp.boot_region = Game.REGION;

                    // setup session
                    this.Session["userType"] = AccountType.USER;
                    this.Session["userid"] = accountID;
                    this.Session["role"] = role;
                    // move the connection to the authenticated user list
                    this.Server.ResolveClientTransport(this);
                    // send the login response
                    this.Socket.Send(rsp);
                    // set second to last packet handler
                    this.Socket.SetReceiveCallback(ReceiveLoginResultResponse);
                }
                else
                {
                    // TODO: IMPLEMENT CLUSTER STARTUP
                    // Pretty funny, "AutClusterStarting" maybe they mean "AuthClusterStarting"
                    this.Socket.Send(new GPSTransportClosed("AutClusterStarting"));

                    Log.Trace("Rejected by server; cluster is starting");

                    this.AbortConnection();
                }
            }
            else if (loginStatus == LoginStatus.Failed)
            {
                this.Socket.Send(new GPSTransportClosed("LoginAuthFailed"));
                this.AbortConnection();
            }
        }
        
        public void SendServiceCall(string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0)
        {
            // queue the call in the service manager and get the callID
            int callID = this.Server.MachoNet.ServiceManager.ExpectRemoteServiceResult(callback, this.Client, extraInfo, timeoutCallback, timeoutSeconds);
            
            // prepare the request packet
            PyPacket packet = new PyPacket(PyPacket.PacketType.CALL_REQ);

            packet.UserID = this.Session["userid"] as PyInteger;
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
        
        public void AbortConnection()
        {
            this.Socket.GracefulDisconnect();

            // remove the transport from the list
            this.Server.OnTransportTerminated(this);
        }

        // TODO: MOVE THIS CODE TO THE SESSION HANDLER INSTEAD

        public void SendSessionChange()
        {
            PyPacket packet = CreateEmptySessionChange();

            if (packet is null)
                return;

            PyDataType client = SetSessionChangeDestination(packet);

            this.Socket.Send(client);
            // TODO: IS THIS REALLY NEEDED?
            // this.ConnectionManager.NotifyAllNodes(packet);
        }

        public PyPacket CreateEmptySessionChange()
        {
            // Fill all the packet data, except the dest/source
            SessionChangeNotification scn = new SessionChangeNotification
            {
                Changes = Session.GenerateSessionChange()
            };

            if (scn.Changes.Length == 0)
                // Nothing to do
                return null;

            PyPacket packet = new PyPacket(PyPacket.PacketType.SESSIONCHANGENOTIFICATION)
            {
                UserID = this.Session["userid"] as PyInteger,
                Payload = scn,
                OutOfBounds = new PyDictionary
                {
                    ["channel"] = "sessionchange"
                }
            };

            return packet;
        }

        public PyDataType SetSessionChangeDestination(PyPacket packet)
        {
            packet.Source = new PyAddressNode(this.Server.MachoNet.Container.NodeID, 0);
            packet.Destination = new PyAddressClient(this.Session["userid"] as PyInteger, 0);

            return packet;
        }

        public PyDataType SetSessionChangeDestination(PyPacket packet, int node)
        {
            packet.Source = new PyAddressNode(1, 0);
            packet.Destination = new PyAddressNode(node, 0);

            return packet;
        }
        
        
        public void UpdateSession(PyPacket packet)
        {
            if (packet.Payload.TryGetValue(0, out PyTuple sessionData) == false)
                throw new InvalidDataException("SessionChangeNotification expected a payload of size 1");
            if (sessionData.TryGetValue(1, out PyDictionary differences) == false)
                throw new InvalidDataException("SessionChangeNotification expected a differences collection");

            this.Session.LoadChanges(differences);
        }

    }    
}