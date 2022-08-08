using System.Collections.Generic;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Notifications;

public interface INotificationSender
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
    
    void NotifyCharacters (PyList <PyInteger> characterIDs,  string                type, PyTuple notification);
    void NotifyCharacters (PyList <PyInteger> characterIDs,  ClientNotification    notification);
    void NotifyCharacter (int                 characterID,   string                type, PyTuple notification);
    void NotifyCharacter (int                 characterID,   ClientNotification    entry);
    void NotifyOwner (int                     ownerID,       ClientNotification    entry);
    void NotifyOwners (PyList <PyInteger>     ownerIDs,      ClientNotification    notification);
    void NotifyOwnerAtLocation (int           ownerID,       int                   locationID, ClientNotification entry);
    void NotifyCorporation (int               corporationID, string                type,       PyTuple            notification);
    void NotifyCorporation (int               corporationID, ClientNotification    notification);
    void NotifyStation (int                   stationID,     string                type, PyTuple notification);
    void NotifyStation (int                   stationID,     ClientNotification    notification);
    void NotifyAlliance (int                  allianceID,    ClientNotification    notification);
    void NotifyCorporationByRole (int         corporationID, long                  roleMask, ClientNotification notification);
    void NotifyCorporationByRole (int         corporationID, CorporationRole       role,     ClientNotification notification);
    void NotifyNode (long                     nodeID,        InterNodeNotification notification);

    void SendNotification (NotificationIdType idType,           int                id,            ClientNotification data);
    void SendNotification (NotificationIdType idType,           PyTuple            ids,           ClientNotification data);
    void SendNotification (string             idType,           int                id,            ClientNotification data);
    void SendNotification (string             idType,           PyTuple            id,            ClientNotification data);
    void SendNotification (string             notificationType, string             idType,        int                id, PyTuple            data);
    void SendNotification (string             notificationType, NotificationIdType idType,        int                id, PyTuple            data);
    void SendNotification (string             notificationType, NotificationIdType idType,        int                id, ClientNotification data);
    void SendNotification (string             idType,           PyList             idsOfInterest, ClientNotification data);
    void SendNotification (NotificationIdType idType,           PyList             idsOfInterest, ClientNotification data);
    void SendNotification (string             notificationType, NotificationIdType idType,        PyList             idsOfInterest, ClientNotification data);
    void SendNotification (string             notificationType, NotificationIdType idType,        PyList             idsOfInterest, PyTuple            data);
    void SendNotification (string             notificationType, string             idType,        PyList             idsOfInterest, PyTuple            data);
}