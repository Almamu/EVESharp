using System.Collections.Generic;
using System.Linq;
using EVESharp.Database.Corporations;
using EVESharp.EVE.Network;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Types.Network;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Notifications;

public class NotificationSender : INotificationSender
{
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
    
    public void NotifyCorporations (PyList <PyInteger> corporationIDs, string type, PyTuple notification)
    {
        this.SendNotification (type, NotificationIdType.Corporation, corporationIDs, notification);
    }

    public void NotifyCorporations (PyList <PyInteger> corporationIDs, ClientNotification notification)
    {
        this.SendNotification (NotificationIdType.Corporation, corporationIDs, notification);
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

    public void NotifyCorporationByRole (int corporationID, ClientNotification notification, IEnumerable <long> roleMask)
    {
        PyList<PyTuple> idsOfInterest = new PyList<PyTuple> ();
        
        // build a PyList with all the tuples
        foreach (long role in roleMask)
        {
            idsOfInterest.Add (
                new PyTuple (2)
                {
                    [0] = corporationID,
                    [1] = role
                }
            );
        }

        this.SendNotification (NotificationIdType.CorporationAndRole, idsOfInterest, notification);
    }
    
    public void NotifyCorporationByRole (int corporationID, ClientNotification notification, params long[] roleMask)
    {
        this.NotifyCorporationByRole (corporationID, notification, roleMask.AsEnumerable ());
    }

    public void NotifyCorporationByRole (int corporationID, CorporationRole role, ClientNotification notification)
    {
        this.NotifyCorporationByRole (corporationID, (long) role, notification);
    }

    public void NotifyCorporationByRole (int corporationID, ClientNotification notification, params CorporationRole[] role)
    {
        this.NotifyCorporationByRole (corporationID, notification, role.Cast<long> ());
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
        this.SendNotification (INotificationSender.NotificationTypeTranslation [idType], id, data);
    }

    public void SendNotification (NotificationIdType idType, PyTuple ids, ClientNotification data)
    {
        this.SendNotification (INotificationSender.NotificationTypeTranslation [idType], ids, data);
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
        this.SendNotification (notificationType, INotificationSender.NotificationTypeTranslation [idType], new PyList (1) {[0] = id}, data);
    }

    public void SendNotification (string notificationType, NotificationIdType idType, int id, ClientNotification data)
    {
        this.SendNotification (notificationType, INotificationSender.NotificationTypeTranslation [idType], id, data.GetElements ());
    }

    public void SendNotification (string idType, PyList idsOfInterest, ClientNotification data)
    {
        this.SendNotification (data.NotificationName, idType, idsOfInterest, data.GetElements ());
    }

    public void SendNotification (NotificationIdType idType, PyList idsOfInterest, ClientNotification data)
    {
        this.SendNotification (data.NotificationName, INotificationSender.NotificationTypeTranslation [idType], idsOfInterest, data.GetElements ());
    }

    public void SendNotification (string notificationType, NotificationIdType idType, PyList idsOfInterest, ClientNotification data)
    {
        this.SendNotification (notificationType, INotificationSender.NotificationTypeTranslation [idType], idsOfInterest, data.GetElements ());
    }

    public void SendNotification (string notificationType, NotificationIdType idType, PyList idsOfInterest, PyTuple data)
    {
        this.SendNotification (notificationType, INotificationSender.NotificationTypeTranslation [idType], idsOfInterest, data);
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