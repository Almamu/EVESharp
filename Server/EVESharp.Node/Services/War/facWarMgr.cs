using System;
using EVESharp.Database.Inventory.Groups;
using EVESharp.Database.Old;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Messages;
using EVESharp.EVE.Exceptions.facWarMgr;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Corporations;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.War;

internal enum FactionWarStatus
{
    CorporationJoining = 0,
    CorporationActive  = 1,
    CorporationLeaving = 2
}

[MustBeCharacter]
public class facWarMgr : Service
{
    public override AccessLevel         AccessLevel    => AccessLevel.None;
    private         ChatDB              ChatDB         { get; }
    private         OldCharacterDB         CharacterDB    { get; }
    private         ICacheStorage       CacheStorage   { get; }
    private         IItems              Items          { get; }
    private         INotificationSender Notifications  { get; }
    private         ISessionManager     SessionManager { get; }

    public facWarMgr
    (
        ChatDB          chatDB, OldCharacterDB characterDB, ICacheStorage cacheStorage, IItems items, INotificationSender notificationSender,
        ISessionManager sessionManager
    )
    {
        ChatDB         = chatDB;
        CharacterDB    = characterDB;
        CacheStorage   = cacheStorage;
        this.Items     = items;
        Notifications  = notificationSender;
        SessionManager = sessionManager;
    }

    public PyDataType GetWarFactions (ServiceCall call)
    {
        CacheStorage.Load (
            "facWarMgr",
            "GetWarFactions",
            "SELECT factionID, militiaCorporationID FROM chrFactions WHERE militiaCorporationID IS NOT NULL",
            CacheObjectType.IntIntDict
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("facWarMgr", "GetWarFactions"));
    }

    public PyDataType GetFacWarSystems (ServiceCall call)
    {
        /*
         * The data is an integer dict (indicating solar system) with these as entries:
         * ["occupierID"] = Faction ID - I guess faction ID that controls the system
         * ["factionID"] = Faction ID - I guess original faction ID that controled the system?
         */
        if (CacheStorage.Exists ("facWarMgr", "GetFacWarSystems") == false)
            CacheStorage.StoreCall (
                "facWarMgr",
                "GetFacWarSystems",
                new PyDictionary (),
                DateTime.UtcNow.ToFileTimeUtc ()
            );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("facWarMgr", "GetFacWarSystems"));
    }

    public PyDataType GetCharacterRankOverview (ServiceCall call, PyInteger characterID)
    {
        return new Rowset (
            new PyList <PyString> (4)
            {
                [0] = "currentRank",
                [1] = "highestRank",
                [2] = "factionID",
                [3] = "lastModified"
            }
        );
    }

    public PyInteger GetFactionMilitiaCorporation (ServiceCall call, PyInteger factionID)
    {
        return this.Items.GetStaticFaction (factionID).MilitiaCorporationId;
    }

    public PyDataType GetFactionalWarStatus (ServiceCall call)
    {
        if (call.Session.WarFactionID is null)
            return KeyVal.FromDictionary (new PyDictionary {["status"] = null});

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["factionID"] = call.Session.WarFactionID,
                ["startDate"] = DateTime.UtcNow.ToFileTimeUtc (),
                ["status"]    = (int) FactionWarStatus.CorporationActive
            }
        );
    }

    public PyDataType IsEnemyFaction (ServiceCall call, PyInteger factionID, PyInteger warFactionID)
    {
        return false;
    }

    public PyDataType JoinFactionAsCharacter (ServiceCall call, PyInteger factionID)
    {
        int callerCharacterID = call.Session.CharacterID;

        // TODO: CHECK FOR PERMISSIONS, TO JOIN TO SOME FACTIONS THE CHARACTER REQUIRES AN INVITATION

        // check if the player joined a militia in the last 24 hours
        long minimumDateTime = DateTime.UtcNow.AddHours (-24).ToFileTimeUtc ();
        long lastTime        = CharacterDB.GetLastFactionJoinDate (callerCharacterID);

        if (lastTime > minimumDateTime)
            throw new FactionCharJoinDenied (MLS.UI_CORP_MILITIAJOIN_DENIED_TOOFREQUENT, TimeSpan.FromTicks (minimumDateTime - lastTime).Hours);

        // first join the character to the militia corporation
        Faction   faction   = this.Items.Factions [factionID];
        Character character = this.Items.GetItem <Character> (callerCharacterID);

        // build the notification of corporation change
        OnCorporationMemberChanged change = new OnCorporationMemberChanged (character.ID, call.Session.CorporationID, faction.MilitiaCorporationId);
        // add the character to the faction's and corp chat channel
        ChatDB.JoinEntityChannel (factionID,                    callerCharacterID);
        ChatDB.JoinEntityChannel (faction.MilitiaCorporationId, character.ID);
        ChatDB.JoinEntityMailingList (faction.MilitiaCorporationId, character.ID);
        // remove character from the old chat channel too
        ChatDB.LeaveChannel (character.CorporationID, character.ID);
        // this change implies a session change
        Session update = new Session ();

        update.CorporationID = faction.MilitiaCorporationId;
        update.WarFactionID  = factionID;

        SessionManager.PerformSessionUpdate (Session.CHAR_ID, character.ID, update);
        // set the new faction id and corporation
        character.WarFactionID        = factionID;
        character.CorporationID       = faction.MilitiaCorporationId;
        character.CorporationDateTime = DateTime.UtcNow.ToFileTimeUtc ();
        // create employment record
        CharacterDB.CreateEmploymentRecord (character.ID, faction.MilitiaCorporationId, character.CorporationDateTime);
        // notify cluster about the corporation changes
        Notifications.NotifyCorporation (change.OldCorporationID, change);
        Notifications.NotifyCorporation (change.NewCorporationID, change);
        // save the character
        character.Persist ();

        return null;
    }

    public PyDataType JoinFactionAsCorporation (ServiceCall call, PyInteger factionID)
    {
        return null;
    }

    public PyDataType GetCharacterRankInfo (ServiceCall call, PyInteger characterID)
    {
        // TODO: IMPLEMENT THIS

        return KeyVal.FromDictionary (
            new PyDictionary
            {
                ["currentRank"] = 1,
                ["highestRank"] = 1,
                ["factionID"]   = call.Session.WarFactionID
            }
        );
    }

    public PyDataType GetSystemsConqueredThisRun (ServiceCall call)
    {
        // TODO: IMPLEMENT THIS, IT'S A LIST OF PYDICTIONARY
        return new PyList (0);
    }

    private PyDictionary <PyString, PyDataType> GetStats_Stub (ServiceCall call)
    {
        return new PyDictionary <PyString, PyDataType>
        {
            ["header"] = new PyList (),
            ["data"]   = new PyDictionary ()
        };
    }

    public PyDataType GetStats_Militia (ServiceCall call)
    {
        return this.GetStats_Stub (call);
    }

    public PyDataType GetStats_Personal (ServiceCall call)
    {
        return new PyDictionary ();
    }

    public PyDataType GetStats_Corp (ServiceCall call)
    {
        return new PyDictionary ();
    }

    public PyDataType GetStats_FactionInfo (ServiceCall call)
    {
        return new PyDictionary ();
    }

    public PyDataType GetStats_Character (ServiceCall call)
    {
        return new PyDictionary
        {
            ["killsY"]     = 0,
            ["killsLW"]    = 0,
            ["killsTotal"] = 0,
            ["vpY"]        = 0,
            ["vpLW"]       = 0,
            ["vpTotal"]    = 0
        };
    }

    public PyDataType GetStats_TopAndAllKillsAndVPs (ServiceCall call)
    {
        PyDictionary values = new PyDictionary
        {
            ["killsY"]     = 0,
            ["killsLW"]    = 0,
            ["killsTotal"] = 0,
            ["vpY"]        = 0,
            ["vpLW"]       = 0,
            ["vpTotal"]    = 0
        };

        return new PyList (2)
        {
            [0] = new PyDictionary
            {
                [(int) GroupID.Corporation] = values,
                [(int) GroupID.Character]   = values
            },
            [1] = new PyDictionary
            {
                [(int) GroupID.Corporation] = values,
                [(int) GroupID.Character]   = values
            }
        };
    }

    public PyDataType GetCorporationWarFactionID (ServiceCall call, PyInteger corporationID)
    {
        return null;
    }
}