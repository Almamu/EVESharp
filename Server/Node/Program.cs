using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Security.Cryptography;

using MySql.Data.MySqlClient;

using Common;
using Common.Network;
using Common.Services;
using Common.Packets;

using EVESharp.Database;
using EVESharp.Inventory;

using Marshal;
using Marshal.Database;

namespace EVESharp
{
    class Program
    {
        static public Dictionary<uint, Client> clients = new Dictionary<uint, Client>();
        static private int nodeID = 0xFFFF;
        static private TCPSocket proxyConnection = null;
        static private StreamPacketizer packetizer = new StreamPacketizer();
        static public ServiceManager SvcMgr = new ServiceManager();

        // Proxy info
        static string[,] proxy = new string[,]
        {
            // IP         Port
            {"loopback", "26000"}
        };

        static public int NodeID
        {
            get
            {
                return nodeID;
            }

            set
            {
                nodeID = value;
            }
        }

        static public void Send(byte[] data)
        {
            byte[] packet = new byte[data.Length + 4];

            Array.Copy(data, 0, packet, 4, data.Length);
            Array.Copy(BitConverter.GetBytes(data.Length), packet, 4);

            proxyConnection.Send(packet);
        }

        static public void Send(PyObject data)
        {
            Send(Marshal.Marshal.Process(data));
        }

        static public void HandlePacket(object input)
        {
            PyPacket packet = (PyPacket)input;
            PyPacket res = new PyPacket();

            if (packet.type == Macho.MachoNetMsg_Type.CALL_REQ)
            {
                PyTuple callInfo = packet.payload.As<PyTuple>().Items[0].As<PyTuple>()[1].As<PySubStream>().Data.As<PyTuple>();

                string call = callInfo.Items[1].As<PyString>().Value;
                PyTuple args = callInfo.Items[2].As<PyTuple>();
                PyDict sub = callInfo.Items[3].As<PyDict>();

                PyObject callRes = null;

                try
                {
                    callRes = Program.SvcMgr.ServiceCall(packet.dest.service, call, args, null);
                }
                catch (ServiceDoesNotContainCallException)
                {
                    Log.Error("HandlePacket", "The service does not contain a definition for " + call);
                }
                catch (ServiceDoesNotExistsException)
                {
                    Log.Error("HandlePacket", "The requested service(" + packet.dest.service + ")does not exists");
                }
                catch (Exception ex)
                {
                    Log.Error("HandlePacket", "Unhadled exception: " + ex.ToString());
                }


                if (callRes == null)
                {
                    return;
                }

                if (callRes.Type == PyObjectType.Tuple)
                {
                    res.type_string = "macho.CallRsp";
                    res.type = Macho.MachoNetMsg_Type.CALL_RSP;

                    res.source = packet.dest;

                    res.dest = packet.source;
                    /*
                    res.dest.type = PyAddress.AddrType.Client;
                    res.dest.typeID = (ulong)clients[packet.userID].AccountID;
                    res.dest.callID = packet.source.callID;
                    */

                    res.userID = packet.userID;

                    res.payload = new PyTuple();
                    res.payload.Items.Add(new PySubStream(callRes));

                    Send(res.Encode());
                }
            }
            else if (packet.type == Macho.MachoNetMsg_Type.SESSIONCHANGENOTIFICATION)
            {
                Log.Debug("Main", "Updating session for client " + packet.userID);

                // Check if the client isnt assigned to this node yet
                if(clients.ContainsKey(packet.userID) == false)
                {
                    Client cli = new Client();
                    clients.Add(packet.userID, cli);
                }

                // Update client session
                clients[packet.userID].UpdateSession(packet);
            }
        }

        static void Main(string[] args)
        {
            Log.Init("evesharp");
            Log.Info("Main", "Starting node...");
            Log.Trace("Database", "Connecting to database...");

            if (Database.Database.Init() == false)
            {
                Log.Error("Main", "Cannot connect to database");
                while (true) ;
            }

            
            /*DBRowDescriptor descriptor = new DBRowDescriptor();

            descriptor.AddColumn("itemID", FieldType.I4);
            descriptor.AddColumn("custominfo", FieldType.Str);
            
            PyPackedRow packed = new PyPackedRow(descriptor);

            packed.SetValue("itemID", new PyInt(500));
            packed.SetValue("custominfo", new PyString("hello world"));
            
            byte[] marshaled = Marshal.Marshal.Process(packed);

            PyPackedRow unmarshaled = Unmarshal.Process<PyPackedRow>(marshaled);

            Console.WriteLine(PrettyPrinter.Print(unmarshaled));*/
            

           /*byte[] raw = new byte[] { 1, 0, 55, 1, 22, 33, 0, 33, 25, 33, 14, 0, 0, 25, 45 };

            MemoryStream output = new MemoryStream(raw);
            BinaryReader reader = new BinaryReader(output);

            MemoryStream stream = new MemoryStream();
            BinaryWriter streamWriter = new BinaryWriter(stream);
            BinaryReader streamReader = new BinaryReader(stream);

            PyPackedRow.SaveZeroCompressed(reader, streamWriter);

            byte[] compressed = stream.ToArray();
            stream.Seek(0, SeekOrigin.Begin);

            byte[] uncompress = PyPackedRow.LoadZeroCompressed(streamReader);


            for (int i = 0; i < uncompress.Length; i++)
            {
                Console.Write(uncompress[i] + " ");
            }

            Console.WriteLine();
            

            while (true) Thread.Sleep(1);*/
            /*
            SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.ASCII.GetBytes("password"));
            char[] strHash = new char[20];

            for (int i = 0; i < 20; i++)
            {
                strHash[i] = (char)hash[i];
            }

            string str = new string(strHash);

            Database.Database.Query("INSERT INTO account(accountID, accountName, password, role, online, banned)VALUES(NULL, 'Username', '" + str + "', 2, 0, 0);");
            */

            Log.Info("Main", "Connection to the DB sucessfull");

            Log.Info("Main", "Done");
            Log.Info("Main", "Connecting to proxy...");

            proxyConnection = new TCPSocket(ushort.Parse(proxy[0, 1]), false);
            if (proxyConnection.Connect(proxy[0, 0]) == false)
            {
                Log.Error("Main", "Cannot connect to proxy. Halting");
                Database.Database.Stop();
                while (true) ;
            }

            Log.Trace("Main", "Server started");

            while (true)
            {
                Thread.Sleep(1);
                try
                {
                    byte[] data = new byte[proxyConnection.Available];
                    int bytes = proxyConnection.Recv(data);

                    if (bytes == -1)
                    {
                        // Proxy is closing, shutdown the node
                        break;
                    }
                    else if (bytes > 0)
                    {
                        packetizer.QueuePackets(data, bytes);
                        int p = packetizer.ProcessPackets();

                        for (int i = 0; i < p; i++)
                        {
                            byte[] packet = packetizer.PopItem();
                            PyObject obj = Unmarshal.Process<PyObject>(packet);
                            
                            if (obj is PyObjectData)
                            {
                                PyObjectData info = obj as PyObjectData;

                                if (info.Name == "machoNet.nodeInfo")
                                {
                                    // Update our local info
                                    NodeInfo nodeinfo = new NodeInfo();

                                    if (nodeinfo.Decode(info) == true)
                                    {
                                        nodeID = nodeinfo.nodeID;

                                        SystemManager.LoadSolarSystems(nodeinfo.solarSystems);
                                    }
                                }
                                else
                                {
                                    // Client packet
                                    PyPacket clientpacket = new PyPacket();
                                    
                                    if (clientpacket.Decode(info) == false)
                                    {
                                        Log.Error("Main", "Unknown packet");
                                    }
                                    else
                                    {
                                        // Something similar to Async calls
                                        new Thread(new ParameterizedThreadStart(HandlePacket)).Start(clientpacket);
                                    }
                                }
                            }
                            else if (obj is PyChecksumedStream) // Checksumed packets
                            {
                                PyPacket clientpacket = new PyPacket();

                                if (clientpacket.Decode(obj) == false)
                                {
                                    Log.Error("Main", "Cannot decode packet");
                                }
                                else
                                {
                                    new Thread(new ParameterizedThreadStart(HandlePacket)).Start(clientpacket);
                                }
                            }
                            else if (obj is PyTuple)
                            {
                                // The only tuple packet is the LowLevelVersionExchange
                                LowLevelVersionExchange ex = new LowLevelVersionExchange();

                                if (ex.Decode(obj) == false)
                                {
                                    Log.Error("Main", "LowLevelVersionExchange error");
                                }

                                // Reply with the node LowLevelVersionExchange
                                LowLevelVersionExchange reply = new LowLevelVersionExchange();

                                reply.codename = Common.Constants.Game.codename;
                                reply.birthday = Common.Constants.Game.birthday;
                                reply.build = Common.Constants.Game.build;
                                reply.machoVersion = Common.Constants.Game.machoVersion;
                                reply.version = Common.Constants.Game.version;
                                reply.region = Common.Constants.Game.region;

                                Send(reply.Encode(true));
                            }
                            else if (obj is PyObjectEx)
                            {
                                Log.Error("PyObjectEx", PrettyPrinter.Print(obj));
                            }
                            else
                            {
                                Log.Error("Main", PrettyPrinter.Print(obj));
                                Log.Error("Main", "Unhandled packet type");
                            }
                        }
                    }

                }
                catch (Exception)
                {

                }
            }

            /* Code to ADD an account:
            SHA1 sha1 = SHA1.Create();
            byte[] hash = sha1.ComputeHash(Encoding.ASCII.GetBytes("password"));
            char[] strHash = new char[20];

            for (int i = 0; i < 20; i++)
            {
                strHash[i] = (char)hash[i];
            }

            string str = new string(strHash);

            Database.Database.Query("INSERT INTO account(accountID, accountName, password, role, online, banned)VALUES(NULL, 'Username', '" + str + "', 2, 0, 0);");
            */
        }
    }
}
