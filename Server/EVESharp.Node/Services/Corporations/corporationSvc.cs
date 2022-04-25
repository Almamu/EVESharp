using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.EVE.Client.Exceptions.corporationSvc;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Market;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.Node.Client.Notifications.Corporations;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Corporations;

[MustBeCharacter]
public class corporationSvc : Service
{
    public override AccessLevel        AccessLevel   => AccessLevel.None;
    private         IDatabaseConnection Database      { get; }
    private         CorporationDB      DB            { get; }
    private         WalletManager      WalletManager { get; }
    private         Constants          Constants     { get; }
    private         ItemFactory        ItemFactory   { get; }
    private         NotificationSender Notifications { get; }

    public corporationSvc (
        IDatabaseConnection databaseConnection, CorporationDB db, Constants constants, WalletManager walletManager, ItemFactory itemFactory,
        NotificationSender  notificationSender
    )
    {
        Database      = databaseConnection;
        DB            = db;
        Constants     = constants;
        WalletManager = walletManager;
        ItemFactory   = itemFactory;
        Notifications = notificationSender;
    }

    public PyTuple GetFactionInfo (CallInformation call)
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

    public PyDataType GetNPCDivisions (CallInformation call)
    {
        return Database.Rowset (CorporationDB.LIST_NPC_DIVISIONS);
    }

    public PyTuple GetMedalsReceived (PyInteger characterID, CallInformation call)
    {
        // TODO: CACHE THIS ANSWER TOO
        int callerCharacterID = call.Session.CharacterID;

        bool publicOnly = callerCharacterID != characterID;

        return new PyTuple (2)
        {
            [0] = DB.GetMedalsReceived (characterID, publicOnly),
            [1] = DB.GetMedalsReceivedDetails (characterID, publicOnly)
        };
    }

    public PyDataType GetEmploymentRecord (PyInteger characterID, CallInformation call)
    {
        return DB.GetEmploymentRecord (characterID);
    }

    public PyDataType GetRecruitmentAdTypes (CallInformation call)
    {
        return Database.CRowset (CorporationDB.GET_RECRUITMENT_AD_TYPES);
    }

    public PyDataType GetRecruitmentAdsByCriteria (
        PyInteger regionID, PyInteger skillPoints,  PyInteger typeMask,
        PyInteger raceMask, PyInteger isInAlliance, PyInteger minMembers, PyInteger maxMembers, CallInformation call
    )
    {
        return this.GetRecruitmentAdsByCriteria (
            regionID, skillPoints * 1.0, typeMask, raceMask, isInAlliance,
            minMembers, maxMembers, call
        );
    }

    public PyDataType GetRecruitmentAdsByCriteria (
        PyInteger regionID, PyDecimal skillPoints,  PyInteger typeMask,
        PyInteger raceMask, PyInteger isInAlliance, PyInteger minMembers, PyInteger maxMembers, CallInformation call
    )
    {
        return DB.GetRecruitmentAds (regionID, skillPoints, typeMask, raceMask, isInAlliance, minMembers, maxMembers);
    }

    public PyTuple GetAllCorpMedals (PyInteger corporationID, CallInformation call)
    {
        Dictionary <string, object> values = new Dictionary <string, object> {{"_corporationID", corporationID}};

        // TODO: IMPLEMENT CACHING FOR THIS ANSWER (THE CLIENT SEEMS TO EXPECT IT, ALTHOUGH IT DOESN'T ENFORCE IT)
        // TODO: THAT WILL REQUIRE TO SEND NOTIFICATIONS ON DATA CHANGES LIKE NEW MEDALS OR MEDALS BEING ISSUED
        // TODO: OnCorporationMedalAdded, OnMedalIssued, OnMedalStatusChanged
        return new PyTuple (2)
        {
            [0] = Database.Rowset (CorporationDB.LIST_MEDALS, values),
            [1] = Database.CRowset (CorporationDB.LIST_MEDALS_DETAILS, values)
        };
    }

    public PyDataType GetCorpInfo (PyInteger corporationID, CallInformation call)
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
        if (title.Length < 3) throw new MedalNameInvalid ();
        if (title.Length > 100) throw new MedalNameTooLong ();
        if (description.Length > 1000) throw new MedalDescriptionTooLong ();
        if (false) throw new MedalDescriptionInvalid (); // TODO: CHECK FOR BANNED WORDS!

        // TODO: VALIDATE PART NAMES TO ENSURE THEY'RE VALID
    }

    [MustHaveCorporationRole(MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT, CorporationRole.PersonnelManager, CorporationRole.Director)]
    public PyDataType CreateMedal (PyString title, PyString description, PyList parts, PyBool pay, CallInformation call)
    {
        int characterID = call.Session.CharacterID;

        this.ValidateMedal (title, description, parts);

        if (pay == false)
            throw new ConfirmCreatingMedal (Constants.MedalCost);

        using (Wallet wallet = WalletManager.AcquireWallet (call.Session.CorporationID, call.Session.CorpAccountKey, true))
        {
            wallet.EnsureEnoughBalance (Constants.MedalCost);
            wallet.CreateJournalRecord (MarketReference.MedalCreation, Constants.MedalTaxCorporation, null, -Constants.MedalCost);
        }

        DB.CreateMedal (call.Session.CorporationID, characterID, title, description, parts.GetEnumerable <PyList> ());

        return null;
    }

    [MustHaveCorporationRole(MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT, CorporationRole.PersonnelManager, CorporationRole.Director)]
    public PyDataType CreateMedal (PyString title, PyString description, PyList parts, CallInformation call)
    {
        this.ValidateMedal (title, description, parts);

        throw new ConfirmCreatingMedal (Constants.MedalCost);
    }

    public PyDataType GetRecipientsOfMedal (PyInteger medalID, CallInformation call)
    {
        // TODO: CHECK IF THERE'S ANY KIND OF PERMISSION CHECK NEEDED (LIKE CORPORATION ID?)
        // TODO: CACHE THIS ANSWER TOO
        return DB.GetRecipientsOfMedal (medalID);
    }

    public PyDataType GetMedalStatuses (CallInformation call)
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

    [MustHaveCorporationRole(MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT, CorporationRole.PersonnelManager, CorporationRole.Director)]
    public PyDataType GiveMedalToCharacters (PyInteger medalID, PyList characterIDs, PyString reason, CallInformation call)
    {
        throw new ConfirmCreatingMedal (Constants.MedalCost);
    }

    [MustHaveCorporationRole(MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT, CorporationRole.PersonnelManager, CorporationRole.Director)]
    public PyDataType GiveMedalToCharacters (PyInteger medalID, PyList characterIDs, PyString reason, PyBool pay, CallInformation call)
    {
        if (pay == false)
            throw new ConfirmGivingMedal (Constants.MedalCost);

        using (Wallet wallet = WalletManager.AcquireWallet (call.Session.CorporationID, call.Session.CorpAccountKey, true))
        {
            wallet.EnsureEnoughBalance (Constants.MedalCost);
            wallet.CreateJournalRecord (MarketReference.MedalIssuing, Constants.MedalTaxCorporation, null, -Constants.MedalCost);
        }

        // create the records for all the characters that have that medal
        foreach (PyInteger characterID in characterIDs.GetEnumerable <PyInteger> ())
            DB.GrantMedal (medalID, characterID, call.Session.CharacterID, reason, 2);

        // notify all the characters
        Notifications.NotifyCharacters (characterIDs.GetEnumerable <PyInteger> (), new OnMedalIssued ());

        // increase recipients for medals
        DB.IncreaseRecepientsForMedal (medalID, characterIDs.Count);

        // no return data required
        return null;
    }

    public PyDataType SetMedalStatus (PyDictionary newStatuses, CallInformation call)
    {
        int characterID = call.Session.CharacterID;

        PyDictionary <PyInteger, PyInteger> newStatus = newStatuses.GetEnumerable <PyInteger, PyInteger> ();

        foreach ((PyInteger medalID, PyInteger status) in newStatus)
            // MEDAL DELETION
            if (status == 1)
                DB.RemoveMedalFromCharacter (medalID, characterID);
            else
                // set whatever status was requested by the user
                DB.UpdateMedalForCharacter (medalID, characterID, status);

        return null;
    }

    public PyDataType GetMedalDetails (PyInteger medalID, CallInformation call)
    {
        return KeyVal.FromDictionary (new PyDictionary {["info"] = new PyList {DB.GetMedalDetails (medalID)}});
    }
}