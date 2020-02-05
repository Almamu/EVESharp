/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Security.Cryptography;

using MySql.Data.MySqlClient;

using Common;
using Common.Database;
using Common.Network;
using Common.Services;
using Common.Packets;
using Node.Inventory;

using Marshal;
using Marshal.Database;
using Node.Configuration;

namespace Node
{
    class Program
    {
        private static CacheStorage sCacheStorage = null;
        private static ServiceManager sServiceManager = null;
        private static DatabaseConnection sDatabase = null;
        private static SystemManager sSystemManager = null;
        private static General sConfiguration = null;
        private static ItemFactory sItemFactory = null;
        static public Dictionary<uint, Client> sClients = new Dictionary<uint, Client>();
        
        static private int nodeID = 0xFFFF;
        static private TCPSocket proxyConnection = null;
        static private StreamPacketizer packetizer = new StreamPacketizer();

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

            Log.Trace("ProxyConnection", $"Sending {data.Length} bytes to the cluster controller");
            
            Array.Copy(data, 0, packet, 4, data.Length);
            Array.Copy(BitConverter.GetBytes(data.Length), packet, 4);

            int sent = 0;

            while (sent != packet.Length)
            {
                try
                {
                    sent += proxyConnection.Socket.Send(packet, sent, packet.Length - sent, SocketFlags.None);
                    // TEMPORAL SOLUTION UNTIL THE TCPSocket CLASS IS REWRITTEN
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock)
                        continue;
                    throw;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        static public void Send(PyObject data)
        {
            Send(Marshal.Marshal.Process(data));
        }

        public static void HandlePacket(object input)
        {
            PyPacket packet = (PyPacket)input;
            PyPacket res = new PyPacket();

            // Check if the client isnt assigned to this node yet
            if(sClients.ContainsKey(packet.userID) == false)
            {
                Client cli = new Client();
                sClients.Add(packet.userID, cli);
            }
            
            if (packet.type == Macho.MachoNetMsg_Type.CALL_REQ)
            {
                PyTuple callInfo = packet.payload.As<PyTuple>().Items[0].As<PyTuple>()[1].As<PySubStream>().Data.As<PyTuple>();

                string call = callInfo.Items[1].As<PyString>().Value;
                PyTuple args = callInfo.Items[2].As<PyTuple>();
                PyDict sub = callInfo.Items[3].As<PyDict>();

                PyObject callRes = null;

                try
                {
                    // TODO: SEPARATE HANDLING OF CLIENT AND NODE PACKETS TO PROPERLY DETECT USERID WITHOUT
                    // TODO: RELYING ON THE PACKET
                    Log.Trace("HandlePacket", $"Calling {packet.dest.service}::{call}");
                    callRes = sServiceManager.ServiceCall(packet.dest.service, call, args, sClients [packet.userID]);
                }
                catch (ServiceDoesNotContainCallException)
                {
                    Log.Error(
                        "HandlePacker",
                        $"The service {packet.dest.service} does not contain a definition for {call}"
                    );
                }
                catch (ServiceDoesNotExistsException)
                {
                    Log.Error(
                        "HandlePacket",
                        $"The requested service {packet.dest.service} doesn't exist"
                    );
                }
                catch (Exception ex)
                {
                    Log.Error(
                        "HandlePacket",
                        $"Unhandled exception: {ex.ToString()}"
                    );
                }
                
                if (callRes == null)
                {
                    Log.Trace("HandlePacket", "No response received from the service");
                    return;
                }
                
                Log.Trace("HandlePacket", PrettyPrinter.Print(callRes));

                //if (callRes.Type == PyObjectType.Tuple)
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
                Log.Debug("Main", $"Updating session for client {packet.userID}");

                // Update client session
                sClients[packet.userID].UpdateSession(packet);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Log.Init("evesharp");
                Log.Info("Main", "Starting node...");

                sConfiguration = General.LoadFromFile("configuration.conf");
                
                // update the loglevel with the new value
                Log.SetLogLevel(sConfiguration.Logging.LogLevel);

                Log.Trace("Database", "Connecting to database...");

                sDatabase = DatabaseConnection.FromConfiguration(sConfiguration.Database);

                Log.Info("Main", "Connection to the DB sucessfull");

                Log.Info("Main", "Priming cache...");
                sCacheStorage = new CacheStorage(sDatabase);
                sCacheStorage.Load(CacheStorage.LoginCacheTable, CacheStorage.LoginCacheQueries, CacheStorage.LoginCacheTypes);
                Log.Debug("Main", "Done");
                
                Log.Info("Main", "Initializing item factory");
                sItemFactory = new ItemFactory(sDatabase);
                Log.Debug("Main", "Done");

                Log.Info("Main", "Initializing solar system manager");
                sSystemManager = new SystemManager(sDatabase, sItemFactory);
                Log.Debug("Main", "Done");

                Log.Info("Main", "Initializing service manager");
                sServiceManager = new ServiceManager(sDatabase, sCacheStorage, sConfiguration);
                Log.Debug("Main", "Done");
                
                Log.Info("Main", "Connecting to proxy...");

                proxyConnection = new TCPSocket(sConfiguration.Proxy.Port, false);
                
                if (proxyConnection.Connect (sConfiguration.Proxy.Hostname) == false)
                {
                    throw new Exception("Cannot connect to proxy. Halting...");
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

                                        nodeinfo.Decode(info);
                                        nodeID = nodeinfo.nodeID;

                                        Log.Debug("Main", "Found machoNet.nodeInfo, our new node id is " + nodeID.ToString("X4"));
                                        sSystemManager.LoadSolarSystems(nodeinfo.solarSystems);
                                    }
                                    else
                                    {
                                        // Client packet
                                        PyPacket clientpacket = new PyPacket();

                                        clientpacket.Decode(info);
                                        // Something similar to Async calls
                                        new Thread(new ParameterizedThreadStart(HandlePacket)).Start(clientpacket);
                                    }
                                }
                                else if (obj is PyChecksumedStream) // Checksumed packets
                                {
                                    PyPacket clientpacket = new PyPacket();

                                    clientpacket.Decode(obj);
                                    
                                    new Thread(new ParameterizedThreadStart(HandlePacket)).Start(clientpacket);
                                }
                                else if (obj is PyTuple)
                                {
                                    // The only tuple packet is the LowLevelVersionExchange
                                    LowLevelVersionExchange ex = new LowLevelVersionExchange();

                                    ex.Decode(obj);
                                    
                                    // Reply with the node LowLevelVersionExchange
                                    LowLevelVersionExchange reply = new LowLevelVersionExchange();

                                    reply.codename = Common.Constants.Game.codename;
                                    reply.birthday = Common.Constants.Game.birthday;
                                    reply.build = Common.Constants.Game.build;
                                    reply.machoVersion = Common.Constants.Game.machoVersion;
                                    reply.version = Common.Constants.Game.version;
                                    reply.region = Common.Constants.Game.region;
                                    reply.isNode = true;

                                    Send(reply.Encode());
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
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Main", e.Message);
                Log.Trace("Main", e.StackTrace);
                Log.Error("Main", "Stopped...");
            }
        }
    }
}
