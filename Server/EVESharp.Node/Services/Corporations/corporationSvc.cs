using System;
using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Old;
using EVESharp.Database.Types;
using EVESharp.EVE.Data.Configuration;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Data.Messages;
using EVESharp.EVE.Exceptions.corporationSvc;
using EVESharp.EVE.Market;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Corporations;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Corporations;

[MustBeCharacter]
public class corporationSvc : Service
{
    public override AccessLevel         AccessLevel   => AccessLevel.None;
    private         IDatabase Database      { get; }
    private         CorporationDB       DB            { get; }
    private         IWallets            Wallets       { get; }
    private         IConstants          Constants     { get; }
    private         IItems              Items         { get; }
    private         INotificationSender Notifications { get; }
    private         ICacheStorage       CacheStorage  { get; }

    public corporationSvc
    (
        IDatabase database, CorporationDB db, IConstants constants, IWallets wallets, IItems items,
        INotificationSender notificationSender, ICacheStorage cacheStorage
    )
    {
        Database      = database;
        DB            = db;
        Constants     = constants;
        this.Wallets  = wallets;
        this.Items    = items;
        Notifications = notificationSender;
        CacheStorage  = cacheStorage;
    }

    public PyTuple GetFactionInfo (ServiceCall call)
    {
        return new PyTuple (8)
        {
            [0] = Database.IntIntDictionary (CorporationDB.LIST_FACTION_CORPORATIONS),
            [1] = Database.IntIntListDictionary (CorporationDB.LIST_FACTION_REGIONS),
            [2] = Database.IntIntListDictionary (CorporationDB.LIST_FACTION_CONSTELLATIONS),
            [3] = Database.IntIntListDictionary (CorporationDB.LIST_FACTION_SOLARSYSTEMS),
            [4] = Database.IntIntListDictionary (CorporationDB.LIST_FACTION_RACES),
            [5] = Database.IntIntDictionary (CorporationDB.LIST_FACTION_STATION_COUNT),
            [6] = Database.IntIntDictionary (CorporationDB.LIST_FACTION_STATION_COUNT),
            [7] = Database.IntRowDictionary (0, CorporationDB.LIST_NPC_INFO)
        };
    }

    public PyDataType GetNPCDivisions (ServiceCall call)
    {
        return Database.Rowset (CorporationDB.LIST_NPC_DIVISIONS);
    }

    public PyDataType GetMedalsReceived (ServiceCall call, PyInteger characterID)
    {
        // TODO: CACHE THIS ANSWER TOO
        int callerCharacterID = call.Session.CharacterID;

        bool publicOnly = callerCharacterID != characterID;

        // check if the cache already exits
        if (CacheStorage.Exists ("corporationSvc", "GetMedalsReceived_" + characterID + "_" + publicOnly) == false)
        {
            CacheStorage.StoreCall (
                "corporationSvc",
                "GetMedalsReceived_" + characterID + "_" + publicOnly,
                new PyTuple (2)
                {
                    [0] = DB.GetMedalsReceived (characterID, publicOnly),
                    [1] = DB.GetMedalsReceivedDetails (characterID, publicOnly)
                },
                DateTime.UtcNow.ToFileTimeUtc ()
            );
        }

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("corporationSvc", "GetMedalsReceived_" + characterID + "_" + publicOnly));
    }

    public PyDataType GetEmploymentRecord (ServiceCall call, PyInteger characterID)
    {
        return DB.GetEmploymentRecord (characterID);
    }

    public PyDataType GetRecruitmentAdTypes (ServiceCall call)
    {
        return Database.CRowset (CorporationDB.GET_RECRUITMENT_AD_TYPES);
    }

    public PyDataType GetRecruitmentAdsByCriteria
    (
        ServiceCall call,     PyInteger regionID,     PyInteger skillPoints, PyInteger typeMask,
        PyInteger       raceMask, PyInteger isInAlliance, PyInteger minMembers,  PyInteger maxMembers
    )
    {
        return this.GetRecruitmentAdsByCriteria (
            call, regionID, skillPoints * 1.0, typeMask, raceMask, isInAlliance,
            minMembers, maxMembers
        );
    }

    public PyDataType GetRecruitmentAdsByCriteria
    (
        ServiceCall call,     PyInteger regionID,     PyDecimal skillPoints, PyInteger typeMask,
        PyInteger       raceMask, PyInteger isInAlliance, PyInteger minMembers,  PyInteger maxMembers
    )
    {
        return DB.GetRecruitmentAds (regionID, skillPoints, typeMask, raceMask, isInAlliance, minMembers, maxMembers);
    }

    public PyDataType GetAllCorpMedals (ServiceCall call, PyInteger corporationID)
    {
        // check if the cache already exits
        if (CacheStorage.Exists ("corporationSvc", "GetAllCorpMedals_" + corporationID) == false)
        {
            Dictionary <string, object> values = new Dictionary <string, object> {{"_corporationID", corporationID}};
            
            CacheStorage.StoreCall (
                "corporationSvc",
                "GetAllCorpMedals_" + corporationID,
                new PyTuple (2)
                {
                    [0] = Database.Rowset (CorporationDB.LIST_MEDALS, values),
                    [1] = Database.CRowset (CorporationDB.LIST_MEDALS_DETAILS, values)
                },
                DateTime.UtcNow.ToFileTimeUtc ()
            );
        }

        return CachedMethodCallResult.FromCacheHint (CacheStorage.GetHint ("corporationSvc", "GetAllCorpMedals_" + corporationID));
    }

    public PyDataType GetCorpInfo (ServiceCall call, PyInteger corporationID)
    {
        DBRowDescriptor descriptor = new DBRowDescriptor ();

        descriptor.Columns.Add (new DBRowDescriptor.Column ("corporationID", FieldType.I4));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("typeID",        FieldType.I4));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("buyDate",       FieldType.FileTime));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("buyPrice",      FieldType.CY));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("buyQuantity",   FieldType.I4));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("buyStationID",  FieldType.I4));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("sellDate",      FieldType.FileTime));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("sellPrice",     FieldType.CY));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("sellQuantity",  FieldType.I4));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("sellStationID", FieldType.I4));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("agtBuyPrice",   FieldType.CY));
        descriptor.Columns.Add (new DBRowDescriptor.Column ("agtSellPrice",  FieldType.CY));

        return new CRowset (descriptor);
    }

    private void ValidateMedal (PyString title, PyString description, PyList parts)
    {
        if (title.Length < 3)
            throw new MedalNameInvalid ();

        if (title.Length > 100)
            throw new MedalNameTooLong ();

        if (description.Length > 1000)
            throw new MedalDescriptionTooLong ();

        if (false)
            throw new MedalDescriptionInvalid (); // TODO: CHECK FOR BANNED WORDS!

        // TODO: VALIDATE PART NAMES TO ENSURE THEY'RE VALID
    }

    [MustHaveCorporationRole (MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT, CorporationRole.PersonnelManager, CorporationRole.Director)]
    public PyDataType CreateMedal (ServiceCall call, PyString title, PyString description, PyList parts, PyBool pay)
    {
        int characterID = call.Session.CharacterID;

        this.ValidateMedal (title, description, parts);

        if (pay == false)
            throw new ConfirmCreatingMedal (Constants.MedalCost);

        using (IWallet wallet = this.Wallets.AcquireWallet (call.Session.CorporationID, call.Session.CorpAccountKey, true))
        {
            wallet.EnsureEnoughBalance (Constants.MedalCost);
            wallet.CreateJournalRecord (MarketReference.MedalCreation, Constants.MedalTaxCorporation, null, -Constants.MedalCost);
        }

        int medalID = (int) DB.CreateMedal (call.Session.CorporationID, characterID, title, description, parts.GetEnumerable <PyList> ());

        // remove the cached data from memory
        CacheStorage.Remove ("corporationSvc", "GetAllCorpMedals_" + call.Session.CorporationID);
        
        // notify everyone
        Notifications.NotifyCorporation (call.Session.CorporationID, new OnCorporationMedalAdded (medalID));
        
        return null;
    }

    [MustHaveCorporationRole (MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT, CorporationRole.PersonnelManager, CorporationRole.Director)]
    public PyDataType CreateMedal (ServiceCall call, PyString title, PyString description, PyList parts)
    {
        this.ValidateMedal (title, description, parts);

        throw new ConfirmCreatingMedal (Constants.MedalCost);
    }

    public PyDataType GetRecipientsOfMedal (ServiceCall call, PyInteger medalID)
    {
        // TODO: CHECK IF THERE'S ANY KIND OF PERMISSION CHECK NEEDED (LIKE CORPORATION ID?)
        // TODO: CACHE THIS ANSWER TOO
        return DB.GetRecipientsOfMedal (medalID);
    }

    public PyDataType GetMedalStatuses (ServiceCall call)
    {
        return new Rowset (
            new PyList <PyString>
            {
                "statusID",
                "statusName"
            },
            new PyList <PyList>
            {
                new PyList
                {
                    1,
                    "Remove"
                },
                new PyList
                {
                    2,
                    "Private"
                },
                new PyList
                {
                    3,
                    "Public"
                }
            }
        );
    }

    [MustHaveCorporationRole (MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT, CorporationRole.PersonnelManager, CorporationRole.Director)]
    public PyDataType GiveMedalToCharacters (ServiceCall call, PyInteger medalID, PyList characterIDs, PyString reason)
    {
        throw new ConfirmGivingMedal (Constants.MedalCost);
    }

    [MustHaveCorporationRole (MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT, CorporationRole.PersonnelManager, CorporationRole.Director)]
    public PyDataType GiveMedalToCharacters (ServiceCall call, PyInteger medalID, PyList characterIDs, PyString reason, PyBool pay)
    {
        if (pay == false)
            throw new ConfirmGivingMedal (Constants.MedalCost);

        using (IWallet wallet = this.Wallets.AcquireWallet (call.Session.CorporationID, call.Session.CorpAccountKey, true))
        {
            wallet.EnsureEnoughBalance (Constants.MedalCost);
            wallet.CreateJournalRecord (MarketReference.MedalIssuing, Constants.MedalTaxCorporation, null, -Constants.MedalCost);
        }

        // create the records for all the characters that have that medal
        foreach (PyInteger characterID in characterIDs.GetEnumerable <PyInteger> ())
            DB.GrantMedal (medalID, characterID, call.Session.CharacterID, reason, 2);

        // notify all the characters
        Notifications.NotifyCharacters (characterIDs.GetEnumerable <PyInteger> (), new OnMedalIssued ());
        // notify the corporation members so the window updates
        Notifications.NotifyCorporation (call.Session.CorporationID, new OnCorporationMedalAdded (medalID));

        // increase recipients for medals
        DB.IncreaseRecepientsForMedal (medalID, characterIDs.Count);

        // no return data required
        return null;
    }

    public PyDataType SetMedalStatus (ServiceCall call, PyDictionary newStatuses)
    {
        int  characterID   = call.Session.CharacterID;
        bool wasAnyDeleted = false;

        PyDictionary <PyInteger, PyInteger> newStatus = newStatuses.GetEnumerable <PyInteger, PyInteger> ();

        foreach ((PyInteger medalID, PyInteger status) in newStatus)
        {
            // MEDAL DELETION
            if (status == 1)
            {
                // TODO: VALIDATE THAT THE MEDAL HAS TO ACTUALLY BE REMOVED FROM THE CHARACTER
                // TODO: AS MAYBE CCP WASN'T REMOVING IT FROM THE DATABASE, JUST HIDING IT IN THE CHARACTER INFO
                wasAnyDeleted = true;
                DB.RemoveMedalFromCharacter (medalID, characterID);                
            }
            else
            {
                // set whatever status was requested by the user
                DB.UpdateMedalForCharacter (medalID, characterID, status);    
            }
        }

        // tell the client to refresh the local cache
        Notifications.NotifyCharacter (characterID, new OnMedalStatusChanged());
        // TODO: CLEAR VALUES FROM CACHE STORAGE
        CacheStorage.Remove ("corporationSvc", "GetMedalsReceived_" + characterID + "_0");
        CacheStorage.Remove ("corporationSvc", "GetMedalsReceived_" + characterID + "_1");
        
        return null;
    }

    public PyDataType GetMedalDetails (ServiceCall call, PyInteger medalID)
    {
        return KeyVal.FromDictionary (new PyDictionary {["info"] = new PyList {DB.GetMedalDetails (medalID)}});
    }
}