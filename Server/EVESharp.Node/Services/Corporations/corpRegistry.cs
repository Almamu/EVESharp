using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EVESharp.Database;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Data.Alliances;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Chat;
using EVESharp.Node.Client.Notifications.Alliances;
using EVESharp.Node.Client.Notifications.Corporations;
using EVESharp.Node.Client.Notifications.Wallet;
using EVESharp.Node.Configuration;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Nodes.Corps;
using EVESharp.Node.Server.Shared;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Character = EVESharp.Node.Inventory.Items.Types.Character;
using OnCorporationChanged = EVESharp.Node.Client.Notifications.Corporations.OnCorporationChanged;
using OnCorporationMemberChanged = EVESharp.Node.Client.Notifications.Corporations.OnCorporationMemberChanged;
using SessionManager = EVESharp.Node.Sessions.SessionManager;

namespace EVESharp.Node.Services.Corporations;

[MustBeCharacter]
// TODO: REWRITE THE USAGE OF THE CHARACTER CLASS HERE TO FETCH THE DATA OFF THE DATABASE TO PREVENT ISSUES ON MULTI-NODE INSTALLATIONS
public class corpRegistry : MultiClientBoundService
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    public Corporation Corporation { get; }
    public int         IsMaster    { get; }

    private CorporationDB              DB                  { get; }
    private ChatDB                     ChatDB              { get; }
    private CharacterDB                CharacterDB         { get; }
    private IDatabaseConnection        Database            { get; }
    private ItemFactory                ItemFactory         { get; }
    private IWalletManager             WalletManager       { get; }
    private NotificationSender         Notifications       { get; }
    private MailManager                MailManager         { get; }
    public  MembersSparseRowsetService MembersSparseRowset { get; private set; }
    public  OfficesSparseRowsetService OfficesSparseRowset { get; private set; }
    private Ancestries                 Ancestries          { get; }
    private Constants                  Constants           { get; }
    private SessionManager             SessionManager      { get; }
    private ClusterManager             ClusterManager      { get; }

    // constants
    private long CorporationAdvertisementFlatFee   { get; }
    private long CorporationAdvertisementDailyRate { get; }

    public corpRegistry (
        CorporationDB db,          IDatabaseConnection databaseConnection, ChatDB      chatDB, CharacterDB characterDB, NotificationSender notificationSender,
        MailManager   mailManager, IWalletManager      walletManager,      ItemFactory itemFactory, Constants constants, BoundServiceManager manager,
        Ancestries    ancestries,  SessionManager      sessionManager,     ClusterManager clusterManager
    ) : base (manager)
    {
        DB             = db;
        Database       = databaseConnection;
        ChatDB         = chatDB;
        CharacterDB    = characterDB;
        Notifications  = notificationSender;
        Constants      = constants;
        MailManager    = mailManager;
        WalletManager  = walletManager;
        ItemFactory    = itemFactory;
        Ancestries     = ancestries;
        SessionManager = sessionManager;
        ClusterManager = clusterManager;

        ClusterManager.OnClusterTimer += this.PerformTimedEvents;
    }

    protected corpRegistry (
        CorporationDB  db,             IDatabaseConnection databaseConnection, ChatDB    chatDB, CharacterDB characterDB, NotificationSender notificationSender,
        MailManager    mailManager,    IWalletManager     walletManager,       Constants constants, ItemFactory itemFactory, Ancestries ancestries,
        SessionManager sessionManager, Corporation        corp,                int       isMaster, corpRegistry parent
    ) : base (parent, corp.ID)
    {
        DB                                = db;
        Database                          = databaseConnection;
        ChatDB                            = chatDB;
        CharacterDB                       = characterDB;
        Notifications                     = notificationSender;
        Constants                         = constants;
        MailManager                       = mailManager;
        WalletManager                     = walletManager;
        ItemFactory                       = itemFactory;
        SessionManager                    = sessionManager;
        Corporation                       = corp;
        IsMaster                          = isMaster;
        CorporationAdvertisementFlatFee   = Constants.CorporationAdvertisementFlatFee;
        CorporationAdvertisementDailyRate = Constants.CorporationAdvertisementDailyRate;
        Ancestries                        = ancestries;
    }

    /// <summary>
    /// Checks orders that are expired, cancels them and returns the items to the hangar if required
    /// </summary>
    private void PerformTimedEvents (object sender, EventArgs args)
    {
        DB.RemoveExpiredCorporationAds ();
    }

    public PyDataType GetEveOwners (CallInformation call)
    {
        // this call seems to be getting all the members of the given corporationID
        return DB.GetEveOwners (Corporation.ID);
    }

    public PyDataType GetCorporation (CallInformation call)
    {
        return Corporation.GetCorporationInfoRow ();
    }

    public PyDataType GetSharesByShareholder (CallInformation call, PyInteger corpShares)
    {
        return this.GetSharesByShareholder (call, corpShares == 1);
    }

    public PyDataType GetSharesByShareholder (CallInformation call, PyBool corpShares)
    {
        int entityID = call.Session.CharacterID;

        if (corpShares == true)
            entityID = call.Session.CorporationID;

        return DB.GetSharesByShareholder (entityID);
    }

    public PyDataType GetShareholders (CallInformation call, PyInteger corporationID)
    {
        return DB.GetShareholders (corporationID);
    }

    [MustHaveCorporationRole(MLS.UI_CORP_DO_NOT_HAVE_ROLE_DIRECTOR, CorporationRole.Director)]
    public PyDataType MoveCompanyShares (CallInformation call, PyInteger corporationID, PyInteger to, PyInteger quantity)
    {
        // TODO: the WALLETKEY SHOULD BE SOMETHING ELSE?
        using (IWallet corporationWallet = WalletManager.AcquireWallet (call.Session.CorporationID, 2000))
        using (IWallet shareholderWallet = WalletManager.AcquireWallet (to, 2000))
        {
            // first make sure there's enough shares available
            int availableShares = DB.GetSharesForOwner (call.Session.CorporationID, call.Session.CorporationID);

            if (availableShares < quantity)
                throw new NotEnoughShares (quantity, availableShares);

            // get the shares the destination already has
            int currentShares = DB.GetSharesForOwner (call.Session.CorporationID, to);

            // make the transaction
            DB.UpdateShares (call.Session.CorporationID, to,                         quantity + currentShares);
            DB.UpdateShares (call.Session.CorporationID, call.Session.CorporationID, availableShares - quantity);

            // create the notifications
            OnShareChange changeForNewHolder = new OnShareChange (
                to, call.Session.CorporationID,
                currentShares == 0 ? null : currentShares, currentShares + quantity
            );
            OnShareChange changeForRestCorp = new OnShareChange (
                call.Session.CorporationID,
                call.Session.CorporationID, availableShares, availableShares - quantity
            );

            // send both notifications
            Notifications.NotifyCorporation (call.Session.CorporationID, changeForNewHolder);
            Notifications.NotifyCorporation (call.Session.CorporationID, changeForRestCorp);
        }

        return null;
    }

    public PyDataType MovePrivateShares (CallInformation call, PyInteger corporationID, PyInteger toShareholderID, PyInteger quantity)
    {
        int callerCharacterID = call.Session.CharacterID;

        // TODO: the WALLETKEY SHOULD BE SOMETHING ELSE?
        using (IWallet corporationWallet = WalletManager.AcquireWallet (callerCharacterID, 2000))
        using (IWallet shareholderWallet = WalletManager.AcquireWallet (toShareholderID, 2000))
        {
            // first make sure there's enough shares available
            int availableShares = DB.GetSharesForOwner (corporationID, callerCharacterID);

            if (availableShares < quantity)
                throw new NotEnoughShares (quantity, availableShares);

            // get the shares the destination already has
            int currentShares = DB.GetSharesForOwner (corporationID, toShareholderID);

            // make the transaction
            DB.UpdateShares (corporationID, toShareholderID,   quantity + currentShares);
            DB.UpdateShares (corporationID, callerCharacterID, availableShares - quantity);

            // create the notifications
            OnShareChange changeForNewHolder = new OnShareChange (
                toShareholderID, call.Session.CorporationID,
                currentShares == 0 ? null : currentShares, currentShares + quantity
            );
            OnShareChange changeForRestCorp = new OnShareChange (
                call.Session.CorporationID,
                call.Session.CorporationID, availableShares, availableShares - quantity
            );

            // send both notifications
            Notifications.NotifyCorporation (call.Session.CorporationID, changeForNewHolder);
            Notifications.NotifyCorporation (call.Session.CorporationID, changeForRestCorp);
        }

        return null;
    }

    public PyDataType GetMember (CallInformation call, PyInteger memberID)
    {
        return DB.GetMember (memberID, call.Session.CorporationID);
    }

    public PyDataType GetMembers (CallInformation call)
    {
        if (MembersSparseRowset is null)
        {
            // generate the sparse rowset
            SparseRowsetHeader rowsetHeader = DB.GetMembersSparseRowset (call.Session.CorporationID);

            PyDictionary dict = new PyDictionary {["realRowCount"] = rowsetHeader.Count};

            // create a service for handling it's calls
            MembersSparseRowset =
                new MembersSparseRowsetService (Corporation, DB, rowsetHeader, Notifications, BoundServiceManager, call.Session);

            rowsetHeader.BoundObjectIdentifier = MembersSparseRowset.MachoBindObject (dict, call.Session);
        }

        // ensure the bound service knows that this client is bound to it
        MembersSparseRowset.BindToSession (call.Session);

        // finally return the data
        return MembersSparseRowset.RowsetHeader;
    }

    public PyDataType GetOffices (CallInformation call)
    {
        if (OfficesSparseRowset is null)
        {
            // generate the sparse rowset
            SparseRowsetHeader rowsetHeader = DB.GetOfficesSparseRowset (call.Session.CorporationID);

            PyDictionary dict = new PyDictionary {["realRowCount"] = rowsetHeader.Count};

            // create a service for handling it's calls
            OfficesSparseRowset =
                new OfficesSparseRowsetService (Corporation, DB, rowsetHeader, BoundServiceManager, call.Session, Notifications);

            rowsetHeader.BoundObjectIdentifier = OfficesSparseRowset.MachoBindObject (dict, call.Session);
        }

        // ensure the bound service knows that this client is bound to it
        OfficesSparseRowset.BindToSession (call.Session);

        // finally return the data
        return OfficesSparseRowset.RowsetHeader;
    }

    public PyDataType GetRoleGroups (CallInformation call)
    {
        return Database.Rowset (CorporationDB.GET_ROLE_GROUPS);
    }

    public PyDataType GetRoles (CallInformation call)
    {
        return Database.Rowset (CorporationDB.GET_ROLES);
    }

    public PyDataType GetDivisions (CallInformation call)
    {
        // TODO: THESE MIGHT BE CUSTOMIZABLE (most likely)
        // TODO: BUT FOR NOW THESE SHOULD BE ENOUGH
        return Database.Rowset (CorporationDB.LIST_NPC_DIVISIONS);
    }

    public PyDataType GetTitles (CallInformation call)
    {
        // check if the corp is NPC and return placeholder data from the crpTitlesTemplate
        if (ItemRanges.IsNPCCorporationID (call.Session.CorporationID))
            return Database.DictRowList (CorporationDB.GET_TITLES_TEMPLATE);

        return Database.DictRowList (
            CorporationDB.GET_TITLES,
            new Dictionary <string, object> {{"_corporationID", call.Session.CorporationID}}
        );
    }

    public PyDataType GetStations (CallInformation call)
    {
        return DB.GetStations (call.Session.CorporationID);
    }

    [MustHaveCorporationRole(CorporationRole.Director)]
    public PyDataType GetMemberTrackingInfo (CallInformation call, PyInteger characterID)
    {
        return DB.GetMemberTrackingInfo (call.Session.CorporationID, characterID);
    }

    public PyDataType GetMemberTrackingInfoSimple (CallInformation call)
    {
        return DB.GetMemberTrackingInfoSimple (call.Session.CorporationID);
    }

    public PyDataType GetInfoWindowDataForChar (CallInformation call, PyInteger characterID)
    {
        DB.GetCorporationInformationForCharacter (
            characterID, out string title, out int titleMask,
            out int corporationID, out int? allianceID
        );

        Dictionary <int, string> titles        = DB.GetTitlesNames (call.Session.CorporationID);
        PyDictionary             dictForKeyVal = new PyDictionary ();

        int number = 0;

        foreach ((int _, string name) in titles)
            dictForKeyVal ["title" + ++number] = name;

        dictForKeyVal ["corpID"]     = corporationID;
        dictForKeyVal ["allianceID"] = allianceID;
        dictForKeyVal ["title"]      = title;

        return KeyVal.FromDictionary (dictForKeyVal);
    }

    public PyDataType GetMyApplications (CallInformation call)
    {
        return DB.GetCharacterApplications (call.Session.CharacterID);
    }

    public PyDataType GetLockedItemLocations (CallInformation call)
    {
        // this just returns a list of itemIDs (locations) that are locked
        // most likely used by the corp stuff for SOMETHING(tm)
        return new PyList <PyInteger> ();
    }

    public PyBool CanBeKickedOut (CallInformation call, PyInteger characterID)
    {
        int callerCharacterID = call.Session.CharacterID;

        // check for corporation stasis for this character

        // can personel manager kick other players? are there other conditions?
        return characterID == callerCharacterID || CorporationRole.Director.Is (call.Session.CorporationRole);
    }

    public PyDataType KickOutMember (CallInformation call, PyInteger characterID)
    {
        if (this.CanBeKickedOut (call, characterID) == false)
            return null;

        // TODO: IMPLEMENT THIS

        return null;
    }

    public PyString GetSuggestedTickerNames (CallInformation call, PyString corpName)
    {
        // get all the upercase letters
        string result = string.Concat (
            Regex.Matches (corpName.Value, @"\p{Lu}")
                 .Select (match => match.Value)
        );

        return new PyString (result.Substring (0, result.Length < 3 ? result.Length : 3));
    }

    private void ValidateAllianceName (PyString allianceName, PyString shortName)
    {
        // validate corporation name
        if (allianceName.Length < 4)
            throw new AllianceNameInvalidMinLength ();
        if (allianceName.Length > 24)
            throw new AllianceNameInvalidMaxLength ();
        if (shortName.Length < 3 || shortName.Length > 5)
            throw new AllianceShortNameInvalid ();

        // check if name is taken
        if (DB.IsAllianceNameTaken (allianceName))
            throw new AllianceNameInvalidTaken ();
        if (DB.IsShortNameTaken (shortName))
            throw new AllianceShortNameInvalidTaken ();

        // TODO: ADD SUPPORT FOR BANNED WORDS
        if (false)
            throw new AllianceNameInvalidBannedWord ();
    }

    [MustNotHaveSessionValue(Session.ALLIANCE_ID, typeof(AllianceCreateFailCorpInAlliance))]
    public PyDataType CreateAlliance (CallInformation call, PyString name, PyString shortName, PyString description, PyString url)
    {
        int callerCharacterID = call.Session.CharacterID;

        if (Corporation.CeoID != callerCharacterID)
            throw new OnlyActiveCEOCanCreateAlliance ();

        // TODO: CHECK FOR ACTIVE WARS AND THROW A CUSTOMERROR WITH THIS TEXT: UI_CORP_HINT7

        this.ValidateAllianceName (name, shortName);

        // TODO: PROPERLY IMPLEMENT THIS CHECK, RIGHT NOW THE CHARACTER AND THE CORPREGISTRY INSTANCES DO NOT HAVE TO BE LOADED ON THE SAME NODE
        // TODO: SWITCH UP THE CORPORATION CHANGE MECHANISM TO NOT RELY ON THE CHARACTER OBJECT SO THIS CAN BE DONE THROUGH THE DATABASE
        // TODO: DIRECTLY
        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

        // ensure empire control is trained and at least level 5
        character.EnsureSkillLevel (Types.EmpireControl, 5);

        // the alliance costs 1b ISK to establish, and that's taken from the corporation's wallet
        using (IWallet wallet = WalletManager.AcquireWallet (Corporation.ID, call.Session.CorpAccountKey, true))
        {
            // ensure there's enough balance
            wallet.EnsureEnoughBalance (Constants.AllianceCreationCost);

            // create the journal record for the alliance creation
            wallet.CreateJournalRecord (MarketReference.AllianceRegistrationFee, null, null, -Constants.AllianceCreationCost);

            // now create the alliance
            ItemEntity allianceItem = ItemFactory.CreateSimpleItem (
                name, (int) Types.Alliance, call.Session.CorporationID,
                ItemFactory.LocationSystem.ID, Flags.None, 1, false, true, 0, 0, 0,
                ""
            );

            int allianceID = allianceItem.ID;

            // unload the item as alliances shouldn't really be loaded anywhere
            ItemFactory.UnloadItem (allianceItem);

            // now record the alliance into the database
            Database.CrpAlliancesCreate (allianceID, shortName, description, url, call.Session.CorporationID, callerCharacterID);

            // delete any existant application to alliances
            this.DeleteAllianceApplicationIfExists ();

            Corporation.AllianceID     = allianceID;
            Corporation.ExecutorCorpID = Corporation.ID;
            Corporation.StartDate      = DateTime.UtcNow.ToFileTimeUtc ();
            Corporation.Persist ();

            // create the new chat channel
            ChatDB.CreateChannel (allianceID, allianceID, "System Channels\\Alliance", true);
            ChatDB.CreateChannel (allianceID, allianceID, "System Channels\\Alliance", false);

            foreach (int characterID in DB.GetMembersForCorp (ObjectID))
            {
                // join the player to the corp channel
                ChatDB.JoinEntityChannel (
                    allianceID, characterID, characterID == callerCharacterID ? ChatDB.CHATROLE_CREATOR : ChatDB.CHATROLE_CONVERSATIONALIST
                );
                ChatDB.JoinChannel (allianceID, characterID, characterID == callerCharacterID ? ChatDB.CHATROLE_CREATOR : ChatDB.CHATROLE_CONVERSATIONALIST);
            }

            // perform a session change for all the corp members available
            SessionManager.PerformSessionUpdate (Session.CORP_ID, ObjectID, new Session {[Session.ALLIANCE_ID] = allianceID});

            // TODO: CREATE BILLS REQUIRED FOR ALLIANCES, CHECK HOW THEY ARE INVOICED
        }

        return null;
    }

    private void ValidateCorporationName (PyString corporationName, PyString tickerName)
    {
        // validate corporation name
        if (corporationName.Length < 4)
            throw new CorpNameInvalidMinLength ();
        if (corporationName.Length > 24)
            throw new CorpNameInvalidMaxLength ();
        if (tickerName.Length < 2 || tickerName.Length > 4)
            throw new CorpTickerNameInvalid ();

        // check if name is taken
        if (DB.IsCorporationNameTaken (corporationName))
            throw new CorpNameInvalidTaken ();
        if (DB.IsTickerNameTaken (tickerName))
            throw new CorpTickerNameInvalidTaken ();

        // TODO: ADD SUPPORT FOR BANNED WORDS
        if (false)
            throw new CorpNameInvalidBannedWord ();
    }

    [MustHaveSessionValue(Session.STATION_ID, typeof(CanOnlyCreateCorpInStation))]
    [MustNotHaveCorporationRole(CorporationRole.Director, typeof(CEOCannotCreateCorporation))]
    [MustBeInStation]
    public PyDataType AddCorporation (
        CallInformation call, PyString  corporationName, PyString  tickerName, PyString   description,
        PyString  url,             PyDecimal taxRate,    PyInteger  shape1,   PyInteger       shape2, PyInteger shape3, PyInteger color1,
        PyInteger color2,          PyInteger color3,     PyDataType typeface
    )
    {
        // TODO: CHECK FOR POS AS ITS NOT POSSIBLE TO CREATE CORPORATIONS THERE
        this.ValidateCorporationName (corporationName, tickerName);

        int stationID              = call.Session.StationID;
        int corporationStartupCost = -Constants.CorporationStartupCost;
        int callerCharacterID      = call.Session.CharacterID;

        // TODO: PROPERLY IMPLEMENT THIS CHECK, RIGHT NOW THE CHARACTER AND THE CORPREGISTRY INSTANCES DO NOT HAVE TO BE LOADED ON THE SAME NODE
        // TODO: SWITCH UP THE CORPORATION CHANGE MECHANISM TO NOT RELY ON THE CHARACTER OBJECT SO THIS CAN BE DONE THROUGH THE DATABASE
        // TODO: DIRECTLY
        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

        this.CalculateCorporationLimits (character, out int maximumMembers, out int allowedMemberRaceIDs);

        // ensure the character has the required skills
        long corporationManagementLevel = character.GetSkillLevel (Types.CorporationManagement);

        if (corporationManagementLevel < 1)
            throw new PlayerCantCreateCorporation (-corporationStartupCost);

        try
        {
            // acquire the wallet for this character too
            using (IWallet wallet = WalletManager.AcquireWallet (character.ID, WalletKeys.MAIN))
            {
                // ensure there's enough balance
                wallet.EnsureEnoughBalance (-corporationStartupCost);

                // create the corporation in the corporation table
                int corporationID = DB.CreateCorporation (
                    corporationName, description, tickerName, url, taxRate, callerCharacterID,
                    stationID, maximumMembers, (int) call.Session.RaceID, allowedMemberRaceIDs,
                    shape1, shape2, shape3, color1, color2, color3, typeface as PyString
                );
                // create default titles
                DB.CreateDefaultTitlesForCorporation (corporationID);
                // create the record in the journal
                wallet.CreateJournalRecord (MarketReference.CorporationRegistrationFee, null, null, corporationStartupCost);

                // leave the old corporation channels first
                ChatDB.LeaveChannel (call.Session.CorporationID, character.ID);
                ChatDB.LeaveEntityChannel (call.Session.CorporationID, character.ID);
                // create the required chat channels
                ChatDB.CreateChannel (corporationID, corporationID, "System Channels\\Corp", true);
                ChatDB.CreateChannel (corporationID, corporationID, "System Channels\\Corp", false);
                // join the player to the corp channel
                ChatDB.JoinEntityChannel (corporationID, callerCharacterID, ChatDB.CHATROLE_CREATOR);
                ChatDB.JoinChannel (corporationID, callerCharacterID, ChatDB.CHATROLE_CREATOR);
                // build the notification of corporation change
                OnCorporationMemberChanged change = new OnCorporationMemberChanged (character.ID, call.Session.CorporationID, corporationID);
                // start the session change for the client
                Session update = new Session
                {
                    CorporationID   = corporationID,
                    CorporationRole = long.MaxValue, // this gives all the permissions to the character
                    RolesAtBase     = long.MaxValue,
                    RolesAtOther    = long.MaxValue,
                    RolesAtHQ       = long.MaxValue,
                    RolesAtAll      = long.MaxValue
                };

                // update the character to reflect the new ownership
                character.CorporationID = corporationID;
                character.Roles         = long.MaxValue;
                character.RolesAtBase   = long.MaxValue;
                character.RolesAtHq     = long.MaxValue;
                character.RolesAtOther  = long.MaxValue;
                character.TitleMask     = ushort.MaxValue;
                // ensure the database reflects these changes
                CharacterDB.UpdateCharacterRoles (
                    character.ID, long.MaxValue, long.MaxValue, long.MaxValue,
                    long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, ushort.MaxValue
                );
                CharacterDB.UpdateCharacterBlockRole (character.ID, 0);
                CharacterDB.SetCharacterStasisTimer (character.ID, null);
                character.CorporationDateTime = DateTime.UtcNow.ToFileTimeUtc ();
                // notify cluster about the corporation changes
                Notifications.NotifyCorporation (change.OldCorporationID, change);
                Notifications.NotifyCorporation (change.NewCorporationID, change);
                // create default wallets
                WalletManager.CreateWallet (corporationID, WalletKeys.MAIN,    0.0);
                WalletManager.CreateWallet (corporationID, WalletKeys.SECOND,  0.0);
                WalletManager.CreateWallet (corporationID, WalletKeys.THIRD,   0.0);
                WalletManager.CreateWallet (corporationID, WalletKeys.FOURTH,  0.0);
                WalletManager.CreateWallet (corporationID, WalletKeys.FIFTH,   0.0);
                WalletManager.CreateWallet (corporationID, WalletKeys.SIXTH,   0.0);
                WalletManager.CreateWallet (corporationID, WalletKeys.SEVENTH, 0.0);
                // create the employment record for the character
                CharacterDB.CreateEmploymentRecord (character.ID, corporationID, DateTime.UtcNow.ToFileTimeUtc ());
                // create company shares too!
                DB.UpdateShares (corporationID, corporationID, 1000);

                // set the default wallet for the character
                update.CorpAccountKey = WalletKeys.MAIN;

                // send the corporation update
                SessionManager.PerformSessionUpdate (Session.CHAR_ID, callerCharacterID, update);

                character.Persist ();

                // load the corporation item
                ItemFactory.LoadItem <Corporation> (corporationID);
            }

            return null;
        }
        catch (NotEnoughMoney)
        {
            throw new PlayerCantCreateCorporation (corporationStartupCost);
        }
    }

    private void ValidateDivisionName (string name, int divisionNumber)
    {
        if (name.Length < 3)
            throw new UserError ("CorpDiv" + divisionNumber + "NameInvalidMinLength");
        if (name.Length > 24)
            throw new UserError ("CorpDiv" + divisionNumber + "NameInvalidMaxLength");

        // TODO: ADD SUPPORT FOR BANNED WORDS
        if (false)
            throw new UserError ("CorpDiv" + divisionNumber + "NameInvalidBannedWord");
    }

    [MustHaveCorporationRole(CorporationRole.Director)]
    public PyDataType UpdateDivisionNames (
        CallInformation call, PyString        division1, PyString division2, PyString division3,
        PyString        division4, PyString division5, PyString division6, PyString division7, PyString wallet1,
        PyString        wallet2,   PyString wallet3,   PyString wallet4,   PyString wallet5,   PyString wallet6, PyString wallet7
    )
    {
        if (call.Session.CorporationID != Corporation.ID)
            return null;

        // validate division names
        this.ValidateDivisionName (division1, 1);
        this.ValidateDivisionName (division2, 2);
        this.ValidateDivisionName (division3, 3);
        this.ValidateDivisionName (division4, 4);
        this.ValidateDivisionName (division5, 5);
        this.ValidateDivisionName (division6, 6);
        this.ValidateDivisionName (division7, 7);

        // generate update notification
        OnCorporationChanged change = new OnCorporationChanged (Corporation.ID);

        if (Corporation.Division1 != division1)
            change.AddChange ("division1", Corporation.Division1, division1);
        if (Corporation.Division2 != division2)
            change.AddChange ("division2", Corporation.Division2, division2);
        if (Corporation.Division3 != division3)
            change.AddChange ("division3", Corporation.Division3, division3);
        if (Corporation.Division4 != division4)
            change.AddChange ("division4", Corporation.Division4, division4);
        if (Corporation.Division5 != division5)
            change.AddChange ("division5", Corporation.Division5, division5);
        if (Corporation.Division6 != division6)
            change.AddChange ("division6", Corporation.Division6, division6);
        if (Corporation.Division7 != division7)
            change.AddChange ("division7", Corporation.Division7, division7);

        if (Corporation.WalletDivision1 != wallet1)
            change.AddChange ("walletDivision1", Corporation.WalletDivision1, wallet1);
        if (Corporation.WalletDivision2 != wallet2)
            change.AddChange ("walletDivision2", Corporation.WalletDivision2, wallet2);
        if (Corporation.WalletDivision3 != wallet3)
            change.AddChange ("walletDivision3", Corporation.WalletDivision3, wallet3);
        if (Corporation.WalletDivision4 != wallet4)
            change.AddChange ("walletDivision4", Corporation.WalletDivision4, wallet4);
        if (Corporation.WalletDivision5 != wallet5)
            change.AddChange ("walletDivision5", Corporation.WalletDivision5, wallet5);
        if (Corporation.WalletDivision6 != wallet6)
            change.AddChange ("walletDivision6", Corporation.WalletDivision6, wallet6);
        if (Corporation.WalletDivision7 != wallet7)
            change.AddChange ("walletDivision7", Corporation.WalletDivision7, wallet7);

        Corporation.Division1       = division1;
        Corporation.Division2       = division2;
        Corporation.Division3       = division3;
        Corporation.Division4       = division4;
        Corporation.Division5       = division5;
        Corporation.Division6       = division6;
        Corporation.Division7       = division7;
        Corporation.WalletDivision1 = wallet1;
        Corporation.WalletDivision2 = wallet2;
        Corporation.WalletDivision3 = wallet3;
        Corporation.WalletDivision4 = wallet4;
        Corporation.WalletDivision5 = wallet5;
        Corporation.WalletDivision6 = wallet6;
        Corporation.WalletDivision7 = wallet7;

        // update division names
        DB.UpdateDivisions (
            call.Session.CorporationID,
            division1, division2, division3, division4, division5, division6, division7,
            wallet1, wallet2, wallet3, wallet4, wallet5, wallet6, wallet7
        );

        // notify all the players in the corp
        Notifications.NotifyCorporation (call.Session.CorporationID, change);

        return null;
    }

    [MustHaveCorporationRole(CorporationRole.Director)]
    public PyDataType UpdateCorporation (CallInformation call, PyString newDescription, PyString newUrl, PyDecimal newTax)
    {
        if (call.Session.CorporationID != Corporation.ID)
            return null;

        // update information in the database
        DB.UpdateCorporation (call.Session.CorporationID, newDescription, newUrl, newTax);

        // generate update notification
        OnCorporationChanged change =
            new OnCorporationChanged (call.Session.CorporationID)
                .AddChange ("description", Corporation.Description, newDescription)
                .AddChange ("url",         Corporation.Url,         newUrl)
                .AddChange ("taxRate",     Corporation.TaxRate,     newTax)
                .AddChange ("ceoID",       Corporation.CeoID,       Corporation.CeoID);

        Corporation.Description = newDescription;
        Corporation.Url         = newUrl;
        Corporation.TaxRate     = newTax;

        Notifications.NotifyCorporation (Corporation.ID, change);

        return null;
    }

    [MustHaveCorporationRole(CorporationRole.Director)]
    public PyDataType GetMemberTrackingInfo (CallInformation call)
    {
        return DB.GetMemberTrackingInfo (call.Session.CorporationID);
    }

    public PyDataType SetAccountKey (CallInformation call, PyInteger accountKey)
    {
        if (WalletManager.IsTakeAllowed (call.Session, accountKey, call.Session.CorporationID))
            SessionManager.PerformSessionUpdate (
                Session.CHAR_ID, call.Session.CharacterID, new Session {[Session.CORP_ACCOUNT_KEY] = accountKey}
            );

        return null;
    }

    private void PayoutDividendsToShareholders (double totalAmount, Session session)
    {
        double                pricePerShare = totalAmount / Corporation.Shares;
        Dictionary <int, int> shares        = DB.GetShareholdersList (Corporation.ID);

        foreach ((int ownerID, int sharesCount) in shares)
        {
            // send evemail to the owner
            MailManager.SendMail (
                session.CorporationID,
                ownerID,
                $"Dividend from {Corporation.Name}",
                $"<a href=\"showinfo:2//{Corporation.ID}\">{Corporation.Name}</a> may have credited your account as part of a total payout of <b>{totalAmount} ISK</b> to their shareholders. The amount awarded is based upon the number of shares you hold, in relation to the total number of shares issued by the company."
            );

            // TODO: INCLUDE WETHER THE SHAREHOLDER IS A CORPORATION OR NOT, MAYBE CREATE A CUSTOM OBJECT FOR THIS
            // calculate amount to give and acquire it's wallet
            using (IWallet dest = WalletManager.AcquireWallet (ownerID, WalletKeys.MAIN))
            {
                dest.CreateJournalRecord (MarketReference.CorporationDividendPayment, ownerID, Corporation.ID, pricePerShare * sharesCount);
            }
        }
    }

    private void PayoutDividendsToMembers (double totalAmount, Session session)
    {
        double pricePerMember = totalAmount / Corporation.MemberCount;

        foreach (int characterID in DB.GetMembersForCorp (Corporation.ID))
            using (IWallet dest = WalletManager.AcquireWallet (characterID, WalletKeys.MAIN))
            {
                dest.CreateJournalRecord (MarketReference.CorporationDividendPayment, characterID, Corporation.ID, pricePerMember);
            }

        // send evemail to corporation mail as this will be received by all the members
        MailManager.SendMail (
            session.CorporationID,
            session.CorporationID,
            $"Dividend from {Corporation.Name}",
            $"<a href=\"showinfo:2//{Corporation.ID}\">{Corporation.Name}</a> may have credited your account as part of a total payout of <b>{totalAmount} ISK</b> to their corporation members. The amount awarded is split evenly between all members of the corporation."
        );
    }

    public PyDataType PayoutDividend (CallInformation call, PyInteger payShareholders, PyInteger amount)
    {
        // check if the player is the CEO
        if (Corporation.CeoID != call.Session.CharacterID)
            throw new OnlyCEOCanPayoutDividends ();

        using (IWallet wallet = WalletManager.AcquireWallet (call.Session.CorporationID, call.Session.CorpAccountKey, true))
        {
            // check if there's enough cash left
            wallet.EnsureEnoughBalance (amount);
            // make transaction
            wallet.CreateJournalRecord (MarketReference.CorporationDividendPayment, ItemFactory.OwnerBank.ID, null, amount);
        }

        if (payShareholders == 1)
            this.PayoutDividendsToShareholders (amount, call.Session);
        else
            this.PayoutDividendsToMembers (amount, call.Session);

        return null;
    }

    private void CalculateCorporationLimits (Character ceo, out int maximumMembers, out int allowedRaceIDs)
    {
        int corporationManagementLevel = (int) ceo.GetSkillLevel (Types.CorporationManagement); // +10 members per level 
        int ethnicRelationsLevel =
            (int) ceo.GetSkillLevel (Types.EthnicRelations); // 20% more members of other races based off the character's corporation levels TODO: SUPPORT THIS!
        int empireControlLevel      = (int) ceo.GetSkillLevel (Types.EmpireControl); // adds +200 members per level
        int megacorpManagementLevel = (int) ceo.GetSkillLevel (Types.MegacorpManagement); // adds +50 members per level
        int sovereigntyLevel        = (int) ceo.GetSkillLevel (Types.Sovereignty); // adds +1000 members per level

        maximumMembers = corporationManagementLevel * 10 + empireControlLevel * 200 +
                         megacorpManagementLevel * 50 + sovereigntyLevel * 1000;

        allowedRaceIDs = ethnicRelationsLevel > 0 ? 63 : Ancestries [ceo.AncestryID].Bloodline.RaceID;
    }

    public PyDataType UpdateCorporationAbilities (CallInformation call)
    {
        if (Corporation.CeoID != call.Session.CharacterID)
            throw new CrpAccessDenied (MLS.UI_CORP_ACCESSDENIED12);

        // TODO: CHANGE THIS UP SO IT DOESN'T REQUIRE THE OBJECT IN MEMORY AS CORPREGISTRY MIGHT NOT BE ON THE SAME NODE AS OUR CHARACTER
        Character character = ItemFactory.GetItem <Character> (call.Session.CharacterID);

        this.CalculateCorporationLimits (character, out int maximumMembers, out int allowedMemberRaceIDs);

        // update the abilities of the corporation
        OnCorporationChanged change = new OnCorporationChanged (Corporation.ID);

        if (Corporation.MemberLimit != maximumMembers)
        {
            change.AddChange ("memberLimit", Corporation.MemberLimit, maximumMembers);
            Corporation.MemberLimit = maximumMembers;
        }

        if (Corporation.AllowedMemberRaceIDs != allowedMemberRaceIDs)
        {
            change.AddChange ("allowedMemberRaceIDs", Corporation.AllowedMemberRaceIDs, allowedMemberRaceIDs);
            Corporation.AllowedMemberRaceIDs = allowedMemberRaceIDs;
        }

        if (change.Changes.Count > 0)
        {
            DB.UpdateMemberLimits (Corporation.ID, Corporation.MemberLimit, Corporation.AllowedMemberRaceIDs);

            Notifications.NotifyCorporation (Corporation.ID, change);

            return true;
        }

        return null;
    }

    public PyDataType GetRecruitmentAdsForCorporation (CallInformation call)
    {
        return DB.GetRecruitmentAds (null, null, null, null, null, null, null, Corporation.ID);
    }

    public PyDataType UpdateMembers (CallInformation call, PyDataType rowset)
    {
        // parse the rowset to have proper access to the data
        Rowset parsed = rowset;
        // position of each header
        int characterID           = 0;
        int title                 = 1;
        int divisionID            = 2;
        int squadronID            = 3;
        int roles                 = 4;
        int grantableRoles        = 5;
        int rolesAtHQ             = 6;
        int grantableRolesAtHQ    = 7;
        int rolesAtBase           = 8;
        int grantableRolesAtBase  = 9;
        int rolesAtOther          = 10;
        int grantableRolesAtOther = 11;
        int baseID                = 12;
        int titleMask             = 13;

        foreach (PyList entry in parsed.Rows)
            this.UpdateMember (
                call, 
                entry [characterID] as PyInteger,
                entry [title] as PyString,
                entry [divisionID] as PyInteger,
                entry [squadronID] as PyInteger,
                entry [roles] as PyInteger,
                entry [grantableRoles] as PyInteger,
                entry [rolesAtHQ] as PyInteger,
                entry [grantableRolesAtHQ] as PyInteger,
                entry [rolesAtBase] as PyInteger,
                entry [grantableRolesAtBase] as PyInteger,
                entry [rolesAtOther] as PyInteger,
                entry [grantableRolesAtOther] as PyInteger,
                entry [baseID] as PyInteger,
                entry [titleMask] as PyInteger,
                0
            );

        return null;
    }

    public PyDataType UpdateMember (
        CallInformation call, PyInteger       characterID,           PyString  title,       PyInteger divisionID,
        PyInteger       squadronID,            PyInteger roles,       PyInteger grantableRoles,       PyInteger rolesAtHQ,
        PyInteger       grantableRolesAtHQ,    PyInteger rolesAtBase, PyInteger grantableRolesAtBase, PyInteger rolesAtOther,
        PyInteger       grantableRolesAtOther, PyInteger baseID,      PyInteger titleMask,            PyInteger blockRoles
    )
    {
        // TODO: HANDLE DIVISION AND SQUADRON CHANGES
        int       callerCharacterID = call.Session.CharacterID;
        Character character         = ItemFactory.GetItem <Character> (callerCharacterID);
        Session   update            = new Session ();

        // get current roles for that character
        CharacterDB.GetCharacterRoles (
            characterID, out long currentRoles, out long currentRolesAtBase,
            out long currentRolesAtHQ, out long currentRolesAtOther, out long currentGrantableRoles,
            out long currentGrantableRolesAtBase, out long currentGrantableRolesAtHQ,
            out long currentGrantableRolesAtOther, out int? currentBlockRoles, out int? currentBaseID,
            out int currentTitleMask
        );

        // the only modification a member can perform on itself is blocking roles
        if (characterID == callerCharacterID && blockRoles != currentBlockRoles)
        {
            CharacterDB.UpdateCharacterBlockRole (characterID, blockRoles == 1 ? 1 : null);

            long? currentStasisTimer = CharacterDB.GetCharacterStasisTimer (characterID);

            if (blockRoles == 1)
            {
                if (currentStasisTimer > 0)
                {
                    long currentTime    = DateTime.UtcNow.ToFileTimeUtc ();
                    long stasisTimerEnd = (long) currentStasisTimer + TimeSpan.FromHours (24).Ticks;

                    if (stasisTimerEnd > currentTime)
                    {
                        int hoursTimerStarted = (int) ((currentTime - currentStasisTimer) / TimeSpan.TicksPerHour);
                        int hoursLeft         = (int) ((stasisTimerEnd - currentTime) / TimeSpan.TicksPerHour);

                        throw new CrpCantQuitNotCompletedStasisPeriod (characterID, hoursTimerStarted, hoursLeft);
                    }
                }

                // store the new roles and title mask
                CharacterDB.UpdateCharacterRoles (
                    characterID, 0, 0, 0, 0,
                    0, 0, 0, 0, 0
                );

                roles                 = 0;
                rolesAtHQ             = 0;
                rolesAtBase           = 0;
                rolesAtOther          = 0;
                grantableRoles        = 0;
                grantableRolesAtHQ    = 0;
                grantableRolesAtBase  = 0;
                grantableRolesAtOther = 0;
                titleMask             = 0;

                // TODO: WEIRD HACK TO ENSURE THAT THE CORPORATION WINDOW UPDATES WITH THE CHANGE
                // TODO: THERE MIGHT BE SOMETHING ELSE WE CAN DO, BUT THIS WORKS FOR NOW
                if (call.Session.CorporationRole == 0)
                    update.CorporationRole = ~long.MaxValue;
                else
                    update.CorporationRole = 0;

                update.RolesAtAll   = 0;
                update.RolesAtBase  = 0;
                update.RolesAtOther = 0;
                update.RolesAtHQ    = 0;

                character.TitleMask = 0;
            }

            Notifications.NotifyNode (
                ItemFactory.ItemDB.GetItemNode (characterID),
                new OnCorporationMemberUpdated (
                    characterID, currentRoles, grantableRoles, rolesAtHQ, grantableRolesAtHQ,
                    rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther, currentBaseID,
                    blockRoles == 1 ? 1 : null, titleMask
                )
            );

            if (MembersSparseRowset is not null)
            {
                PyDictionary <PyString, PyTuple> changes = new PyDictionary <PyString, PyTuple>
                {
                    ["roles"] = new PyTuple (2)
                    {
                        [0] = currentRoles,
                        [1] = roles
                    },
                    ["rolesAtHQ"] = new PyTuple (2)
                    {
                        [0] = currentRolesAtHQ,
                        [1] = rolesAtHQ
                    },
                    ["rolesAtBase"] = new PyTuple (2)
                    {
                        [0] = currentRolesAtBase,
                        [1] = rolesAtBase
                    },
                    ["rolesAtOther"] = new PyTuple (2)
                    {
                        [0] = currentRolesAtOther,
                        [1] = rolesAtOther
                    },
                    ["grantableRoles"] = new PyTuple (2)
                    {
                        [0] = currentGrantableRoles,
                        [1] = grantableRoles
                    },
                    ["grantableRolesAtHQ"] = new PyTuple (2)
                    {
                        [0] = currentGrantableRolesAtHQ,
                        [1] = grantableRolesAtHQ
                    },
                    ["grantableRolesAtBase"] = new PyTuple (2)
                    {
                        [0] = currentRolesAtBase,
                        [1] = grantableRolesAtBase
                    },
                    ["grantableRolesAtOther"] = new PyTuple (2)
                    {
                        [0] = currentRolesAtOther,
                        [1] = grantableRolesAtOther
                    },
                    ["baseID"] = new PyTuple (2)
                    {
                        [0] = currentBaseID,
                        [1] = baseID
                    },
                    ["blockRoles"] = new PyTuple (2)
                    {
                        [0] = blockRoles == 0 ? null : 1,
                        [1] = blockRoles == 1 ? 1 : null
                    },
                    ["titleMask"] = new PyTuple (2)
                    {
                        [0] = currentTitleMask,
                        [1] = titleMask
                    }
                };

                MembersSparseRowset.UpdateRow (characterID, changes);
            }

            // update the stasis timer
            if (currentStasisTimer == null)
                CharacterDB.SetCharacterStasisTimer (characterID, blockRoles == 1 ? DateTime.UtcNow.ToFileTimeUtc () : null);

            // send the session change
            SessionManager.PerformSessionUpdate (Session.CHAR_ID, characterID, update);

            return null;
        }

        if (currentBlockRoles == 1)
            throw new CrpRolesDenied (characterID);

        // if the character is not a director, it cannot grant grantable roles (duh)
        if (CorporationRole.Director.Is (call.Session.CorporationRole) == false && (grantableRoles != 0 ||
                                                                                    grantableRolesAtBase != 0 || grantableRolesAtOther != 0 ||
                                                                                    grantableRolesAtHQ != 0))
            throw new CrpAccessDenied (MLS.UI_CORP_DO_NOT_HAVE_ROLE_DIRECTOR);

        // get differences with the current specified roles, we want to know what changes
        long rolesDifference        = currentRoles ^ roles;
        long rolesAtBaseDifference  = currentRolesAtBase ^ rolesAtBase;
        long rolesAtHQDifference    = currentRolesAtHQ ^ rolesAtHQ;
        long rolesAtOtherDifference = currentRolesAtOther ^ rolesAtOther;

        // ensure the character has permissions to modify those roles
        if ((rolesDifference & character.GrantableRoles) != rolesDifference ||
            (rolesAtBaseDifference & character.GrantableRolesAtBase) != rolesAtBaseDifference ||
            (rolesAtOtherDifference & character.GrantableRolesAtOther) != rolesAtOtherDifference ||
            (rolesAtHQDifference & character.GrantableRolesAtHQ) != rolesAtHQDifference)
            throw new CrpAccessDenied (MLS.UI_CORP_INSUFFICIENT_RIGHTS_TO_EDIT_MEMBERS_DETAILS);

        // if the new role is director then all the roles must be granted
        if (roles == 1)
        {
            roles        = long.MaxValue;
            rolesAtHQ    = long.MaxValue;
            rolesAtBase  = long.MaxValue;
            rolesAtOther = long.MaxValue;
        }

        // store the new roles and title mask
        CharacterDB.UpdateCharacterRoles (
            characterID, roles, rolesAtHQ, rolesAtBase, rolesAtOther,
            grantableRoles, grantableRolesAtHQ, grantableRolesAtBase, grantableRolesAtOther, titleMask ?? 0
        );

        // let the sparse rowset know that a change was done, this should refresh the character information
        if (MembersSparseRowset is not null)
        {
            PyDictionary <PyString, PyTuple> changes = new PyDictionary <PyString, PyTuple>
            {
                ["roles"] = new PyTuple (2)
                {
                    [0] = currentRoles,
                    [1] = roles
                },
                ["rolesAtHQ"] = new PyTuple (2)
                {
                    [0] = currentRolesAtHQ,
                    [1] = rolesAtHQ
                },
                ["rolesAtBase"] = new PyTuple (2)
                {
                    [0] = currentRolesAtBase,
                    [1] = rolesAtBase
                },
                ["rolesAtOther"] = new PyTuple (2)
                {
                    [0] = currentRolesAtOther,
                    [1] = rolesAtOther
                },
                ["grantableRoles"] = new PyTuple (2)
                {
                    [0] = currentGrantableRoles,
                    [1] = grantableRoles
                },
                ["grantableRolesAtHQ"] = new PyTuple (2)
                {
                    [0] = currentGrantableRolesAtHQ,
                    [1] = grantableRolesAtHQ
                },
                ["grantableRolesAtBase"] = new PyTuple (2)
                {
                    [0] = currentRolesAtBase,
                    [1] = grantableRolesAtBase
                },
                ["grantableRolesAtOther"] = new PyTuple (2)
                {
                    [0] = currentRolesAtOther,
                    [1] = grantableRolesAtOther
                },
                ["baseID"] = new PyTuple (2)
                {
                    [0] = currentBaseID,
                    [1] = baseID
                }
            };

            if (titleMask is not null)
                changes ["titleMask"] = new PyTuple (2)
                {
                    [0] = currentTitleMask,
                    [1] = titleMask
                };

            if (blockRoles is not null)
                changes ["blockRoles"] = new PyTuple (2)
                {
                    [0] = null,
                    [1] = blockRoles
                };

            MembersSparseRowset.UpdateRow (characterID, changes);
        }

        // notify the node about the changes
        Notifications.NotifyNode (
            ItemFactory.ItemDB.GetItemNode (characterID),
            new OnCorporationMemberUpdated (
                characterID, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ,
                rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther, baseID, currentBlockRoles, titleMask ?? 0
            )
        );

        // check if the character is connected and update it's session
        // this also allows to notify the node where the character is loaded
        if (Sessions.ContainsKey (characterID) == false)
            return null;

        // get the title roles and calculate current roles for the session
        DB.GetTitleInformation (
            character.CorporationID, character.TitleMask,
            out long titleRoles, out long titleRolesAtHQ, out long titleRolesAtBase, out long titleRolesAtOther,
            out long titleGrantableRoles, out long titleGrantableRolesAtHQ, out long titleGrantableRolesAtBase,
            out long titleGrantableRolesAtOther, out _
        );

        // update the roles on the session and send the session change to the player
        update.CorporationRole = roles | titleRoles;
        update.RolesAtOther    = rolesAtOther | titleRolesAtOther;
        update.RolesAtHQ       = rolesAtHQ | titleRolesAtHQ;
        update.RolesAtBase     = rolesAtBase | titleRolesAtBase;
        update.RolesAtAll      = update.CorporationRole | update.RolesAtOther | update.RolesAtHQ | update.RolesAtBase;
        // notify the session change
        SessionManager.PerformSessionUpdate (Session.CHAR_ID, characterID, update);

        // TODO: CHECK THAT NEW BASE ID IS VALID

        return null;
    }

    public PyDataType CanLeaveCurrentCorporation (CallInformation call)
    {
        int   characterID = call.Session.CharacterID;
        long? stasisTimer = CharacterDB.GetCharacterStasisTimer (characterID);

        try
        {
            if (call.Session.CorporationID <= ItemRanges.NPC_CORPORATION_ID_MAX)
                if (call.Session.WarFactionID is null)
                    throw new CrpCantQuitDefaultCorporation ();

            long currentTime = DateTime.UtcNow.ToFileTimeUtc ();

            if (stasisTimer is null &&
                (call.Session.CorporationRole > 0 ||
                 call.Session.RolesAtAll > 0 ||
                 call.Session.RolesAtBase > 0 ||
                 call.Session.RolesAtOther > 0 ||
                 call.Session.RolesAtHQ > 0)
               )
                throw new CrpCantQuitNotInStasis (
                    characterID,
                    call.Session.CorporationRole | call.Session.RolesAtAll | call.Session.RolesAtBase | call.Session.RolesAtOther | call.Session.RolesAtHQ
                );

            if (stasisTimer is not null)
            {
                long stasisTimerEnd = (long) stasisTimer + TimeSpan.FromHours (24).Ticks;

                if (stasisTimerEnd > DateTime.UtcNow.ToFileTimeUtc ())
                {
                    int hoursTimerStarted = (int) ((currentTime - stasisTimer) / TimeSpan.TicksPerHour);
                    int hoursLeft         = (int) ((stasisTimerEnd - currentTime) / TimeSpan.TicksPerHour);

                    throw new CrpCantQuitNotCompletedStasisPeriodIsBlocked (characterID, hoursTimerStarted, hoursLeft);
                }
            }

            return new PyTuple (3)
            {
                [0] = true,
                [1] = null,
                [2] = null
            };
        }
        catch (UserError e)
        {
            return new PyTuple (3)
            {
                [0] = false, // can leave
                [1] = e.Reason, // error message
                [2] = e.Dictionary
            };
        }
    }

    public PyDataType CreateRecruitmentAd (
        CallInformation call, PyInteger       days, PyInteger stationID, PyInteger raceMask, PyInteger typeMask, PyInteger allianceID, PyInteger skillpoints, PyString description
    )
    {
        Station station           = ItemFactory.GetStaticStation (stationID);
        int     callerCharacterID = call.Session.CharacterID;
        long    price             = CorporationAdvertisementFlatFee + CorporationAdvertisementDailyRate * days;

        // TODO: ENSURE stationID MATCHES ONE OF OUR OFFICES FOR THE ADVERT TO BE CREATED
        // get the current wallet and check if there's enough money on it
        using (IWallet wallet = WalletManager.AcquireWallet (call.Session.CorporationID, call.Session.CorpAccountKey, true))
        {
            wallet.EnsureEnoughBalance (price);
            wallet.CreateJournalRecord (MarketReference.CorporationAdvertisementFee, callerCharacterID, null, null, price);
        }

        // now create the ad
        ulong adID = DB.CreateRecruitmentAd (stationID, days, call.Session.CorporationID, typeMask, raceMask, description, skillpoints);
        // create the notification and notify everyone at that station
        OnCorporationRecruitmentAdChanged changes = new OnCorporationRecruitmentAdChanged (call.Session.CorporationID, adID);
        // add the fields for the recruitment ad change
        changes
            .AddValue ("adID",            null, adID)
            .AddValue ("corporationID",   null, call.Session.CorporationID)
            .AddValue ("channelID",       null, call.Session.CorporationID)
            .AddValue ("typeMask",        null, typeMask)
            .AddValue ("description",     null, description)
            .AddValue ("stationID",       null, stationID)
            .AddValue ("raceMask",        null, raceMask)
            .AddValue ("allianceID",      null, call.Session.AllianceID)
            .AddValue ("expiryDateTime",  null, DateTime.UtcNow.AddDays (days).ToFileTimeUtc ())
            .AddValue ("createDateTime",  null, DateTime.UtcNow.ToFileTimeUtc ())
            .AddValue ("regionID",        null, station.RegionID)
            .AddValue ("solarSystemID",   null, station.SolarSystemID)
            .AddValue ("constellationID", null, station.ConstellationID)
            .AddValue ("skillPoints",     null, skillpoints);

        // TODO: MAYBE NOTIFY CHARACTERS IN THE STATION?
        // notify corporation members
        Notifications.NotifyCorporation (call.Session.CorporationID, changes);

        return null;
    }

    [MustHaveCorporationRole(MLS.UI_CORP_DO_NOT_HAVE_ROLE_DIRECTOR, CorporationRole.Director)]
    public PyDataType UpdateTitles (CallInformation call, PyObjectData rowset)
    {
        Rowset list = rowset;

        // update the changed titles first
        foreach (PyList entry in list.Rows)
        {
            // titleID, titleName, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther
            int    titleID               = entry [0] as PyInteger;
            string newName               = entry [1] as PyString;
            long   roles                 = entry [2] as PyInteger;
            long   grantableRoles        = entry [3] as PyInteger;
            long   rolesAtHQ             = entry [4] as PyInteger;
            long   grantableRolesAtHQ    = entry [5] as PyInteger;
            long   rolesAtBase           = entry [6] as PyInteger;
            long   grantableRolesAtBase  = entry [7] as PyInteger;
            long   rolesAtOther          = entry [8] as PyInteger;
            long   grantableRolesAtOther = entry [9] as PyInteger;

            // get previous roles first
            DB.GetTitleInformation (
                call.Session.CorporationID, titleID,
                out long titleRoles, out long titleRolesAtHQ, out long titleRolesAtBase, out long titleRolesAtOther,
                out long titleGrantableRoles, out long titleGrantableRolesAtHQ, out long titleGrantableRolesAtBase,
                out long titleGrantableRolesAtOther, out string titleName
            );

            // store the new information
            DB.UpdateTitle (
                call.Session.CorporationID, titleID, newName, roles, grantableRoles, rolesAtHQ,
                grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther
            );

            // notify everyone about the title change
            Notifications.NotifyCorporation (
                call.Session.CorporationID,
                new OnTitleChanged (call.Session.CorporationID, titleID)
                    .AddChange ("titleName",             titleName,                  newName)
                    .AddChange ("roles",                 titleRoles,                 roles)
                    .AddChange ("grantableRoles",        titleGrantableRoles,        grantableRoles)
                    .AddChange ("rolesAtHQ",             titleRolesAtHQ,             rolesAtHQ)
                    .AddChange ("grantableRolesAtHQ",    titleGrantableRolesAtHQ,    grantableRolesAtHQ)
                    .AddChange ("rolesAtBase",           titleRolesAtBase,           rolesAtBase)
                    .AddChange ("grantableRolesAtBase",  titleGrantableRolesAtBase,  grantableRolesAtBase)
                    .AddChange ("rolesAtOther",          titleRolesAtOther,          rolesAtOther)
                    .AddChange ("grantableRolesAtOther", titleGrantableRolesAtOther, grantableRolesAtOther)
            );
        }

        foreach ((long _, Session session) in Sessions)
        {
            int characterID = session.CharacterID;
            // characterID should never be null here
            long titleMask = DB.GetTitleMaskForCharacter (characterID);

            // get the title roles and calculate current roles for the session
            DB.GetTitleInformation (
                session.CorporationID, titleMask,
                out long titleRoles, out long titleRolesAtHQ, out long titleRolesAtBase, out long titleRolesAtOther,
                out long titleGrantableRoles, out long titleGrantableRolesAtHQ, out long titleGrantableRolesAtBase,
                out long titleGrantableRolesAtOther, out _
            );
            CharacterDB.GetCharacterRoles (
                characterID,
                out long characterRoles, out long characterRolesAtBase, out long characterRolesAtHQ, out long characterRolesAtOther,
                out long characterGrantableRoles, out long characterGrantableRolesAtBase, out long characterGrantableRolesAtHQ,
                out long characterGrantableRolesAtOther, out _, out _, out _
            );

            // update the roles on the session and send the session change to the player
            Session updates = new Session ();

            updates.CorporationRole = characterRoles | titleRoles;
            updates.RolesAtOther    = characterRolesAtOther | titleRolesAtOther;
            updates.RolesAtHQ       = characterRolesAtHQ | titleRolesAtHQ;
            updates.RolesAtBase     = characterRolesAtBase | titleRolesAtBase;
            updates.RolesAtAll      = updates.CorporationRole | updates.RolesAtOther | updates.RolesAtHQ | updates.RolesAtBase;
            // let the session manager know about this change so it can update the client
            SessionManager.PerformSessionUpdate (Session.CHAR_ID, characterID, updates);
        }

        return null;
    }

    protected override long MachoResolveObject (CallInformation call, ServiceBindParams parameters)
    {
        return Database.CluResolveAddress ("corpRegistry", parameters.ObjectID);
    }

    protected override MultiClientBoundService CreateBoundInstance (CallInformation call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != BoundServiceManager.MachoNet.NodeID)
             throw new CustomError ("Trying to bind an object that does not belong to us!");

        Corporation corp = ItemFactory.LoadItem <Corporation> (bindParams.ObjectID);

        return new corpRegistry (
            DB, Database, ChatDB, CharacterDB, Notifications, MailManager, WalletManager, Constants,
            ItemFactory, Ancestries, SessionManager, corp, bindParams.ExtraValue, this
        );
    }

    public override bool IsClientAllowedToCall (Session session)
    {
        return Corporation.ID == session.CorporationID;
    }

    [MustHaveCorporationRole(MLS.UI_CORP_NEED_ROLE_PERS_MAN_TO_MANAGE_ADS, CorporationRole.PersonnelManager)]
    public PyDataType DeleteRecruitmentAd (CallInformation call, PyInteger advertID)
    {
        if (DB.DeleteRecruitmentAd (advertID, call.Session.CorporationID))
            // TODO: MAYBE NOTIFY CHARACTERS IN THE STATION?
            // send notification
            Notifications.NotifyCorporation (
                call.Session.CorporationID,
                new OnCorporationRecruitmentAdChanged (call.Session.CorporationID, advertID)
                    .AddValue ("adID", advertID, null)
            );

        return null;
    }

    [MustHaveCorporationRole(MLS.UI_CORP_NEED_ROLE_PERS_MAN_TO_MANAGE_ADS, CorporationRole.PersonnelManager)]
    public PyDataType UpdateRecruitmentAd (
        CallInformation call, PyInteger adID, PyInteger typeMask, PyInteger raceMask, PyInteger skillPoints, PyString description
    )
    {
        if (DB.UpdateRecruitmentAd (adID, call.Session.CorporationID, typeMask, raceMask, description, skillPoints))
        {
            OnCorporationRecruitmentAdChanged changes = new OnCorporationRecruitmentAdChanged (call.Session.CorporationID, adID);
            // add the fields for the recruitment ad change
            changes
                .AddValue ("typeMask",    typeMask,    typeMask)
                .AddValue ("description", description, description)
                .AddValue ("raceMask",    raceMask,    raceMask)
                .AddValue ("skillPoints", skillPoints, skillPoints);

            // TODO: MAYBE NOTIFY CHARACTERS IN THE STATION?
            Notifications.NotifyCorporation (call.Session.CorporationID, changes);
        }

        return null;
    }

    [MustHaveCorporationRole(MLS.UI_CORP_NEED_ROLE_PERS_MAN_TO_MANAGE_APPLICATIONS, CorporationRole.PersonnelManager)]
    public PyDataType GetApplications (CallInformation call)
    {
        return Database.DictRowList (
            CorporationDB.LIST_APPLICATIONS,
            new Dictionary <string, object> {{"_corporationID", call.Session.CorporationID}}
        );
    }

    public PyDataType InsertApplication (CallInformation call, PyInteger corporationID, PyString text)
    {
        // TODO: CHECK IF THE CHARACTER IS A CEO AND DENY THE APPLICATION CREATION
        int characterID = call.Session.CharacterID;

        // create the application in the database
        DB.CreateApplication (characterID, corporationID, text);
        OnCorporationApplicationChanged change = new OnCorporationApplicationChanged (corporationID, characterID);

        change
            .AddValue ("corporationID",       null, corporationID)
            .AddValue ("characterID",         null, characterID)
            .AddValue ("applicationDateTime", null, DateTime.UtcNow.ToFileTimeUtc ())
            .AddValue ("applicationText",     null, text)
            .AddValue ("status",              null, 0);

        Notifications.NotifyCorporationByRole (corporationID, CorporationRole.PersonnelManager, change);
        Notifications.NotifyCharacter (characterID, change);

        return null;
    }

    public PyDataType DeleteApplication (CallInformation call, PyInteger corporationID, PyInteger characterID)
    {
        int currentCharacterID = call.Session.CharacterID;

        if (characterID != currentCharacterID)
            throw new CrpAccessDenied ("This application does not belong to you");

        DB.DeleteApplication (characterID, corporationID);
        OnCorporationApplicationChanged change = new OnCorporationApplicationChanged (corporationID, characterID);

        // this notification doesn't seem to update the window if it's focused by the corporation personnel
        change
            .AddValue ("corporationID", corporationID, null)
            .AddValue ("characterID",   characterID,   null)
            .AddValue ("status",        0,             null);

        // notify about the application change
        Notifications.NotifyCorporationByRole (corporationID, CorporationRole.PersonnelManager, change);
        Notifications.NotifyCharacter (characterID, change);

        return null;
    }

    [MustHaveCorporationRole(MLS.UI_CORP_NEED_ROLE_PERS_MAN_TO_MANAGE_APPLICATIONS, CorporationRole.PersonnelManager)]
    public PyDataType UpdateApplicationOffer (CallInformation call, PyInteger characterID, PyString text, PyInteger newStatus, PyInteger applicationDateTime)
    {
        // TODO: CHECK THAT THE APPLICATION EXISTS TO PREVENT A CHARACTER FROM BEING FORCE TO JOIN A CORPORATION!!

        string characterName = CharacterDB.GetCharacterName (characterID);

        int corporationID = call.Session.CorporationID;

        // accept application
        if (newStatus == 6)
        {
            // ensure things are updated on the database first
            int oldCorporationID = CharacterDB.GetCharacterCorporationID (characterID);
            // remove the character from old channels
            ChatDB.LeaveChannel (oldCorporationID, characterID);
            ChatDB.LeaveEntityChannel (oldCorporationID, characterID);
            // join the character to the new channels
            ChatDB.JoinEntityChannel (corporationID, characterID);
            ChatDB.JoinChannel (corporationID, characterID);
            // build the notification of corporation change
            OnCorporationMemberChanged change = new OnCorporationMemberChanged (characterID, oldCorporationID, corporationID);
            // ensure the database reflects these changes
            CharacterDB.UpdateCharacterRoles (
                characterID, 0, 0, 0,
                0, 0, 0, 0, 0, 0
            );
            CharacterDB.UpdateCorporationID (characterID, corporationID);
            // notify cluster about the corporation changes
            Notifications.NotifyCorporation (change.OldCorporationID, change);
            Notifications.NotifyCorporation (change.NewCorporationID, change);
            // create the employment record for the character
            CharacterDB.CreateEmploymentRecord (characterID, corporationID, DateTime.UtcNow.ToFileTimeUtc ());

            // check if the character is connected and update it's session
            int characterNodeID = ItemFactory.ItemDB.GetItemNode (characterID);

            if (characterNodeID > 0)
            {
                Session update = new Session {
                    // update character's session
                    CorporationID   = corporationID,
                    RolesAtAll      = 0,
                    CorporationRole = 0,
                    RolesAtBase     = 0,
                    RolesAtOther    = 0,
                    RolesAtHQ       = 0
                };
                // finally send the session change
                SessionManager.PerformSessionUpdate (Session.CHAR_ID, characterID, update);

                // player is connected, notify the node that owns it to make the changes required
                Notifications.Nodes.Corps.OnCorporationMemberChanged nodeNotification =
                    new Notifications.Nodes.Corps.OnCorporationMemberChanged (characterID, oldCorporationID, corporationID);

                // TODO: WORKOUT A BETTER WAY OF NOTIFYING THIS TO THE NODE, IT MIGHT BE BETTER TO INSPECT THE SESSION CHANGE INSTEAD OF DOING IT LIKE THIS
                long newCorporationNodeID = ItemFactory.ItemDB.GetItemNode (corporationID);
                long oldCorporationNodeID = ItemFactory.ItemDB.GetItemNode (oldCorporationID);

                // get the node where the character is loaded
                // the node where the corporation is loaded also needs to get notified so the members rowset can be updated
                if (characterNodeID > 0)
                    Notifications.NotifyNode (characterNodeID, nodeNotification);
                if (newCorporationNodeID != characterNodeID)
                    Notifications.NotifyNode (newCorporationNodeID, nodeNotification);
                if (oldCorporationNodeID != newCorporationNodeID && oldCorporationNodeID != characterNodeID)
                    Notifications.NotifyNode (oldCorporationNodeID, nodeNotification);
            }

            // this one is a bit harder as this character might not be in the same node as this service
            // take care of the proper 
            MailManager.SendMail (
                call.Session.CorporationID,
                characterID,
                $"Welcome to {Corporation.Name}",
                $"Dear {characterName}. Your application to join <b>{Corporation.Name}</b> has been accepted."
            );
        }
        // reject application
        else if (newStatus == 4)
        {
            MailManager.SendMail (
                call.Session.CorporationID,
                characterID,
                $"Rejected application to join {Corporation.Name}",
                $"Dear {characterName}. Your application to join <b>{Corporation.Name}</b> has been REJECTED."
            );
        }

        DB.DeleteApplication (characterID, corporationID);
        OnCorporationApplicationChanged applicationChange = new OnCorporationApplicationChanged (corporationID, characterID);

        // this notification doesn't seem to update the window if it's focused by the corporation personnel
        applicationChange
            .AddValue ("corporationID", corporationID, null)
            .AddValue ("characterID",   characterID,   null)
            .AddValue ("status",        0,             null);

        // notify about the application change
        Notifications.NotifyCorporationByRole (corporationID, CorporationRole.PersonnelManager, applicationChange);
        Notifications.NotifyCharacter (characterID, applicationChange);

        return null;
    }

    public PyDataType GetMemberIDsWithMoreThanAvgShares (CallInformation call)
    {
        return DB.GetMemberIDsWithMoreThanAvgShares (call.Session.CorporationID);
    }

    public PyBool CanViewVotes (CallInformation call, PyInteger corporationID)
    {
        return (call.Session.CorporationID == corporationID && CorporationRole.Director.Is (call.Session.CorporationRole)) ||
               DB.GetSharesForOwner (corporationID, call.Session.CharacterID) > 0;
    }

    [MustHaveCorporationRole(CorporationRole.Director, typeof(CrpOnlyDirectorsCanProposeVotes))]
    public PyDataType InsertVoteCase (
        CallInformation call, PyString text, PyString description, PyInteger corporationID, PyInteger type, PyDataType rowsetOptions, PyInteger startDateTime, PyInteger endDateTime
    )
    {
        // TODO: current CEO seems to loose control if the vote is for a new CEO, this might complicate things a little on the permissions side of things
        /*                'MAIL_TEMPLATE_CEO_ROLES_REVOKED_BODY': (2424454,
                                                     u'%(candidateName)s is running for CEO in %(corporationName)s. Your roles as CEO have been revoked for the duration of the voting period.'),
            'MAIL_TEMPLATE_CEO_ROLES_REVOKED_SUBJECT': (2424457,
                                                        u'CEO roles revoked'),*/

        // check if the character is trying to run for CEO and ensure only if he belongs to the same corporation that can be done
        if (type == (int) CorporationVotes.CEO && call.Session.CorporationID != corporationID)
            throw new CantRunForCEOAtTheMoment ();
        // TODO: CHECK CORPORATION MANAGEMENT SKILL

        int characterID = call.Session.CharacterID;

        // parse rowset options
        Rowset options = rowsetOptions;

        int voteCaseID = (int) DB.InsertVoteCase (corporationID, characterID, type, startDateTime, endDateTime, text, description);
        OnCorporationVoteCaseChanged change = new OnCorporationVoteCaseChanged (corporationID, voteCaseID)
                                              .AddValue ("corporationID", null, corporationID)
                                              .AddValue ("characterID",   null, characterID)
                                              .AddValue ("type",          null, type)
                                              .AddValue ("startDateTime", null, startDateTime)
                                              .AddValue ("endDateTime",   null, endDateTime)
                                              .AddValue ("text",          null, text)
                                              .AddValue ("description",   null, description);

        // notify the new vote being created to all shareholders
        this.NotifyShareholders (corporationID, change);
        Notifications.NotifyCorporationByRole (corporationID, CorporationRole.Director, change);

        int optionText = 0;
        int parameter  = 1;
        int parameter1 = 2;
        int parameter2 = 3;

        foreach (PyList entry in options.Rows)
            DB.InsertVoteOption (
                voteCaseID,
                entry [optionText] as PyString,
                entry [parameter] as PyInteger,
                entry [parameter1] as PyInteger,
                entry [parameter2] as PyInteger
            );

        return null;
    }

    public PyDataType GetSanctionedActionsByCorporation (CallInformation call, PyInteger corporationID, PyInteger status)
    {
        // TODO: CHECK PERMISSIONS FOR THIS
        return DB.GetSanctionedActionsByCorporation (corporationID, status);
    }

    public PyDataType GetVoteCasesByCorporation (CallInformation call, PyInteger corporationID)
    {
        // TODO: DETERMINE WHAT THIS DOES?
        if (this.CanViewVotes (call, corporationID) == false)
            throw new CrpAccessDenied (MLS.UI_SHARED_WALLETHINT12);

        return DB.GetAllVoteCasesByCorporation (corporationID);
    }

    public PyDataType GetVoteCasesByCorporation (CallInformation call, PyInteger corporationID, PyInteger status)
    {
        if (this.CanViewVotes (call, corporationID) == false)
            throw new CrpAccessDenied (MLS.UI_SHARED_WALLETHINT12);

        if (status == 2)
            return DB.GetOpenVoteCasesByCorporation (corporationID);

        return DB.GetClosedVoteCasesByCorporation (corporationID);
    }

    public PyDataType GetVoteCasesByCorporation (CallInformation call, PyInteger corporationID, PyInteger status, PyInteger maxLen)
    {
        if (this.CanViewVotes (call, corporationID) == false)
            throw new CrpAccessDenied (MLS.UI_SHARED_WALLETHINT12);

        if (status == 2)
            return DB.GetOpenVoteCasesByCorporation (corporationID);

        return DB.GetClosedVoteCasesByCorporation (corporationID);
    }

    public PyDataType GetVoteCaseOptions (CallInformation call, PyInteger corporationID, PyInteger voteCaseID)
    {
        if (this.CanViewVotes (call, corporationID) == false)
            throw new CrpAccessDenied (MLS.UI_SHARED_WALLETHINT12);

        return DB.GetVoteCaseOptions (corporationID, voteCaseID);
    }

    public PyDataType GetVotes (CallInformation call, PyInteger corporationID, PyInteger voteCaseID)
    {
        if (this.CanViewVotes (call, corporationID) == false)
            throw new CrpAccessDenied (MLS.UI_SHARED_WALLETHINT12);

        return DB.GetVotes (corporationID, voteCaseID, call.Session.CharacterID);
    }

    public PyDataType InsertVote (CallInformation call, PyInteger corporationID, PyInteger voteCaseID, PyInteger optionID)
    {
        int characterID = call.Session.CharacterID;

        if (this.CanViewVotes (call, corporationID) == false)
            throw new CrpAccessDenied (MLS.UI_SHARED_WALLETHINT12);

        DB.InsertVote (voteCaseID, optionID, characterID);

        OnCorporationVoteChanged change = new OnCorporationVoteChanged (corporationID, voteCaseID, characterID)
                                          .AddValue ("characterID", null, characterID)
                                          .AddValue ("optionID",    null, optionID);
        // notify the new vote to the original character
        this.NotifyShareholders (corporationID, change);
        Notifications.NotifyCorporationByRole (corporationID, CorporationRole.Director, change);

        return null;
    }

    public PyDataType GetAllianceApplications (CallInformation call)
    {
        return Database.IndexRowset (
            0, CorporationDB.GET_ALLIANCE_APPLICATIONS,
            new Dictionary <string, object> {{"_corporationID", ObjectID}}
        );
    }

    public PyDataType ApplyToJoinAlliance (CallInformation call, PyInteger allianceID, PyString applicationText)
    {
        // TODO: CHECK PERMISSIONS, ONLY DIRECTOR CAN DO THAT?
        // delete any existant application
        this.DeleteAllianceApplicationIfExists ();
        // insert the application
        DB.InsertAllianceApplication (allianceID, ObjectID, applicationText);

        // notify the new application
        OnAllianceApplicationChanged newChange =
            new OnAllianceApplicationChanged (allianceID, ObjectID)
                .AddChange ("allianceID",          null, allianceID)
                .AddChange ("corporationID",       null, ObjectID)
                .AddChange ("applicationText",     null, applicationText)
                .AddChange ("applicationDateTime", null, DateTime.UtcNow.ToFileTimeUtc ())
                .AddChange ("state",               null, (int) ApplicationStatus.New);

        // TODO: WRITE A CUSTOM NOTIFICATION FOR ALLIANCE AND ROLE BASED
        Notifications.NotifyAlliance (allianceID, newChange);
        // notify the player creating the application
        Notifications.NotifyCorporationByRole (call.Session.CorporationID, CorporationRole.Director, newChange);

        return null;
    }

    private void DeleteAllianceApplicationIfExists ()
    {
        // get the current alliance and notify members of that alliance that the application is no more
        int? currentApplicationAllianceID = DB.GetCurrentAllianceApplication (ObjectID);

        if (currentApplicationAllianceID is not null)
        {
            OnAllianceApplicationChanged change =
                new OnAllianceApplicationChanged ((int) currentApplicationAllianceID, ObjectID)
                    .AddChange ("allianceID",          currentApplicationAllianceID,     null)
                    .AddChange ("corporationID",       ObjectID,                         null)
                    .AddChange ("applicationText",     "",                               null)
                    .AddChange ("applicationDateTime", DateTime.UtcNow.ToFileTimeUtc (), null)
                    .AddChange ("state",               0,                                null);

            Notifications.NotifyAlliance ((int) currentApplicationAllianceID, change);
        }
    }

    /// <summary>
    /// Sends a notification to the given corporation's shareholders
    /// </summary>
    /// <param name="corporationID"></param>
    /// <param name="change"></param>
    private void NotifyShareholders (int corporationID, ClientNotification change)
    {
        PyList <PyInteger> shareholders = Database.List <PyInteger> (
            CorporationDB.LIST_SHAREHOLDERS,
            new Dictionary <string, object> {{"_corporationID", corporationID}}
        );

        Notifications.NotifyCharacters (shareholders, change);
    }
}