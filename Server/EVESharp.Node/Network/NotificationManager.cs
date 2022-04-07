﻿using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.Node.Dogma;
using EVESharp.Node.StaticData.Corporation;
using EVESharp.Node.Inventory;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Nodes;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Network;

public class NotificationManager
{
    /// <summary>
    /// idType to use when notificating characters
    /// </summary>
    public const string NOTIFICATION_TYPE_CHARACTER = "charid";
    /// <summary>
    /// idType to use when notificating corporations
    /// </summary>
    public const string NOTIFICATION_TYPE_CORPORATON = "corpid";
    /// <summary>
    /// idType to use when notificating stations
    /// </summary>
    public const string NOTIFICATION_TYPE_STATION = "stationid";
    /// <summary>
    /// idType to use when notificating owners (corporation, character, alliances...)
    /// </summary>
    public const string NOTIFICATION_TYPE_OWNER = "ownerid";
    /// <summary>
    /// idType to use when notificating owners (corporation, character, alliances...) at a specific station
    /// </summary>
    public const string NOTIFICATION_TYPE_OWNER_LOCATIONID = "ownerid&locationid";
    /// <summary>
    /// idType to use when notificating corporation members based on role
    /// </summary>
    public const string NOTIFICATION_TYPE_CORPORATION_ROLE = "corpid&corprole";
    /// <summary>
    /// idType to use when notificating corporation members based on role
    /// </summary>
    public const string NOTIFICATION_TYPE_ALLIANCE = "allianceid";
        
    /// <summary>
    /// The node this notification manager belongs to
    /// </summary>
    public IMachoNet MachoNet { get; }
        
    public NotificationManager(IMachoNet machoNet)
    {
        this.MachoNet = machoNet;
    }

    public void NotifyCharacters(PyList<PyInteger> characterIDs, string type, PyTuple notification)
    {
        // multiple notifications are not checked as the traffic generated by these is usually less
        // inside our network
        this.SendNotification(type, NOTIFICATION_TYPE_CHARACTER, characterIDs, notification);
    }

    public void NotifyCharacter(int characterID, string type, PyTuple notification)
    {
        // do not waste network resources on useless notifications
        this.SendNotification(type, NOTIFICATION_TYPE_CHARACTER, characterID, notification);
    }

    public void NotifyCharacters(PyList<PyInteger> characterIDs, ClientNotification notification)
    {
        this.SendNotification(NOTIFICATION_TYPE_CHARACTER, characterIDs, notification);
    }
        
    public void NotifyCharacter(int characterID, ClientNotification entry)
    {
        // build a proper notification for this
        this.SendNotification(NOTIFICATION_TYPE_CHARACTER, characterID, entry);
    }

    public void NotifyOwner(int ownerID, ClientNotification entry)
    {
        this.SendNotification(NOTIFICATION_TYPE_OWNER, ownerID, entry);
    }

    public void NotifyOwners(PyList<PyInteger> ownerIDs, ClientNotification notification)
    {
        this.SendNotification(NOTIFICATION_TYPE_OWNER, ownerIDs, notification);
    }

    public void NotifyOwnerAtLocation(int ownerID, int locationID, ClientNotification entry)
    {
        this.SendNotification(NOTIFICATION_TYPE_OWNER_LOCATIONID, new PyTuple(2) {[0] = ownerID, [1] = locationID}, entry);
    }
    public void NotifyCorporation(int corporationID, string type, PyTuple notification)
    {
        this.SendNotification(type, NOTIFICATION_TYPE_CORPORATON, corporationID, notification);
    }

    public void NotifyCorporation(int corporationID, ClientNotification notification)
    {
        this.SendNotification(NOTIFICATION_TYPE_CORPORATON, corporationID, notification);
    }

    public void NotifyStation(int stationID, string type, PyTuple notification)
    {
        this.SendNotification(type, NOTIFICATION_TYPE_STATION, stationID, notification);
    }

    public void NotifyStation(int stationID, ClientNotification notification)
    {
        this.SendNotification(NOTIFICATION_TYPE_STATION, stationID, notification);
    }
        
    public void NotifyAlliance(int allianceID, ClientNotification notification)
    {
        this.SendNotification(NOTIFICATION_TYPE_ALLIANCE, allianceID, notification);
    }

    public void NotifyCorporationByRole(int corporationID, long roleMask, ClientNotification notification)
    {
        this.SendNotification(NOTIFICATION_TYPE_CORPORATION_ROLE, new PyTuple(2) {[0] = corporationID, [1] = roleMask}, notification);
    }
        
    public void NotifyCorporationByRole(int corporationID, CorporationRole role, ClientNotification notification)
    {
        this.NotifyCorporationByRole(corporationID, (long) role, notification);
    }

    /// <summary>
    /// Send a notification to the given node
    /// </summary>
    /// <param name="nodeID">The node to notify</param>
    /// <param name="notification">The notification to send</param>
    public void NotifyNode(long nodeID, InterNodeNotification notification)
    {
        // do not notify if the notification is for a non-existant node (nodeID = 0)
        if (nodeID == 0)
            return;
            
        PyPacket packet = new PyPacket(PyPacket.PacketType.NOTIFICATION)
        {
            Source      = new PyAddressAny(0),
            Destination = new PyAddressBroadcast(new PyList(1) {[0] = nodeID}, "nodeid"),
            Payload     = notification,
            OutOfBounds = new PyDictionary(),
                
            // set the userID to -1, this will indicate the cluster controller to fill it in
            UserID = -1
        };

        this.MachoNet.QueueOutputPacket(packet);
    }

    public void SendNotification(string idType, int id, ClientNotification data)
    {
        this.SendNotification(data.NotificationName, idType, new PyList(1) {[0] = id}, data.GetElements());
    }

    public void SendNotification(string idType, PyTuple id, ClientNotification data)
    {
        this.SendNotification(data.NotificationName, idType, new PyList(1) { [0] = id}, data.GetElements());
    }

    public void SendNotification(string notificationType, string idType, int id, PyTuple data)
    {
        this.SendNotification(notificationType, idType, new PyList(1) {[0] = id}, data);
    }

    public void SendNotification(string idType, PyList idsOfInterest, ClientNotification data)
    {
        this.SendNotification(data.NotificationName, idType, idsOfInterest, data.GetElements());
    }
        
    public void SendNotification(string notificationType, string idType, PyList idsOfInterest, PyTuple data)
    {
        PyTuple dataContainer = new PyTuple(2)
        {
            [0] = 1, // gpcs.ObjectCall::ObjectCall
            [1] = data
        };

        dataContainer = new PyTuple(2)
        {
            [0] = 0, // gpcs.ServiceCall::NotifyDown
            [1] = dataContainer
        };

        dataContainer = new PyTuple(2)
        {
            [0] = 0, // gpcs.ObjectCall::NotifyDown
            [1] = new PySubStream(dataContainer)
        };

        dataContainer = new PyTuple(2)
        {
            [0] = dataContainer,
            [1] = null
        };

        PyPacket packet = new PyPacket(PyPacket.PacketType.NOTIFICATION)
        {
            Destination = new PyAddressBroadcast(idsOfInterest, idType, notificationType),
            Source      = new PyAddressNode(this.MachoNet.NodeID),
                
            // set the userID to -1, this will indicate the cluster controller to fill it in
            UserID  = -1,
            Payload = dataContainer
        };

        this.MachoNet.QueueOutputPacket(packet);
    }
}