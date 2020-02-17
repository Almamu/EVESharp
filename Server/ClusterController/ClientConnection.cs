using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Constants;
using Common.Game;
using Common.Logging;
using Common.Network;
using Common.Packets;
using PythonTypes;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace ClusterControler
{
    public class ClientConnection : Connection
    {
        private Channel Log { get; set; }
        public int NodeID { get; private set; }
        
        public Session Session { get; private set; }

        public long AccountID
        {
            get { return this.Session["userid"] as PyInteger; }
            private set { }
        }
        
        public ClientConnection(EVEClientSocket socket, ConnectionManager connectionManager, Logger logger)
            : base(socket, connectionManager)
        {
            // to easily identify the clients use the address as channel
            this.Log = logger.CreateLogChannel(this.Socket.GetRemoteAddress());
            this.Socket.Log = this.Log;
            // set the exception handler
            this.Socket.SetExceptionHandler(ExceptionHandler);
            // send the lowlevelversion exchange
            // this.SendLowLevelVersionExchange();
            // setup the first async handler (for the low level version exchange)
            this.Socket.SetReceiveCallback(ReceiveCommandCallback);
            // prepare the user's session
            this.Session = new Session();
            // TODO: GET SOCKET ADDRESS
            this.Session["address"] = this.Socket.GetRemoteAddress();
        }

        protected override void OnConnectionLost()
        {
            // remove the user from the correct lists
            if(this.Session.ContainsKey("userid") == false)
                this.ConnectionManager.RemoveUnauthenticatedClientConnection(this);
            else
                this.ConnectionManager.RemoveAuthenticatedClientConnection(this);
            // and free the socket resources
            this.Socket.ForcefullyDisconnect();
        }

        private void ReceiveLowLevelVersionExchangeCallback(PyDataType ar)
        {
            try
            {
                LowLevelVersionExchange exchange = ar;
            }
            catch (Exception e)
            {
                Log.Error($"Exception caught on LowLevelVersionExchange: {e.Message}");
                throw;
            }
            // assign the new packet handler to wait for commands again
            this.Socket.SetReceiveCallback(ReceiveCommandCallback);
        }
        private void ReceiveCommandCallback(PyDataType packet)
        {
            ClientCommand command = packet;
            
            if(command.Command == "QC")
            {
                Log.Debug("Received QueueCheck command");
                // send player position on the queue
                this.Socket.Send(new PyInteger(this.ConnectionManager.LoginQueue.Count()));
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
            
            if(request.Command != "placebo")
                throw new InvalidDataException($"Unknown command {request.Command}, expected 'placebo'");
            
            if(request.Arguments.Length > 0)
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
            
            if (request.user_password == null)
            {
                Log.Trace("Rejected by server; requesting plain password");
                // request the user a plain password
                this.Socket.Send(new PyInteger(1)); // 1 => plain, 2 => hashed
                return;
            }
            // set languageid in the session
            this.Session["languageID"] = request.user_languageid;
            
            // add the user to the authentication queue
            this.ConnectionManager.LoginQueue.Enqueue(this, request);
        }

        private void ReceiveLoginResultResponse(PyDataType packet)
        {
            PyTuple data = packet as PyTuple;
            
            if (data.Count != 3)
                throw new Exception($"Expected tuple to have 3 items but got {data.Count}");
            
            // Handshake sent when we are mostly in
            HandshakeAck ack = new HandshakeAck();

            ack.live_updates = new PyList();
            ack.jit = this.Session["languageID"] as PyString;
            ack.userid = this.Session["userid"] as PyInteger;
            ack.maxSessionTime = new PyNone();
            ack.userType = Common.Constants.AccountType.User;
            ack.role = this.Session["role"] as PyInteger;
            ack.address = this.Session["address"] as PyString;
            ack.inDetention = new PyNone();
            ack.client_hashes = new PyList();
            ack.user_clientid = this.Session["userid"] as PyInteger;

            // send the response first
            this.Socket.Send(ack);
            // send the session change
            this.SendSessionChange();
            // finally assign the correct packet handler
            this.Socket.SetReceiveCallback(ReceivePacketResponse);
        }

        private void ReceivePacketResponse(PyDataType packet)
        {
            if(packet is PyObject)
                throw new Exception("Got exception from client");

            PyPacket pyPacket = packet;
            
            if (pyPacket.UserID != this.Session["userid"] as PyInteger)
                throw new Exception("Received a packet coming from a client trying to spoof It's userID");

            if(pyPacket.Destination is PyAddressNode)
            {
                // search for the node in the list
                if(pyPacket.Source is PyAddressClient == false)
                    throw new Exception("Received a packet coming from a client trying to spoof the address");

                PyAddressNode dest = pyPacket.Destination as PyAddressNode;
                
                this.ConnectionManager.NotifyNode((int) dest.NodeID, packet);
            }
            else if (pyPacket.Destination is PyAddressAny)
            {
                // send packet to node proxy
                Log.Trace("Sending service request from any to proxy node");

                this.ConnectionManager.NotifyNode((int) Network.PROXY_NODE_ID, pyPacket);
            }
            else
            {
                throw new Exception($"Unexpected destination tyoe {pyPacket.Destination.GetType().Name} for packet");
            }
        }

        protected void ExceptionHandler(Exception exception)
        {
            Log.Error(exception.Message);
            Log.Trace(exception.StackTrace);
        }
        
        private void SendLowLevelVersionExchange()
        {
            Log.Debug("Sending LowLevelVersionExchange...");

            LowLevelVersionExchange data = new LowLevelVersionExchange();

            data.codename = Common.Constants.Game.codename;
            data.birthday = Common.Constants.Game.birthday;
            data.build = Common.Constants.Game.build;
            data.machoVersion = Common.Constants.Game.machoVersion;
            data.version = Common.Constants.Game.version;
            data.usercount = this.ConnectionManager.ClientsCount;
            data.region = Common.Constants.Game.region;

            this.Socket.Send(data);
        }
        
        public void SendLoginNotification(LoginStatus loginStatus, long accountID, long role)
        {
            if (loginStatus == LoginStatus.Sucess)
            {
                // We should check for a exact number of nodes here when we have the needed infraestructure
                if (this.ConnectionManager.NodesCount > 0)
                {
                    this.NodeID = Network.PROXY_NODE_ID;

                    AuthenticationRsp rsp = new AuthenticationRsp();

                    // String "None" marshaled
                    byte[] func_marshaled_code = new byte[] { 0x74, 0x04, 0x00, 0x00, 0x00, 0x4E, 0x6F, 0x6E, 0x65 };

                    rsp.serverChallenge = "";
                    rsp.func_marshaled_code = func_marshaled_code;
                    rsp.verification = false;
                    rsp.cluster_usercount = this.ConnectionManager.ClientsCount;
                    rsp.proxy_nodeid = this.NodeID;
                    rsp.user_logonqueueposition = 1;
                    rsp.challenge_responsehash = "55087";

                    rsp.macho_version = Common.Constants.Game.machoVersion;
                    rsp.boot_version = Common.Constants.Game.version;
                    rsp.boot_build = Common.Constants.Game.build;
                    rsp.boot_codename = Common.Constants.Game.codename;
                    rsp.boot_region = Common.Constants.Game.region;

                    // setup session
                    this.Session["userType"] = Common.Constants.AccountType.User;
                    this.Session["userid"] = accountID;
                    this.Session["role"] = role;
                    // move the connection to the authenticated user list
                    this.ConnectionManager.RemoveUnauthenticatedClientConnection(this);
                    this.ConnectionManager.AddAuthenticatedClientConnection(this);
                    // send the login response
                    this.Socket.Send(rsp);
                    // set second to last packet handler
                    this.Socket.SetReceiveCallback(ReceiveLoginResultResponse);
                }
                else
                {
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

        private void AbortConnection()
        {
            this.Socket.GracefulDisconnect();
            
            // remove the user from the correct lists
            if(this.Session.ContainsKey("userid") == false)
                this.ConnectionManager.RemoveUnauthenticatedClientConnection(this);
            else
                this.ConnectionManager.RemoveAuthenticatedClientConnection(this);
        }
        
        // TODO: MOVE THIS CODE TO THE SESSION HANDLER INSTEAD
        
        public void SendSessionChange()
        {
            PyPacket packet = CreateEmptySessionChange();

            if (packet == null)
                return;

            PyDataType client = SetSessionChangeDestination(packet);
            PyDataType node = SetSessionChangeDestination(packet, NodeID);

            this.Socket.Send(client);
            this.ConnectionManager.NotifyNode(NodeID, node);
        }

        public PyPacket CreateEmptySessionChange()
        {
            // Fill all the packet data, except the dest/source
            SessionChangeNotification scn = new SessionChangeNotification();
            scn.changes = Session.GenerateSessionChange();

            if (scn.changes.Length == 0)
            {
                // Nothing to do
                return null;
            }

            Dictionary<long, NodeConnection> nodes = this.ConnectionManager.Nodes;

            // Add all the nodeIDs
            foreach (KeyValuePair<long, NodeConnection> node in nodes)
            {
                scn.nodesOfInterest.Add(node.Key);
            }

            PyPacket p = new PyPacket();

            p.type_string = "macho.SessionChangeNotification";
            p.Type = MachoMessageType.SESSIONCHANGENOTIFICATION;

            p.UserID = this.Session["userid"] as PyInteger;

            p.Payload = scn;

            p.NamedPayload = new PyDictionary();
            p.NamedPayload["channel"] = "sessionchange";

            return p;
        }

        public PyDataType SetSessionChangeDestination(PyPacket p)
        {
            p.Source = new PyAddressNode(NodeID, 0);
            p.Destination = new PyAddressClient(this.Session["userid"] as PyInteger, 0);

            return p;
        }

        public PyDataType SetSessionChangeDestination(PyPacket p, int node)
        {
            p.Source = new PyAddressNode(1, 0);
            p.Destination = new PyAddressNode(node, 0);

            return p;
        }
    }
}