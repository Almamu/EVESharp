using System;
using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Game;
using Common.Network;
using Common.Packets;
using Marshal;

namespace ClusterControler
{
    public class ClientConnection : Connection
    {
        public int NodeID { get; private set; }
        
        public Session Session { get; private set; }

        public long AccountID
        {
            get { return this.Session.GetCurrentLong("userid"); }
            private set { }
        }
        
        public ClientConnection(EVEClientSocket socket, ConnectionManager connectionManager)
            : base(socket, connectionManager)
        {
            // set the exception handler
            this.Socket.SetExceptionHandler(ExceptionHandler);
            // send the lowlevelversion exchange
            // this.SendLowLevelVersionExchange();
            // setup the first async handler (for the low level version exchange)
            this.Socket.SetReceiveCallback(ReceiveCommandCallback);
            // prepare the user's session
            this.Session = new Session();
            // TODO: GET SOCKET ADDRESS
            this.Session.SetString("address", this.Socket.GetRemoteAddress());
        }

        protected override void OnConnectionLost()
        {
            // remove the client from the active list
            this.ConnectionManager.RemoveUnauthenticatedClientConnection(this);
            // and free the socket resources
            this.Socket.ForcefullyDisconnect();
        }

        private void ReceiveLowLevelVersionExchangeCallback(PyObject ar)
        {
            try
            {
                LowLevelVersionExchange exchange = this.CheckLowLevelVersionExchange(ar);

                exchange.Decode(ar);
            }
            catch (Exception e)
            {
                Log.Error("LowLevelVersionExchange", e.Message);
                throw;
            }
            // assign the new packet handler to wait for commands again
            this.Socket.SetReceiveCallback(ReceiveCommandCallback);
        }
        private void ReceiveCommandCallback(PyObject packet)
        {
            if (packet.Type != PyObjectType.Tuple)
                throw new Exception($"Expected command to be of type Tuple but got {packet.Type}");

            PyTuple tuple = packet as PyTuple;

            if (tuple.Items.Count == 2)
            {
                QueueCheckCommand command = new QueueCheckCommand();
                
                command.Decode(packet);
                
                Log.Debug("Client", "Received QueueCheck command");
                // send player position on the queue
                this.Socket.Send(new PyInt(this.ConnectionManager.LoginQueue.Count()));
                // send low level version exchange required
                this.SendLowLevelVersionExchange();
                // wait for a new low level version exchange again
                this.Socket.SetReceiveCallback(ReceiveLowLevelVersionExchangeCallback);
            }
            else if (tuple.Items.Count == 3)
            {
                VipKeyCommand command = new VipKeyCommand();

                command.Decode(packet);
                
                Log.Debug("Client", "Received VipKey command");
                // next is the placebo challenge
                this.Socket.SetReceiveCallback(ReceiveCryptoRequestCallback);
            }
            else
            {
                throw new Exception("Received unknown data!");
            }
        }

        private void ReceiveCryptoRequestCallback(PyObject packet)
        {
            PlaceboRequest request = new PlaceboRequest();
            
            request.Decode(packet);
            
            Log.Debug("Client", "Received correct Crypto request");
            // answer the client with a correct crypto challenge
            this.Socket.Send(new PyString("OK CC"));
            // next is the first login attempt
            this.Socket.SetReceiveCallback(ReceiveAuthenticationRequestCallback);
        }

        private void ReceiveAuthenticationRequestCallback(PyObject packet)
        {
            AuthenticationReq request = new AuthenticationReq();

            request.Decode(packet);

            if (request.user_password == null)
            {
                Log.Trace("Client", "Rejected by server; requesting plain password");
                // request the user a plain password
                this.Socket.Send(new PyInt(1)); // 1 => plain, 2 => hashed
                return;
            }
            // set languageid in the session
            this.Session.SetString("languageID", request.user_languageid);
            
            // add the user to the authentication queue
            this.ConnectionManager.LoginQueue.Enqueue(this, request);
        }

        private void ReceiveLoginResultResponse(PyObject packet)
        {
            if (packet.Type != PyObjectType.Tuple)
                throw new Exception($"Expected login response to be a tuple but got {packet.Type}");

            PyTuple tuple = packet.As<PyTuple>();
            
            if (tuple.Items.Count != 3)
                throw new Exception($"Expected tuple to have 3 items but got {tuple.Items.Count}");
            
            // Handshake sent when we are mostly in
            HandshakeAck ack = new HandshakeAck();

            ack.live_updates = new PyList();
            ack.jit = this.Session.GetCurrentString("languageid");
            ack.userid = this.Session.GetCurrentLong("userid");
            ack.maxSessionTime = new PyNone();
            ack.userType = Common.Constants.AccountType.User;
            ack.role = this.Session.GetCurrentInt("role");
            ack.address = this.Session.GetCurrentString("address");
            ack.inDetention = new PyNone();
            ack.client_hashes = new PyList();
            ack.user_clientid = this.Session.GetCurrentLong("userid");;

            // send the response first
            this.Socket.Send(ack);
            // send the session change
            this.SendSessionChange();
            // finally assign the correct packet handler
            this.Socket.SetReceiveCallback(ReceivePacketResponse);
        }

        private void ReceivePacketResponse(PyObject packet)
        {
            if(packet is PyObjectEx)
                throw new Exception("Got exception from client");

            PyPacket pyPacket = new PyPacket();

            pyPacket.Decode(packet);
            
            if (pyPacket.userID != this.Session.GetCurrentLong("userid"))
                throw new Exception("Received a packet coming from a client trying to spoof It's userID");

            if (pyPacket.dest.type == PyAddress.AddrType.Node)
            {
                // search for the node in the list
                if(pyPacket.source.type != PyAddress.AddrType.Client)
                    throw new Exception("Received a packet coming from a client trying to spoof the address");
                if(pyPacket.dest.typeID == Network.PROXY_NODE_ID)
                    Log.Warning("Client", "Sending packet to proxy");
                
                this.ConnectionManager.NotifyNode((int) pyPacket.dest.typeID, packet);
            }
            else if (pyPacket.dest.type == PyAddress.AddrType.Any)
            {
                // send packet to node proxy
                Log.Trace("Client", "Sending service request from any to proxy node");

                pyPacket.dest.type = PyAddress.AddrType.Node;
                pyPacket.dest.typeID = Network.PROXY_NODE_ID;
                
                this.ConnectionManager.NotifyNode((int) pyPacket.dest.typeID, pyPacket);
            }
        }

        protected void ExceptionHandler(Exception exception)
        {
            Log.Error("Client", exception.Message);
            Log.Trace("Client", exception.StackTrace);
        }
        
        private void SendLowLevelVersionExchange()
        {
            Log.Debug("Client", "Sending LowLevelVersionExchange...");

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
                    this.Session.SetInt("userType", Common.Constants.AccountType.User);
                    this.Session.SetLong("userid", accountID);
                    this.Session.SetLong("role", role);
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

                    Log.Trace("Client", "Rejected by server; cluster is starting");

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
            if(this.Session.KeyExists("userid") == false)
                this.ConnectionManager.RemoveUnauthenticatedClientConnection(this);
            else
                this.ConnectionManager.RemoveAuthenticatedClientConnection(this);
        }
        
        // TODO: MOVE THIS CODE TO THE SESSION HANDLER INSTEAD
        
        public void SendSessionChange()
        {
            PyPacket sc = CreateEmptySessionChange();

            PyObject client = SetSessionChangeDestination(sc);
            PyObject node = SetSessionChangeDestination(sc, NodeID);

            if (sc != null)
            {
                this.Socket.Send(client);
                this.ConnectionManager.NotifyNode(NodeID, node);
            }
        }

        public PyPacket CreateEmptySessionChange()
        {
            // Fill all the packet data, except the dest/source
            SessionChangeNotification scn = new SessionChangeNotification();
            scn.changes = Session.EncodeChanges();

            if (scn.changes.Dictionary.Count == 0)
            {
                // Nothing to do
                return null;
            }

            Dictionary<long, NodeConnection> nodes = this.ConnectionManager.Nodes;

            // Add all the nodeIDs
            foreach (KeyValuePair<long, NodeConnection> node in nodes)
            {
                scn.nodesOfInterest.Items.Add(new PyIntegerVar(node.Key));
            }

            PyPacket p = new PyPacket();

            p.type_string = "macho.SessionChangeNotification";
            p.type = Macho.MachoNetMsg_Type.SESSIONCHANGENOTIFICATION;

            p.userID = (uint) this.Session.GetCurrentLong("userid");

            p.payload = scn.Encode().As<PyTuple>();

            p.named_payload = new PyDict();
            p.named_payload.Set("channel", new PyString("sessionchange"));

            return p;
        }

        public PyObject SetSessionChangeDestination(PyPacket p)
        {
            p.source.type = PyAddress.AddrType.Node;
            p.source.typeID = NodeID;
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Client;
            p.dest.typeID = this.Session.GetCurrentLong("userid");
            p.dest.callID = 0;

            return p.Encode();
        }

        public PyObject SetSessionChangeDestination(PyPacket p, int node)
        {
            // The session change info should never come from the client
            p.source.type = PyAddress.AddrType.Node;
            p.source.typeID = 1;
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Node;
            p.dest.typeID = node;
            p.dest.callID = 0;

            return p.Encode();
        }
    }
}