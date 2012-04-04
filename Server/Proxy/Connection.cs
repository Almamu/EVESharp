using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;
using Common.Packets;
using Common.Network;
using Marshal;

namespace Proxy
{
    public class Connection
    {
        public enum LoginStatus
        {
            NotStarted = 0,
            Waiting = 1,
            Sucess = 2,
            Failed = 3,
        };

        private TCPSocket socket = null;
        private Thread thr = null;
        private StreamPacketizer packetizer = null;
        private bool isNode = false;
        private LoginStatus loginStatus = LoginStatus.NotStarted;
        private AuthenticationReq request = null;
        public int accountid = 0;
        public int role = 0;
        public bool banned = false;
        private Client cli = null;
        private bool forClose = true;

        public Connection(TCPSocket sock)
        {
            socket = sock;
            packetizer = new StreamPacketizer();
            thr = new Thread(Run);
            thr.Start();
        }

        public AuthenticationReq GetAuthenticationReq()
        {
            return request;
        }

        public void SetLoginStatus(LoginStatus status)
        {
            loginStatus = status;
        }

        private void Run()
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
                    catch (Exception)
                    {
                        throw new DisconnectException();
                    }

                    if (bytes == -1)
                    {
                        // Disconnected
                        throw new DisconnectException();
                    }

                    if (bytes > 0)
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
                Log.Error("Connection", "Someone has disconnected");
            }
            catch (ThreadAbortException)
            {
                Program.waiting.Remove(this);
            }
            catch (Exception ex)
            {
                Log.Error("Client", "Unhandled exception... " + ex.Message);
                Log.Error("ExceptionHandler", "Stack trace: " + ex.StackTrace);
            }

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
                    thr.Abort();
                }

                if (isNode)
                {
                    // We are a node, the next packets will be handled by the Node class
                    Node n = new Node(new StreamPacketizer(), socket);
                    NodeManager.AddNode(n);
                    thr.Abort();
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
                        thr.Abort();

                        return null;
                    }

                    return null;
                }
                else
                {
                    // Handshake sent when we are mostly in
                    HandshakeAck ack = new HandshakeAck();

                    ack.live_updates = new PyList();
                    ack.jit = cli.GetLanguageID();
                    ack.userid = cli.GetAccountID();
                    ack.maxSessionTime = new PyNone();
                    ack.userType = Common.Constants.AccountType.User;
                    ack.role = cli.GetAccountRole();
                    ack.address = cli.GetAddress();
                    ack.inDetention = new PyNone();
                    ack.client_hashes = new PyList();
                    ack.user_clientid = cli.GetAccountID();

                    // We have to send this just before the sessionchange
                    Send(ack.Encode());

                    cli.SendSessionChange();

                    // Now we are completely in, add us to the list
                    Program.clients.Add(cli);

                    // Delete ourselves from the list
                    forClose = false;
                    thr.Abort();
                    return null;
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
                        thr.Abort();

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
                            thr.Abort();

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
                            thr.Abort();
                            return null;
                        }

                        if (req.user_password == null)
                        {
                            Log.Trace("Client", "Rejected by server; requesting plain password");
                            return new PyInt(1); // Ask for unhashed password( 1 -> Plain, 2 -> Hashed )
                        }

                        request = req;

                        // Login request, add it to the queue and wait until we are accepted or rejected
                        LoginQueue.Enqueue(this);
                        SetLoginStatus(LoginStatus.Waiting);

                        // Wait until LoginQueue changes our state
                        while (loginStatus == LoginStatus.Waiting)
                        {
                            Thread.Sleep(1);
                        }

                        if (loginStatus == LoginStatus.Sucess)
                        {
                            // This should do the work left
                            cli = new Client(new StreamPacketizer(), socket);
                            cli.AuthCorrectInfo(accountid, role, request.user_languageid);
                        }
                        else
                        {
                            Log.Error("Connection", "Wrong login status");
                            thr.Abort();
                        }
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
            socket.Send(data);
        }
    }
}
