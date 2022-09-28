using EVESharp.Database.Old;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Standings;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Relationships;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.War;

[MustBeCharacter]
public class standing2 : Service
{
    public override AccessLevel         AccessLevel   => AccessLevel.None;
    private         StandingDB          DB            { get; }
    private         ICacheStorage       CacheStorage  { get; }
    private         IItems              Items         { get; }
    private         INotificationSender Notifications { get; }
    private         IStandings          Standings     { get; }

    public standing2 (ICacheStorage cacheStorage, StandingDB db, IItems items, INotificationSender notificationSender, IStandings standings)
    {
        CacheStorage  = cacheStorage;
        DB            = db;
        this.Items    = items;
        Notifications = notificationSender;
        Standings     = standings;
    }

    public PyTuple GetMyKillRights (ServiceCall call)
    {
        PyDictionary killRights   = new PyDictionary ();
        PyDictionary killedRights = new PyDictionary ();

        return new PyTuple (2)
        {
            [0] = killRights,
            [1] = killedRights
        };
    }

    public PyDataType GetNPCNPCStandings (ServiceCall call)
    {
        CacheStorage.Load (
            "standing2",
            "GetNPCNPCStandings",
            "SELECT fromID, toID, standing FROM npcStandings",
            CacheObjectType.Rowset
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("standing2", "GetNPCNPCStandings"));
    }

    private PyTuple GetStandingsFor (int entityID)
    {
        return new PyTuple (3)
        {
            [0] = DB.GetStandings (entityID),
            [1] = DB.GetPrime (entityID),
            [2] = DB.GetNPCStandings (entityID)
        };
    }

    public PyTuple GetCharStandings (ServiceCall call)
    {
        return GetStandingsFor (call.Session.CharacterID);
    }

    public PyTuple GetCorpStandings (ServiceCall call)
    {
        return GetStandingsFor (call.Session.CorporationID);
    }

    public PyDataType GetStandingTransactions
    (
        ServiceCall call,        PyInteger from, PyInteger to, PyInteger direction, PyInteger eventID,
        PyInteger       eventTypeID, PyInteger eventDateTime
    )
    {
        int callerCharacterID = call.Session.CharacterID;

        if (from != call.Session.CorporationID && from != callerCharacterID && to != call.Session.CorporationID &&
            to != callerCharacterID)
            throw new CustomError ("You can only view standings that concern you");

        return DB.GetStandingTransactions (from, to, direction, eventID, eventTypeID, eventDateTime);
    }

    public PyDecimal GetSecurityRating (ServiceCall call, PyInteger characterID)
    {
        return this.Items.TryGetItem (characterID, out Character character)
            ? character.SecurityRating
            : this.DB.GetSecurityRating (characterID);
    }

    public PyDataType GetNPCStandingsTo (ServiceCall call, PyInteger characterID)
    {
        return DB.GetNPCStandings (characterID);
    }

    public PyDataType SetPlayerStanding (ServiceCall call, PyInteger entityID, PyDecimal standing, PyString reason)
    {
        Standings.SetStanding (EventType.StandingPlayerSetStanding, call.Session.CharacterID, entityID, standing, reason);
        
        return null;
    }

    public PyDataType SetCorpStanding (ServiceCall call, PyInteger entityID, PyDecimal standing, PyString reason)
    {
        Standings.SetStanding (EventType.StandingPlayerCorpSetStanding, call.Session.CorporationID, entityID, standing, reason);
        
        return null;
    }
}