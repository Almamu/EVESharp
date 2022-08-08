using System;
using System.Text.RegularExpressions;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Network;
using EVESharp.EVE.Network.Messages;
using EVESharp.EVE.Network.Transports;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Notifications.Nodes.Corps;
using EVESharp.Node.Services;
using EVESharp.Node.Services.Corporations;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;
using OnItemChange = EVESharp.Node.Notifications.Nodes.Inventory.OnItemChange;

namespace EVESharp.Node.Server.Shared.Handlers;

public class LocalNotificationHandler
{
    public IMachoNet           MachoNet            { get; }
    public ILogger             Log                 { get; }
    public BoundServiceManager BoundServiceManager { get; }
    public ServiceManager      ServiceManager      { get; }
    public IItems         Items         { get; }
    public ISolarSystems       SolarSystems       { get; }
    public INotificationSender  Notifications       { get; }
    public ISessionManager      SessionManager      { get; }

    public LocalNotificationHandler (
        IMachoNet machoNet, ILogger logger, ServiceManager serviceManager, BoundServiceManager boundServiceManager, IItems items, ISolarSystems solarSystems,
        INotificationSender notificationSender, ISessionManager sessionManager
    )
    {
        MachoNet            = machoNet;
        ServiceManager      = serviceManager;
        BoundServiceManager = boundServiceManager;
        this.Items          = items;
        this.SolarSystems        = solarSystems;
        Notifications       = notificationSender;
        SessionManager      = sessionManager;
        Log                 = logger;
    }

    private void HandleClientNotification (MachoMessage message)
    {
        PyPacket packet = message.Packet;
        // this notification is only for ClientHasReleasedTheseObjects
        PyTuple callInfo = ((packet.Payload [0] as PyTuple) [1] as PySubStream).Stream as PyTuple;

        PyList objectIDs = callInfo [0] as PyList;
        string call      = callInfo [1] as PyString;

        if (call != "ClientHasReleasedTheseObjects")
        {
            Log.Error ($"Received notification from client with unknown method {call}");

            return;
        }

        Session session;

        if (message.Transport is MachoClientTransport)
            session = message.Transport.Session;
        else
            session = Session.FromPyDictionary (packet.OutOfBounds ["Session"] as PyDictionary);

        // search for the given objects in the bound service
        // and sure they're freed
        foreach (PyTuple objectID in objectIDs.GetEnumerable <PyTuple> ())
        {
            if (objectID [0] is PyString == false)
            {
                Log.Fatal ("Expected bound call with bound string, but got something different");

                return;
            }

            string boundString = objectID [0] as PyString;

            // parse the bound string to get back proper node and bound ids
            Match regexMatch = Regex.Match (boundString, "N=([0-9]+):([0-9]+)");

            if (regexMatch.Groups.Count != 3)
            {
                Log.Fatal ($"Cannot find nodeID and boundID in the boundString {boundString}");

                return;
            }

            int nodeID  = int.Parse (regexMatch.Groups [1].Value);
            int boundID = int.Parse (regexMatch.Groups [2].Value);

            if (nodeID == MachoNet.NodeID)
                BoundServiceManager.ClientHasReleasedThisObject (boundID, session);
        }
    }

    public void HandleNotification (MachoMessage machoMessage)
    {
        PyPacket packet = machoMessage.Packet;

        if (packet.Source is PyAddressClient)
        {
            this.HandleClientNotification (machoMessage);

            return;
        }

        // this packet is an internal one
        if (packet.Payload.Count != 2)
        {
            Log.Error ("Received ClusterController notification with the wrong format");

            return;
        }

        if (packet.Payload [0] is not PyString notification)
        {
            Log.Error ("Received ClusterController notification with the wrong format");

            return;
        }

        Log.Debug ("Received a notification from ClusterController of type {notification}", notification);

        switch (notification)
        {
            case "UpdateSessionAttributes":
                this.HandleUpdateSessionAttributes (packet.Payload[1] as PyTuple);
                break;

            case "OnSolarSystemLoad":
                this.HandleOnSolarSystemLoaded (packet.Payload [1] as PyTuple);
                break;

            case OnItemChange.NOTIFICATION_NAME:
                this.HandleOnItemUpdate (packet.Payload);
                break;

            case OnCorporationMemberChanged.NOTIFICATION_NAME:
                this.HandleOnCorporationMemberChanged (packet.Payload);
                break;

            case OnCorporationMemberUpdated.NOTIFICATION_NAME:
                this.HandleOnCorporationMemberUpdated (packet.Payload);
                break;

            case OnCorporationChanged.NOTIFICATION_NAME:
                this.HandleOnCorporationChanged (packet.Payload);
                break;

            
            case OnCorporationOfficeRented.NOTIFICATION_NAME:
                this.HandleOnCorporationOfficeRented (packet.Payload);
                break;
            
            case "ClientHasDisconnected":
                this.HandleClientHasDisconnected (packet.Payload [1] as PyTuple, packet.OutOfBounds);
                break;

            default:
                Log.Fatal ("Received ClusterController notification with the wrong format");
                break;

        }
    }

    private void HandleUpdateSessionAttributes (PyTuple payload)
    {
        // very simple version for now, should properly handle these sometime in the future
        PyString     idType    = payload [0] as PyString;
        PyInteger    id        = payload [1] as PyInteger;
        PyDictionary newValues = payload [2] as PyDictionary;

        SessionManager.PerformSessionUpdate (idType, id, Session.FromPyDictionary (newValues));
    }
    
    private void HandleOnSolarSystemLoaded (PyTuple data)
    {
        if (data.Count != 1)
        {
            Log.Error ("Received OnSolarSystemLoad notification with the wrong format");

            return;
        }

        PyDataType first = data [0];

        if (first is PyInteger == false)
        {
            Log.Error ("Received OnSolarSystemLoad notification with the wrong format");

            return;
        }

        PyInteger solarSystemID = first as PyInteger;

        // mark as loaded
        this.SolarSystems.LoadSolarSystemOnCluster (solarSystemID);
    }

    private void HandleOnItemUpdate (OnItemChange change)
    {
        foreach ((PyInteger itemID, PyDictionary _changes) in change.Updates)
        {
            PyDictionary <PyString, PyTuple> changes = _changes.GetEnumerable <PyString, PyTuple> ();

            ItemEntity item = this.Items.LoadItem (itemID, out bool loadRequired);

            // if the item was just loaded there's extra things to take into account
            // as the item might not even need a notification to the character it belongs to
            if (loadRequired)
            {
                // trust that the notification got to the correct node
                // load the item and check the owner, if it's logged in and the locationID is loaded by us
                // that means the item should be kept here
                if (this.Items.TryGetItem (item.LocationID, out ItemEntity location) == false)
                    return;

                bool locationBelongsToUs = true;

                switch (location)
                {
                    case Station _:
                        locationBelongsToUs = this.SolarSystems.StationBelongsToUs (location.ID);
                        break;

                    case SolarSystem _:
                        locationBelongsToUs = this.SolarSystems.SolarSystemBelongsToUs (location.ID);
                        break;

                }

                if (locationBelongsToUs == false)
                {
                    this.Items.UnloadItem (item);

                    return;
                }
            }

            EVE.Notifications.Inventory.OnItemChange itemChange = new EVE.Notifications.Inventory.OnItemChange (item);

            // update item and build change notification
            if (changes.TryGetValue ("locationID", out PyTuple locationChange))
            {
                PyInteger oldValue = locationChange [0] as PyInteger;
                PyInteger newValue = locationChange [1] as PyInteger;

                itemChange.AddChange (ItemChange.LocationID, oldValue);
                item.LocationID = newValue;
            }

            if (changes.TryGetValue ("quantity", out PyTuple quantityChange))
            {
                PyInteger oldValue = quantityChange [0] as PyInteger;
                PyInteger newValue = quantityChange [1] as PyInteger;

                itemChange.AddChange (ItemChange.Quantity, oldValue);
                item.Quantity = newValue;
            }

            if (changes.TryGetValue ("ownerID", out PyTuple ownerChange))
            {
                PyInteger oldValue = ownerChange [0] as PyInteger;
                PyInteger newValue = ownerChange [1] as PyInteger;

                itemChange.AddChange (ItemChange.OwnerID, oldValue);
                item.OwnerID = newValue;
            }

            if (changes.TryGetValue ("singleton", out PyTuple singletonChange))
            {
                PyBool oldValue = singletonChange [0] as PyBool;
                PyBool newValue = singletonChange [1] as PyBool;

                itemChange.AddChange (ItemChange.Singleton, oldValue);
                item.Singleton = newValue;
            }

            // TODO: IDEALLY THIS WOULD BE ENQUEUED SO ALL OF THEM ARE SENT AT THE SAME TIME
            // TODO: BUT FOR NOW THIS SHOULD SUFFICE
            // send the notification
            Notifications.NotifyCharacter (
                item.OwnerID, "OnMultiEvent",
                new PyTuple (1) {[0] = new PyList (1) {[0] = itemChange}}
            );

            if (item.LocationID == this.Items.LocationRecycler.ID)
                // the item is removed off the database if the new location is the recycler
                item.Destroy ();
            else if (item.LocationID == this.Items.LocationMarket.ID)
                // items that are moved to the market can be unloaded
                this.Items.UnloadItem (item);
            else
                // save the item if the new location is not removal
                item.Persist ();
        }
    }

    private void HandleOnCorporationMemberChanged (OnCorporationMemberChanged change)
    {
        // this notification does not need to send anything to anyone as the clients will already get notified
        // based on their corporation IDs
        if (ServiceManager.corpRegistry.FindInstanceForObjectID (change.OldCorporationID, out corpRegistry oldService) &&
            oldService.MembersSparseRowset is not null)
            oldService.MembersSparseRowset.RemoveRow (change.MemberID);

        if (ServiceManager.corpRegistry.FindInstanceForObjectID (change.NewCorporationID, out corpRegistry newService) &&
            newService.MembersSparseRowset is not null)
        {
            PyDictionary <PyString, PyTuple> changes = new PyDictionary <PyString, PyTuple>
            {
                ["characterID"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = change.MemberID
                },
                ["title"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = ""
                },
                ["startDateTime"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = DateTime.UtcNow.ToFileTimeUtc ()
                },
                ["roles"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["rolesAtHQ"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["rolesAtBase"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["rolesAtOther"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["titleMask"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["grantableRoles"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["grantableRolesAtHQ"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["grantableRolesAtBase"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["grantableRolesAtOther"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["divisionID"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["squadronID"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["baseID"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["blockRoles"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                },
                ["gender"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = 0
                }
            };

            newService.MembersSparseRowset.AddRow (change.MemberID, changes);
        }

        // the only thing needed is to check for a Character reference and update it's corporationID to the correct one
        if (this.Items.TryGetItem (change.MemberID, out Character character) == false)
            // if the character is not loaded it could mean that the package arrived on to the node while the player was logging out
            // so this is safe to ignore 
            return;

        // set the corporation to the new one
        character.CorporationID = change.NewCorporationID;
        // this change usually means that the character is now in a new corporation, so everything corp-related is back to 0
        character.Roles                 = 0;
        character.RolesAtBase           = 0;
        character.RolesAtHq             = 0;
        character.RolesAtOther          = 0;
        character.TitleMask             = 0;
        character.GrantableRoles        = 0;
        character.GrantableRolesAtBase  = 0;
        character.GrantableRolesAtOther = 0;
        character.GrantableRolesAtHQ    = 0;
        // persist the character
        character.Persist ();
        // nothing else needed
        Log.Debug ($"Updated character ({character.ID}) coporation ID from {change.OldCorporationID} to {change.NewCorporationID}");
    }

    private void HandleOnCorporationMemberUpdated (OnCorporationMemberUpdated change)
    {
        // this notification does not need to send anything to anyone as the clients will already get notified
        // by the session change

        // the only thing needed is to check for a Character reference and update it's roles to the correct onews
        if (this.Items.TryGetItem (change.CharacterID, out Character character) == false)
            // if the character is not loaded it could mean that the package arrived on the node wile the player was logging out
            // so this is safe to ignore
            return;

        // update the roles and everything else
        character.Roles                 = change.Roles;
        character.RolesAtBase           = change.RolesAtBase;
        character.RolesAtHq             = change.RolesAtHQ;
        character.RolesAtOther          = change.RolesAtOther;
        character.GrantableRoles        = change.GrantableRoles;
        character.GrantableRolesAtBase  = change.GrantableRolesAtBase;
        character.GrantableRolesAtOther = change.GrantableRolesAtOther;
        character.GrantableRolesAtHQ    = change.GrantableRolesAtHQ;
        // some debugging is well received
        Log.Debug ($"Updated character ({character.ID}) roles");
    }

    private void HandleOnCorporationChanged (OnCorporationChanged change)
    {
        // this notification does not need to send anything to anyone as the clients will already get notified
        // by the session change

        // the only thing needed is to check for a Corporation reference and update it's alliance information to the new values
        if (this.Items.TryGetItem (change.CorporationID, out Corporation corporation) == false)
            // if the corporation is not loaded it could mean that the package arrived on the node wile the last player was logging out
            // so this is safe to ignore
            return;

        // update basic information
        corporation.AllianceID     = change.AllianceID;
        corporation.ExecutorCorpID = change.ExecutorCorpID;
        corporation.StartDate      = change.AllianceID is null ? null : DateTime.UtcNow.ToFileTimeUtc ();
        corporation.Persist ();

        // some debugging is well received
        Log.Debug ($"Updated corporation affiliation ({corporation.ID}) to ({corporation.AllianceID})");
    }

    private void HandleOnCorporationOfficeRented (OnCorporationOfficeRented change)
    {
        if (ServiceManager.corpRegistry.FindInstanceForObjectID (change.CorporationID, out corpRegistry newService) &&
            newService.OfficesSparseRowset is not null)
        {
            PyDictionary <PyString, PyTuple> changes = new PyDictionary <PyString, PyTuple>
            {
                ["officeID"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = change.OfficeFolderID
                },
                ["stationID"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = change.StationID
                },
                ["typeID"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = change.TypeID
                },
                ["officeFolderID"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = change.OfficeFolderID
                }
            };

            newService.OfficesSparseRowset.AddRow (change.OfficeFolderID, changes);
        }
    }

    private void HandleClientHasDisconnected (PyTuple data, PyDictionary oob)
    {
        // unbind the player from all the services
        BoundServiceManager.OnClientDisconnected (Session.FromPyDictionary (oob ["Session"] as PyDictionary));
    }
}