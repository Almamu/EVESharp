using System.Collections.Generic;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Network;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Notifications;

public class NotificationSender
{
    /// <summary>
    /// Translates the NotificationIdType enumeration to their string values
    /// </summary>
    public static readonly Dictionary <NotificationIdType, string> NotificationTypeTranslation = new Dictionary <NotificationIdType, string>
    {
        [NotificationIdType.Character]          = Session.CHAR_ID,
        [NotificationIdType.Corporation]        = Session.CORP_ID,
        [NotificationIdType.Station]            = Session.STATION_ID,
        [NotificationIdType.Owner]              = "ownerid",
        [NotificationIdType.OwnerAndLocation]   = "ownerid&" + Session.LOCATION_ID,
        [NotificationIdType.CorporationAndRole] = Session.CORP_ID + "&" + Session.CORP_ROLE,
        [NotificationIdType.Alliance]           = Session.ALLIANCE_ID
    };

    /// <summary>
    /// Translates the session key name into how it has to be compared for notifications
    /// </summary>
    public static readonly Dictionary <string, ComparisonType> NotificationComparison = new Dictionary <string, ComparisonType>
    {
        [Session.CORP_ROLE] = ComparisonType.Bitmask,
    };

    /// <summary>
    /// The node this notification manager belongs to
    /// </summary>
    public IMachoNet MachoNet { get; }

    public NotificationSender (IMachoNet machoNet)
    {
        MachoNet = machoNet;
    }

    public void NotifyCharacters (PyList <PyInteger> characterIDs, string type, PyTuple notification)
    {
        this.SendNotification (type, NotificationIdType.Character, characterIDs, notification);
    }

    public void NotifyCharacter (int characterID, string type, PyTuple notification)
    {
        this.SendNotification (type, NotificationIdType.Character, characterID, notification);
    }

    public void NotifyCharacters (PyList <PyInteger> characterIDs, ClientNotification notification)
    {
        this.SendNotification (NotificationIdType.Character, characterIDs, notification);
    }

    public void NotifyCharacter (int characterID, ClientNotification entry)
    {
        // build a proper notification for this
        this.SendNotification (NotificationIdType.Character, characterID, entry);
    }

    public void NotifyOwner (int ownerID, ClientNotification entry)
    {
        this.SendNotification (NotificationIdType.Owner, ownerID, entry);
    }

    public void NotifyOwners (PyList <PyInteger> ownerIDs, ClientNotification notification)
    {
        this.SendNotification (NotificationIdType.Owner, ownerIDs, notification);
    }

    public void NotifyOwnerAtLocation (int ownerID, int locationID, ClientNotification entry)
    {
        this.SendNotification (
            NotificationIdType.OwnerAndLocation, new PyTuple (2)
            {
                [0] = ownerID,
                [1] = locationID
            }, entry
        );
    }

    public void NotifyCorporation (int corporationID, string type, PyTuple notification)
    {
        this.SendNotification (type, NotificationIdType.Corporation, corporationID, notification);
    }

    public void NotifyCorporation (int corporationID, ClientNotification notification)
    {
        this.SendNotification (NotificationIdType.Corporation, corporationID, notification);
    }

    public void NotifyStation (int stationID, string type, PyTuple notification)
    {
        this.SendNotification (type, NotificationIdType.Station, stationID, notification);
    }

    public void NotifyStation (int stationID, ClientNotification notification)
    {
        this.SendNotification (NotificationIdType.Station, stationID, notification);
    }

    public void NotifyAlliance (int allianceID, ClientNotification notification)
    {
        this.SendNotification (NotificationIdType.Alliance, allianceID, notification);
    }

    public void NotifyCorporationByRole (int corporationID, long roleMask, ClientNotification notification)
    {
        this.SendNotification (
            NotificationIdType.CorporationAndRole, new PyTuple (2)
            {
                [0] = corporationID,
                [1] = roleMask
            }, notification
        );
    }

    public void NotifyCorporationByRole (int corporationID, CorporationRole role, ClientNotification notification)
    {
        this.NotifyCorporationByRole (corporationID, (long) role, notification);
    }

    /// <summary>
    /// Send a notification to the given node
    /// </summary>
    /// <param name="nodeID">The node to notify</param>
    /// <param name="notification">The notification to send</param>
    public void NotifyNode (long nodeID, InterNodeNotification notification)
    {
        // do not notify if the notification is for a non-existant node (nodeID = 0)
        if (nodeID == 0)
            return;

        PyPacket packet = new PyPacket (PyPacket.PacketType.NOTIFICATION)
        {
            Source      = new PyAddressAny (0),
            Destination = new PyAddressNode (nodeID),
            Payload     = notification,
            OutOfBounds = new PyDictionary (),

            // set the userID to -1, this will indicate the cluster controller to fill it in
            UserID = -1
        };

        MachoNet.QueueOutputPacket (packet);
    }

    public void SendNotification (NotificationIdType idType, int id, ClientNotification data)
    {
        this.SendNotification (NotificationTypeTranslation [idType], id, data);
    }

    public void SendNotification (NotificationIdType idType, PyTuple ids, ClientNotification data)
    {
        this.SendNotification (NotificationTypeTranslation [idType], ids, data);
    }

    public void SendNotification (string idType, int id, ClientNotification data)
    {
        this.SendNotification (data.NotificationName, idType, new PyList (1) {[0] = id}, data.GetElements ());
    }

    public void SendNotification (string idType, PyTuple id, ClientNotification data)
    {
        this.SendNotification (data.NotificationName, idType, new PyList (1) {[0] = id}, data.GetElements ());
    }

    public void SendNotification (string notificationType, string idType, int id, PyTuple data)
    {
        this.SendNotification (notificationType, idType, new PyList (1) {[0] = id}, data);
    }

    public void SendNotification (string notificationType, NotificationIdType idType, int id, PyTuple data)
    {
        this.SendNotification (notificationType, NotificationTypeTranslation [idType], new PyList (1) {[0] = id}, data);
    }

    public void SendNotification (string notificationType, NotificationIdType idType, int id, ClientNotification data)
    {
        this.SendNotification (notificationType, NotificationTypeTranslation [idType], id, data.GetElements ());
    }

    public void SendNotification (string idType, PyList idsOfInterest, ClientNotification data)
    {
        this.SendNotification (data.NotificationName, idType, idsOfInterest, data.GetElements ());
    }

    public void SendNotification (NotificationIdType idType, PyList idsOfInterest, ClientNotification data)
    {
        this.SendNotification (data.NotificationName, NotificationTypeTranslation [idType], idsOfInterest, data.GetElements ());
    }

    public void SendNotification (string notificationType, NotificationIdType idType, PyList idsOfInterest, ClientNotification data)
    {
        this.SendNotification (notificationType, NotificationTypeTranslation [idType], idsOfInterest, data.GetElements ());
    }

    public void SendNotification (string notificationType, NotificationIdType idType, PyList idsOfInterest, PyTuple data)
    {
        this.SendNotification (notificationType, NotificationTypeTranslation [idType], idsOfInterest, data);
    }

    public void SendNotification (string notificationType, string idType, PyList idsOfInterest, PyTuple data)
    {
        PyTuple dataContainer = new PyTuple (2)
        {
            [0] = 1, // gpcs.ObjectCall::ObjectCall
            [1] = data
        };

        dataContainer = new PyTuple (2)
        {
            [0] = 0, // gpcs.ServiceCall::NotifyDown
            [1] = dataContainer
        };

        dataContainer = new PyTuple (2)
        {
            [0] = 0, // gpcs.ObjectCall::NotifyDown
            [1] = new PySubStream (dataContainer)
        };

        dataContainer = new PyTuple (2)
        {
            [0] = dataContainer,
            [1] = null
        };

        PyPacket packet = new PyPacket (PyPacket.PacketType.NOTIFICATION)
        {
            Destination = new PyAddressBroadcast (idsOfInterest, idType, notificationType),
            Source      = new PyAddressNode (MachoNet.NodeID),

            // set the userID to -1, this will indicate the cluster controller to fill it in
            UserID  = -1,
            Payload = dataContainer
        };

        MachoNet.QueueOutputPacket (packet);
    }
}