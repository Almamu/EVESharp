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
    /*public class MachoNet
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
            this.HttpClient = httpClient;
            this.Database = databaseConnection;
            this.DependencyInjection = dependencyInjection;
        }

        public async void Initialize()
        {
            Log.Information("Initializing service manager");

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
            
            Log.Information($"Orchestrator assigned node id {this.Container.NodeID} with address {this.Container.Address}");
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
            Log.Information("Waiting 5 seconds before connecting to all the currently active proxies");

            Task.Delay(5000).Wait();
            
            this.EstablishConnectionWithProxies();
        }

        private void RunInSingleNodeMode()
        {
            Log.Fatal("Starting up in single-node mode");
            Log.Error("Starting up in single-node mode");
            Log.Debug("Starting up in single-node mode");
            Log.Information("Starting up in single-node mode");
            Log.Warning("Starting up in single-node mode");
            Log.Verbose("Starting up in single-node mode");
            
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
    }*/
}