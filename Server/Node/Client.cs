using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Network;
using System.Threading;
using Marshal;
using Common;
using Common.Packets;
using Common.Constants;
using System.IO;

namespace EVESharp
{
    class Client
    {
        public Client(TCPSocket sock)
        {
            connection = sock;
            thread = new Thread(Run);
            packetizer = new StreamPacketizer();
            session = new Session();
        }

        public void Start()
        {
            thread.Start();
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
                        data = new byte[connection.Available];
                        bytes = connection.Recv(data);
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
                // Just ignore it as the message is already printed
                Log.Error("Client", "Client closed the connection");
            }
            catch (Exception ex)
            {
                Log.Error("Client", "Unhandled exception... " + ex.Message);
                Log.Error("ExceptionHandler", "Stack trace: " + ex.StackTrace);
            }

            Program.clients.Remove(this);
            connection.Close();
        }

        private PyObject Process(PyObject packet)
        {
            if (packet.Type == PyObjectType.Tuple)
            {
                PyTuple tmp = packet.As<PyTuple>();

                if (tmp.Items.Count == 6)
                {
                    if (CheckLowLevelVersionExchange(tmp) == false)
                    {
                        thread.Abort();
                    }

                    return null;
                }

                if (tmp.Items.Count == 2)
                {
                    // Placebo request, login packet or QueueCheck, deal with them
                    if (tmp.Items[0].Type == PyObjectType.None)
                    {
                        QueueCheckCommand qc = new QueueCheckCommand();

                        if (qc.Decode(tmp) == false)
                        {
                            Log.Error("Client", "Wrong QueueCheck command");
                            thread.Abort();

                            return null;
                        }

                        // Hack it by now
                        // TODO: Add something to keep this number updated
                        Send(new PyInt(1));
                        SendLowLevelVersionExchange();

                        return null;
                    }
                    else if (tmp.Items[0].Type == PyObjectType.String)
                    {
                        if (tmp.Items[0].As<PyString>().Value == "placebo")
                        {
                            // We assume it is a placebo request
                            PlaceboRequest req = new PlaceboRequest();

                            if (req.Decode(tmp) == false)
                            {
                                Log.Error("Client", "Wrong placebo request");
                                thread.Abort();

                                return null;
                            }

                            return new PyString("OK CC") ;
                        }
                        else
                        {
                            // Login Auth packet
                            AuthenticationReq req = new AuthenticationReq();

                            if (req.Decode(tmp) == false)
                            {
                                Log.Error("Client", "Wrong login packet");
                                thread.Abort();

                                return null;
                            }

                            if (req.user_password == null)
                            {
                                Log.Trace("Client", "Rejected by server; requesting plain password");
                                return new PyInt(1); // Ask for unhashed password( 1 -> Plain, 2 -> Hashed )
                            }

                            Log.Debug("Client", "Login try " + req.user_name);

                            int accountid = 0;
                            bool banned = false;
                            int role = 0;

                            if ((Database.AccountDB.LoginPlayer(req.user_name, req.user_password, ref accountid, ref banned, ref role) == false) || (banned == true))
                            {
                                Log.Trace("Client", " Rejected by database");
                                
                                GPSTransportClosed ex = new GPSTransportClosed("LoginAuthFailed");
                                Send(ex.Encode());
                                
                                return null;
                            }

                            Log.Trace("Client", " Sucesful");

                            AuthenticationRsp rsp = new AuthenticationRsp();

                            // String "None" marshaled
                            byte[] func_marshaled_code = new byte[] { 0x74, 0x04, 0x00, 0x00, 0x00, 0x4E, 0x6F, 0x6E, 0x65 };
                            
                            rsp.serverChallenge = "";
                            rsp.func_marshaled_code = func_marshaled_code;
                            rsp.verification = false;
                            rsp.cluster_usercount = Program.clients.Count;
                            rsp.proxy_nodeid = Program.GetNodeID(); // Hardcoded until nodes are working
                            rsp.user_logonqueueposition = 1;
                            rsp.challenge_responsehash = "55087";

                            rsp.macho_version = Common.Constants.Game.machoVersion;
                            rsp.boot_version = Common.Constants.Game.version;
                            rsp.boot_build = Common.Constants.Game.build;
                            rsp.boot_codename = Common.Constants.Game.codename;
                            rsp.boot_region = Common.Constants.Game.region;

                            // Setup session
                            session.SetString("address", connection.GetAddress());
                            session.SetString("languageID", req.user_languageid);
                            session.SetInt("userType", Common.Constants.AccountType.User);
                            session.SetInt("userid", accountid); // Harcoded, this should be retrieved when login happens
                            session.SetInt("role", role);

                            return rsp.Encode();
                        }
                    }
                }

                if (tmp.Items.Count == 3)
                {
                    if (tmp.Items[0].Type == PyObjectType.None)
                    {
                        // VipKey
                        VipKeyCommand vk = new VipKeyCommand();

                        if (vk.Decode(tmp) == false)
                        {
                            Log.Error("Client", "Wrong vipKey command");
                            thread.Abort();

                            return null;
                        }

                        return null;
                    }
                    else
                    {
                        // Handshake sent when we are mostly in
                        HandshakeAck ack = new HandshakeAck();

                        ack.live_updates = new PyList();
                        ack.jit = GetLanguageID();
                        ack.userid = GetAccountID();
                        ack.maxSessionTime = new PyNone();
                        ack.userType = Common.Constants.AccountType.User;
                        ack.role = GetAccountRole();
                        ack.address = GetAddress();
                        ack.inDetention = new PyNone();
                        ack.client_hashes = new PyList();
                        ack.user_clientid = GetAccountID();

                        // We have to send this just before the sessionchange
                        Send(ack.Encode());

                        SendSessionChange();
                    }
                }
            }
            else if (packet.Type == PyObjectType.ObjectEx)
            {
                return HandleException(packet);
            }
            else if (packet.Type == PyObjectType.ChecksumedStream)
            {
                return HandlePacket(packet);
            }
            else
            {
                Log.Error("Client", "Unhandled packet");
                try
                {
                    if (File.Exists("logs/unhandledpackets.txt") == false)
                    {
                        File.Create("logs/unhandledpackets.txt");
                    }

                    File.AppendAllText("logs/unhandledpackets.txt", PrettyPrinter.Print(packet));
                }
                catch (Exception)
                {

                }

                return null;
            }

            return null;
        }

        private void Send(PyObject packet)
        {
            if (packet == null)
                return;

            byte[] data = Marshal.Marshal.Process(packet);
            Send(data);
        }

        private void Send(byte[] data)
        {
            byte[] packet = new byte[data.Length + 4];
            Array.Copy(data, 0, packet, 4, data.Length);
            Array.Copy(BitConverter.GetBytes(data.Length), packet, 4);
            connection.Send(packet);
        }

        private void Send(PyPacket data)
        {
            PyObject packet = data.Encode();
            Send(packet);
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
            Send(data.Encode());
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

            return true;
        }

        public PyObject HandlePacket(PyObject data)
        {
            PyPacket packet = new PyPacket();

            if (packet.Decode(data) == false )
            {
                Log.Error("Client", "Wrong packet data. Ignoring...");
                return null;
            }

            // We should see the difference between macho.CallReq and another packets
            if (packet.type_string == "macho.CallReq")
            {
                return HandleCall(packet);
            }

            Log.Error("Client", "Unknown packet type " + packet.type_string);
            return null;
        }

        public PyObject HandleCall(PyPacket packet)
        {
            PyPacket res = new PyPacket();

            if (packet.dest.service == "")
            {
                Log.Error("Client", "Called a bound object, which is not supported yet");
                return null;
            }
            else
            {
                PyTuple callInfo = packet.payload.As<PyTuple>().Items[0].As<PyTuple>();
                callInfo = callInfo.Items[1].As<PyTuple>();

                string call = callInfo.Items[1].As<PyString>().Value;
                PyTuple args = callInfo.Items[2].As<PyTuple>();
                PyDict sub = callInfo.Items[3].As<PyDict>();

                if (Program.SvcMgr.FindService(packet.dest.service) == false)
                {
                    Log.Error("Client", "Cannot find service " + packet.dest.service + " to call " + call);
                    return null;
                }

                if (Program.SvcMgr.GetService(packet.dest.service).FindServiceCall(call) == false)
                {
                    Log.Error("Client", "Service " + packet.dest.service + " doesnt contains a call to " + call);
                    return null;
                }

                PyObject callRes = Program.SvcMgr.Call(packet.dest.service, call, args, this);

                if (callRes == null)
                {
                    return null;
                }

                if (callRes.Type == PyObjectType.Tuple)
                {
                    PyTuple payload = callRes.As<PyTuple>();
                    packet.payload = payload;

                    res.type_string = "macho.CallRsp";
                    res.type = Macho.MachoNetMsg_Type.CALL_RSP;

                    res.source = packet.dest;

                    res.dest.type = PyAddress.AddrType.Client;
                    res.dest.typeID = (ulong)GetAccountID();
                    res.dest.callID = packet.source.callID;

                    res.userID = (uint)GetAccountID();

                    res.payload = new PyTuple();
                    res.payload.Items.Add(new PySubStream(payload));
                }
                else
                {
                    // We should not return anything
                    return null;
                }
            }

            return res.Encode();
        }

        public PyObject HandleException(PyObject packet)
        {
            PyException ex = new PyException();

            if (ex.Decode(packet) == false)
            {
                Log.Error("Client", "Unhandled exception");
                return null;
            }

            Log.Warning("Client", "Got client exception " + ex.exception_type + ": " + ex.message);
            return null;
        }

        public void SendSessionChange()
        {
            SessionChangeNotification scn = new SessionChangeNotification();
            scn.changes = session.EncodeChanges();

            if (scn.changes.Dictionary.Count == 0)
            {
                // Nothing to do
                return;
            }

            scn.nodesOfInterest.Items.Add(new PyInt(Program.GetNodeID()));

            PyPacket p = new PyPacket();

            p.type_string = "macho.SessionChangeNotification";
            p.type = Macho.MachoNetMsg_Type.SESSIONCHANGENOTIFICATION;

            p.source.type = PyAddress.AddrType.Node;
            p.source.typeID = (ulong)Program.GetNodeID();
            p.source.callID = 0;

            p.dest.type = PyAddress.AddrType.Client;
            p.dest.typeID = (ulong)GetAccountID();
            p.dest.callID = 0;

            p.userID = (uint)GetAccountID();

            p.payload = scn.Encode().As<PyTuple>();

            p.named_payload = new PyDict();
            p.named_payload.Set("channel", new PyString("sessionchange"));
        }

        public string GetLanguageID()
        {
            return session.GetCurrentString("languageID");
        }

        public int GetAccountID()
        {
            return session.GetCurrentInt("userid");
        }

        public int GetAccountRole()
        {
            return session.GetCurrentInt("role");
        }

        public string GetAddress()
        {
            return session.GetCurrentString("address");
        }

        private TCPSocket connection = null;
        private Thread thread = null;
        private StreamPacketizer packetizer = null;
        private Session session = null;
    }
}
