using System;
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
using EVESharp.Node.Accounts;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
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
using Character = EVESharp.Node.Inventory.Items.Types.Character;
using Container = SimpleInjector.Container;

namespace EVESharp.Node.Network
{
    public class MachoNet
    {
        private Channel Log { get; }
#if DEBUG
        private Channel CallLog { get; }
        private Channel ResultLog { get; }
#endif
        private MachoServerTransport MachoServerTransport { get; }
        public NodeContainer Container { get; }
        public SystemManager SystemManager { get; }
        public ItemFactory ItemFactory { get; }
        public ServiceManager ServiceManager { get; set; }
        public ClientManager ClientManager { get; }
        public BoundServiceManager BoundServiceManager { get; }
        public AccountDB AccountDB { get; }
        public NotificationManager NotificationManager { get; }
        public TimerManager TimerManager { get; }
        private General Configuration { get; }
        public GeneralDB GeneralDB { get; }
        public LoginQueue LoginQueue { get; init; }
        private Container DependencyInjection { get; }
        public int ErrorCount = 0;

        public event EventHandler OnClusterTimer;
        
        public MachoNet(NodeContainer container, SystemManager systemManager,
            ClientManager clientManager, BoundServiceManager boundServiceManager, ItemFactory itemFactory,
            Logger logger, AccountDB accountDB, General configuration, NotificationManager notificationManager,
            TimerManager timerManager, LoginQueue loginQueue, GeneralDB generalDB, Container dependencyInjection)
        {
            this.Log = logger.CreateLogChannel("MachoNet");
#if DEBUG
            this.CallLog = logger.CreateLogChannel("CallDebug", true);
            this.ResultLog = logger.CreateLogChannel("ResultDebug", true);
#endif
            this.SystemManager = systemManager;
            this.ClientManager = clientManager;
            this.BoundServiceManager = boundServiceManager;
            this.ItemFactory = itemFactory;
            this.Container = container;
            this.AccountDB = accountDB;
            this.Configuration = configuration;
            this.NotificationManager = notificationManager;
            this.TimerManager = timerManager;
            this.LoginQueue = loginQueue;
            this.GeneralDB = generalDB;
            this.MachoServerTransport = new MachoServerTransport(this.Configuration.Listening.Port, this, logger);
            this.DependencyInjection = dependencyInjection;
        }

        public async void Initialize()
        {
            Log.Info("Initializing service manager");

            this.ServiceManager = this.DependencyInjection.GetInstance<ServiceManager>();
            
            // register ourselves with the orchestrator and get our node id AND address
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.PostAsJsonAsync($"{this.Configuration.Cluster.OrchestatorURL}/Nodes/register", this.MachoServerTransport.Port);

                // make sure we have a proper answer
                response.EnsureSuccessStatusCode();
                // read the json and extract the required information
                Stream inputStream = await response.Content.ReadAsStreamAsync();

                JsonObject result = JsonSerializer.Deserialize<JsonObject>(inputStream);

                this.Container.Address = result["address"].ToString();
                this.Container.NodeID = (long) result["nodeId"];
                
                Log.Info($"Orchestrator assigned node id {this.Container.NodeID} with address {this.Container.Address}");
            }
            catch (Exception e)
            {
                Log.Error($"Error contacting orchestrator: {e.Message}");
                this.RunInSingleNodeMode();
            }

            this.MachoServerTransport.Listen();
        }

        private void RunInSingleNodeMode()
        {
            Log.Fatal("Starting up in single-node mode");
            Log.Error("Starting up in single-node mode");
            Log.Debug("Starting up in single-node mode");
            Log.Info("Starting up in single-node mode");
            Log.Warning("Starting up in single-node mode");
            Log.Trace("Starting up in single-node mode");
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

        private void HandleOnClientDisconnected(PyTuple data)
        {
            if (data.Count != 1)
            {
                Log.Error("Received OnClientDisconnected notification with the wrong format");
                return;
            }

            PyDataType first = data[0];

            if (first is PyInteger == false)
            {
                Log.Error("Received OnClientDisconnected notification with the wrong format");
                return;
            }

            PyInteger clientID = first as PyInteger;
            
            // remove the client from the session list and free it's data
            if (this.ClientManager.TryGetClient(clientID, out Client client) == false)
            {
                Log.Error($"Received OnClientDisconnected notification for an unknown client {clientID}");
                return;
            }

            // finally remove the client from the manager, this will take care of freeing everything else
            this.ClientManager.Remove(client);
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
                    if (this.ItemFactory.TryGetItem(item.LocationID, out ItemEntity location) == false || this.ClientManager.ContainsCharacterID(item.OwnerID) == false)
                    {
                        // this item should not be loaded, so unload and return
                        this.ItemFactory.UnloadItem(item);
                        return;
                    }

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
                case "OnClientDisconnected":
                    this.HandleOnClientDisconnected(packet.Payload[1] as PyTuple);
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