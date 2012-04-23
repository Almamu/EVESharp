using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using Common.Packets;
using Common.Network;
using Marshal;
using System.Net;
using System.Net.Sockets;

namespace Proxy
{
    public class Connection
    {
        public enum LoginStatus
        {
            Waiting = 0,
            Sucess = 1,
            Failed = 2,
        };

        private TCPSocket socket = null;
        private Thread thr = null;
        private StreamPacketizer packetizer = null;
        private bool isNode = false;
        private AuthenticationReq request = null;
        public int accountid = 0;
        public int role = 0;
        public bool banned = false;
        public int nodeID = 0;
        private bool forClose = true;
        private Session session = new Session();

        public Connection(TCPSocket sock)
        {
            socket = sock;
            packetizer = new StreamPacketizer();
            thr = new Thread(Run);

            if (sock.Blocking == true)
            {
                sock.Blocking = false;
            }
        }

        public void Start()
        {
            thr.Start();
        }

        public AuthenticationReq GetAuthenticationReq()
        {
            return request;
        }

        public void Run()
        {
            try
            {
                SendLowLevelVersionExchange();

                while (true)
                {
                    Thread.Sleep(1);
                    byte[] data = null;
                    int bytes = 0;

                    try
                    {
                        data = new byte[socket.Available];
                        bytes = socket.Recv(data);
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode != 10035)
                            throw new DisconnectException();
                    }

                    if (bytes == -1)
                    {
                        // Disconnected
                        throw new DisconnectException();
                    }
                    else if (bytes > 0)
                    {
                        int p = packetizer.QueuePackets(data);
                        byte[] packet = null;

                        for (int i = 0; i < p; i++)
                        {
                            packet = packetizer.PopItem();
                            PyObject obj = Unmarshal.Process<PyObject>(packet);
                            PyObject res = Process(obj);

                            if (res != null)
                            {
                                Send(res);
                            }
                        }
                    }
                }
            }
            catch (DisconnectException)
            {
                Log.Error("Connection", "Connection closed");
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Log.Error("Connection", "Unhandled exception... " + ex.Message);
                Log.Error("ExceptionHandler", "Stack trace: " + ex.StackTrace);
            }

            Program.waiting.Remove(this);
            if(forClose) socket.Close();
        }

        public void Close()
        {
            thr.Abort();
        }

        private PyObject Process(PyObject packet)
        {
            // All these packets should be a Tuple
            if (packet.Type == PyObjectType.Tuple)
            {
                return HandleTuple(packet as PyTuple);
            }
            else if (packet.Type == PyObjectType.ObjectData)
            {
                return HandleObject(packet as PyObjectData);
            }
            else
            {
                // Close the connection, we dont like them
                GPSTransportClosed ex = new GPSTransportClosed("None");
                Send(ex.Encode());
                throw new DisconnectException();
            }
        }

        private PyObject HandleTuple(PyTuple tup)
        {
            int items = tup.Items.Count;

            if (items == 6)
            {
                // Only LowLeverVersionExchange
                if (CheckLowLevelVersionExchange(tup) == false)
                {
                    Close();
                }

                if (isNode)
                {
                    // We are a node, the next packets will be handled by the Node class
                    Node n = new Node(new StreamPacketizer(), socket);
                    NodeManager.AddNode(n);

                    forClose = false;
                    Close();
                }

                return null;
            }
            else if (items == 3)
            {
                if (tup.Items[0].Type == PyObjectType.None)
                {
                    // VipKey
                    VipKeyCommand vk = new VipKeyCommand();

                    if (vk.Decode(tup) == false)
                    {
                        Log.Error("Client", "Wrong vipKey command");
                        Close();

                        return null;
                    }

                    return null;
                }
                else
                {
                    // Handshake sent when we are mostly in
                    HandshakeAck ack = new HandshakeAck();

                    ack.live_updates = new PyList();
                    ack.jit = session.GetCurrentString("languageID");
                    ack.userid = session.GetCurrentInt("userid");
                    ack.maxSessionTime = new PyNone();
                    ack.userType = Common.Constants.AccountType.User;
                    ack.role = session.GetCurrentInt("role");
                    ack.address = session.GetCurrentString("address");
                    ack.inDetention = new PyNone();
                    ack.client_hashes = new PyList();
                    ack.user_clientid = session.GetCurrentInt("userid") ;

                    // We have to send this just before the sessionchange
                    Send(ack.Encode());

                    // Create the client instance
                    Client cli = new Client(new StreamPacketizer(), socket);

                    // Update the Client class session data
                    cli.InitialSession(session);

                    // Set the node id for the client
                    cli.SetNodeID(nodeID);

                    // Send session change
                    cli.SendSessionChange();

                    // Start the client packet reader thread
                    cli.Start();

                    // Now we are completely in, add us to the list
                    ClientManager.AddClient(cli);

                    // Delete ourselves from the list
                    forClose = false;
                    Close();
                }
            }
            else if (items == 2) // PlaceboRequest, QueueCheck and Login packet
            {
                if (tup.Items[0].Type == PyObjectType.None)
                {
                    QueueCheckCommand qc = new QueueCheckCommand();

                    if (qc.Decode(tup) == false)
                    {
                        Log.Error("Client", "Wrong QueueCheck command");
                        Close();

                        return null;
                    }

                    // Queued logins
                    Send(new PyInt(LoginQueue.queue.Count + 1));
                    SendLowLevelVersionExchange();

                    return null;
                }
                else if (tup.Items[0].Type == PyObjectType.String)
                {
                    if (tup.Items[0].As<PyString>().Value == "placebo")
                    {
                        // We assume it is a placebo request
                        PlaceboRequest req = new PlaceboRequest();

                        if (req.Decode(tup) == false)
                        {
                            Log.Error("Client", "Wrong placebo request");
                            Close();

                            return null;
                        }

                        return new PyString("OK CC");
                    }
                    else
                    {
                        // Check if the password is hashed or not and ask for plain password
                        AuthenticationReq req = new AuthenticationReq();

                        if (req.Decode(tup) == false)
                        {
                            Log.Error("Client", "Wrong login packet");
                            GPSTransportClosed ex = new GPSTransportClosed("LoginAuthFailed");
                            Send(ex.Encode());
                            Close();
                            return null;
                        }

                        // The hash is in sha1, we should handle it later
                        if (req.user_password == null)
                        {
                            Log.Trace("Client", "Rejected by server; requesting plain password");
                            return new PyInt(1); // Ask for unhashed password( 1 -> Plain, 2 -> Hashed )
                        }

                        request = req;

                        // Login request, add it to the queue and wait until we are accepted or rejected
                        LoginQueue.Enqueue(this);

                        // The login queue will call send the data to the client
                        return null;
                    }
                }
            }
            else
            {
                Log.Error("Connection", "Unhandled Tuple packet with " + items + " items");
                thr.Abort();

                return null;
            }

            return null;
        }

        private PyObject HandleObject(PyObjectData dat)
        {
            // Only exceptions should be this type
            PyException ex = new PyException();

            if (ex.Decode(dat) == false)
            {
                Log.Error("Connection", "Unhandled PyObjectData packet");
                return null;
            }

            Log.Error("Connection", "Got an exception packet of type: " + ex.exception_type + ". " + ex.message);
            throw new DisconnectException();
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
            data.usercount = Program.clients.Count;
            data.region = Common.Constants.Game.region;

            Send(data.Encode(false));
        }

        private bool CheckLowLevelVersionExchange(PyTuple packet)
        {
            LowLevelVersionExchange data = new LowLevelVersionExchange();

            if (data.Decode(packet) == false)
            {
                Log.Error("Client", "Wrong LowLevelVersionExchange packet");
                return false;
            }

            if (data.birthday != Common.Constants.Game.birthday)
            {
                Log.Error("Client", "Wrong birthday in LowLevelVersionExchange");
                return false;
            }

            if (data.build != Common.Constants.Game.build)
            {
                Log.Error("Client", "Wrong build in LowLevelVersionExchange");
                return false;
            }

            if (data.codename != Common.Constants.Game.codename + "@" + Common.Constants.Game.region)
            {
                Log.Error("Client", "Wrong codename in LowLevelVersionExchange");
                return false;
            }

            if (data.machoVersion != Common.Constants.Game.machoVersion)
            {
                Log.Error("Client", "Wrong machoVersion in LowLevelVersionExchange");
                return false;
            }

            if (data.version != Common.Constants.Game.version)
            {
                Log.Error("Client", "Wrong version in LowLevelVersionExchange");
                return false;
            }

            if (data.isNode == true)
            {
                if (data.nodeIdentifier != "Node")
                {
                    Log.Error("Client", "Wrong node string in LowLevelVersionExchange");
                    return false;
                }

                isNode = data.isNode;
            }

            return true;
        }

        public void Send(PyObject data)
        {
            Send(Marshal.Marshal.Process(data));
        }

        private void Send(byte[] data)
        {
            // The biggest stupid error I've ever seen, I havent added the size indicator to the packet data
            // This was causing the client to not answer...
            // Fixed now
            byte[] packet = new byte[data.Length + 4];

            Array.Copy(data, 0, packet, 4, data.Length);
            Array.Copy(BitConverter.GetBytes(data.Length), packet, 4);

            socket.Send(packet);
        }

        public void SendLoginNotification(LoginStatus loginStatus)
        {
            if (loginStatus == LoginStatus.Sucess)
            {
                AuthenticationRsp rsp = new AuthenticationRsp();

                // String "None" marshaled
                byte[] func_marshaled_code = new byte[] { 0x74, 0x04, 0x00, 0x00, 0x00, 0x4E, 0x6F, 0x6E, 0x65 };

                rsp.serverChallenge = "";
                rsp.func_marshaled_code = func_marshaled_code;
                rsp.verification = false;
                rsp.cluster_usercount = Program.clients.Count + 1; // We're not in the list yet
                rsp.proxy_nodeid = 1;
                rsp.user_logonqueueposition = 1;
                rsp.challenge_responsehash = "55087";

                rsp.macho_version = Common.Constants.Game.machoVersion;
                rsp.boot_version = Common.Constants.Game.version;
                rsp.boot_build = Common.Constants.Game.build;
                rsp.boot_codename = Common.Constants.Game.codename;
                rsp.boot_region = Common.Constants.Game.region;

                // Setup session
                session.SetString("address", socket.GetAddress());
                session.SetString("languageID", request.user_languageid);
                session.SetInt("userType", Common.Constants.AccountType.User);
                session.SetInt("userid", accountid);
                session.SetInt("role", role);

                // We should check for a exact number of nodes here when we have the needed infraestructure
                if (NodeManager.nodes.Count > 0)
                {
                    nodeID = NodeManager.GetRandomNode();
                    Send(rsp.Encode());
                }
                else
                {
                    GPSTransportClosed ex = new GPSTransportClosed("AutClusterStarting");
                    Send(ex.Encode());

                    Close();
                }
            }
            else if (loginStatus == LoginStatus.Failed)
            {
                GPSTransportClosed ex = new GPSTransportClosed("LoginAuthFailed");
                Send(ex.Encode());

                Close();
            }
        }
    }
}
