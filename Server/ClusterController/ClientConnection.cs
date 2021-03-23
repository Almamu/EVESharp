using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClusterController.Database;
using Common.Constants;
using Common.Database;
using Common.Game;
using Common.Logging;
using Common.Network;
using Common.Packets;
using MySql.Data.MySqlClient.Authentication;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;

namespace ClusterController
{
    public class ClientConnection : Connection
    {
        private Channel Log { get; }
        public Session Session { get; }
        private GeneralDB GeneralDB { get; }
        public long NodeID { get; set; }

        public long AccountID
        {
            get
            {
                PyDataType userid = this.Session["userid"];

                if (userid is PyInteger useridInt)
                    return useridInt;

                return 0;
            }
        }

        public int SolarSystemID2
        {
            get
            {
                PyDataType solarsystemid2 = this.Session["solarsystemid2"];

                if (solarsystemid2 is PyInteger solarsystemid2Int)
                    return solarsystemid2Int;

                return 0;
            }
        }

        public int ConstellationID
        {
            get
            {
                PyDataType constellationid = this.Session["constellationid"];

                if (constellationid is PyInteger constellationidInt)
                    return constellationidInt;

                return 0;
            }
        }

        public int CorporationID
        {
            get
            {
                PyDataType corpid = this.Session["corpid"];

                if (corpid is PyInteger corpidInt)
                    return corpidInt;

                return 0;
            }
        }

        public int RegionID
        {
            get
            {
                PyDataType regionid = this.Session["regionid"];

                if (regionid is PyInteger regionidInt)
                    return regionidInt;

                return 0;
            }
        }

        public int CharacterID
        {
            get
            {
                PyDataType charid = this.Session["charid"];

                if (charid is PyInteger charidInt)
                    return charidInt;

                return 0;
            }
        }

        public int StationID
        {
            get
            {
                PyDataType stationid = this.Session["stationid"];

                if (stationid is PyInteger stationidInt)
                    return stationidInt;

                return 0;
            }
        }

        public int AllianceID
        {
            get
            {
                PyDataType allianceid = this.Session["allianceid"];

                if (allianceid is PyInteger allianceidInt)
                    return allianceidInt;

                return 0;
            }
        }
        
        public ClientConnection(EVEClientSocket socket, ConnectionManager connectionManager, GeneralDB generalDB, Logger logger)
            : base(socket, connectionManager)
        {
            // access the GeneralDB
            this.GeneralDB = generalDB;
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
            // store address in session
            this.Session["address"] = this.Socket.GetRemoteAddress();
        }

        protected override void OnConnectionLost()
        {
            Log.Warning("Client closed the connection");
            
            // remove the user from the correct lists
            if (this.AccountID == 0)
            {
                this.ConnectionManager.RemoveUnauthenticatedClientConnection(this);                
            }
            else
            {
                // authenticated users are somewhat special as they also have to be free'd from the nodes
                this.ConnectionManager.RemoveAuthenticatedClientConnection(this);

                // notify the node of the disconnection of the user
                this.ConnectionManager.NotifyAllNodes("OnClientDisconnected", new PyTuple(1) { [0] = this.AccountID });
            }
            
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

            if (command.Command == "QC")
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
            HandshakeAck ack = new HandshakeAck
            {
                live_updates = this.GeneralDB.FetchLiveUpdates(),
                jit = this.Session["languageID"] as PyString,
                userid = this.Session["userid"] as PyInteger,
                maxSessionTime = null,
                userType = Common.Constants.AccountType.User,
                role = this.Session["role"] as PyInteger,
                address = this.Session["address"] as PyString,
                inDetention = null,
                client_hashes = new PyList(),
                user_clientid = this.Session["userid"] as PyInteger
            };
            
            // send the response first
            this.Socket.Send(ack);
            // send the session change
            this.SendSessionChange();
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
                pyPacket.Source = new PyAddressClient(this.AccountID);
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
                if (destNodeID == 0)
                {
                    pyPacket.Destination = new PyAddressNode(this.NodeID, dest.CallID, dest.Service);
                    destNodeID = this.NodeID;
                }

                // send the packet to the correct node
                this.ConnectionManager.NotifyNode(destNodeID, pyPacket);
            }
            else if (pyPacket.Destination is PyAddressAny)
            {
                // send packet to node proxy
                Log.Trace("Sending service request to any node");

                // request to any should be routed to whatever node we want
                // there might be a good algorithm for determining this
                // but for now use the node this player belongs to and call it a day
                this.ConnectionManager.NotifyNode(this.NodeID, pyPacket);
            }
            else
            {
                throw new Exception($"Unexpected destination type {pyPacket.Destination.GetType().Name} for packet");
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
            if (loginStatus == LoginStatus.Success)
            {
                // We should check for a exact number of nodes here when we have the needed infraestructure
                if (this.ConnectionManager.NodesCount > 0)
                {
                    AuthenticationRsp rsp = new AuthenticationRsp();

                    // String "None" marshaled
                    byte[] func_marshaled_code = new byte[] {0x74, 0x04, 0x00, 0x00, 0x00, 0x4E, 0x6F, 0x6E, 0x65};

                    rsp.serverChallenge = "";
                    rsp.func_marshaled_code = func_marshaled_code;
                    rsp.verification = false;
                    rsp.cluster_usercount = this.ConnectionManager.ClientsCount;
                    rsp.proxy_nodeid = 0; // ProxyNodeID is 0
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
                    // set our NodeID to something sensible
                    this.NodeID = this.ConnectionManager.Nodes.Keys.First();
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
            if (this.Session.ContainsKey("userid") == false)
                this.ConnectionManager.RemoveUnauthenticatedClientConnection(this);
            else
                this.ConnectionManager.RemoveAuthenticatedClientConnection(this);
        }

        // TODO: MOVE THIS CODE TO THE SESSION HANDLER INSTEAD

        public void SendSessionChange()
        {
            PyPacket packet = CreateEmptySessionChange();

            if (packet is null)
                return;

            PyDataType client = SetSessionChangeDestination(packet);

            this.Socket.Send(client);
            this.ConnectionManager.NotifyAllNodes(packet);
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
            packet.Source = new PyAddressNode(Network.PROXY_NODE_ID, 0);
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