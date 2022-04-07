using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Character;
using EVESharp.Node.Sessions;
using EVESharp.Node.StaticData.Standings;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.War;

public class standing2 : Service
{
    public override AccessLevel         AccessLevel         => AccessLevel.None;
    private         StandingDB          DB                  { get; }
    private         CacheStorage        CacheStorage        { get; }
    private         ItemFactory         ItemFactory         { get; }
    private         NotificationManager NotificationManager { get; }

    public standing2 (CacheStorage cacheStorage, StandingDB db, ItemFactory itemFactory, NotificationManager notificationManager)
    {
        CacheStorage        = cacheStorage;
        DB                  = db;
        ItemFactory         = itemFactory;
        NotificationManager = notificationManager;
    }

    public PyTuple GetMyKillRights (CallInformation call)
    {
        PyDictionary killRights   = new PyDictionary ();
        PyDictionary killedRights = new PyDictionary ();

        return new PyTuple (2)
        {
            [0] = killRights,
            [1] = killedRights
        };
    }

    public PyDataType GetNPCNPCStandings (CallInformation call)
    {
        CacheStorage.Load (
            "standing2",
            "GetNPCNPCStandings",
            "SELECT fromID, toID, standing FROM npcStandings",
            CacheStorage.CacheObjectType.Rowset
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("standing2", "GetNPCNPCStandings"));
    }

    public PyTuple GetCharStandings (CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        return new PyTuple (3)
        {
            [0] = DB.GetStandings (callerCharacterID),
            [1] = DB.GetPrime (callerCharacterID),
            [2] = DB.GetNPCStandings (callerCharacterID)
        };
    }

    public PyTuple GetCorpStandings (CallInformation call)
    {
        int corporationID = call.Session.CorporationID;

        return new PyTuple (3)
        {
            [0] = DB.GetStandings (corporationID),
            [1] = DB.GetPrime (corporationID),
            [2] = DB.GetNPCStandings (corporationID)
        };
    }

    public PyDataType GetStandingTransactions (
        PyInteger from,        PyInteger to,            PyInteger       direction, PyInteger eventID,
        PyInteger eventTypeID, PyInteger eventDateTime, CallInformation call
    )
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        if (from != call.Session.CorporationID && from != callerCharacterID && to != call.Session.CorporationID &&
            to != callerCharacterID)
            throw new CustomError ("You can only view standings that concern you");

        return DB.GetStandingTransactions (from, to, direction, eventID, eventTypeID, eventDateTime);
    }

    public PyDecimal GetSecurityRating (PyInteger characterID, CallInformation call)
    {
        if (ItemFactory.TryGetItem (characterID, out Character character))
            return character.SecurityRating;

        return DB.GetSecurityRating (characterID);
    }

    public PyDataType GetNPCStandingsTo (PyInteger characterID, CallInformation call)
    {
        return DB.GetNPCStandings (characterID);
    }

    public PyDataType SetPlayerStanding (PyInteger entityID, PyDecimal standing, PyString reason, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        DB.CreateStandingTransaction ((int) EventType.StandingPlayerSetStanding, callerCharacterID, entityID, standing, reason);
        DB.SetPlayerStanding (callerCharacterID, entityID, standing);

        // send the same notification to both players
        NotificationManager.NotifyOwners (
            new PyList <PyInteger> (2)
            {
                [0] = callerCharacterID,
                [1] = entityID
            },
            new OnStandingSet (callerCharacterID, entityID, standing)
        );

        return null;
    }

    public PyDataType SetCorpStanding (PyInteger entityID, PyDecimal standing, PyString reason, CallInformation call)
    {
        // check for permissions
        DB.CreateStandingTransaction ((int) EventType.StandingPlayerCorpSetStanding, call.Session.CorporationID, entityID, standing, reason);
        DB.SetPlayerStanding (call.Session.CorporationID, entityID, standing);

        // TODO: MAYBE SEND ONSETCORPSTANDING NOTIFICATION?!
        // send the same notification to both players
        NotificationManager.NotifyOwners (
            new PyList <PyInteger> (2)
            {
                [0] = call.Session.CorporationID,
                [1] = entityID
            },
            new OnStandingSet (call.Session.CorporationID, entityID, standing)
        );

        return null;
    }
}