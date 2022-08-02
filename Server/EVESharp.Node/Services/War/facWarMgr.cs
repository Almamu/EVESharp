using System;
using EVESharp.EVE.Client.Exceptions.facWarMgr;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Cache;
using EVESharp.Node.Client.Notifications.Corporations;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Notifications;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Groups = EVESharp.EVE.StaticData.Inventory.Groups;
using SessionManager = EVESharp.Node.Sessions.SessionManager;

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
    public override AccessLevel        AccessLevel    => AccessLevel.None;
    private         ChatDB             ChatDB         { get; }
    private         CharacterDB        CharacterDB    { get; }
    private         CacheStorage       CacheStorage   { get; }
    private         ItemFactory        ItemFactory    { get; }
    private         NotificationSender Notifications  { get; }
    private         SessionManager     SessionManager { get; }

    public facWarMgr (
        ChatDB         chatDB, CharacterDB characterDB, CacheStorage cacheStorage, ItemFactory itemFactory, NotificationSender notificationSender,
        SessionManager sessionManager
    )
    {
        ChatDB         = chatDB;
        CharacterDB    = characterDB;
        CacheStorage   = cacheStorage;
        ItemFactory    = itemFactory;
        Notifications  = notificationSender;
        SessionManager = sessionManager;
    }

    public PyDataType GetWarFactions (CallInformation call)
    {
        CacheStorage.Load (
            "facWarMgr",
            "GetWarFactions",
            "SELECT factionID, militiaCorporationID FROM chrFactions WHERE militiaCorporationID IS NOT NULL",
            CacheStorage.CacheObjectType.IntIntDict
        );

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("facWarMgr", "GetWarFactions"));
    }

    public PyDataType GetFacWarSystems (CallInformation call)
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

    public PyDataType GetCharacterRankOverview (CallInformation call, PyInteger characterID)
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

    public PyInteger GetFactionMilitiaCorporation (CallInformation call, PyInteger factionID)
    {
        return ItemFactory.GetStaticFaction (factionID).MilitiaCorporationId;
    }

    public PyDataType GetFactionalWarStatus (CallInformation call)
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

    public PyDataType IsEnemyFaction (CallInformation call, PyInteger factionID, PyInteger warFactionID)
    {
        return false;
    }

    public PyDataType JoinFactionAsCharacter (CallInformation call, PyInteger factionID)
    {
        int callerCharacterID = call.Session.CharacterID;

        // TODO: CHECK FOR PERMISSIONS, TO JOIN TO SOME FACTIONS THE CHARACTER REQUIRES AN INVITATION

        // check if the player joined a militia in the last 24 hours
        long minimumDateTime = DateTime.UtcNow.AddHours (-24).ToFileTimeUtc ();
        long lastTime        = CharacterDB.GetLastFactionJoinDate (callerCharacterID);

        if (lastTime > minimumDateTime)
            throw new FactionCharJoinDenied (MLS.UI_CORP_MILITIAJOIN_DENIED_TOOFREQUENT, TimeSpan.FromTicks (minimumDateTime - lastTime).Hours);

        // first join the character to the militia corporation
        Faction   faction   = ItemFactory.Factions [factionID];
        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

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

    public PyDataType JoinFactionAsCorporation (CallInformation call, PyInteger factionID)
    {
        return null;
    }

    public PyDataType GetCharacterRankInfo (CallInformation call, PyInteger characterID)
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

    public PyDataType GetSystemsConqueredThisRun (CallInformation call)
    {
        // TODO: IMPLEMENT THIS, IT'S A LIST OF PYDICTIONARY
        return new PyList (0);
    }

    private PyDictionary <PyString, PyDataType> GetStats_Stub (CallInformation call)
    {
        return new PyDictionary <PyString, PyDataType>
        {
            ["header"] = new PyList (),
            ["data"]   = new PyDictionary ()
        };
    }

    public PyDataType GetStats_Militia (CallInformation call)
    {
        return this.GetStats_Stub (call);
    }

    public PyDataType GetStats_Personal (CallInformation call)
    {
        return new PyDictionary ();
    }

    public PyDataType GetStats_Corp (CallInformation call)
    {
        return new PyDictionary ();
    }

    public PyDataType GetStats_FactionInfo (CallInformation call)
    {
        return new PyDictionary ();
    }

    public PyDataType GetStats_Character (CallInformation call)
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

    public PyDataType GetStats_TopAndAllKillsAndVPs (CallInformation call)
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
                [(int) Groups.Corporation] = values,
                [(int) Groups.Character]   = values
            },
            [1] = new PyDictionary
            {
                [(int) Groups.Corporation] = values,
                [(int) Groups.Character]   = values
            }
        };
    }

    public PyDataType GetCorporationWarFactionID (CallInformation call, PyInteger corporationID)
    {
        return null;
    }
}