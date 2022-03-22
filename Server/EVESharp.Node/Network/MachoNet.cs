using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EVESharp.Common.Constants;
using EVESharp.Common.Database;
using EVESharp.Common.Logging;
using EVESharp.EVE;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.contractMgr;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Inventory.SystemEntities;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.Notifications.Nodes.Corporations;
using EVESharp.Node.Notifications.Nodes.Corps;
using EVESharp.Node.Services.Corporations;
using EVESharp.Node.Services;
using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Org.BouncyCastle.Bcpg;
using AccountDB = EVESharp.Database.AccountDB;
using Character = EVESharp.Node.Inventory.Items.Types.Character;
using Container = SimpleInjector.Container;
using SessionManager = EVESharp.Node.Sessions.SessionManager;

namespace EVESharp.Node.Network
{
    public class MachoNet
    {
        private Channel Log { get; }
#if DEBUG
        private Channel CallLog { get; }
        private Channel ResultLog { get; }
#endif
        public MachoServerTransport Transport { get; }
        public NodeContainer Container { get; }
        public ItemFactory ItemFactory { get; }
        public ServiceManager ServiceManager { get; private set; }
        public BoundServiceManager BoundServiceManager { get; }
        public NotificationManager NotificationManager { get; }
        public TimerManager TimerManager { get; }
        public SessionManager SessionManager { get; set; }
        public SystemManager SystemManager { get; set; }
        public General Configuration { get; }
        public GeneralDB GeneralDB { get; }
        public LoginQueue LoginQueue { get; set; }
        private Container DependencyInjection { get; }
        private HttpClient HttpClient { get; }
        private DatabaseConnection Database { get; }
        public int ErrorCount = 0;
        private Dictionary<int, long> ClientToProxyCache { get; } = new Dictionary<int, long>();

        public event EventHandler OnClusterTimer;
        
        public MachoNet(NodeContainer container, BoundServiceManager boundServiceManager,
            ItemFactory itemFactory, Logger logger, General configuration, NotificationManager notificationManager,
            TimerManager timerManager, GeneralDB generalDB, HttpClient httpClient,
            DatabaseConnection databaseConnection, Container dependencyInjection)
        {
            this.Log = logger.CreateLogChannel("MachoNet");
#if DEBUG
            this.CallLog = logger.CreateLogChannel("CallDebug", true);
            this.ResultLog = logger.CreateLogChannel("ResultDebug", true);
#endif
            this.BoundServiceManager = boundServiceManager;
            this.ItemFactory = itemFactory;
            this.Container = container;
            this.Configuration = configuration;
            this.NotificationManager = notificationManager;
            this.TimerManager = timerManager;
            this.GeneralDB = generalDB;
            this.Transport = new MachoServerTransport(this.Configuration.MachoNet.Port, this, logger);
            this.NotificationManager.MachoServerTransport = this.Transport;
            this.HttpClient = httpClient;
            this.Database = databaseConnection;
            this.DependencyInjection = dependencyInjection;
        }

        public async void Initialize()
        {
            Log.Info("Initializing service manager");

            this.ServiceManager = this.DependencyInjection.GetInstance<ServiceManager>();
            
            switch (this.Configuration.MachoNet.Mode)
            {
                case MachoNetMode.Proxy:
                    this.RunInProxyMode();
                    break;
                case MachoNetMode.Server:
                    this.RunInServerMode();
                    break;
                case MachoNetMode.Single:
                    this.RunInSingleNodeMode();
                    break;
            }
        }

        private async void RegisterNode()
        {
            // register ourselves with the orchestrator and get our node id AND address
            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"port", this.Configuration.MachoNet.Port.ToString()},
                {"role", this.Configuration.MachoNet.Mode switch
                {
                    MachoNetMode.Proxy => "proxy",
                    MachoNetMode.Server => "server"
                }}
            });
            HttpResponseMessage response = await this.HttpClient.PostAsync($"{this.Configuration.Cluster.OrchestatorURL}/Nodes/register",  content);

            // make sure we have a proper answer
            response.EnsureSuccessStatusCode();
            // read the json and extract the required information
            Stream inputStream = await response.Content.ReadAsStreamAsync();

            JsonObject result = JsonSerializer.Deserialize<JsonObject>(inputStream);

            this.Container.Address = result["address"].ToString();
            this.Container.NodeID = (long) result["nodeId"];
            
            Log.Info($"Orchestrator assigned node id {this.Container.NodeID} with address {this.Container.Address}");
        }

        private void RunInProxyMode()
        {
            try
            {
                this.RegisterNode();
                this.StartListening();
            }
            catch (Exception e)
            {
                Log.Error($"Error contacting orchestrator: {e.Message}");
                this.RunInSingleNodeMode();
            }
        }

        private void RunInServerMode()
        {
            try
            {
                this.RegisterNode();
                this.StartListening();
            }
            catch (Exception e)
            {
                Log.Error($"Error contacting orchestrator: {e.Message}");
                this.RunInSingleNodeMode();
            }
            
            // wait 5 seconds and connect to the proxy
            Log.Info("Waiting 5 seconds before connecting to all the currently active proxies");

            Task.Delay(5000).Wait();
            
            this.EstablishConnectionWithProxies();
        }

        private void RunInSingleNodeMode()
        {
            Log.Fatal("Starting up in single-node mode");
            Log.Error("Starting up in single-node mode");
            Log.Debug("Starting up in single-node mode");
            Log.Info("Starting up in single-node mode");
            Log.Warning("Starting up in single-node mode");
            Log.Trace("Starting up in single-node mode");
            
            // update the configuration to reflect the mode change
            this.Configuration.MachoNet.Mode = MachoNetMode.Single;
            // set the nodeID to something that is not 0
            this.Container.NodeID = Common.Constants.Network.PROXY_NODE_ID;
            // clear nodeIDs from the invItems table
            this.ItemFactory.ItemDB.ClearNodeOwnership();
            Database.Procedure(AccountDB.RESET_CLIENT_ADDRESSES);
            
            this.StartListening();
        }

        private void StartListening()
        {
            this.Transport.Listen();
        }

        private async void EstablishConnectionWithProxies()
        {
            HttpResponseMessage response = await this.HttpClient.GetAsync($"{this.Configuration.Cluster.OrchestatorURL}/Nodes/proxies");

            // make sure we have a proper answer
            response.EnsureSuccessStatusCode();
            // read the json and extract the required information
            Stream inputStream = await response.Content.ReadAsStreamAsync();

            JsonArray result = JsonSerializer.Deserialize<JsonArray>(inputStream);

            foreach (JsonObject proxy in result)
            {
                long nodeID = (long) proxy["nodeID"];

                this.OpenNodeConnection(nodeID);
            }
        }

        /// <summary>
        /// Opens a connection to the given proxy
        /// </summary>
        /// <param name="nodeID">The nodeID of the proxy to connect to</param>
        public async Task<MachoTransport> OpenNodeConnection(long nodeID)
        {
            Log.Info($"Looking up NodeID {nodeID}...");
            HttpResponseMessage response = await this.HttpClient.GetAsync($"{this.Configuration.Cluster.OrchestatorURL}/Nodes/node/{nodeID}");

            // make sure we have a proper answer
            response.EnsureSuccessStatusCode();
            // read the json and extract the required information
            Stream inputStream = await response.Content.ReadAsStreamAsync();

            JsonObject result = JsonSerializer.Deserialize<JsonObject>(inputStream);

            // get address and port
            string ip = result["ip"].ToString();
            ushort port = (ushort) result["port"];
            string role = result["role"].ToString();
            
            Log.Info($"Found {role} with NodeID {nodeID} on address {ip}, opening connection...");
            
            // finally open a connection and register it in the transport list
            MachoUnauthenticatedTransport transport =
                new MachoUnauthenticatedTransport(this.Transport, Log.Logger.CreateLogChannel($"{role}-{nodeID}"));
            // open a connection
            transport.Connect(ip, port);
            // send an identification req to start the authentication flow
            transport.Socket.Send(
                new IdentificationReq()
                {
                    Address = this.Container.Address,
                    NodeID = this.Container.NodeID,
                    Mode = this.Configuration.MachoNet.Mode switch
                    {
                        MachoNetMode.Proxy => "proxy",
                        MachoNetMode.Server => "server"
                    }
                }
            );
            
            return transport;
        }

        /// <summary>
        /// Adds an outgoing packet to the queue so it gets sent to the correct transports
        /// </summary>
        /// <param name="packet">The packet to send</param>
        public void QueuePacket(PyPacket packet)
        {
            // this function is kind of a lie, it won't queue anything
            // everything is sent
            switch (packet.Destination)
            {
                case PyAddressBroadcast:
                    this.QueueBroadcastPacket(packet);
                    break;
                
                case PyAddressClient:
                    this.QueueClientPacket(packet);
                    break;
                
                case PyAddressNode:
                    this.QueueNodePacket(packet);
                    break;
            }
        }

        private void QueueMulticastPacket(PyPacket packet)
        {
            // TODO: IMPLEMENT MULTICAST
            Log.Error("Multicast not supported yet!");
        }

        private void QueueSimpleBroadcastPacket(PyPacket packet)
        {
            PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;
            // an ownerid requires some special handling
            bool isOwnerID = dest.IDType == "ownerid";

            foreach (PyInteger id in dest.IDsOfInterest.GetEnumerable<PyInteger>())
            {
                foreach (MachoTransport transport in this.Transport.TransportList)
                {
                    if (isOwnerID == true)
                    {
                        if (transport.Session[Session.ALLIANCE_ID] == id ||
                            transport.Session[Session.CHAR_ID] == id ||
                            transport.Session[Session.CORP_ID] == id)
                            transport.Socket.Send(packet);
                    }
                    else if (transport.Session[dest.IDType] == id)
                    {
                        // transport found, notify it
                        transport.Socket.Send(packet);
                    }
                }
            }
        }

        private void QueueComplexBroadcastPacket(PyPacket packet)
        {
            PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;
            
            // extract the actual ids used in the destination
            string[] criteria = dest.IDType.Value.Split('&');
            // determine which ones are ownerIds to take them into account
            bool[] isOwnerID = Array.ConvertAll(criteria, x => x == "ownerid");

            foreach (PyTuple id in dest.IDsOfInterest.GetEnumerable<PyTuple>())
            {
                // ignore invalid ids
                if (id.Count != criteria.Length)
                    continue;

                foreach (MachoTransport transport in this.Transport.TransportList)
                {
                    bool found = true;
                    
                    // validate both values
                    for (int i = 0; i < criteria.Length; i++)
                    {
                        if (isOwnerID[i] == true)
                        {
                            if (transport.Session[Session.ALLIANCE_ID] != id[i] &&
                                transport.Session[Session.CHAR_ID] != id[i] &&
                                transport.Session[Session.CORP_ID] != id[i])
                            {
                                found = false;
                                break;
                            }
                        }
                        else if (transport.Session[criteria[i]] != id[i])
                        {
                            // transport found, notify it
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        transport.Socket.Send(packet);
                }
            }
        }

        private async void QueueNodeBroadcastPacket(PyPacket packet)
        {
            PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;

            foreach (PyInteger id in dest.IDsOfInterest.GetEnumerable<PyInteger>())
            {
                if (this.Transport.NodeTransports.TryGetValue(id, out MachoNodeTransport transport) == true)
                    transport.Socket.Send(packet);
                else
                {
                    // try a connection to the node and queue the notification
                    MachoTransport newTransport = await this.OpenNodeConnection(id);

                    newTransport.QueuePostAuthenticationPacket(packet);
                }
            }
        }
        
        private void QueueBroadcastPacket(PyPacket packet)
        {
            PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;
            
            if (this.Configuration.MachoNet.Mode == MachoNetMode.Server)
            {
                switch (dest.IDType)
                {
                    case "nodeid":
                        // TODO: IS THIS NECCESARY? THIS SHOULDN'T REALLY HAPPEN? BUT MIGHT COME HANDY
                        this.QueueNodeBroadcastPacket(packet);
                        break;
                    default:
                        // send packet to the proxies
                        foreach ((long _, MachoProxyTransport proxy) in this.Transport.ProxyTransports)
                            proxy.Socket.Send(packet);
                        break;
                }
            }
            else
            {
                switch (dest.IDType)
                {
                    case "*multicastID":
                        this.QueueMulticastPacket(packet);
                        break;
                    case "corpid&corprole":
                        this.QueueComplexBroadcastPacket(packet);
                        break;
                    case "ownerid&locationid":
                        this.QueueComplexBroadcastPacket(packet);
                        break;
                    case "nodeid":
                        // TODO: IS THIS NECCESARY? THIS SHOULDN'T REALLY HAPPEN? BUT MIGHT COME HANDY
                        this.QueueNodeBroadcastPacket(packet);
                        break;
                    default:
                        this.QueueSimpleBroadcastPacket(packet);
                        break;
                }                
            }
        }

        private void QueueClientPacketFromNode(PyPacket packet)
        {
            PyAddressClient dest = packet.Destination as PyAddressClient;
            
            // check if the client is in our transport cache
            // otherwise fetch it from the database
            if (this.ClientToProxyCache.TryGetValue(dest.ClientID, out long nodeID) == false)
            {
                // resolve the nodeID from the database
                nodeID = Database.Scalar<long>(
                    AccountDB.RESOLVE_CLIENT_ADDRESS,
                    new Dictionary<string, object>()
                    {
                        {"_clientID", (int) dest.ClientID}
                    }
                );
            }
            
            // notify proxy
            if (this.Transport.ProxyTransports.TryGetValue(nodeID, out MachoProxyTransport transport) == false)
            {
                // open a connection to the proxy and HOPE it's actually what we're looking for
                // make sure the packet is queue on it's output list
                Task<MachoTransport> task = this.OpenNodeConnection(nodeID);
                task.Wait();
                task.Result.QueuePostAuthenticationPacket(packet);
                return;
            }
            
            // finally let the proxy know
            transport.Socket.Send(packet);
        }

        private void QueueClientPacket(PyPacket packet)
        {
            if (this.Configuration.MachoNet.Mode == MachoNetMode.Server)
            {
                this.QueueClientPacketFromNode(packet);
                return;
            }

            PyAddressClient dest = packet.Destination as PyAddressClient;

            if (this.Transport.ClientTransports.TryGetValue(dest.ClientID, out MachoClientTransport transport) == false)
                throw new Exception("Trying to queue a packet for an unknown client");
            
            // client found, send packet
            transport.Socket.Send(packet);
        }

        private void QueueNodePacket(PyPacket packet)
        {
            PyAddressNode dest = packet.Destination as PyAddressNode;

            if (this.Transport.NodeTransports.TryGetValue(dest.NodeID, out MachoNodeTransport transport) == false)
                throw new Exception($"Trying to queue a packet for an unknown node ({dest.NodeID}");
            
            // node found, send packet
            transport.Socket.Send(packet);
        }

        /// <summary>
        /// Sends a heartbeat to the orchestrator agent to signal our node being up and running healthily
        /// </summary>
        public async void PerformHeartbeat()
        {
            Log.Debug("Sending heartbeat to orchestration agent");
            // register ourselves with the orchestrator and get our node id AND address
            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"address", this.Container.Address},
                {"load", "0.0"}
            });
            await this.HttpClient.PostAsync($"{this.Configuration.Cluster.OrchestatorURL}/Nodes/heartbeat",  content);
        }
        
        /// <summary>
        /// Sends a provisional response to the given call
        /// </summary>
        /// <param name="answerTo"></param>
        /// <param name="response"></param>
        public void SendProvisionalResponse(CallInformation answerTo, ProvisionalResponse response)
        {
            PyPacket result = new PyPacket(PyPacket.PacketType.CALL_RSP);
            
            // ensure destination has clientID in it
            PyAddressClient source = answerTo.Source as PyAddressClient;

            source.ClientID = answerTo.Session.UserID;
            // switch source and dest
            result.Source = answerTo.Destination;
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

            this.QueuePacket(result);
        }

        /// <summary>
        /// Sends a call result to the given call
        /// </summary>
        /// <param name="answerTo"></param>
        /// <param name="content"></param>
        public void SendCallResult(CallInformation answerTo, PyDataType content, PyDictionary namedPayload)
        {
            // ensure destination has clientID in it
            PyAddressClient originalSource = answerTo.Source as PyAddressClient;
            originalSource.ClientID = answerTo.Session.UserID;

            this.QueuePacket(
                new PyPacket(PyPacket.PacketType.CALL_RSP)
                {
                    // switch source and dest
                    Source = answerTo.Destination,
                    Destination = originalSource,
                    UserID = originalSource.ClientID,
                    Payload = new PyTuple(1) {[0] = new PySubStream(content)},
                    OutOfBounds = namedPayload
                }
            );
        }
        
        /// <summary>
        /// Sends an exception as answer to the given call
        /// </summary>
        /// <param name="answerTo"></param>
        /// <param name="packetType"></param>
        /// <param name="content"></param>
        public void SendException(CallInformation answerTo, PyPacket.PacketType packetType, PyDataType content)
        {
            // build a new packet with the correct information
            PyPacket result = new PyPacket(PyPacket.PacketType.ERRORRESPONSE);

            // ensure destination has clientID in it
            PyAddressClient source = answerTo.Source as PyAddressClient;

            source.ClientID = answerTo.Session.UserID;
            // switch source and dest
            result.Source = answerTo.Destination;
            result.Destination = source;

            result.UserID = source.ClientID;
            result.Payload = new PyTuple(3)
            {
                [0] = (int) packetType,
                [1] = (int) MachoErrorType.WrappedException,
                [2] = new PyTuple (1) { [0] = new PySubStream(content) }, 
            };

            this.QueuePacket(result);
        }
        
        /// <summary>
        /// Sends an exception as answer to the given call
        /// </summary>
        /// <param name="answerTo"></param>
        /// <param name="packetType"></param>
        /// <param name="content"></param>
        public void SendException(PyAddress source, PyAddress destination, PyPacket.PacketType packetType, PyDataType content)
        {
            int userID = 0;

            if (destination is PyAddressClient client)
                userID = client.ClientID;

            PyPacket result = new PyPacket(PyPacket.PacketType.ERRORRESPONSE)
            {
                Source = source,
                Destination = destination,
                UserID = userID,
                Payload = new PyTuple(3)
                {
                    [0] = (int) packetType,
                    [1] = (int) MachoErrorType.WrappedException,
                    [2] = new PyTuple(1) {[0] = new PySubStream(content)}
                }
            };

            this.QueuePacket(result);
        }
    }
}