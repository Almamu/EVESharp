using System;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using EVESharp.Common.Logging;
using EVESharp.Common.Network;
using EVESharp.EVE;
using EVESharp.EVE.Packets;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Exceptions.contractMgr;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Inventory.SystemEntities;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.Notifications.Nodes.Corporations;
using EVESharp.Node.Notifications.Nodes.Corps;
using EVESharp.Node.Services.Corporations;
using EVESharp.PythonTypes;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class MachoProxyTransport : MachoTransport
{
    private MachoNet MachoNet => this.Server.MachoNet;
    
    public MachoProxyTransport(MachoTransport source) : base(source)
    {
        this.Socket.SetReceiveCallback(HandleProxyPacket);
        this.SendPostAuthenticationPackets();
    }

    private void HandleProxyPacket(PyDataType data)
    {
        // these should directly be PyPackets
        PyPacket packet = data;

        switch (packet.Type)
        {
            case PyPacket.PacketType.NOTIFICATION:
                this.HandleNotification(packet);
                break;
            case PyPacket.PacketType.SESSIONCHANGENOTIFICATION:
                this.HandleSessionChangeNotification(packet);
                break;
            case PyPacket.PacketType.CALL_REQ:
                this.HandleCallReq(packet);
                break;
        }
    }

    private void HandleCallReq(PyPacket packet)
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

            if (destNode.NodeID != this.Server.MachoNet.Container.NodeID)
            {
                Log.Fatal(
                    "Received a call request for a node that is not us, did the ClusterController get confused or something?!"
                );
                return;
            }
        }

        callInformation = new CallInformation
        {
            CallID = source.CallID,
            Payload = args,
            NamedPayload = sub,
            Source = packet.Source,
            Destination = packet.Destination,
            MachoNet = this.Server.MachoNet,
            Session = Session.FromPyDictionary(packet.OutOfBounds["Session"] as PyDictionary),
            ResutOutOfBounds = new PyDictionary<PyString, PyDataType>()
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

                if (nodeID != this.Server.MachoNet.Container.NodeID)
                {
                    Log.Fatal("Got bound service call for a different node");
                    // TODO: MIGHT BE A GOOD IDEA TO RELAY THIS CALL TO THE CORRECT NODE
                    // TODO: INSIDE THE NETWORK, AT LEAST THAT'S WHAT CCP IS DOING BASED
                    // TODO: ON THE CLIENT'S CODE... NEEDS MORE INVESTIGATION
                    return;
                }

                callResult = this.Server.MachoNet.BoundServiceManager.ServiceCall(
                    boundID, call, callInformation
                );
            }
            else
            {
                Log.Trace($"Calling {destinationService}::{call}");

                callResult = this.Server.MachoNet.ServiceManager.ServiceCall(
                    destinationService, call, callInformation
                );

            }

            this.MachoNet.SendCallResult(callInformation, callResult, callInformation.ResutOutOfBounds);
        }
        catch (PyException e)
        {
            this.MachoNet.SendException(callInformation, packet.Type, e);
        }
        catch (ProvisionalResponse provisional)
        {
            this.MachoNet.SendProvisionalResponse(callInformation, provisional);
        }
        catch (Exception ex)
        {
            int errorID = ++this.Server.MachoNet.ErrorCount;

            Log.Fatal($"Detected non-client exception on call to {destinationService}::{call}, registered as error {errorID}. Extra information: ");
            Log.Fatal(ex.Message);
            Log.Fatal(ex.StackTrace);
            
            // send client a proper notification about the error based on the roles
            if ((callInformation.Session.Role & (int) Roles.ROLE_PROGRAMMER) == (int) Roles.ROLE_PROGRAMMER)
            {
                this.MachoNet.SendException(callInformation, packet.Type, new CustomError($"An internal server error occurred.<br><b>Reference</b>: {errorID}<br><b>Message</b>: {ex.Message}<br><b>Stack trace</b>:<br>{ex.StackTrace.Replace("\n", "<br>")}"));
            }
            else
            {
                this.MachoNet.SendException(callInformation, packet.Type, new CustomError($"An internal server error occurred. <b>Reference</b>: {errorID}"));
            }
        }
    }

    private void HandleSessionChangeNotification(PyPacket packet)
    {
        SessionChangeNotification scn = packet.Payload;
        
        // look in all the bound services for the given client
        foreach ((int _, BoundService service) in this.MachoNet.BoundServiceManager.BoundServices)
            service.ApplySessionChange(packet.UserID, scn.Changes);
    }

    private void HandleClientNotification(PyPacket packet)
    {
        // this notification is only for ClientHasReleasedTheseObjects
        // TODO: HANDLE THIS
    }
    
    private void HandleNotification(PyPacket packet)
    {
        Log.Debug(PrettyPrinter.FromDataType(packet));
        
        if (packet.Source is PyAddressClient)
        {
            this.HandleClientNotification(packet);
            return;
        }
        
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
        this.MachoNet.SystemManager.LoadSolarSystemOnCluster(solarSystemID);
    }

    private void HandleOnItemUpdate(Notifications.Nodes.Inventory.OnItemChange change)
    {
        foreach ((PyInteger itemID, PyDictionary _changes) in change.Updates)
        {
            PyDictionary<PyString, PyTuple> changes = _changes.GetEnumerable<PyString, PyTuple>();
            
            ItemEntity item = this.MachoNet.ItemFactory.LoadItem(itemID, out bool loadRequired);
            
            // if the item was just loaded there's extra things to take into account
            // as the item might not even need a notification to the character it belongs to
            if (loadRequired == true)
            {
                // trust that the notification got to the correct node
                // load the item and check the owner, if it's logged in and the locationID is loaded by us
                // that means the item should be kept here
                if (this.MachoNet.ItemFactory.TryGetItem(item.LocationID, out ItemEntity location) == false)
                    return;

                bool locationBelongsToUs = true;

                switch (location)
                {
                    case Station _:
                        locationBelongsToUs = this.MachoNet.SystemManager.StationBelongsToUs(location.ID);
                        break;
                    case SolarSystem _:
                        locationBelongsToUs = this.MachoNet.SystemManager.SolarSystemBelongsToUs(location.ID);
                        break;
                }

                if (locationBelongsToUs == false)
                {
                    this.MachoNet.ItemFactory.UnloadItem(item);
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
            this.MachoNet.NotificationManager.NotifyCharacter(item.OwnerID, "OnMultiEvent", 
                new PyTuple(1) {[0] = new PyList(1) {[0] = itemChange}});

            if (item.LocationID == this.MachoNet.ItemFactory.LocationRecycler.ID)
                // the item is removed off the database if the new location is the recycler
                item.Destroy();
            else if (item.LocationID == this.MachoNet.ItemFactory.LocationMarket.ID)
                // items that are moved to the market can be unloaded
                this.MachoNet.ItemFactory.UnloadItem(item);
            else
                // save the item if the new location is not removal
                item.Persist();    
        }
    }

    private void HandleOnClusterTimer(PyTuple data)
    {
        Log.Info("Received a cluster request to run timed events on services...");

        // this.OnClusterTimer?.Invoke(this, null);
    }

    private void HandleOnCorporationMemberChanged(OnCorporationMemberChanged change)
    {
        // this notification does not need to send anything to anyone as the clients will already get notified
        // based on their corporation IDs
        
        if (this.MachoNet.ServiceManager.corpRegistry.FindInstanceForObjectID(change.OldCorporationID, out corpRegistry oldService) == true && oldService.MembersSparseRowset is not null)
            oldService.MembersSparseRowset.RemoveRow(change.MemberID);

        if (this.MachoNet.ServiceManager.corpRegistry.FindInstanceForObjectID(change.NewCorporationID, out corpRegistry newService) == true && newService.MembersSparseRowset is not null)
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
        if (this.MachoNet.ItemFactory.TryGetItem(change.MemberID, out Inventory.Items.Types.Character character) == false)
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
        if (this.MachoNet.ItemFactory.TryGetItem(change.CharacterID, out Inventory.Items.Types.Character character) == false)
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
        if (this.MachoNet.ItemFactory.TryGetItem(change.CorporationID, out Corporation corporation) == false)
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
}