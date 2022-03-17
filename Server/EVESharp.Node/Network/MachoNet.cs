using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EVESharp.Common.Constants;
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
        private MachoServerTransport Transport { get; }
        public NodeContainer Container { get; }
        public SystemManager SystemManager { get; }
        public ItemFactory ItemFactory { get; }
        public ServiceManager ServiceManager { get; private set; }
        public BoundServiceManager BoundServiceManager { get; }
        public NotificationManager NotificationManager { get; }
        public TimerManager TimerManager { get; }
        public SessionManager SessionManager { get; set; }
        public General Configuration { get; }
        public GeneralDB GeneralDB { get; }
        public LoginQueue LoginQueue { get; init; }
        private Container DependencyInjection { get; }
        public int ErrorCount = 0;

        public event EventHandler OnClusterTimer;
        
        public MachoNet(NodeContainer container, SystemManager systemManager, BoundServiceManager boundServiceManager,
            ItemFactory itemFactory, Logger logger, General configuration, NotificationManager notificationManager,
            TimerManager timerManager, LoginQueue loginQueue, GeneralDB generalDB, Container dependencyInjection)
        {
            this.Log = logger.CreateLogChannel("MachoNet");
#if DEBUG
            this.CallLog = logger.CreateLogChannel("CallDebug", true);
            this.ResultLog = logger.CreateLogChannel("ResultDebug", true);
#endif
            this.SystemManager = systemManager;
            this.BoundServiceManager = boundServiceManager;
            this.ItemFactory = itemFactory;
            this.Container = container;
            this.Configuration = configuration;
            this.NotificationManager = notificationManager;
            this.TimerManager = timerManager;
            this.LoginQueue = loginQueue;
            this.GeneralDB = generalDB;
            this.Transport = new MachoServerTransport(this.Configuration.MachoNet.Port, this, logger);
            this.NotificationManager.MachoServerTransport = this.Transport;
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
            HttpClient client = new HttpClient();
            HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string> {
                {"port", this.Transport.Port.ToString()},
                {"role", this.Configuration.MachoNet.Mode switch
                {
                    MachoNetMode.Proxy => "proxy",
                    MachoNetMode.Server => "server"
                }}
            });
            HttpResponseMessage response = await client.PostAsync($"{this.Configuration.Cluster.OrchestatorURL}/Nodes/register",  content);

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
            
            this.StartListening();
        }

        private void StartListening()
        {
            this.Transport.Listen();
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
                foreach ((int _, MachoClientTransport transport) in this.Transport.ClientTransports)
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
            // TODO: SUPPORT ownerid AS NOTIFICATION HERE

            foreach (PyTuple id in dest.IDsOfInterest.GetEnumerable<PyTuple>())
            {
                // ignore invalid ids
                if (id.Count != criteria.Length)
                    continue;

                foreach ((int _, MachoClientTransport transport) in this.Transport.ClientTransports)
                {
                    bool found = true;
                    
                    // validate both values
                    for (int i = 0; i < criteria.Length; i++)
                    {
                        if (transport.Session[criteria[i]] != id[i])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                        transport.Socket.Send(packet);
                }
            }
        }

        private void QueueNodeBroadcastPacket(PyPacket packet)
        {
            PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;

            foreach (PyInteger id in dest.IDsOfInterest.GetEnumerable<PyInteger>())
            {
                if (this.Transport.NodeTransports.TryGetValue(id, out MachoNodeTransport transport) == true)
                    transport.Socket.Send(packet);
            }
        }
        
        private void QueueBroadcastPacket(PyPacket packet)
        {
            PyAddressBroadcast dest = packet.Destination as PyAddressBroadcast;

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

        private void QueueClientPacket(PyPacket packet)
        {
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

        private void HandleOnSolarSystemLoaded(PyTuple data)
        {
            if (data.Count != 1)
            {
                Log.Error("Received OnSolarSystemLoad notification with the wrong format");
                return;
            }

            PyDataType first = data[0];

            if (first is PyInteger == false)
            {
                Log.Error("Received OnSolarSystemLoad notification with the wrong format");
                return;
            }

            PyInteger solarSystemID = first as PyInteger;

            // mark as loaded
            this.SystemManager.LoadSolarSystem(solarSystemID);
        }

        private void HandleOnItemUpdate(Notifications.Nodes.Inventory.OnItemChange change)
        {
            foreach ((PyInteger itemID, PyDictionary _changes) in change.Updates)
            {
                PyDictionary<PyString, PyTuple> changes = _changes.GetEnumerable<PyString, PyTuple>();
                
                ItemEntity item = this.ItemFactory.LoadItem(itemID, out bool loadRequired);
                
                // if the item was just loaded there's extra things to take into account
                // as the item might not even need a notification to the character it belongs to
                if (loadRequired == true)
                {
                    // trust that the notification got to the correct node
                    // load the item and check the owner, if it's logged in and the locationID is loaded by us
                    // that means the item should be kept here
                    if (this.ItemFactory.TryGetItem(item.LocationID, out ItemEntity location) == false)
                        return;

                    bool locationBelongsToUs = true;

                    switch (location)
                    {
                        case Station _:
                            locationBelongsToUs = this.SystemManager.StationBelongsToUs(location.ID);
                            break;
                        case SolarSystem _:
                            locationBelongsToUs = this.SystemManager.SolarSystemBelongsToUs(location.ID);
                            break;
                    }

                    if (locationBelongsToUs == false)
                    {
                        this.ItemFactory.UnloadItem(item);
                        return;
                    }
                }

                OnItemChange itemChange = new OnItemChange(item);
                
                // update item and build change notification
                if (changes.TryGetValue("locationID", out PyTuple locationChange) == true)
                {
                    PyInteger oldValue = locationChange[0] as PyInteger;
                    PyInteger newValue = locationChange[1] as PyInteger;
                    
                    itemChange.AddChange(ItemChange.LocationID, oldValue);
                    item.LocationID = newValue;
                }
                
                if (changes.TryGetValue ("quantity", out PyTuple quantityChange) == true)
                {
                    PyInteger oldValue = quantityChange[0] as PyInteger;
                    PyInteger newValue = quantityChange[1] as PyInteger;

                    itemChange.AddChange(ItemChange.Quantity, oldValue);
                    item.Quantity = newValue;
                }
                
                if (changes.TryGetValue("ownerID", out PyTuple ownerChange) == true)
                {
                    PyInteger oldValue = ownerChange[0] as PyInteger;
                    PyInteger newValue = ownerChange[1] as PyInteger;

                    itemChange.AddChange(ItemChange.OwnerID, oldValue);
                    item.OwnerID = newValue;
                }

                if (changes.TryGetValue("singleton", out PyTuple singletonChange) == true)
                {
                    PyBool oldValue = singletonChange[0] as PyBool;
                    PyBool newValue = singletonChange[1] as PyBool;

                    itemChange.AddChange(ItemChange.Singleton, oldValue);
                    item.Singleton = newValue;
                }
                
                // TODO: IDEALLY THIS WOULD BE ENQUEUED SO ALL OF THEM ARE SENT AT THE SAME TIME
                // TODO: BUT FOR NOW THIS SHOULD SUFFICE
                // send the notification
                this.NotificationManager.NotifyCharacter(item.OwnerID, "OnMultiEvent", 
                    new PyTuple(1) {[0] = new PyList(1) {[0] = itemChange}});

                if (item.LocationID == this.ItemFactory.LocationRecycler.ID)
                    // the item is removed off the database if the new location is the recycler
                    item.Destroy();
                else if (item.LocationID == this.ItemFactory.LocationMarket.ID)
                    // items that are moved to the market can be unloaded
                    this.ItemFactory.UnloadItem(item);
                else
                    // save the item if the new location is not removal
                    item.Persist();    
            }
        }

        private void HandleOnClusterTimer(PyTuple data)
        {
            Log.Info("Received a cluster request to run timed events on services...");

            this.OnClusterTimer?.Invoke(this, null);
        }

        private void HandleOnCorporationMemberChanged(OnCorporationMemberChanged change)
        {
            // this notification does not need to send anything to anyone as the clients will already get notified
            // based on their corporation IDs
            
            if (this.ServiceManager.corpRegistry.FindInstanceForObjectID(change.OldCorporationID, out corpRegistry oldService) == true && oldService.MembersSparseRowset is not null)
                oldService.MembersSparseRowset.RemoveRow(change.MemberID);

            if (this.ServiceManager.corpRegistry.FindInstanceForObjectID(change.NewCorporationID, out corpRegistry newService) == true && newService.MembersSparseRowset is not null)
            {
                PyDictionary<PyString, PyTuple> changes = new PyDictionary<PyString, PyTuple>()
                {
                    ["characterID"] = new PyTuple(2) {[0] = null, [1] = change.MemberID},
                    ["title"] = new PyTuple(2) {[0] = null, [1] = ""},
                    ["startDateTime"] = new PyTuple(2) {[0] = null, [1] = DateTime.UtcNow.ToFileTimeUtc ()},
                    ["roles"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["rolesAtHQ"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["rolesAtBase"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["rolesAtOther"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["titleMask"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["grantableRoles"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["grantableRolesAtHQ"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["grantableRolesAtBase"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["grantableRolesAtOther"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["divisionID"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["squadronID"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["baseID"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["blockRoles"] = new PyTuple(2) {[0] = null, [1] = 0},
                    ["gender"] = new PyTuple(2) {[0] = null, [1] = 0}
                };
                
                newService.MembersSparseRowset.AddRow(change.MemberID, changes);
            }
            
            // the only thing needed is to check for a Character reference and update it's corporationID to the correct one
            if (this.ItemFactory.TryGetItem(change.MemberID, out Inventory.Items.Types.Character character) == false)
                // if the character is not loaded it could mean that the package arrived on to the node while the player was logging out
                // so this is safe to ignore 
                return;

            // set the corporation to the new one
            character.CorporationID = change.NewCorporationID;
            // this change usually means that the character is now in a new corporation, so everything corp-related is back to 0
            character.Roles = 0;
            character.RolesAtBase = 0;
            character.RolesAtHq = 0;
            character.RolesAtOther = 0;
            character.TitleMask = 0;
            character.GrantableRoles = 0;
            character.GrantableRolesAtBase = 0;
            character.GrantableRolesAtOther = 0;
            character.GrantableRolesAtHQ = 0;
            // persist the character
            character.Persist();
            // nothing else needed
            Log.Debug($"Updated character ({character.ID}) coporation ID from {change.OldCorporationID} to {change.NewCorporationID}");
        }

        private void HandleOnCorporationMemberUpdated(OnCorporationMemberUpdated change)
        {
            // this notification does not need to send anything to anyone as the clients will already get notified
            // by the session change
            
            // the only thing needed is to check for a Character reference and update it's roles to the correct onews
            if (this.ItemFactory.TryGetItem(change.CharacterID, out Inventory.Items.Types.Character character) == false)
                // if the character is not loaded it could mean that the package arrived on the node wile the player was logging out
                // so this is safe to ignore
                return;
            
            // update the roles and everything else
            character.Roles = change.Roles;
            character.RolesAtBase = change.RolesAtBase;
            character.RolesAtHq = change.RolesAtHQ;
            character.RolesAtOther = change.RolesAtOther;
            character.GrantableRoles = change.GrantableRoles;
            character.GrantableRolesAtBase = change.GrantableRolesAtBase;
            character.GrantableRolesAtOther = change.GrantableRolesAtOther;
            character.GrantableRolesAtHQ = change.GrantableRolesAtHQ;
            // some debugging is well received
            Log.Debug($"Updated character ({character.ID}) roles");
        }

        private void HandleOnCorporationChanged(OnCorporationChanged change)
        {
            // this notification does not need to send anything to anyone as the clients will already get notified
            // by the session change
            
            // the only thing needed is to check for a Corporation reference and update it's alliance information to the new values
            if (this.ItemFactory.TryGetItem(change.CorporationID, out Corporation corporation) == false)
                // if the corporation is not loaded it could mean that the package arrived on the node wile the last player was logging out
                // so this is safe to ignore
                return;
            
            // update basic information
            corporation.AllianceID = change.AllianceID;
            corporation.ExecutorCorpID = change.ExecutorCorpID;
            corporation.StartDate = change.AllianceID is null ? null : DateTime.UtcNow.ToFileTimeUtc();
            corporation.Persist();
            
            // some debugging is well received
            Log.Debug($"Updated corporation afilliation ({corporation.ID}) to ({corporation.AllianceID})");
        }

        public void HandleBroadcastNotification(PyPacket packet)
        {
            // this packet is an internal one
            if (packet.Payload.Count != 2)
            {
                Log.Error("Received ClusterController notification with the wrong format");
                return;
            }

            if (packet.Payload[0] is not PyString notification)
            {
                Log.Error("Received ClusterController notification with the wrong format");
                return;
            }
            
            Log.Debug($"Received a notification from ClusterController of type {notification.Value}");
            
            switch (notification)
            {
                case "OnSolarSystemLoad":
                    this.HandleOnSolarSystemLoaded(packet.Payload[1] as PyTuple);
                    break;
                case Notifications.Nodes.Inventory.OnItemChange.NOTIFICATION_NAME:
                    this.HandleOnItemUpdate(packet.Payload);
                    break;
                case "OnClusterTimer":
                    this.HandleOnClusterTimer(packet.Payload[1] as PyTuple);
                    break;
                case OnCorporationMemberChanged.NOTIFICATION_NAME:
                    this.HandleOnCorporationMemberChanged(packet.Payload);
                    break;
                case OnCorporationMemberUpdated.NOTIFICATION_NAME:
                    this.HandleOnCorporationMemberUpdated(packet.Payload);
                    break;
                case OnCorporationChanged.NOTIFICATION_NAME:
                    this.HandleOnCorporationChanged(packet.Payload);
                    break;
                default:
                    Log.Fatal("Received ClusterController notification with the wrong format");
                    break;
            }
        }
    }
}