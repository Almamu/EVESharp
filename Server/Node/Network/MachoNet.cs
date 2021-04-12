using System;
using System.IO;
using System.Text.RegularExpressions;
using Common.Constants;
using Common.Logging;
using Common.Packets;
using Node.Configuration;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Inventory.SystemEntities;
using Node.Notifications.Client.Character;
using Node.Notifications.Client.Inventory;
using Node.Notifications.Nodes.Character;
using Node.Services;
using PythonTypes;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;
using Character = Node.Inventory.Items.Types.Character;
using Container = SimpleInjector.Container;

namespace Node.Network
{
    public class MachoNet
    {
        private Channel Log { get; }
#if DEBUG
        private Channel CallLog { get; }
        private Channel ResultLog { get; }
#endif
        private ClusterConnection ClusterConnection { get; }
        private NodeContainer Container { get; }
        private SystemManager SystemManager { get; }
        private ItemManager ItemManager => this.ItemFactory.ItemManager;
        private ItemFactory ItemFactory { get; }
        private ServiceManager ServiceManager { get; set; }
        private ClientManager ClientManager { get; }
        private BoundServiceManager BoundServiceManager { get; }
        private AccountDB AccountDB { get; }
        private CharacterManager CharacterManager { get; }
        private NotificationManager NotificationManager { get; }
        private TimerManager TimerManager { get; }
        private General Configuration { get; }
        private Container DependencyInjection { get; }
        private int mErrorCount = 0;

        public MachoNet(ClusterConnection clusterConnection, NodeContainer container, SystemManager systemManager,
            ClientManager clientManager, BoundServiceManager boundServiceManager, ItemFactory itemFactory,
            Logger logger, AccountDB accountDB, General configuration, CharacterManager characterManager,
            NotificationManager notificationManager, TimerManager timerManager, Container dependencyInjection)
        {
            this.Log = logger.CreateLogChannel("MachoNet");
#if DEBUG
            this.CallLog = logger.CreateLogChannel("CallDebug", true);
            this.ResultLog = logger.CreateLogChannel("ResultDebug", true);
#endif
            this.ClusterConnection = clusterConnection;
            this.SystemManager = systemManager;
            this.ClientManager = clientManager;
            this.BoundServiceManager = boundServiceManager;
            this.ItemFactory = itemFactory;
            this.Container = container;
            this.AccountDB = accountDB;
            this.Configuration = configuration;
            this.CharacterManager = characterManager;
            this.NotificationManager = notificationManager;
            this.TimerManager = timerManager;
            this.DependencyInjection = dependencyInjection;
        }

        public void ConnectToProxy()
        {
            Log.Info("Initializing service manager");

            this.ServiceManager = this.DependencyInjection.GetInstance<ServiceManager>();
            
            Log.Info("Connecting to proxy...");
            
            // setup the cluster connection
            this.ClusterConnection.SetReceiveCallback(ReceiveLowLevelVersionExchangeCallback);
            this.ClusterConnection.SetExceptionHandler(HandleException);

            // finally connect to the cluster
            this.ClusterConnection.Connect(this.Configuration.Proxy.Hostname, this.Configuration.Proxy.Port);
        }

        private void HandleException(Exception ex)
        {
            Log.Error("Exception detected: ");

            do
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            } while ((ex = ex.InnerException) != null);
        }

        private void ReceiveLowLevelVersionExchangeCallback(PyDataType ar)
        {
            try
            {
                LowLevelVersionExchange exchange = CheckLowLevelVersionExchange(ar);

                // reply with the node LowLevelVersionExchange
                LowLevelVersionExchange reply = new LowLevelVersionExchange
                {
                    Codename = Common.Constants.Game.CODENAME,
                    Birthday = Common.Constants.Game.BIRTHDAY,
                    Build = Common.Constants.Game.BUILD,
                    MachoVersion = Common.Constants.Game.MACHO_VERSION,
                    Version = Common.Constants.Game.VERSION,
                    Region = Common.Constants.Game.REGION,
                    IsNode = true,
                    NodeIdentifier = "Node"
                };
                
                this.ClusterConnection.Send(reply);

                // set the new handler
                this.ClusterConnection.SetReceiveCallback(ReceiveNodeInitialState);
            }
            catch (Exception e)
            {
                Log.Error($"Exception caught on LowLevelVersionExchange: {e.Message}");
                throw;
            }
        }

        private void ReceiveNodeInitialState(PyDataType ar)
        {
            if (ar is PyObjectData == false)
                throw new Exception($"Expected PyObjectData for machoNet.nodeInfo but got {ar.GetType()}");

            PyObjectData info = ar as PyObjectData;

            if (info.Name != "machoNet.nodeInfo")
                throw new Exception($"Expected PyObjectData of type machoNet.nodeInfo but got {info.Name}");

            // Update our local info
            NodeInfo nodeinfo = info;

            this.Container.NodeID = nodeinfo.NodeID;

            Log.Debug("Found machoNet.nodeInfo, our new node id is " + nodeinfo.NodeID.ToString("X4"));

            // load the specified solar systems
            this.SystemManager.LoadSolarSystems(nodeinfo.SolarSystems.GetEnumerable<PyInteger>());

            // finally set the new packet handler
            this.ClusterConnection.SetReceiveCallback(ReceiveNormalPacketCallback);
        }

        private void HandleCallReq(PyPacket packet, Client client)
        {
            PyTuple callInfo = ((packet.Payload[0] as PyTuple)[1] as PySubStream).Stream as PyTuple;
            
            string call = callInfo[1] as PyString;
            PyTuple args = callInfo[2] as PyTuple;
            PyDictionary sub = callInfo[3] as PyDictionary;
            PyDataType callResult = null;
            PyAddressClient source = packet.Source as PyAddressClient;
            string destinationService = null;
            CallInformation callInformation;
            
            if (packet.Destination is PyAddressAny destAny)
            {
                destinationService = destAny.Service;
            }
            else if (packet.Destination is PyAddressNode destNode)
            {
                destinationService = destNode.Service;

                if (destNode.NodeID != Container.NodeID)
                {
                    Log.Fatal(
                        "Received a call request for a node that is not us, did the ClusterController get confused or something?!"
                    );
                    return;
                }
            }
            
            callInformation = new CallInformation
            {
                Client = client,
                CallID = source.CallID,
                NamedPayload = sub,
                PacketType = packet.Type,
                Service = destinationService,
                From = packet.Source,
                To = packet.Destination
            };

            try
            {
                if (destinationService == null)
                {
                    if (callInfo[0] is PyString == false)
                    {
                        Log.Fatal("Expected bound call with bound string, but got something different");
                        return;
                    }

                    string boundString = callInfo[0] as PyString;

                    // parse the bound string to get back proper node and bound ids
                    Match regexMatch = Regex.Match(boundString, "N=([0-9]+):([0-9]+)");

                    if (regexMatch.Groups.Count != 3)
                    {
                        Log.Fatal($"Cannot find nodeID and boundID in the boundString {boundString}");
                        return;
                    }

                    int nodeID = int.Parse(regexMatch.Groups[1].Value);
                    int boundID = int.Parse(regexMatch.Groups[2].Value);

                    if (nodeID != this.Container.NodeID)
                    {
                        Log.Fatal("Got bound service call for a different node");
                        // TODO: MIGHT BE A GOOD IDEA TO RELAY THIS CALL TO THE CORRECT NODE
                        // TODO: INSIDE THE NETWORK, AT LEAST THAT'S WHAT CCP IS DOING BASED
                        // TODO: ON THE CLIENT'S CODE... NEEDS MORE INVESTIGATION
                        return;
                    }

#if DEBUG
                    CallLog.Trace("Payload");
                    CallLog.Trace(PrettyPrinter.FromDataType(args));
                    CallLog.Trace("Named payload");
                    CallLog.Trace(PrettyPrinter.FromDataType(sub));
#endif

                    callResult = this.BoundServiceManager.ServiceCall(
                        boundID, call, args, callInformation
                    );

#if DEBUG
                    ResultLog.Trace("Result");
                    ResultLog.Trace(PrettyPrinter.FromDataType(callResult));
#endif
                }
                else
                {
                    Log.Trace($"Calling {destinationService}::{call}");

#if DEBUG
                    CallLog.Trace("Payload");
                    CallLog.Trace(PrettyPrinter.FromDataType(args));
                    CallLog.Trace("Named payload");
                    CallLog.Trace(PrettyPrinter.FromDataType(sub));
#endif

                    callResult = this.ServiceManager.ServiceCall(
                        destinationService, call, args, callInformation
                    );

#if DEBUG
                    ResultLog.Trace("Result");
                    ResultLog.Trace(PrettyPrinter.FromDataType(callResult));
#endif
                }

                this.SendCallResult(callInformation, callResult);
            }
            catch (PyException e)
            {
                this.SendException(callInformation, e);
            }
            catch (ProvisionalResponse provisional)
            {
                this.SendProvisionalResponse(callInformation, provisional);
            }
            catch (Exception ex)
            {
                int errorID = ++this.mErrorCount;

                Log.Fatal($"Detected non-client exception, registered as error {errorID}. Extra information: ");
                Log.Fatal(ex.Message);
                Log.Fatal(ex.StackTrace);
                
                // send client a proper notification about the error based on the roles
                if ((callInformation.Client.Role & (int) Roles.ROLE_PROGRAMMER) == (int) Roles.ROLE_PROGRAMMER)
                {
                    this.SendException(callInformation, new CustomError($"An internal server error occurred.<br><b>Reference</b>: {errorID}<br><b>Message</b>: {ex.Message}<br><b>Stack trace</b>:<br>{ex.StackTrace.Replace("\n", "<br>")}"));
                }
                else
                {
                    this.SendException(callInformation, new CustomError($"An internal server error occurred. <b>Reference</b>: {errorID}"));
                }
            }
        }

        private void HandleSessionChangeNotification(PyPacket packet, Client client)
        {
            Log.Debug($"Updating session for client {packet.UserID}");

            // ensure the client is registered in the node and store his session
            if (client == null)
                this.ClientManager.Add(
                    packet.UserID,
                    client = new Client(
                        this.Container, this.ClusterConnection, this.ServiceManager, this.TimerManager,
                        this.ItemFactory, this.CharacterManager, this.SystemManager, this.NotificationManager, this
                    )
                );

            client.UpdateSession(packet);
        }

        private void HandlePingReq(PyPacket packet, Client client)
        {
            // alter package to include the times the data
            PyAddressClient source = packet.Source as PyAddressClient;

            // this time should come from the stream packetizer or the socket itself
            // but there's no way we're adding time tracking for all the goddamned packets
            // so this should be sufficient
            PyTuple handleMessage = new PyTuple(3)
            {
                [0] = DateTime.UtcNow.ToFileTime(),
                [1] = DateTime.UtcNow.ToFileTime(),
                [2] = "server::handle_message"
            };

            PyTuple turnaround = new PyTuple(3)
            {
                [0] = DateTime.UtcNow.ToFileTime(),
                [1] = DateTime.UtcNow.ToFileTime(),
                [2] = "server::turnaround"
            };


            (packet.Payload[0] as PyList)?.Add(handleMessage);
            (packet.Payload[0] as PyList)?.Add(turnaround);

            // change to a response
            packet.Type = PyPacket.PacketType.PING_RSP;
                
            // switch source and destination
            packet.Source = packet.Destination;
            packet.Destination = source;
                
            // queue the packet back
            this.ClusterConnection.Send(packet);
        }

        private void HandleCallRes(PyPacket packet, Client client)
        {
            // ensure the response is directed to us
            if (packet.Destination is PyAddressNode == false)
            {
                Log.Error("Received a call response not directed to us");
                return;
            }

            PyAddressNode dest = packet.Destination as PyAddressNode;

            if (dest.NodeID != Container.NodeID)
            {
                Log.Error($"Received a call response for node {dest.NodeID} but we are {Container.NodeID}");
                return;
            }
                
            // handle call response
            if (packet.Payload.Count != 1)
            {
                Log.Error("Received a call response without proper response data");
                return;
            }

            PyDataType first = packet.Payload[0];

            if (first is PySubStream == false)
            {
                Log.Error("Received a call response without proper response data");
                return;
            }
                
            PySubStream subStream = packet.Payload[0] as PySubStream;
                
            this.ServiceManager.ReceivedRemoteCallAnswer(dest.CallID, subStream.Stream);
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
            if (this.ClientManager.Contains(clientID) == false)
            {
                Log.Error($"Received OnClientDisconnected notification for an unknown client {clientID}");
                return;
            }

            // get the client, search for it's common items and meta inventories and free them
            Client client = this.ClientManager.Get(clientID);

            // clear bound services for this character
            this.BoundServiceManager.OnClientDisconnected(client);
            
            // ensure the client is removed from other places where it shouldn't be
            client.OnClientDisconnected();

            // finally remove the client from the manager
            this.ClientManager.Remove(clientID);
        }

        private void HandleOnItemUpdate(Notifications.Nodes.Inventory.OnItemChange change)
        {
            foreach ((PyInteger itemID, PyDictionary _changes) in change.Updates)
            {
                PyDictionary<PyString, PyTuple> changes = _changes.GetEnumerable<PyString, PyTuple>();
                
                ItemEntity item = this.ItemManager.LoadItem(itemID, out bool loadRequired);
                
                // if the item was just loaded there's extra things to take into account
                // as the item might not even need a notification to the character it belongs to
                if (loadRequired == true)
                {
                    // trust that the notification got to the correct node
                    // load the item and check the owner, if it's logged in and the locationID is loaded by us
                    // that means the item should be kept here
                    if (this.ItemManager.TryGetItem(item.LocationID, out ItemEntity location) == false || this.CharacterManager.IsCharacterConnected(item.OwnerID) == false)
                    {
                        // this item should not be loaded, so unload and return
                        this.ItemManager.UnloadItem(item);
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
                        this.ItemManager.UnloadItem(item);
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

                if (item.LocationID == this.ItemManager.LocationRecycler.ID)
                    // the item is removed off the database if the new location is the recycler
                    item.Destroy();
                else if (item.LocationID == this.ItemManager.LocationMarket.ID)
                    // items that are moved to the market can be unloaded
                    this.ItemManager.UnloadItem(item);
                else
                    // save the item if the new location is not removal
                    item.Persist();    
            }
        }

        private void HandleOnBalanceUpdate(PyTuple data)
        {
            if (data.Count != 3)
            {
                Log.Error("Received OnBalanceUpdate notification with the wrong format");
                return;
            }

            PyDataType first = data[0];

            if (first is PyInteger == false)
            {
                Log.Error("Received OnBalanceUpdate notification with the wrong format");
                return;
            }

            PyDataType second = data[1];
            
            if (second is PyInteger == false)
            {
                Log.Error("Received OnBalanceUpdate notification with the wrong format");
                return;
            }

            PyDataType third = data[2];

            if (third is PyDecimal == false)
            {
                Log.Error("Received OnBalanceUpdate notification with the wrong format");
                return;
            }

            PyInteger characterID = first as PyInteger;
            PyInteger walletKey = second as PyInteger;
            PyDecimal newBalance = third as PyDecimal;
            
            if (this.ItemManager.TryGetItem(characterID, out Character character) == false)
            {
                Log.Warning("Received a wallet update for a character that does not belong to us...");
                return;
            }

            character.Balance = newBalance;
            character.Persist();
            
            this.NotificationManager.NotifyCharacter(characterID, new OnAccountChange(walletKey, characterID, newBalance));
        }

        private void HandleOnClusterTimer(PyTuple data)
        {
            Log.Info("Received a cluster request to run timed events on services...");
            
            this.ServiceManager.marketProxy.PerformTimedEvents();
        }

        private void HandleBroadcastNotification(PyPacket packet)
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
                case OnBalanceUpdate.NOTIFICATION_NAME:
                    this.HandleOnBalanceUpdate(packet.Payload[1] as PyTuple);
                    break;
                case "OnClusterTimer":
                    this.HandleOnClusterTimer(packet.Payload[1] as PyTuple);
                    break;
                default:
                    Log.Fatal("Received ClusterController notification with the wrong format");
                    break;
            }
        }

        private void HandleNotification(PyPacket packet, Client client)
        {
            if (packet.Source is PyAddressAny)
            {
                this.HandleBroadcastNotification(packet);
                return;
            }
            
            PyTuple callInfo = ((packet.Payload[0] as PyTuple)[1] as PySubStream).Stream as PyTuple;

            PyList objectIDs = callInfo[0] as PyList;
            string call = callInfo[1] as PyString;

            if (call != "ClientHasReleasedTheseObjects")
            {
                Log.Error($"Received notification from client with unknown method {call}");
                return;
            }
            
            // search for the given objects in the bound service
            // and sure they're freed
            foreach (PyTuple objectID in objectIDs.GetEnumerable<PyTuple>())
            {
                if (objectID[0] is PyString == false)
                {
                    Log.Fatal("Expected bound call with bound string, but got something different");
                    return;
                }

                string boundString = objectID[0] as PyString;

                // parse the bound string to get back proper node and bound ids
                Match regexMatch = Regex.Match(boundString, "N=([0-9]+):([0-9]+)");

                if (regexMatch.Groups.Count != 3)
                {
                    Log.Fatal($"Cannot find nodeID and boundID in the boundString {boundString}");
                    return;
                }

                int nodeID = int.Parse(regexMatch.Groups[1].Value);
                int boundID = int.Parse(regexMatch.Groups[2].Value);

                if (nodeID != this.Container.NodeID)
                {
                    Log.Fatal("Got a ClientHasReleasedTheseObjects call for an object ID that doesn't belong to us");
                    // TODO: MIGHT BE A GOOD IDEA TO RELAY THIS CALL TO THE CORRECT NODE
                    // TODO: INSIDE THE NETWORK, AT LEAST THAT'S WHAT CCP IS DOING BASED
                    // TODO: ON THE CLIENT'S CODE... NEEDS MORE INVESTIGATION
                    return;
                }
                
                this.BoundServiceManager.FreeBoundService(boundID);
            }
        }
        
        private void ReceiveNormalPacketCallback(PyDataType ar)
        {
            PyPacket packet = ar;

            this.ClientManager.TryGetClient(packet.UserID, out Client client);

            switch (packet.Type)
            {
                case PyPacket.PacketType.CALL_REQ:
                    this.HandleCallReq(packet, client);
                    break;
                case PyPacket.PacketType.SESSIONCHANGENOTIFICATION:
                    this.HandleSessionChangeNotification(packet, client);
                    break;
                case PyPacket.PacketType.PING_REQ:
                    this.HandlePingReq(packet, client);
                    break;
                case PyPacket.PacketType.CALL_RSP:
                    this.HandleCallRes(packet, client);
                    break;
                case PyPacket.PacketType.NOTIFICATION:
                    this.HandleNotification(packet, client);
                    break;
            }
            
            // send any notification that might be pending
            client?.SendPendingNotifications();
        }

        private static LowLevelVersionExchange CheckLowLevelVersionExchange(PyDataType exchange)
        {
            LowLevelVersionExchange data = exchange;

            if (data.Birthday != Common.Constants.Game.BIRTHDAY)
                throw new Exception("Wrong birthday in LowLevelVersionExchange");

            if (data.Build != Common.Constants.Game.BUILD)
                throw new Exception("Wrong build in LowLevelVersionExchange");

            if (data.Codename != Common.Constants.Game.CODENAME + "@" + Common.Constants.Game.REGION)
                throw new Exception("Wrong codename in LowLevelVersionExchange");

            if (data.MachoVersion != Common.Constants.Game.MACHO_VERSION)
                throw new Exception("Wrong machoVersion in LowLevelVersionExchange");

            if (data.Version != Common.Constants.Game.VERSION)
                throw new Exception("Wrong version in LowLevelVersionExchange");

            if (data.IsNode == true)
            {
                if (data.NodeIdentifier != "Node")
                    throw new Exception("Wrong node string in LowLevelVersionExchange");
            }
            
            return data;
        }

        public void SendProvisionalResponse(CallInformation answerTo, ProvisionalResponse response)
        {
            PyPacket result = new PyPacket(PyPacket.PacketType.CALL_RSP);
            
            // ensure destination has clientID in it
            PyAddressClient source = answerTo.From as PyAddressClient;

            source.ClientID = answerTo.Client.AccountID;
            // switch source and dest
            result.Source = answerTo.To;
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

            this.ClusterConnection.Send(result);
        }
        
        public void SendException(CallInformation answerTo, PyDataType content)
        {
            // build a new packet with the correct information
            PyPacket result = new PyPacket(PyPacket.PacketType.ERRORRESPONSE);
            
            // ensure destination has clientID in it
            PyAddressClient source = answerTo.From as PyAddressClient;

            source.ClientID = answerTo.Client.AccountID;
            // switch source and dest
            result.Source = answerTo.To;
            result.Destination = source;

            result.UserID = source.ClientID;
            result.Payload = new PyTuple(3)
            {
                [0] = (int) answerTo.PacketType,
                [1] = (int) MachoErrorType.WrappedException,
                [2] = new PyTuple (1) { [0] = new PySubStream(content) }, 
            };

            this.ClusterConnection.Send(result);
        }

        public void SendCallResult(CallInformation answerTo, PyDataType content)
        {
            // ensure destination has clientID in it
            PyAddressClient originalSource = answerTo.From as PyAddressClient;
            originalSource.ClientID = answerTo.Client.AccountID;

            this.ClusterConnection.Send(
                new PyPacket(PyPacket.PacketType.CALL_RSP)
                {
                    // switch source and dest
                    Source = answerTo.To,
                    Destination = originalSource,
                    UserID = originalSource.ClientID,
                    Payload = new PyTuple(1) {[0] = new PySubStream(content)}
                }
            );
        }

        private void NodeSendServiceCall(int nodeID, string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback, object extraInfo = null, int timeoutSeconds = 0)
        {
            // node's do not have notion of nodes so just let the cluster controller know
            int callID = this.ServiceManager.ExpectRemoteServiceResult(callback, nodeID, extraInfo, timeoutCallback, timeoutSeconds);
        }

        private void ClientSendServiceCall(int clientID, string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback, object extraInfo = null, int timeoutSeconds = 0)
        {
            if (this.ClientManager.Contains(clientID) == false)
                throw new InvalidDataException("Cannot send a service call to a userID that is not registered");

            Client destination = this.ClientManager.Get(clientID);
            
            // queue the call in the service manager and get the callID
            int callID = this.ServiceManager.ExpectRemoteServiceResult(callback, destination, extraInfo, timeoutCallback, timeoutSeconds);
            
            // prepare the request packet
            PyPacket packet = new PyPacket(PyPacket.PacketType.CALL_REQ);

            packet.UserID = clientID;
            packet.Destination = new PyAddressClient(clientID, null, service);
            packet.Source = new PyAddressNode(this.Container.NodeID, callID);
            packet.OutOfBounds = new PyDictionary();
            packet.OutOfBounds["role"] = (int) Roles.ROLE_SERVICE | (int) Roles.ROLE_REMOTESERVICE;
            packet.Payload = new PyTuple(2)
            {
                [0] = new PyTuple (2)
                {
                    [0] = 0,
                    [1] = new PySubStream(new PyTuple(4)
                    {
                        [0] = 1,
                        [1] = call,
                        [2] = args,
                        [3] = namedPayload
                    })
                },
                [1] = null
            };
            
            // everything is ready, send the packet to the client
            this.ClusterConnection.Send(packet);
        }

        public void SendServiceCall(Client client, string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0)
        {
            this.ClientSendServiceCall(client.AccountID, service, call, args, namedPayload, callback, timeoutCallback, extraInfo, timeoutSeconds);
        }

        public void SendServiceCall(int characterID, string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0)
        {
            this.ClientSendServiceCall(
                this.AccountDB.GetAccountIDFromCharacterID(characterID),
                service, call, args, namedPayload, callback, timeoutCallback, extraInfo, timeoutSeconds
            );
        }

        public void SendServiceCall(Character character, string service, string call, PyTuple args, PyDictionary namedPayload,
            Action<RemoteCall, PyDataType> callback, Action<RemoteCall> timeoutCallback = null, object extraInfo = null, int timeoutSeconds = 0)
        {
            this.SendServiceCall(character.ID, service, call, args, namedPayload, callback,timeoutCallback, extraInfo, timeoutSeconds);
        }
    }
}