using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using EVE;
using EVE.Packets.Exceptions;
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
using Node.Notifications.Client.Corporations;
using Node.Notifications.Client.Wallet;
using Node.Notifications.Nodes.Corporations;
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
        private ChatDB ChatDB { get; init; }
        private CharacterDB CharacterDB { get; init; }
        private ItemFactory ItemFactory { get; }
        private WalletManager WalletManager { get; init; }
        private NodeContainer Container { get; init; }
        private NotificationManager NotificationManager { get; init; }
        private MailManager MailManager { get; init; }
        private ClientManager ClientManager { get; init; }
        private MembersSparseRowsetService MembersSparseRowset { get; set; }
        private OfficesSparseRowsetService OfficesSparseRowset { get; set; }
        
        // constants
        private long CorporationAdvertisementFlatFee { get; init; }
        private long CorporationAdvertisementDailyRate { get; init; }
        
        public corpRegistry(CorporationDB db, ChatDB chatDB, CharacterDB characterDB, NotificationManager notificationManager, MailManager mailManager, WalletManager walletManager, NodeContainer container, ItemFactory itemFactory, ClientManager clientManager, BoundServiceManager manager) : base(manager)
        {
            this.DB = db;
            this.ChatDB = chatDB;
            this.CharacterDB = characterDB;
            this.NotificationManager = notificationManager;
            this.MailManager = mailManager;
            this.WalletManager = walletManager;
            this.Container = container;
            this.ItemFactory = itemFactory;
            this.ClientManager = clientManager;
        }

        protected corpRegistry(CorporationDB db, ChatDB chatDB, CharacterDB characterDB, NotificationManager notificationManager, MailManager mailManager, WalletManager walletManager, NodeContainer container, ItemFactory itemFactory, ClientManager clientManager, Corporation corp, int isMaster, corpRegistry parent) : base (parent, corp.ID)
        {
            this.DB = db;
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
            if (this.MembersSparseRowset == null)
            {
                // generate the sparse rowset
                SparseRowsetHeader rowsetHeader = this.DB.GetMembersSparseRowset(call.Client.CorporationID);

                PyDictionary dict = new PyDictionary
                {
                    ["realRowCount"] = rowsetHeader.Count
                };
            
                // create a service for handling it's calls
                this.MembersSparseRowset =
                    new MembersSparseRowsetService(this.Corporation, this.DB, rowsetHeader, this.BoundServiceManager, call.Client);

                rowsetHeader.BoundObjectIdentifier = this.MembersSparseRowset.MachoBindObject(dict, call.Client);
            }
            
            // finally return the data
            return this.MembersSparseRowset.RowsetHeader;
        }

        public PyDataType GetOffices(CallInformation call)
        {
            if (this.OfficesSparseRowset == null)
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
            int titleMask = this.DB.GetTitleMaskForCharacter(characterID, call.Client.CorporationID);
            Dictionary<int, string> titles = this.DB.GetTitlesNames(call.Client.CorporationID);
            PyDictionary dictForKeyVal = new PyDictionary();

            int number = 0;

            foreach ((int _, string name) in titles)
                dictForKeyVal["title" + (++number)] = name;
            
            // we're supposed to be from the same corp, so add the extra information manually
            // TODO: TEST WITH USERS FROM OTHER CORPS
            dictForKeyVal["corpID"] = call.Client.CorporationID;
            dictForKeyVal["allianceID"] = call.Client.AllianceID;
            dictForKeyVal["title"] = "TITLE HERE";

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

        public PyDataType AddCorporation(PyString corporationName, PyString tickerName, PyString description,
            PyString url, PyDecimal taxRate, PyInteger shape1, PyInteger shape2, PyInteger shape3, PyInteger color1,
            PyInteger color2, PyInteger color3, PyDataType typeface, CallInformation call)
        {
            // do some basic checks on what the player can do
            if (CorporationRole.Director.Is(call.Client.CorporationRole) == true)
                throw new CEOCannotCreateCorporation();
            if (call.Client.StationID == null)
                throw new CanOnlyCreateCorpInStation();
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
            
            int stationID = call.Client.EnsureCharacterIsInStation();
            int corporationStartupCost = this.Container.Constants[Constants.corporationStartupCost];
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            // TODO: PROPERLY IMPLEMENT THIS CHECK, RIGHT NOW THE CHARACTER AND THE CORPREGISTRY INSTANCES DO NOT HAVE TO BE LOADED ON THE SAME NODE
            // TODO: SWITCH UP THE CORPORATION CHANGE MECHANISM TO NOT RELY ON THE CHARACTER OBJECT SO THIS CAN BE DONE THROUGH THE DATABASE
            // TODO: DIRECTLY
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            // ensure the character has the required skills
            long corporationManagementLevel = character.GetSkillLevel(Types.CorporationManagement);
            long ethnicRelationsLevel = character.GetSkillLevel(Types.EthnicRelations);
            
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
                        stationID, (int) corporationManagementLevel * 10, (int) call.Client.RaceID, (ethnicRelationsLevel > 0) ? 63 : (int) call.Client.RaceID,
                        shape1, shape2, shape3, color1, color2, color3, typeface as PyString
                    );
                    // create default titles
                    this.DB.CreateDefaultTitlesForCorporation(corporationID);
                    // create the record in the journal
                    wallet.CreateJournalRecord(MarketReference.CorporationRegistrationFee, null, null, corporationStartupCost);
                    
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
                    // ensure the database reflects these changes
                    this.CharacterDB.UpdateCharacterRoles(
                        character.ID, long.MaxValue, long.MaxValue, long.MaxValue,
                        long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue, long.MaxValue
                    );
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
            OnCorporationChanged change = new OnCorporationChanged(call.Client.CorporationID);

            change
                .AddChange("description", this.Corporation.Description, newDescription)
                .AddChange("url", this.Corporation.Url, newUrl)
                .AddChange("taxRate", this.Corporation.TaxRate, newTax);

            this.Corporation.Description = newDescription;
            this.Corporation.Url = newUrl;
            this.Corporation.TaxRate = newTax;

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
            
            using (Wallet wallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, 1000))
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

        public PyDataType UpdateCorporationAbilities(CallInformation call)
        {
            if (this.mCorporation.CeoID != call.Client.CharacterID)
                throw new CrpAccessDenied(MLS.UI_CORP_ACCESSDENIED12);

            Character character = this.ItemFactory.GetItem<Character>(call.Client.EnsureCharacterIsSelected());
            
            // update the abilities of the corporation
            long corporationManagementLevel = character.GetSkillLevel(Types.CorporationManagement);
            long ethnicRelationsLevel = character.GetSkillLevel(Types.EthnicRelations);
            
            return null;
        }
        
        public PyDataType GetRecruitmentAdsForCorporation(CallInformation call)
        {
            return this.DB.GetRecruitmentAds(null, null, null, null, null, null, null, this.mCorporation.ID);
        }

        public PyDataType UpdateMember(PyInteger characterID, PyString title, PyInteger divisionID,
            PyInteger squadronID, PyInteger roles, PyInteger grantableRoles, PyInteger rolesAtHQ,
            PyInteger grantableRolesAtHQ, PyInteger rolesAtBase, PyInteger grantableRolesAtBase, PyInteger rolesAtOther,
            PyInteger grantableRolesAtOther, PyInteger baseID, PyInteger titleMask, PyInteger blockRoles,
            CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            
            // get current roles for that character
            this.CharacterDB.GetCharacterRoles(characterID, out long currentRoles, out long currentRolesAtBase,
                out long currentRolesAtHQ, out long currentRolesAtOther, out long currentGrantableRoles,
                out long currentGrantableRolesAtBase, out long currentGrantableRolesAtHQ,
                out long currentGrantableRolesAtOther, out bool currentBlockRoles, out int? currentBaseID
            );
            
            // the only modification a member can perform on itself is blocking roles
            if (characterID == callerCharacterID)
            {
                this.CharacterDB.UpdateCharacterBlockRole(characterID, blockRoles);
                
                this.NotificationManager.NotifyNode(
                    this.ItemFactory.ItemDB.GetItemNode(characterID), 
                    new OnCorporationMemberUpdated(
                        characterID, roles, currentGrantableRoles, rolesAtHQ, currentGrantableRolesAtHQ,
                        rolesAtBase, currentGrantableRolesAtBase, rolesAtOther, currentGrantableRolesAtOther, currentBaseID,
                        blockRoles == 1, character.TitleMask
                    )
                );
                
                return null;
            }
            
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
            this.DB.GetTitleRoles(character.CorporationID, character.TitleMask,
                out long titleRoles, out long titleRolesAtHQ, out long titleRolesAtBase, out long titleRolesAtOther,
                out long titleGrantableRoles, out long titleGrantableRolesAtHQ, out long titleGrantableRolesAtBase,
                out long titleGrantableRolesAtOther
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
            // TODO: IMPLEMENT THIS
            return new PyTuple(3)
            {
                [0] = false, // can leave
                [1] = "CustomError", // error message
                [2] = new PyDictionary() {["error"] = "Not supported yet"} // error details (info for the exception)
            };
        }

        public PyDataType CreateRecruitmentAd(PyInteger days, PyInteger stationID, PyInteger raceMask, PyInteger typeMask, PyInteger allianceID, PyInteger skillpoints, PyString description, CallInformation call)
        {
            Station station = this.ItemFactory.GetStaticStation(stationID);
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            long price = this.CorporationAdvertisementFlatFee + (this.CorporationAdvertisementDailyRate * days);
            
            // get the current wallet and check if there's enough money on it
            using (Wallet wallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, call.Client.CorpAccountKey))
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
                .AddValue("constellationID", null, station.ConstellationID);

            this.NotificationManager.NotifyStation(stationID, changes);
            
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

                this.DB.UpdateTitle(call.Client.CorporationID, titleID, newName, roles, grantableRoles, rolesAtHQ,
                    grantableRolesAtHQ, rolesAtBase, grantableRolesAtBase, rolesAtOther, grantableRolesAtOther);
                
                // notify everyone about the title change
                this.NotificationManager.NotifyCorporation(
                    call.Client.CorporationID,
                    new OnTitleChanged(call.Client.CorporationID, titleID)
                        .AddChange("titleName", null, newName)
                        .AddChange("roles", null, roles)
                        .AddChange("grantableRoles", null, grantableRoles)
                        .AddChange("rolesAtHQ", null, rolesAtHQ)
                        .AddChange("grantableRolesAtHQ", null, grantableRolesAtHQ)
                        .AddChange("rolesAtBase", null, rolesAtBase)
                        .AddChange("grantableRolesAtBase", null, grantableRolesAtBase)
                        .AddChange("rolesAtOther", null, rolesAtOther)
                        .AddChange("grantableRolesAtOther", null, grantableRolesAtOther)
                );
            }
            
            // get all the players online for this corporation
            if (this.ClientManager.TryGetClientsByCorporationID(call.Client.CorporationID, out List<Client> clients) == true)
            {
                foreach (Client client in clients)
                {
                    // characterID should never be null here
                    long titleMask = this.DB.GetTitleMaskForCharacter((int) client.CharacterID, client.CorporationID);
                    
                    // get the title roles and calculate current roles for the session
                    this.DB.GetTitleRoles(client.CorporationID, titleMask,
                        out long titleRoles, out long titleRolesAtHQ, out long titleRolesAtBase, out long titleRolesAtOther,
                        out long titleGrantableRoles, out long titleGrantableRolesAtHQ, out long titleGrantableRolesAtBase,
                        out long titleGrantableRolesAtOther
                    );
                    this.CharacterDB.GetCharacterRoles((int) client.CharacterID,
                        out long characterRoles, out long characterRolesAtBase, out long characterRolesAtHQ,  out long characterRolesAtOther,
                        out long characterGrantableRoles, out long characterGrantableRolesAtBase, out long characterGrantableRolesAtHQ, 
                        out long characterGrantableRolesAtOther, out _, out _);
            
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

        public PyDataType CanViewVotes(PyInteger corporationID, CallInformation call)
        {
            // TODO: CHECK SHARES ON THE CORPORATION ID AND DETERMINE IF THE PLAYER CAN VIEW THE VOTES OR NOT
            return false;
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
            
            return new corpRegistry (this.DB, this.ChatDB, this.CharacterDB, this.NotificationManager, this.MailManager, this.WalletManager, this.Container, this.ItemFactory, this.ClientManager, corp, bindParams.ExtraValue, this);
        }

        public override bool IsClientAllowedToCall(CallInformation call)
        {
            return this.Corporation.ID == call.Client.CorporationID;
        }

        protected override void OnClientDisconnected()
        {
            // free the sparse rowsets if any
        }
    }
}