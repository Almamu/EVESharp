using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using EVE;
using EVE.Packets.Exceptions;
using Node.Alliances;
using Node.Chat;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.corpRegistry;
using Node.Exceptions.corpStationMgr;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Market;
using Node.Network;
using Node.Notifications.Client.Alliances;
using Node.Notifications.Client.Corporations;
using Node.Notifications.Client.Wallet;
using Node.Notifications.Nodes.Corporations;
using Node.Services.Characters;
using Node.StaticData;
using Node.StaticData.Corporation;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using OnCorporationMemberChanged = Node.Notifications.Client.Corporations.OnCorporationMemberChanged;

namespace Node.Services.Corporations
{
    // TODO: REWRITE THE USAGE OF THE CHARACTER CLASS HERE TO FETCH THE DATA OFF THE DATABASE TO PREVENT ISSUES ON MULTI-NODE INSTALLATIONS
    public class corpRegistry : MultiClientBoundService
    {
        private Corporation mCorporation = null;
        private int mIsMaster = 0;

        public Corporation Corporation => this.mCorporation;
        public int IsMaster => this.mIsMaster;

        private CorporationDB DB { get; }
        private AlliancesDB AlliancesDB { get; init; }
        private ChatDB ChatDB { get; init; }
        private CharacterDB CharacterDB { get; init; }
        private ItemFactory ItemFactory { get; }
        private WalletManager WalletManager { get; init; }
        private NodeContainer Container { get; init; }
        private NotificationManager NotificationManager { get; init; }
        private MailManager MailManager { get; init; }
        private ClientManager ClientManager { get; init; }
        public MembersSparseRowsetService MembersSparseRowset { get; private set; }
        private OfficesSparseRowsetService OfficesSparseRowset { get; set; }
        private AncestryManager AncestryManager { get; set; }
        
        // constants
        private long CorporationAdvertisementFlatFee { get; init; }
        private long CorporationAdvertisementDailyRate { get; init; }
        
        public corpRegistry(CorporationDB db, AlliancesDB alliancesDB, ChatDB chatDB, CharacterDB characterDB, NotificationManager notificationManager, MailManager mailManager, WalletManager walletManager, NodeContainer container, ItemFactory itemFactory, ClientManager clientManager, BoundServiceManager manager, AncestryManager ancestryManager, MachoNet machoNet) : base(manager)
        {
            this.DB = db;
            this.AlliancesDB = alliancesDB;
            this.ChatDB = chatDB;
            this.CharacterDB = characterDB;
            this.NotificationManager = notificationManager;
            this.MailManager = mailManager;
            this.WalletManager = walletManager;
            this.Container = container;
            this.ItemFactory = itemFactory;
            this.ClientManager = clientManager;
            this.AncestryManager = ancestryManager;
            
            machoNet.OnClusterTimer += this.PerformTimedEvents;
        }

        protected corpRegistry(CorporationDB db, AlliancesDB alliancesDB, ChatDB chatDB, CharacterDB characterDB, NotificationManager notificationManager, MailManager mailManager, WalletManager walletManager, NodeContainer container, ItemFactory itemFactory, ClientManager clientManager, AncestryManager ancestryManager, Corporation corp, int isMaster, corpRegistry parent) : base (parent, corp.ID)
        {
            this.DB = db;
            this.AlliancesDB = alliancesDB;
            this.ChatDB = chatDB;
            this.CharacterDB = characterDB;
            this.NotificationManager = notificationManager;
            this.MailManager = mailManager;
            this.WalletManager = walletManager;
            this.Container = container;
            this.ItemFactory = itemFactory;
            this.mCorporation = corp;
            this.mIsMaster = isMaster;
            this.ClientManager = clientManager;
            this.CorporationAdvertisementFlatFee = this.Container.Constants["corporationAdvertisementFlatFee"].Value;
            this.CorporationAdvertisementDailyRate = this.Container.Constants["corporationAdvertisementDailyRate"].Value;
            this.AncestryManager = ancestryManager;
        }

        /// <summary>
        /// Checks orders that are expired, cancels them and returns the items to the hangar if required
        /// </summary>
        private void PerformTimedEvents(object sender, EventArgs args)
        {
            this.DB.RemoveExpiredCorporationAds();
        }

        public PyDataType GetEveOwners(CallInformation call)
        {
            // this call seems to be getting all the members of the given corporationID
            return this.DB.GetEveOwners(this.Corporation.ID);
        }

        public PyDataType GetCorporation(CallInformation call)
        {
            return this.Corporation.GetCorporationInfoRow();
        }

        public PyDataType GetSharesByShareholder(PyInteger corpShares, CallInformation call)
        {
            return this.GetSharesByShareholder(corpShares == 1, call);
        }
        
        public PyDataType GetSharesByShareholder(PyBool corpShares, CallInformation call)
        {
            int entityID = call.Client.EnsureCharacterIsSelected();

            if (corpShares == true)
                entityID = call.Client.CorporationID;
            
            return this.DB.GetSharesByShareholder(entityID);
        }

        public PyDataType GetShareholders(PyInteger corporationID, CallInformation call)
        {
            return this.DB.GetShareholders(corporationID);
        }

        public PyDataType MoveCompanyShares(PyInteger corporationID, PyInteger to, PyInteger quantity, CallInformation call)
        {
            // check that we're allowed to do that
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_DO_NOT_HAVE_ROLE_DIRECTOR);
            
            // TODO: the WALLETKEY SHOULD BE SOMETHING ELSE?
            using (Wallet corporationWallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, 2000))
            using (Wallet shareholderWallet = this.WalletManager.AcquireWallet(to, 2000))
            {
                // first make sure there's enough shares available
                int availableShares = this.DB.GetSharesForOwner(call.Client.CorporationID, call.Client.CorporationID);

                if (availableShares < quantity)
                    throw new NotEnoughShares(quantity, availableShares);
            
                // get the shares the destination already has
                int currentShares = this.DB.GetSharesForOwner(call.Client.CorporationID, to);
                
                // make the transaction
                this.DB.UpdateShares(call.Client.CorporationID, to, quantity + currentShares);
                this.DB.UpdateShares(call.Client.CorporationID, call.Client.CorporationID, availableShares - quantity);
                
                // create the notifications
                OnShareChange changeForNewHolder = new OnShareChange(to, call.Client.CorporationID,
                    currentShares == 0 ? null : currentShares, currentShares + quantity);
                OnShareChange changeForRestCorp = new OnShareChange(call.Client.CorporationID,
                    call.Client.CorporationID, availableShares, availableShares - quantity);
                
                // send both notifications
                this.NotificationManager.NotifyCorporation(call.Client.CorporationID, changeForNewHolder);
                this.NotificationManager.NotifyCorporation(call.Client.CorporationID, changeForRestCorp);
            }

            return null;
        }

        public PyDataType MovePrivateShares(PyInteger corporationID, PyInteger toShareholderID, PyInteger quantity, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            // TODO: the WALLETKEY SHOULD BE SOMETHING ELSE?
            using (Wallet corporationWallet = this.WalletManager.AcquireWallet(callerCharacterID, 2000))
            using (Wallet shareholderWallet = this.WalletManager.AcquireWallet(toShareholderID, 2000))
            {
                // first make sure there's enough shares available
                int availableShares = this.DB.GetSharesForOwner(corporationID, callerCharacterID);

                if (availableShares < quantity)
                    throw new NotEnoughShares(quantity, availableShares);
            
                // get the shares the destination already has
                int currentShares = this.DB.GetSharesForOwner(corporationID, toShareholderID);
                
                // make the transaction
                this.DB.UpdateShares(corporationID, toShareholderID, quantity + currentShares);
                this.DB.UpdateShares(corporationID, callerCharacterID, availableShares - quantity);
                
                // create the notifications
                OnShareChange changeForNewHolder = new OnShareChange(toShareholderID, call.Client.CorporationID,
                    currentShares == 0 ? null : currentShares, currentShares + quantity);
                OnShareChange changeForRestCorp = new OnShareChange(call.Client.CorporationID,
                    call.Client.CorporationID, availableShares, availableShares - quantity);
                
                // send both notifications
                this.NotificationManager.NotifyCorporation(call.Client.CorporationID, changeForNewHolder);
                this.NotificationManager.NotifyCorporation(call.Client.CorporationID, changeForRestCorp);
            }

            return null;
        }

        public PyDataType GetMember(PyInteger memberID, CallInformation call)
        {
            return this.DB.GetMember(memberID, call.Client.CorporationID);
        }

        public PyDataType GetMembers(CallInformation call)
        {
            if (this.MembersSparseRowset is null)
            {
                // generate the sparse rowset
                SparseRowsetHeader rowsetHeader = this.DB.GetMembersSparseRowset(call.Client.CorporationID);

                PyDictionary dict = new PyDictionary
                {
                    ["realRowCount"] = rowsetHeader.Count
                };
            
                // create a service for handling it's calls
                this.MembersSparseRowset =
                    new MembersSparseRowsetService(this.Corporation, this.DB, rowsetHeader, this.NotificationManager, this.BoundServiceManager, call.Client);

                rowsetHeader.BoundObjectIdentifier = this.MembersSparseRowset.MachoBindObject(dict, call.Client);
            }
            
            // ensure the bound service knows that this client is bound to it
            this.MembersSparseRowset.BindToClient(call.Client);
            
            // finally return the data
            return this.MembersSparseRowset.RowsetHeader;
        }

        public PyDataType GetOffices(CallInformation call)
        {
            if (this.OfficesSparseRowset is null)
            {
                // generate the sparse rowset
                SparseRowsetHeader rowsetHeader = this.DB.GetOfficesSparseRowset(call.Client.CorporationID);

                PyDictionary dict = new PyDictionary
                {
                    ["realRowCount"] = rowsetHeader.Count
                };
            
                // create a service for handling it's calls
                this.OfficesSparseRowset =
                    new OfficesSparseRowsetService(this.Corporation, this.DB, rowsetHeader, this.BoundServiceManager, call.Client);

                rowsetHeader.BoundObjectIdentifier = this.OfficesSparseRowset.MachoBindObject(dict, call.Client);
            }
            
            // ensure the bound service knows that this client is bound to it
            this.OfficesSparseRowset.BindToClient(call.Client);
            
            // finally return the data
            return this.OfficesSparseRowset.RowsetHeader;
        }

        public PyDataType GetRoleGroups(CallInformation call)
        {
            return this.DB.GetRoleGroups();
        }

        public PyDataType GetRoles(CallInformation call)
        {
            return this.DB.GetRoles();
        }

        public PyDataType GetDivisions(CallInformation call)
        {
            return this.DB.GetDivisions();
        }

        public PyDataType GetTitles(CallInformation call)
        {
            // check if the corp is NPC and return placeholder data from the crpTitlesTemplate
            if (ItemFactory.IsNPCCorporationID(call.Client.CorporationID) == true)
            {
                return this.DB.GetTitlesTemplate();
            }
            else
            {
                return this.DB.GetTitles(call.Client.CorporationID);                
            }
        }

        public PyDataType GetStations(CallInformation call)
        {
            return this.DB.GetStations(call.Client.CorporationID);
        }

        public PyDataType GetMemberTrackingInfo(PyInteger characterID, CallInformation call)
        {
            // only directors can call this function
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                return null;
            
            return this.DB.GetMemberTrackingInfo(call.Client.CorporationID, characterID);
        }

        public PyDataType GetMemberTrackingInfoSimple(CallInformation call)
        {
            return this.DB.GetMemberTrackingInfoSimple(call.Client.CorporationID);
        }

        public PyDataType GetInfoWindowDataForChar(PyInteger characterID, CallInformation call)
        {
            this.DB.GetCorporationInformationForCharacter(characterID, out string title, out int titleMask,
                out int corporationID, out int? allianceID);
            
            Dictionary<int, string> titles = this.DB.GetTitlesNames(call.Client.CorporationID);
            PyDictionary dictForKeyVal = new PyDictionary();

            int number = 0;

            foreach ((int _, string name) in titles)
                dictForKeyVal["title" + (++number)] = name;
            
            dictForKeyVal["corpID"] = corporationID;
            dictForKeyVal["allianceID"] = allianceID;
            dictForKeyVal["title"] = title;

            return KeyVal.FromDictionary(dictForKeyVal);
        }

        public PyDataType GetMyApplications(CallInformation call)
        {
            return this.DB.GetCharacterApplications(call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType GetLockedItemLocations(CallInformation call)
        {
            // this just returns a list of itemIDs (locations) that are locked
            // most likely used by the corp stuff for SOMETHING(tm)
            return new PyList<PyInteger>();
        }

        public PyBool CanBeKickedOut(PyInteger characterID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            // check for corporation stasis for this character
            
            // can personel manager kick other players? are there other conditions?
            return characterID == callerCharacterID || CorporationRole.Director.Is(call.Client.CorporationRole);
        }

        public PyDataType KickOutMember(PyInteger characterID, CallInformation call)
        {
            if (this.CanBeKickedOut(characterID, call) == false)
                return null;
            
            // 
            
            return null;
        }

        public PyString GetSuggestedTickerNames(PyString corpName, CallInformation call)
        {
            // get all the upercase letters
            string result = string.Concat(Regex.Matches(corpName.Value, @"\p{Lu}")
                .Select(match => match.Value));

            return new PyString(result.Substring(0, result.Length < 3 ? result.Length : 3));
        }

        private void ValidateAllianceName(PyString allianceName, PyString shortName)
        {
            // validate corporation name
            if (allianceName.Length < 4)
                throw new AllianceNameInvalidMinLength();
            if (allianceName.Length > 24)
                throw new AllianceNameInvalidMaxLength();
            if (shortName.Length < 3 || shortName.Length > 5)
                throw new AllianceShortNameInvalid();
            // check if name is taken
            if (this.DB.IsAllianceNameTaken(allianceName) == true)
                throw new AllianceNameInvalidTaken();
            if (this.DB.IsShortNameTaken(shortName) == true)
                throw new AllianceShortNameInvalidTaken();
            // TODO: ADD SUPPORT FOR BANNED WORDS
            if (false)
                throw new AllianceNameInvalidBannedWord();
        }
        
        public PyDataType CreateAlliance(PyString name, PyString shortName, PyString description, PyString url, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            if (this.Corporation.CeoID != callerCharacterID)
                throw new OnlyActiveCEOCanCreateAlliance();
            
            // TODO: CHECK FOR ACTIVE WARS AND THROW A CUSTOMERROR WITH THIS TEXT: UI_CORP_HINT7

            if (call.Client.AllianceID != null)
                throw new AllianceCreateFailCorpInAlliance();
            
            this.ValidateAllianceName(name, shortName);

            // TODO: PROPERLY IMPLEMENT THIS CHECK, RIGHT NOW THE CHARACTER AND THE CORPREGISTRY INSTANCES DO NOT HAVE TO BE LOADED ON THE SAME NODE
            // TODO: SWITCH UP THE CORPORATION CHANGE MECHANISM TO NOT RELY ON THE CHARACTER OBJECT SO THIS CAN BE DONE THROUGH THE DATABASE
            // TODO: DIRECTLY
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);

            // ensure empire control is trained and at least level 5
            character.EnsureSkillLevel(Types.EmpireControl, 5);
            
            // the alliance costs 1b ISK to establish, and that's taken from the corporation's wallet
            using (Wallet wallet = this.WalletManager.AcquireWallet(this.Corporation.ID, call.Client.CorpAccountKey, true))
            {
                // ensure there's enough balance
                wallet.EnsureEnoughBalance(this.Container.Constants[Constants.allianceCreationCost]);
                
                // create the journal record for the alliance creation
                wallet.CreateJournalRecord(MarketReference.AllianceRegistrationFee, null, null, -this.Container.Constants[Constants.allianceCreationCost]);
                
                // now create the alliance
                int allianceID =
                    (int) this.AlliancesDB.CreateAlliance(name, shortName, url, description, call.Client.CorporationID, callerCharacterID);

                this.Corporation.AllianceID = allianceID;
                this.Corporation.ExecutorCorpID = this.Corporation.ID;
                this.Corporation.StartDate = DateTime.UtcNow.ToFileTimeUtc();
                this.Corporation.Persist();

                // create the new chat channel
                this.ChatDB.CreateChannel(allianceID, allianceID, "System Channels\\Alliance", true);
                this.ChatDB.CreateChannel(allianceID, allianceID, "System Channels\\Alliance", false);
            
                // this will update the allianceID for all the characters out there!
                if (this.ClientManager.TryGetClientsByCorporationID(this.Corporation.ID, out Dictionary<int, Client> clients) == true)
                {
                    foreach ((int _, Client client) in clients)
                    {
                        int characterID = (int) client.CharacterID;
                    
                        // join the player to the corp channel
                        this.ChatDB.JoinEntityChannel(allianceID, characterID, characterID == callerCharacterID ? ChatDB.CHATROLE_CREATOR : ChatDB.CHATROLE_CONVERSATIONALIST);
                        this.ChatDB.JoinChannel(allianceID, characterID, characterID == callerCharacterID ? ChatDB.CHATROLE_CREATOR : ChatDB.CHATROLE_CONVERSATIONALIST);
                    
                        // update session and send session change
                        client.AllianceID = allianceID;
                        client.SendSessionChange();
                    }
                }
    
                // TODO: CREATE BILLS REQUIRED FOR ALLIANCES, CHECK HOW THEY ARE INVOICED
            }
            
            return null;
        }

        private void ValidateCorporationName(PyString corporationName, PyString tickerName)
        {
            // validate corporation name
            if (corporationName.Length < 4)
                throw new CorpNameInvalidMinLength();
            if (corporationName.Length > 24)
                throw new CorpNameInvalidMaxLength();
            if (tickerName.Length < 2 || tickerName.Length > 4)
                throw new CorpTickerNameInvalid();
            // check if name is taken
            if (this.DB.IsCorporationNameTaken(corporationName) == true)
                throw new CorpNameInvalidTaken();
            if (this.DB.IsTickerNameTaken(tickerName) == true)
                throw new CorpTickerNameInvalidTaken();
            // TODO: ADD SUPPORT FOR BANNED WORDS
            if (false)
                throw new CorpNameInvalidBannedWord();
        }
        
        public PyDataType AddCorporation(PyString corporationName, PyString tickerName, PyString description,
            PyString url, PyDecimal taxRate, PyInteger shape1, PyInteger shape2, PyInteger shape3, PyInteger color1,
            PyInteger color2, PyInteger color3, PyDataType typeface, CallInformation call)
        {
            // do some basic checks on what the player can do
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == true)
                throw new CEOCannotCreateCorporation();
            if (call.Client.StationID == null)
                throw new CanOnlyCreateCorpInStation();
            // TODO: CHECK FOR POS AS ITS NOT POSSIBLE TO CREATE CORPORATIONS THERE
            this.ValidateCorporationName(corporationName, tickerName);
            
            int stationID = call.Client.EnsureCharacterIsInStation();
            int corporationStartupCost = this.Container.Constants[Constants.corporationStartupCost];
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            // TODO: PROPERLY IMPLEMENT THIS CHECK, RIGHT NOW THE CHARACTER AND THE CORPREGISTRY INSTANCES DO NOT HAVE TO BE LOADED ON THE SAME NODE
            // TODO: SWITCH UP THE CORPORATION CHANGE MECHANISM TO NOT RELY ON THE CHARACTER OBJECT SO THIS CAN BE DONE THROUGH THE DATABASE
            // TODO: DIRECTLY
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            
            this.CalculateCorporationLimits(character, out int maximumMembers, out int allowedMemberRaceIDs);
            
            // ensure the character has the required skills
            long corporationManagementLevel = character.GetSkillLevel(Types.CorporationManagement);
            
            if (corporationManagementLevel < 1)
                throw new PlayerCantCreateCorporation(corporationStartupCost);
            
            try
            {
                // acquire the wallet for this character too
                using (Wallet wallet = this.WalletManager.AcquireWallet(character.ID, 1000))
                {
                    // ensure there's enough balance
                    wallet.EnsureEnoughBalance(corporationStartupCost);
                    
                    // create the corporation in the corporation table
                    int corporationID = this.DB.CreateCorporation(
                        corporationName, description, tickerName, url, taxRate, callerCharacterID,
                        stationID, maximumMembers, (int) call.Client.RaceID, allowedMemberRaceIDs,
                        shape1, shape2, shape3, color1, color2, color3, typeface as PyString
                    );
                    // create default titles
                    this.DB.CreateDefaultTitlesForCorporation(corporationID);
                    // create the record in the journal
                    wallet.CreateJournalRecord(MarketReference.CorporationRegistrationFee, null, null, corporationStartupCost);
                    
                    // leave the old corporation channels first
                    this.ChatDB.LeaveChannel(call.Client.CorporationID, character.ID);
                    this.ChatDB.LeaveEntityChannel(call.Client.CorporationID, character.ID);
                    // create the required chat channels
                    this.ChatDB.CreateChannel(corporationID, corporationID, "System Channels\\Corp", true);
                    this.ChatDB.CreateChannel(corporationID, corporationID, "System Channels\\Corp", false);
                    // join the player to the corp channel
                    this.ChatDB.JoinEntityChannel(corporationID, callerCharacterID, ChatDB.CHATROLE_CREATOR);
                    this.ChatDB.JoinChannel(corporationID, callerCharacterID, ChatDB.CHATROLE_CREATOR);
                    // build the notification of corporation change
                    OnCorporationMemberChanged change = new OnCorporationMemberChanged(character.ID, call.Client.CorporationID, corporationID);
                    // start the session change for the client
                    call.Client.CorporationID = corporationID;
                    call.Client.CorporationRole = long.MaxValue; // this gives all the permissions to the character
                    call.Client.RolesAtBase = long.MaxValue;
                    call.Client.RolesAtOther = long.MaxValue;
                    call.Client.RolesAtHQ = long.MaxValue;
                    call.Client.RolesAtAll = long.MaxValue;
                    // update the character to reflect the new ownership
                    character.CorporationID = corporationID;
                    character.Roles = long.MaxValue;
                    character.RolesAtBase = long.MaxValue;
                    character.RolesAtHq = long.MaxValue;
                    character.RolesAtOther = long.MaxValue;
                    character.TitleMask = ushort.MaxValue;
                    // ensure the database reflects these changes
                    this.CharacterDB.UpdateCharacterRoles(
                        character.ID, long.MaxValue, long.MaxValue, long.MaxValue,
                        long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, ushort.MaxValue
                    );
                    this.CharacterDB.UpdateCharacterBlockRole(character.ID, 0);
                    this.CharacterDB.SetCharacterStasisTimer(character.ID, null);
                    character.CorporationDateTime = DateTime.UtcNow.ToFileTimeUtc();
                    // notify cluster about the corporation changes
                    this.NotificationManager.NotifyCorporation(change.OldCorporationID, change);
                    this.NotificationManager.NotifyCorporation(change.NewCorporationID, change);
                    // create default wallets
                    this.WalletManager.CreateWallet(corporationID, 1000, 0.0);
                    this.WalletManager.CreateWallet(corporationID, 1001, 0.0);
                    this.WalletManager.CreateWallet(corporationID, 1002, 0.0);
                    this.WalletManager.CreateWallet(corporationID, 1003, 0.0);
                    this.WalletManager.CreateWallet(corporationID, 1004, 0.0);
                    this.WalletManager.CreateWallet(corporationID, 1005, 0.0);
                    this.WalletManager.CreateWallet(corporationID, 1006, 0.0);
                    // create the employment record for the character
                    this.CharacterDB.CreateEmploymentRecord(character.ID, corporationID, DateTime.UtcNow.ToFileTimeUtc());
                    // create company shares too!
                    this.DB.UpdateShares(corporationID, corporationID, 1000);

                    // set the default wallet for the character
                    call.Client.CorpAccountKey = 1000;

                    character.Persist();
                    
                    // load the corporation item
                    this.ItemFactory.LoadItem<Corporation>(corporationID);
                }
                
                return null;
            }
            catch (NotEnoughMoney)
            {
                throw new PlayerCantCreateCorporation(corporationStartupCost);
            }
        }

        private void ValidateDivisionName(string name, int divisionNumber)
        {
            if (name.Length < 3)
                throw new UserError("CorpDiv" + divisionNumber + "NameInvalidMinLength");
            if (name.Length > 24)
                throw new UserError("CorpDiv" + divisionNumber + "NameInvalidMaxLength");
            // TODO: ADD SUPPORT FOR BANNED WORDS
            if (false)
                throw new UserError("CorpDiv" + divisionNumber + "NameInvalidBannedWord");
        }

        public PyDataType UpdateDivisionNames(PyString division1, PyString division2, PyString division3,
            PyString division4, PyString division5, PyString division6, PyString division7, PyString wallet1,
            PyString wallet2, PyString wallet3, PyString wallet4, PyString wallet5, PyString wallet6, PyString wallet7,
            CallInformation call)
        {
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false || call.Client.CorporationID != this.Corporation.ID)
                return null;

            // validate division names
            this.ValidateDivisionName(division1, 1);
            this.ValidateDivisionName(division2, 2);
            this.ValidateDivisionName(division3, 3);
            this.ValidateDivisionName(division4, 4);
            this.ValidateDivisionName(division5, 5);
            this.ValidateDivisionName(division6, 6);
            this.ValidateDivisionName(division7, 7);

            // generate update notification
            OnCorporationChanged change = new OnCorporationChanged(this.Corporation.ID);

            if (this.Corporation.Division1 != division1)
                change.AddChange("division1", this.Corporation.Division1, division1);
            if (this.Corporation.Division2 != division2)
                change.AddChange("division2", this.Corporation.Division2, division2);
            if (this.Corporation.Division3 != division3)
                change.AddChange("division3", this.Corporation.Division3, division3);
            if (this.Corporation.Division4 != division4)
                change.AddChange("division4", this.Corporation.Division4, division4);
            if (this.Corporation.Division5 != division5)
                change.AddChange("division5", this.Corporation.Division5, division5);
            if (this.Corporation.Division6 != division6)
                change.AddChange("division6", this.Corporation.Division6, division6);
            if (this.Corporation.Division7 != division7)
                change.AddChange("division7", this.Corporation.Division7, division7);

            if (this.Corporation.WalletDivision1 != wallet1)
                change.AddChange("walletDivision1", this.Corporation.WalletDivision1, wallet1);
            if (this.Corporation.WalletDivision2 != wallet2)
                change.AddChange("walletDivision2", this.Corporation.WalletDivision2, wallet2);
            if (this.Corporation.WalletDivision3 != wallet3)
                change.AddChange("walletDivision3", this.Corporation.WalletDivision3, wallet3);
            if (this.Corporation.WalletDivision4 != wallet4)
                change.AddChange("walletDivision4", this.Corporation.WalletDivision4, wallet4);
            if (this.Corporation.WalletDivision5 != wallet5)
                change.AddChange("walletDivision5", this.Corporation.WalletDivision5, wallet5);
            if (this.Corporation.WalletDivision6 != wallet6)
                change.AddChange("walletDivision6", this.Corporation.WalletDivision6, wallet6);
            if (this.Corporation.WalletDivision7 != wallet7)
                change.AddChange("walletDivision7", this.Corporation.WalletDivision7, wallet7);

            this.Corporation.Division1 = division1;
            this.Corporation.Division2 = division2;
            this.Corporation.Division3 = division3;
            this.Corporation.Division4 = division4;
            this.Corporation.Division5 = division5;
            this.Corporation.Division6 = division6;
            this.Corporation.Division7 = division7;
            this.Corporation.WalletDivision1 = wallet1;
            this.Corporation.WalletDivision2 = wallet2;
            this.Corporation.WalletDivision3 = wallet3;
            this.Corporation.WalletDivision4 = wallet4;
            this.Corporation.WalletDivision5 = wallet5;
            this.Corporation.WalletDivision6 = wallet6;
            this.Corporation.WalletDivision7 = wallet7;
            
            // update division names
            this.DB.UpdateDivisions(
                call.Client.CorporationID,
                division1, division2, division3, division4, division5, division6, division7,
                wallet1, wallet2, wallet3, wallet4, wallet5, wallet6, wallet7
            );
            
            // notify all the players in the corp
            this.NotificationManager.NotifyCorporation(call.Client.CorporationID, change);
            
            return null;
        }

        public PyDataType UpdateCorporation(PyString newDescription, PyString newUrl, PyDecimal newTax, CallInformation call)
        {
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false || call.Client.CorporationID != this.Corporation.ID)
                return null;

            // update information in the database
            this.DB.UpdateCorporation(call.Client.CorporationID, newDescription, newUrl, newTax);
            
            // generate update notification
            OnCorporationChanged change =
                new OnCorporationChanged(call.Client.CorporationID)
                    .AddChange("description", this.Corporation.Description, newDescription)
                    .AddChange("url", this.Corporation.Url, newUrl)
                    .AddChange("taxRate", this.Corporation.TaxRate, newTax)
                    .AddChange("ceoID", this.Corporation.CeoID, this.Corporation.CeoID);

            this.Corporation.Description = newDescription;
            this.Corporation.Url = newUrl;
            this.Corporation.TaxRate = newTax;

            this.NotificationManager.NotifyCorporation(this.Corporation.ID, change);

            return null;
        }

        public PyDataType GetMemberTrackingInfo(CallInformation call)
        {
            // only directors can call this function
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                return null;
            
            return this.DB.GetMemberTrackingInfo(call.Client.CorporationID);
        }

        public PyDataType SetAccountKey(PyInteger accountKey, CallInformation call)
        {
            // check if the character has any accounting roles and set the correct accountKey based on the data
            if (CorporationRole.Accountant.Is(call.Client.CorporationRole) == true)
                call.Client.CorpAccountKey = accountKey;
            if (CorporationRole.AccountCanTake1.Is(call.Client.CorporationRole) && accountKey == 1000)
                call.Client.CorpAccountKey = 1000;
            if (CorporationRole.AccountCanTake2.Is(call.Client.CorporationRole) && accountKey == 1001)
                call.Client.CorpAccountKey = 1001;
            if (CorporationRole.AccountCanTake3.Is(call.Client.CorporationRole) && accountKey == 1002)
                call.Client.CorpAccountKey = 1002;
            if (CorporationRole.AccountCanTake4.Is(call.Client.CorporationRole) && accountKey == 1003)
                call.Client.CorpAccountKey = 1003;
            if (CorporationRole.AccountCanTake5.Is(call.Client.CorporationRole) && accountKey == 1004)
                call.Client.CorpAccountKey = 1004;
            if (CorporationRole.AccountCanTake6.Is(call.Client.CorporationRole) && accountKey == 1005)
                call.Client.CorpAccountKey = 1005;
            if (CorporationRole.AccountCanTake7.Is(call.Client.CorporationRole) && accountKey == 1006)
                call.Client.CorpAccountKey = 1006;
            
            return null;
        }

        private void PayoutDividendsToShareholders(double totalAmount, Client client)
        {
            double pricePerShare = totalAmount / this.mCorporation.Shares;
            Dictionary<int, int> shares = this.DB.GetShareholdersList(this.mCorporation.ID);

            foreach ((int ownerID, int sharesCount) in shares)
            {
                // send evemail to the owner
                this.MailManager.SendMail(
                    client.CorporationID,
                    ownerID, 
                    $"Dividend from {this.mCorporation.Name}",
                    $"<a href=\"showinfo:2//{this.mCorporation.ID}\">{this.mCorporation.Name}</a> may have credited your account as part of a total payout of <b>{totalAmount} ISK</b> to their shareholders. The amount awarded is based upon the number of shares you hold, in relation to the total number of shares issued by the company."
                );
                
                // TODO: INCLUDE WETHER THE SHAREHOLDER IS A CORPORATION OR NOT, MAYBE CREATE A CUSTOM OBJECT FOR THIS
                // calculate amount to give and acquire it's wallet
                using (Wallet dest = this.WalletManager.AcquireWallet(ownerID, 1000))
                {
                    dest.CreateJournalRecord(MarketReference.CorporationDividendPayment, ownerID, this.mCorporation.ID, pricePerShare * sharesCount);
                }
            }
        }

        private void PayoutDividendsToMembers(double totalAmount, Client client)
        {
            double pricePerMember = totalAmount / this.mCorporation.MemberCount;
            
            foreach (int characterID in this.DB.GetMembersForCorp(this.mCorporation.ID))
            {
                using (Wallet dest = this.WalletManager.AcquireWallet(characterID, 1000))
                {
                    dest.CreateJournalRecord(MarketReference.CorporationDividendPayment, characterID, this.mCorporation.ID, pricePerMember);
                }
            }
            
            // send evemail to corporation mail as this will be received by all the members
            this.MailManager.SendMail(
                client.CorporationID,
                client.CorporationID, 
                $"Dividend from {this.mCorporation.Name}",
                $"<a href=\"showinfo:2//{this.mCorporation.ID}\">{this.mCorporation.Name}</a> may have credited your account as part of a total payout of <b>{totalAmount} ISK</b> to their corporation members. The amount awarded is split evenly between all members of the corporation."
            );
        }

        public PyDataType PayoutDividend(PyInteger payShareholders, PyInteger amount, CallInformation call)
        {
            // check if the player is the CEO
            if (this.mCorporation.CeoID != call.Client.CharacterID)
                throw new OnlyCEOCanPayoutDividends();
            
            using (Wallet wallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, call.Client.CorpAccountKey, true))
            {
                // check if there's enough cash left
                wallet.EnsureEnoughBalance(amount);
                // make transaction
                wallet.CreateJournalRecord(MarketReference.CorporationDividendPayment, this.ItemFactory.OwnerBank.ID, null, amount);
            }
            
            if (payShareholders == 1)
            {
                this.PayoutDividendsToShareholders(amount, call.Client);
            }
            else
            {
                this.PayoutDividendsToMembers(amount, call.Client);
            }
            
            return null;
        }

        private void CalculateCorporationLimits(Character ceo, out int maximumMembers, out int allowedRaceIDs)
        {
            int corporationManagementLevel = (int) ceo.GetSkillLevel(Types.CorporationManagement); // +10 members per level 
            int ethnicRelationsLevel = (int) ceo.GetSkillLevel(Types.EthnicRelations); // 20% more members of other races based off the character's corporation levels TODO: SUPPORT THIS!
            int empireControlLevel = (int) ceo.GetSkillLevel(Types.EmpireControl); // adds +200 members per level
            int megacorpManagementLevel = (int) ceo.GetSkillLevel(Types.MegacorpManagement); // adds +50 members per level
            int sovereigntyLevel = (int) ceo.GetSkillLevel(Types.Sovereignty); // adds +1000 members per level

            maximumMembers = (corporationManagementLevel * 10) + (empireControlLevel * 200) +
                                 (megacorpManagementLevel * 50) + (sovereigntyLevel * 1000);

            allowedRaceIDs = ethnicRelationsLevel > 0 ? 63 : this.AncestryManager[ceo.AncestryID].Bloodline.RaceID;
        }

        public PyDataType UpdateCorporationAbilities(CallInformation call)
        {
            if (this.mCorporation.CeoID != call.Client.CharacterID)
                throw new CrpAccessDenied(MLS.UI_CORP_ACCESSDENIED12);

            Character character = this.ItemFactory.GetItem<Character>(call.Client.EnsureCharacterIsSelected());
            
            this.CalculateCorporationLimits(character, out int maximumMembers, out int allowedMemberRaceIDs);
            
            // update the abilities of the corporation
            OnCorporationChanged change = new OnCorporationChanged(this.mCorporation.ID);

            if (this.mCorporation.MemberLimit != maximumMembers)
            {
                change.AddChange("memberLimit", this.mCorporation.MemberLimit, maximumMembers);
                this.mCorporation.MemberLimit = maximumMembers;    
            }

            if (this.mCorporation.AllowedMemberRaceIDs != allowedMemberRaceIDs)
            {
                change.AddChange("allowedMemberRaceIDs", this.mCorporation.AllowedMemberRaceIDs, allowedMemberRaceIDs);
                this.mCorporation.AllowedMemberRaceIDs = allowedMemberRaceIDs;
            }

            if (change.Changes.Count > 0)
            {
                this.DB.UpdateMemberLimits(this.mCorporation.ID, this.mCorporation.MemberLimit, this.mCorporation.AllowedMemberRaceIDs);

                this.NotificationManager.NotifyCorporation(this.mCorporation.ID, change);

                return true;
            }

            return null;
        }
        
        public PyDataType GetRecruitmentAdsForCorporation(CallInformation call)
        {
            return this.DB.GetRecruitmentAds(null, null, null, null, null, null, null, this.mCorporation.ID);
        }

        public PyDataType UpdateMembers(PyDataType rowset, CallInformation call)
        {
            // parse the rowset to have proper access to the data
            Rowset parsed = rowset;
            // position of each header
            int characterID = 0;
            int title = 1;
            int divisionID = 2;
            int squadronID = 3;
            int roles = 4;
            int grantableRoles = 5;
            int rolesAtHQ = 6;
            int grantableRolesAtHQ = 7;
            int rolesAtBase = 8;
            int grantableRolesAtBase = 9;
            int rolesAtOther = 10;
            int grantableRolesAtOther = 11;
            int baseID = 12;
            int titleMask  = 13;

            foreach (PyList entry in parsed.Rows)
            {
                this.UpdateMember(
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
                    0, call
                );
            }
            
            return null;
        }

        public PyDataType UpdateMember(PyInteger characterID, PyString title, PyInteger divisionID,
            PyInteger squadronID, PyInteger roles, PyInteger grantableRoles, PyInteger rolesAtHQ,
            PyInteger grantableRolesAtHQ, PyInteger rolesAtBase, PyInteger grantableRolesAtBase, PyInteger rolesAtOther,
            PyInteger grantableRolesAtOther, PyInteger baseID, PyInteger titleMask, PyInteger blockRoles,
            CallInformation call)
        {
            // TODO: HANDLE DIVISION AND SQUADRON CHANGES
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            
            // get current roles for that character
            this.CharacterDB.GetCharacterRoles(characterID, out long currentRoles, out long currentRolesAtBase,
                out long currentRolesAtHQ, out long currentRolesAtOther, out long currentGrantableRoles,
                out long currentGrantableRolesAtBase, out long currentGrantableRolesAtHQ,
                out long currentGrantableRolesAtOther, out int? currentBlockRoles, out int? currentBaseID, 
                out int currentTitleMask
            );
            
            // the only modification a member can perform on itself is blocking roles
            if (characterID == callerCharacterID && blockRoles != currentBlockRoles)
            {
                this.CharacterDB.UpdateCharacterBlockRole(characterID, blockRoles == 1 ? 1 : null);

                long? currentStasisTimer = this.CharacterDB.GetCharacterStasisTimer(characterID);
                
                if (blockRoles == 1)
                {
                    if (currentStasisTimer > 0)
                    {
                        long currentTime = DateTime.UtcNow.ToFileTimeUtc();
                        long stasisTimerEnd = (long) currentStasisTimer + TimeSpan.FromHours(24).Ticks;

                        if (stasisTimerEnd > currentTime)
                        {
                            int hoursTimerStarted = (int) ((currentTime - currentStasisTimer) / TimeSpan.TicksPerHour);
                            int hoursLeft = (int) ((stasisTimerEnd - currentTime) / TimeSpan.TicksPerHour);

                            throw new CrpCantQuitNotCompletedStasisPeriod(characterID, hoursTimerStarted, hoursLeft);
                        }
                    }
                    
                    // store the new roles and title mask
                    this.CharacterDB.UpdateCharacterRoles(
                        characterID, 0, 0, 0, 0,
                        0, 0, 0, 0, 0
                    );

                    roles = 0;
                    rolesAtHQ = 0;
                    rolesAtBase = 0;
                    rolesAtOther = 0;
                    grantableRoles = 0;
                    grantableRolesAtHQ = 0;
                    grantableRolesAtBase = 0;
                    grantableRolesAtOther = 0;
                    titleMask = 0;

                    // TODO: WEIRD HACK TO ENSURE THAT THE CORPORATION WINDOW UPDATES WITH THE CHANGE
                    // TODO: THERE MIGHT BE SOMETHING ELSE WE CAN DO, BUT THIS WORKS FOR NOW
                    if (call.Client.CorporationRole == 0)
                    {
                        call.Client.CorporationRole = ~long.MaxValue;
                    }
                    else
                    {
                        call.Client.CorporationRole = 0;
                    }

                    call.Client.RolesAtAll = 0;
                    call.Client.RolesAtBase = 0;
                    call.Client.RolesAtOther = 0;
                    call.Client.RolesAtHQ = 0;

                    character.TitleMask = 0;
                }

                this.NotificationManager.NotifyNode(
                    this.ItemFactory.ItemDB.GetItemNode(characterID), 
                    new OnCorporationMemberUpdated(
                        characterID, currentRoles, grantableRoles, rolesAtHQ, grantableRolesAtHQ,
                        rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther, currentBaseID,
                        blockRoles == 1 ? 1 : null, titleMask
                    )
                );
                
                if (this.MembersSparseRowset is not null)
                {
                    PyDictionary<PyString, PyTuple> changes = new PyDictionary<PyString, PyTuple>()
                    {
                        ["roles"] = new PyTuple(2) {[0] = currentRoles, [1] = roles},
                        ["rolesAtHQ"] = new PyTuple(2) {[0] = currentRolesAtHQ, [1] = rolesAtHQ},
                        ["rolesAtBase"] = new PyTuple(2) {[0] = currentRolesAtBase, [1] = rolesAtBase},
                        ["rolesAtOther"] = new PyTuple(2) {[0] = currentRolesAtOther, [1] = rolesAtOther},
                        ["grantableRoles"] = new PyTuple(2) {[0] = currentGrantableRoles, [1] = grantableRoles},
                        ["grantableRolesAtHQ"] = new PyTuple(2) {[0] = currentGrantableRolesAtHQ, [1] = grantableRolesAtHQ},
                        ["grantableRolesAtBase"] = new PyTuple(2) {[0] = currentRolesAtBase, [1] = grantableRolesAtBase},
                        ["grantableRolesAtOther"] = new PyTuple(2) {[0] = currentRolesAtOther, [1] = grantableRolesAtOther},
                        ["baseID"] = new PyTuple(2) {[0] = currentBaseID, [1] = baseID},
                        ["blockRoles"] = new PyTuple(2) {[0] = blockRoles == 0 ? null : 1, [1] = blockRoles == 1 ? 1 : null},
                        ["titleMask"] = new PyTuple(2) {[0] = currentTitleMask, [1] = titleMask}
                    };
                    
                    this.MembersSparseRowset.UpdateRow(characterID, changes);
                }
                
                // update the stasis timer
                if (currentStasisTimer == null)
                    this.CharacterDB.SetCharacterStasisTimer(characterID, blockRoles == 1 ? DateTime.UtcNow.ToFileTimeUtc() : null);

                return null;
            }

            if (currentBlockRoles == 1)
                throw new CrpRolesDenied(characterID);
            
            // if the character is not a director, it cannot grant grantable roles (duh)
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false && (grantableRoles != 0 ||
                grantableRolesAtBase != 0 || grantableRolesAtOther != 0 || grantableRolesAtHQ != 0))
                throw new CrpAccessDenied(MLS.UI_CORP_DO_NOT_HAVE_ROLE_DIRECTOR);
            
            // get differences with the current specified roles, we want to know what changes
            long rolesDifference = currentRoles ^ roles;
            long rolesAtBaseDifference = currentRolesAtBase ^ rolesAtBase;
            long rolesAtHQDifference = currentRolesAtHQ ^ rolesAtHQ;
            long rolesAtOtherDifference = currentRolesAtOther ^ rolesAtOther;
            
            // ensure the character has permissions to modify those roles
            if ((rolesDifference & character.GrantableRoles) != rolesDifference ||
                (rolesAtBaseDifference & character.GrantableRolesAtBase) != rolesAtBaseDifference ||
                (rolesAtOtherDifference & character.GrantableRolesAtOther) != rolesAtOtherDifference ||
                (rolesAtHQDifference & character.GrantableRolesAtHQ) != rolesAtHQDifference)
                throw new CrpAccessDenied(MLS.UI_CORP_INSUFFICIENT_RIGHTS_TO_EDIT_MEMBERS_DETAILS);
            
            // if the new role is director then all the roles must be granted
            if (roles == 1)
            {
                roles = long.MaxValue;
                rolesAtHQ = long.MaxValue;
                rolesAtBase = long.MaxValue;
                rolesAtOther = long.MaxValue;
            }
            
            // store the new roles and title mask
            this.CharacterDB.UpdateCharacterRoles(
                characterID, roles, rolesAtHQ, rolesAtBase, rolesAtOther,
                grantableRoles, grantableRolesAtHQ, grantableRolesAtBase, grantableRolesAtOther, titleMask ?? 0
            );
            
            // let the sparse rowset know that a change was done, this should refresh the character information
            if (this.MembersSparseRowset is not null)
            {
                PyDictionary<PyString, PyTuple> changes = new PyDictionary<PyString, PyTuple>()
                {
                    ["roles"] = new PyTuple(2) {[0] = currentRoles, [1] = roles},
                    ["rolesAtHQ"] = new PyTuple(2) {[0] = currentRolesAtHQ, [1] = rolesAtHQ},
                    ["rolesAtBase"] = new PyTuple(2) {[0] = currentRolesAtBase, [1] = rolesAtBase},
                    ["rolesAtOther"] = new PyTuple(2) {[0] = currentRolesAtOther, [1] = rolesAtOther},
                    ["grantableRoles"] = new PyTuple(2) {[0] = currentGrantableRoles, [1] = grantableRoles},
                    ["grantableRolesAtHQ"] = new PyTuple(2) {[0] = currentGrantableRolesAtHQ, [1] = grantableRolesAtHQ},
                    ["grantableRolesAtBase"] = new PyTuple(2) {[0] = currentRolesAtBase, [1] = grantableRolesAtBase},
                    ["grantableRolesAtOther"] = new PyTuple(2) {[0] = currentRolesAtOther, [1] = grantableRolesAtOther},
                    ["baseID"] = new PyTuple(2) {[0] = currentBaseID, [1] = baseID},
                };

                if (titleMask is not null)
                    changes["titleMask"] = new PyTuple(2) {[0] = currentTitleMask, [1] = titleMask};

                if (blockRoles is not null)
                    changes["blockRoles"] = new PyTuple(2) {[0] = null, [1] = blockRoles};
                    
                this.MembersSparseRowset.UpdateRow(characterID, changes);
            }

            // notify the node about the changes
            this.NotificationManager.NotifyNode(
                this.ItemFactory.ItemDB.GetItemNode(characterID),
                new OnCorporationMemberUpdated(characterID, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ,
                    rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther, baseID, currentBlockRoles, titleMask ?? 0)
            );

            // check if the character is connected and update it's session
            // this also allows to notify the node where the character is loaded
            if (this.ClientManager.TryGetClientByCharacterID(characterID, out Client client) == false)
                return null;

            // get the title roles and calculate current roles for the session
            this.DB.GetTitleInformation(character.CorporationID, character.TitleMask,
                out long titleRoles, out long titleRolesAtHQ, out long titleRolesAtBase, out long titleRolesAtOther,
                out long titleGrantableRoles, out long titleGrantableRolesAtHQ, out long titleGrantableRolesAtBase,
                out long titleGrantableRolesAtOther, out _
            );
            
            // update the roles on the session and send the session change to the player
            client.CorporationRole = roles | titleRoles;
            client.RolesAtOther = rolesAtOther | titleRolesAtOther;
            client.RolesAtHQ = rolesAtHQ | titleRolesAtHQ;
            client.RolesAtBase = rolesAtBase | titleRolesAtBase;
            client.RolesAtAll = client.CorporationRole | client.RolesAtOther | client.RolesAtHQ | client.RolesAtBase;
            client.SendSessionChange();
            
            // TODO: CHECK THAT NEW BASE ID IS VALID
            
            return null;
        }

        public PyDataType CanLeaveCurrentCorporation(CallInformation call)
        {
            int characterID = call.Client.EnsureCharacterIsSelected();
            long? stasisTimer = this.CharacterDB.GetCharacterStasisTimer(characterID);

            try
            {
                if (call.Client.CorporationID <= ItemFactory.NPC_CORPORATION_ID_MAX)
                {
                    if (call.Client.WarFactionID is null)
                    {
                        throw new CrpCantQuitDefaultCorporation();
                    }
                }

                long currentTime = DateTime.UtcNow.ToFileTimeUtc();

                if (stasisTimer is null &&
                    (call.Client.CorporationRole > 0 ||
                     call.Client.RolesAtAll > 0 ||
                     call.Client.RolesAtBase > 0 ||
                     call.Client.RolesAtOther > 0 ||
                     call.Client.RolesAtHQ > 0)
                    )
                {
                    throw new CrpCantQuitNotInStasis(
                        characterID,
                        call.Client.CorporationRole | call.Client.RolesAtAll | call.Client.RolesAtBase | call.Client.RolesAtOther | call.Client.RolesAtHQ
                    );
                }
                
                if (stasisTimer is not null)
                {
                    long stasisTimerEnd = (long) stasisTimer + TimeSpan.FromHours(24).Ticks;
                    
                    if (stasisTimerEnd > DateTime.UtcNow.ToFileTimeUtc())
                    {
                        int hoursTimerStarted = (int) ((currentTime - stasisTimer) / TimeSpan.TicksPerHour);
                        int hoursLeft = (int) (((long) stasisTimerEnd - currentTime) / TimeSpan.TicksPerHour);
                        
                        throw new CrpCantQuitNotCompletedStasisPeriodIsBlocked(characterID, hoursTimerStarted, hoursLeft);
                    }
                }

                return new PyTuple(3)
                {
                    [0] = true,
                    [1] = null,
                    [2] = null
                };
            }
            catch (UserError e)
            {
                return new PyTuple(3)
                {
                    [0] = false, // can leave
                    [1] = e.Reason, // error message
                    [2] = e.Dictionary
                };
            }
        }

        public PyDataType CreateRecruitmentAd(PyInteger days, PyInteger stationID, PyInteger raceMask, PyInteger typeMask, PyInteger allianceID, PyInteger skillpoints, PyString description, CallInformation call)
        {
            Station station = this.ItemFactory.GetStaticStation(stationID);
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            long price = this.CorporationAdvertisementFlatFee + (this.CorporationAdvertisementDailyRate * days);
            
            // TODO: ENSURE stationID MATCHES ONE OF OUR OFFICES FOR THE ADVERT TO BE CREATED
            // get the current wallet and check if there's enough money on it
            using (Wallet wallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, call.Client.CorpAccountKey, true))
            {
                wallet.EnsureEnoughBalance(price);
                wallet.CreateJournalRecord(MarketReference.CorporationAdvertisementFee, callerCharacterID, null, null, price);
            }
            
            // now create the ad
            ulong adID = this.DB.CreateRecruitmentAd(stationID, days, call.Client.CorporationID, typeMask, raceMask, description, skillpoints);
            // create the notification and notify everyone at that station
            OnCorporationRecruitmentAdChanged changes = new OnCorporationRecruitmentAdChanged(call.Client.CorporationID, adID);
            // add the fields for the recruitment ad change
            changes
                .AddValue("adID", null, adID)
                .AddValue("corporationID", null, call.Client.CorporationID)
                .AddValue("channelID", null, call.Client.CorporationID)
                .AddValue("typeMask", null, typeMask)
                .AddValue("description", null, description)
                .AddValue("stationID", null, stationID)
                .AddValue("raceMask", null, raceMask)
                .AddValue("allianceID", null, call.Client.AllianceID)
                .AddValue("expiryDateTime", null, DateTime.UtcNow.AddDays(days).ToFileTimeUtc())
                .AddValue("createDateTime", null, DateTime.UtcNow.ToFileTimeUtc())
                .AddValue("regionID", null, station.RegionID)
                .AddValue("solarSystemID", null, station.SolarSystemID)
                .AddValue("constellationID", null, station.ConstellationID)
                .AddValue("skillPoints", null, skillpoints);

            // TODO: MAYBE NOTIFY CHARACTERS IN THE STATION?
            // notify corporation members
            this.NotificationManager.NotifyCorporation(call.Client.CorporationID, changes);
            
            return null;
        }

        public PyDataType UpdateTitles(PyObjectData rowset, CallInformation call)
        {
            // ensure the character has the director role
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_DO_NOT_HAVE_ROLE_DIRECTOR);
            
            Rowset list = rowset;
            
            // update the changed titles first
            foreach (PyList entry in list.Rows)
            {
                // titleID, titleName, roles, grantableRoles, rolesAtHQ, grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther
                int titleID = entry[0] as PyInteger;
                string newName = entry[1] as PyString;
                long roles = entry[2] as PyInteger;
                long grantableRoles = entry[3] as PyInteger;
                long rolesAtHQ = entry[4] as PyInteger;
                long grantableRolesAtHQ = entry[5] as PyInteger;
                long rolesAtBase = entry[6] as PyInteger;
                long grantableRolesAtBase = entry[7] as PyInteger;
                long rolesAtOther = entry[8] as PyInteger;
                long grantableRolesAtOther = entry[9] as PyInteger;
                
                // get previous roles first
                this.DB.GetTitleInformation(call.Client.CorporationID, titleID,
                    out long titleRoles, out long titleRolesAtHQ, out long titleRolesAtBase, out long titleRolesAtOther,
                    out long titleGrantableRoles, out long titleGrantableRolesAtHQ, out long titleGrantableRolesAtBase,
                    out long titleGrantableRolesAtOther, out string titleName
                );

                // store the new information
                this.DB.UpdateTitle(call.Client.CorporationID, titleID, newName, roles, grantableRoles, rolesAtHQ,
                    grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther);
                
                // notify everyone about the title change
                this.NotificationManager.NotifyCorporation(
                    call.Client.CorporationID,
                    new OnTitleChanged(call.Client.CorporationID, titleID)
                        .AddChange("titleName", titleName, newName)
                        .AddChange("roles", titleRoles, roles)
                        .AddChange("grantableRoles", titleGrantableRoles, grantableRoles)
                        .AddChange("rolesAtHQ", titleRolesAtHQ, rolesAtHQ)
                        .AddChange("grantableRolesAtHQ", titleGrantableRolesAtHQ, grantableRolesAtHQ)
                        .AddChange("rolesAtBase", titleRolesAtBase, rolesAtBase)
                        .AddChange("grantableRolesAtBase", titleGrantableRolesAtBase, grantableRolesAtBase)
                        .AddChange("rolesAtOther", titleRolesAtOther, rolesAtOther)
                        .AddChange("grantableRolesAtOther", titleGrantableRolesAtOther, grantableRolesAtOther)
                );
            }
            
            // get all the players online for this corporation
            if (this.ClientManager.TryGetClientsByCorporationID(call.Client.CorporationID, out Dictionary<int, Client> clients) == true)
            {
                foreach ((int _, Client client) in clients)
                {
                    // characterID should never be null here
                    long titleMask = this.DB.GetTitleMaskForCharacter((int) client.CharacterID);
                    
                    // get the title roles and calculate current roles for the session
                    this.DB.GetTitleInformation(client.CorporationID, titleMask,
                        out long titleRoles, out long titleRolesAtHQ, out long titleRolesAtBase, out long titleRolesAtOther,
                        out long titleGrantableRoles, out long titleGrantableRolesAtHQ, out long titleGrantableRolesAtBase,
                        out long titleGrantableRolesAtOther, out _
                    );
                    this.CharacterDB.GetCharacterRoles((int) client.CharacterID,
                        out long characterRoles, out long characterRolesAtBase, out long characterRolesAtHQ,  out long characterRolesAtOther,
                        out long characterGrantableRoles, out long characterGrantableRolesAtBase, out long characterGrantableRolesAtHQ, 
                        out long characterGrantableRolesAtOther, out _, out _, out _);
            
                    // update the roles on the session and send the session change to the player
                    client.CorporationRole = characterRoles | titleRoles;
                    client.RolesAtOther = characterRolesAtOther | titleRolesAtOther;
                    client.RolesAtHQ = characterRolesAtHQ | titleRolesAtHQ;
                    client.RolesAtBase = characterRolesAtBase | titleRolesAtBase;
                    client.RolesAtAll = client.CorporationRole | client.RolesAtOther | client.RolesAtHQ | client.RolesAtBase;
                    client.SendSessionChange();
                }
            }
            
            return null;
        }

        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            // TODO: CHECK IF ANY NODE HAS THIS CORPORATION LOADED
            // TODO: IF NOT, LOAD IT HERE AND RETURN OUR ID
            return this.BoundServiceManager.Container.NodeID;
        }

        protected override MultiClientBoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            if (this.MachoResolveObject(bindParams, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            Corporation corp = this.ItemFactory.LoadItem<Corporation>(bindParams.ObjectID);
            
            return new corpRegistry (this.DB, this.AlliancesDB, this.ChatDB, this.CharacterDB, this.NotificationManager, this.MailManager, this.WalletManager, this.Container, this.ItemFactory, this.ClientManager, this.AncestryManager, corp, bindParams.ExtraValue, this);
        }

        public override bool IsClientAllowedToCall(CallInformation call)
        {
            return this.Corporation.ID == call.Client.CorporationID;
        }

        protected override void OnClientDisconnected()
        {
        }

        public PyDataType DeleteRecruitmentAd(PyInteger advertID, CallInformation call)
        {
            if (CorporationRole.PersonnelManager.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_NEED_ROLE_PERS_MAN_TO_MANAGE_ADS);

            if (this.DB.DeleteRecruitmentAd(advertID, call.Client.CorporationID) == true)
            {
                // TODO: MAYBE NOTIFY CHARACTERS IN THE STATION?
                // send notification
                this.NotificationManager.NotifyCorporation(call.Client.CorporationID,
                    new OnCorporationRecruitmentAdChanged(call.Client.CorporationID, advertID)
                        .AddValue ("adID", advertID, null)
                );
            }

            return null;
        }

        public PyDataType UpdateRecruitmentAd(PyInteger adID, PyInteger typeMask, PyInteger raceMask, PyInteger skillPoints, PyString description, CallInformation call)
        {
            if (CorporationRole.PersonnelManager.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_NEED_ROLE_PERS_MAN_TO_MANAGE_ADS);

            if (this.DB.UpdateRecruitmentAd(adID, call.Client.CorporationID, typeMask, raceMask, description, skillPoints) == true)
            {
                OnCorporationRecruitmentAdChanged changes = new OnCorporationRecruitmentAdChanged(call.Client.CorporationID, adID);
                // add the fields for the recruitment ad change
                changes
                    .AddValue("typeMask", typeMask, typeMask)
                    .AddValue("description", description, description)
                    .AddValue("raceMask", raceMask, raceMask)
                    .AddValue("skillPoints", skillPoints, skillPoints);

                // TODO: MAYBE NOTIFY CHARACTERS IN THE STATION?
                this.NotificationManager.NotifyCorporation(call.Client.CorporationID, changes);
            }

            return null;
        }

        public PyDataType GetApplications(CallInformation call)
        {
            if (CorporationRole.PersonnelManager.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_NEED_ROLE_PERS_MAN_TO_MANAGE_APPLICATIONS);
            
            return this.DB.GetApplicationsToCorporation(call.Client.CorporationID);
        }

        public PyDataType InsertApplication(PyInteger corporationID, PyString text, CallInformation call)
        {
            // TODO: CHECK IF THE CHARACTER IS A CEO AND DENY THE APPLICATION CREATION
            int characterID = call.Client.EnsureCharacterIsSelected();
            
            // create the application in the database
            this.DB.CreateApplication(characterID, corporationID, text);
            OnCorporationApplicationChanged change = new OnCorporationApplicationChanged(corporationID, characterID);

            change
                .AddValue("corporationID", null, corporationID)
                .AddValue("characterID", null, characterID)
                .AddValue("applicationDateTime", null, DateTime.UtcNow.ToFileTimeUtc())
                .AddValue("applicationText", null, text)
                .AddValue("status", null, 0);
            
            this.NotificationManager.NotifyCorporationByRole(corporationID, CorporationRole.PersonnelManager, change);
            this.NotificationManager.NotifyCharacter(characterID, change);
            
            return null;
        }

        public PyDataType DeleteApplication(PyInteger corporationID, PyInteger characterID, CallInformation call)
        {
            int currentCharacterID = call.Client.EnsureCharacterIsSelected();

            if (characterID != currentCharacterID)
                throw new CrpAccessDenied("This application does not belong to you");
            
            this.DB.DeleteApplication(characterID, corporationID);
            OnCorporationApplicationChanged change = new OnCorporationApplicationChanged(corporationID, characterID);

            // this notification doesn't seem to update the window if it's focused by the corporation personnel
            change
                .AddValue("corporationID", corporationID, null)
                .AddValue("characterID", characterID, null)
                .AddValue("status", 0, null);

            // notify about the application change
            this.NotificationManager.NotifyCorporationByRole(corporationID, CorporationRole.PersonnelManager, change);
            this.NotificationManager.NotifyCharacter(characterID, change);

            return null;
        }

        public PyDataType UpdateApplicationOffer(PyInteger characterID, PyString text, PyInteger newStatus, PyInteger applicationDateTime, CallInformation call)
        {
            if (CorporationRole.PersonnelManager.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_NEED_ROLE_PERS_MAN_TO_MANAGE_APPLICATIONS);
            
            // TODO: CHECK THAT THE APPLICATION EXISTS TO PREVENT A CHARACTER FROM BEING FORCE TO JOIN A CORPORATION!!

            string characterName = this.CharacterDB.GetCharacterName(characterID);

            int corporationID = call.Client.CorporationID;
            
            // accept application
            if (newStatus == 6)
            {
                // ensure things are updated on the database first
                int oldCorporationID = this.CharacterDB.GetCharacterCorporationID(characterID);
                // remove the character from old channels
                this.ChatDB.LeaveChannel(oldCorporationID, characterID);
                this.ChatDB.LeaveEntityChannel(oldCorporationID, characterID);
                // join the character to the new channels
                this.ChatDB.JoinEntityChannel(corporationID, characterID, ChatDB.CHATROLE_CONVERSATIONALIST);
                this.ChatDB.JoinChannel(corporationID, characterID, ChatDB.CHATROLE_CONVERSATIONALIST);
                // build the notification of corporation change
                OnCorporationMemberChanged change = new OnCorporationMemberChanged(characterID, oldCorporationID, corporationID);
                // ensure the database reflects these changes
                this.CharacterDB.UpdateCharacterRoles(
                    characterID, 0, 0, 0,
                    0, 0, 0, 0, 0, 0
                );
                this.CharacterDB.UpdateCorporationID(characterID, corporationID);
                // notify cluster about the corporation changes
                this.NotificationManager.NotifyCorporation(change.OldCorporationID, change);
                this.NotificationManager.NotifyCorporation(change.NewCorporationID, change);
                // create the employment record for the character
                this.CharacterDB.CreateEmploymentRecord(characterID, corporationID, DateTime.UtcNow.ToFileTimeUtc());

                // check if the character is connected and update it's session
                if (this.ClientManager.TryGetClientByCharacterID(characterID, out Client client) == true)
                {
                    // update character's session
                    client.CorporationID = corporationID;
                    client.RolesAtAll = 0;
                    client.CorporationRole = 0;
                    client.RolesAtBase = 0;
                    client.RolesAtOther = 0;
                    client.RolesAtHQ = 0;
                    // finally send the session change
                    client.SendSessionChange();
                    
                    // player is conected, notify the node that owns it to make the changes required
                    Notifications.Nodes.Corps.OnCorporationMemberChanged nodeNotification =
                        new Notifications.Nodes.Corps.OnCorporationMemberChanged(
                            characterID, oldCorporationID, corporationID
                    );
                    
                    // TODO: WORKOUT A BETTER WAY OF NOTIFYING THIS TO THE NODE, IT MIGHT BE BETTER TO INSPECT THE SESSION CHANGE INSTEAD OF DOING IT LIKE THIS
                    long characterNodeID = this.ItemFactory.ItemDB.GetItemNode(characterID);
                    long newCorporationNodeID = this.ItemFactory.ItemDB.GetItemNode(corporationID);
                    long oldCorporationNodeID = this.ItemFactory.ItemDB.GetItemNode(oldCorporationID);

                    // get the node where the character is loaded
                    // the node where the corporation is loaded also needs to get notified so the members rowset can be updated
                    if (characterNodeID > 0)
                        this.NotificationManager.NotifyNode(characterNodeID, nodeNotification);
                    if (newCorporationNodeID != characterNodeID)
                        this.NotificationManager.NotifyNode(newCorporationNodeID, nodeNotification);
                    if (oldCorporationNodeID != newCorporationNodeID && oldCorporationNodeID != characterNodeID)
                        this.NotificationManager.NotifyNode(oldCorporationNodeID, nodeNotification);
                }
                
                // this one is a bit harder as this character might not be in the same node as this service
                // take care of the proper 
                this.MailManager.SendMail(
                    call.Client.CorporationID,
                    characterID,
                    $"Welcome to {this.mCorporation.Name}",
                    $"Dear {characterName}. Your application to join <b>{this.mCorporation.Name}</b> has been accepted."
                );
            }
            // reject application
            else if (newStatus == 4)
            {
                this.MailManager.SendMail(
                    call.Client.CorporationID,
                    characterID,
                    $"Rejected application to join {this.mCorporation.Name}",
                    $"Dear {characterName}. Your application to join <b>{this.mCorporation.Name}</b> has been REJECTED."
                );
            }
            
            this.DB.DeleteApplication(characterID, corporationID);
            OnCorporationApplicationChanged applicationChange = new OnCorporationApplicationChanged(corporationID, characterID);

            // this notification doesn't seem to update the window if it's focused by the corporation personnel
            applicationChange
                .AddValue("corporationID", corporationID, null)
                .AddValue("characterID", characterID, null)
                .AddValue("status", 0, null);

            // notify about the application change
            this.NotificationManager.NotifyCorporationByRole(corporationID, CorporationRole.PersonnelManager, applicationChange);
            this.NotificationManager.NotifyCharacter(characterID, applicationChange);

            return null;
        }

        public PyDataType GetMemberIDsWithMoreThanAvgShares(CallInformation call)
        {
            return this.DB.GetMemberIDsWithMoreThanAvgShares(call.Client.CorporationID);
        }

        public PyBool CanViewVotes(PyInteger corporationID, CallInformation call)
        {
            return (call.Client.CorporationID == corporationID && CorporationRole.Director.Is(call.Client.CorporationRole) == true) ||
                   this.DB.GetSharesForOwner(corporationID, call.Client.EnsureCharacterIsSelected()) > 0;
        }

        public PyDataType InsertVoteCase(PyString text, PyString description, PyInteger corporationID, PyInteger type, PyDataType rowsetOptions, PyInteger startDateTime, PyInteger endDateTime, CallInformation call)
        {
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpOnlyDirectorsCanProposeVotes();

            // TODO: current CEO seems to loose control if the vote is for a new CEO, this might complicate things a little on the permissions side of things
            /*                'MAIL_TEMPLATE_CEO_ROLES_REVOKED_BODY': (2424454,
                                                         u'%(candidateName)s is running for CEO in %(corporationName)s. Your roles as CEO have been revoked for the duration of the voting period.'),
                'MAIL_TEMPLATE_CEO_ROLES_REVOKED_SUBJECT': (2424457,
                                                            u'CEO roles revoked'),*/
            
            // check if the character is trying to run for CEO and ensure only if he belongs to the same corporation that can be done
            if (type == (int) CorporationVotes.CEO && call.Client.CorporationID != corporationID)
                throw new CantRunForCEOAtTheMoment();
            // TODO: CHECK CORPORATION MANAGEMENT SKILL
            
            int characterID = call.Client.EnsureCharacterIsSelected();
            
            // parse rowset options
            Rowset options = rowsetOptions;

            int voteCaseID = (int) this.DB.InsertVoteCase(corporationID, characterID, type, startDateTime, endDateTime, text, description);
            OnCorporationVoteCaseChanged change = new OnCorporationVoteCaseChanged(corporationID, voteCaseID)
                .AddValue("corporationID", null, corporationID)
                .AddValue("characterID", null, characterID)
                .AddValue("type", null, type)
                .AddValue("startDateTime", null, startDateTime)
                .AddValue("endDateTime", null, endDateTime)
                .AddValue("text", null, text)
                .AddValue("description", null, description);
                
            // notify the new vote being created to all shareholders
            this.NotificationManager.NotifyCharacters(
                this.DB.GetShareholderList(corporationID),
                change
            );
            this.NotificationManager.NotifyCorporationByRole(corporationID, CorporationRole.Director, change);

            int optionText = 0;
            int parameter = 1;
            int parameter1 = 2;
            int parameter2 = 3;
            
            foreach (PyList entry in options.Rows)
            {
                this.DB.InsertVoteOption(
                    voteCaseID,
                    entry [optionText] as PyString,
                    entry [parameter] as PyInteger,
                    entry [parameter1] as PyInteger,
                    entry [parameter2] as PyInteger
                );
            }
            
            return null;
        }

        public PyDataType GetVoteCasesByCorporation(PyInteger corporationID, PyInteger status, CallInformation call)
        {
            if (this.CanViewVotes(corporationID, call) == false)
                throw new CrpAccessDenied(MLS.UI_SHARED_WALLETHINT12);
            
            if (status == 2)
                return this.DB.GetOpenVoteCasesByCorporation(corporationID);
            else
                return this.DB.GetClosedVoteCasesByCorporation(corporationID);
        }

        public PyDataType GetVoteCasesByCorporation(PyInteger corporationID, PyInteger status, PyInteger maxLen, CallInformation call)
        {
            if (this.CanViewVotes(corporationID, call) == false)
                throw new CrpAccessDenied(MLS.UI_SHARED_WALLETHINT12);
            
            if (status == 2)
                return this.DB.GetOpenVoteCasesByCorporation(corporationID);
            else
                return this.DB.GetClosedVoteCasesByCorporation(corporationID);
        }

        public PyDataType GetVoteCaseOptions(PyInteger corporationID, PyInteger voteCaseID, CallInformation call)
        {
            if (this.CanViewVotes(corporationID, call) == false)
                throw new CrpAccessDenied(MLS.UI_SHARED_WALLETHINT12);
            
            return this.DB.GetVoteCaseOptions(corporationID, voteCaseID);
        }

        public PyDataType GetVotes(PyInteger corporationID, PyInteger voteCaseID, CallInformation call)
        {
            if (this.CanViewVotes(corporationID, call) == false)
                throw new CrpAccessDenied(MLS.UI_SHARED_WALLETHINT12);
            
            return this.DB.GetVotes(corporationID, voteCaseID, call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType InsertVote(PyInteger corporationID, PyInteger voteCaseID, PyInteger optionID, CallInformation call)
        {
            int characterID = call.Client.EnsureCharacterIsSelected();
            
            if (this.CanViewVotes(corporationID, call) == false)
                throw new CrpAccessDenied(MLS.UI_SHARED_WALLETHINT12);
            
            this.DB.InsertVote(voteCaseID, optionID, characterID);

            OnCorporationVoteChanged change = new OnCorporationVoteChanged(corporationID, voteCaseID, characterID)
                .AddValue("characterID", null, characterID)
                .AddValue("optionID", null, optionID);
            // notify the new vote to the original character
            this.NotificationManager.NotifyCharacters(
                this.DB.GetShareholderList(corporationID),
                change
            );
            this.NotificationManager.NotifyCorporationByRole(corporationID, CorporationRole.Director, change);
            
            return null;
        }

        public PyDataType GetAllianceApplications(CallInformation call)
        {
            return this.DB.GetAllianceApplications(this.ObjectID);
        }

        public PyDataType ApplyToJoinAlliance(PyInteger allianceID, PyString applicationText, CallInformation call)
        {
            // TODO: CHECK PERMISSIONS, ONLY DIRECTOR CAN DO THAT?
            // get the current alliance and notify members of that alliance that the application is no more
            int? currentApplicationAllianceID = this.DB.GetCurrentAllianceApplication(this.ObjectID);

            if (currentApplicationAllianceID is not null)
            {
                OnAllianceApplicationChanged change =
                    new OnAllianceApplicationChanged((int) currentApplicationAllianceID, call.Client.CorporationID)
                        .AddChange("allianceID", currentApplicationAllianceID, null)
                        .AddChange("corporationID", call.Client.CorporationID, null)
                        .AddChange("applicationText", "", null)
                        .AddChange("applicationDateTime", DateTime.UtcNow.ToFileTimeUtc(), null)
                        .AddChange("state", 0, null);

                this.NotificationManager.NotifyAlliance((int) currentApplicationAllianceID, change);
            }
            
            // insert the application
            this.DB.InsertAllianceApplication(allianceID, this.ObjectID, applicationText);
            
            // notify the new application
            OnAllianceApplicationChanged newChange =
                new OnAllianceApplicationChanged(allianceID, this.ObjectID)
                    .AddChange("allianceID", null, allianceID)
                    .AddChange("corporationID", null, this.ObjectID)
                    .AddChange("applicationText", null, applicationText)
                    .AddChange("applicationDateTime", null, DateTime.UtcNow.ToFileTimeUtc())
                    .AddChange("state", null, (int) AllianceApplicationStatus.New);

            // TODO: WRITE A CUSTOM NOTIFICATION FOR ALLIANCE AND ROLE BASED
            this.NotificationManager.NotifyAlliance(allianceID, newChange);
            // notify the player creating the application
            this.NotificationManager.NotifyCorporationByRole(call.Client.CorporationID, CorporationRole.Director, newChange);
            
            return null;
        }
    }
}