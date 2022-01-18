using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Common.Services;
using EVESharp.EVE;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.corporationSvc;
using EVESharp.Node.Exceptions.corpRegistry;
using EVESharp.Node.Inventory;
using EVESharp.Node.Market;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Corporations;
using EVESharp.Node.StaticData;
using EVESharp.Node.StaticData.Corporation;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Corporations
{
    public class corporationSvc : IService
    {
        private DatabaseConnection Database { get; init; }
        private CorporationDB DB { get; }
        private NodeContainer Container { get; }
        private WalletManager WalletManager { get; }
        private ItemFactory ItemFactory { get; }
        private NotificationManager NotificationManager { get; }

        public corporationSvc(DatabaseConnection databaseConnection, CorporationDB db, NodeContainer container, WalletManager walletManager, ItemFactory itemFactory, NotificationManager notificationManager)
        {
            this.Database = databaseConnection;
            this.DB = db;
            this.Container = container;
            this.WalletManager = walletManager;
            this.ItemFactory = itemFactory;
            this.NotificationManager = notificationManager;
        }

        public PyTuple GetFactionInfo(CallInformation call)
        {
            return new PyTuple(8)
            {
                [0] = Database.IntIntDictionary(CorporationDB.LIST_FACTION_CORPORATIONS),
                [1] = Database.IntIntListDictionary(CorporationDB.LIST_FACTION_REGIONS),
                [2] = Database.IntIntListDictionary(CorporationDB.LIST_FACTION_CONSTELLATIONS),
                [3] = Database.IntIntListDictionary(CorporationDB.LIST_FACTION_SOLARSYSTEMS),
                [4] = Database.IntIntListDictionary(CorporationDB.LIST_FACTION_RACES),
                [5] = Database.IntIntDictionary(CorporationDB.LIST_FACTION_STATION_COUNT),
                [6] = Database.IntIntDictionary(CorporationDB.LIST_FACTION_STATION_COUNT),
                [7] = Database.IntRowDictionary(0, CorporationDB.LIST_NPC_INFO)
            };
        }

        public PyDataType GetNPCDivisions(CallInformation call)
        {
            return Database.Rowset(CorporationDB.LIST_NPC_DIVISIONS);
        }

        public PyTuple GetMedalsReceived(PyInteger characterID, CallInformation call)
        {
            // TODO: CACHE THIS ANSWER TOO
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            bool publicOnly = callerCharacterID != characterID;
            
            return new PyTuple(2)
            {
                [0] = this.DB.GetMedalsReceived(characterID, publicOnly),
                [1] = this.DB.GetMedalsReceivedDetails(characterID, publicOnly)
            };
        }

        public PyDataType GetEmploymentRecord(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetEmploymentRecord(characterID);
        }

        public PyDataType GetRecruitmentAdTypes(CallInformation call)
        {
            return Database.CRowset(CorporationDB.GET_RECRUITMENT_AD_TYPES);
        }

        public PyDataType GetRecruitmentAdsByCriteria(PyInteger regionID, PyInteger skillPoints, PyInteger typeMask,
            PyInteger raceMask, PyInteger isInAlliance, PyInteger minMembers, PyInteger maxMembers, CallInformation call)
        {
            return this.GetRecruitmentAdsByCriteria(regionID, skillPoints * 1.0, typeMask, raceMask, isInAlliance,
                minMembers, maxMembers, call);
        }
        
        public PyDataType GetRecruitmentAdsByCriteria(PyInteger regionID, PyDecimal skillPoints, PyInteger typeMask,
            PyInteger raceMask, PyInteger isInAlliance, PyInteger minMembers, PyInteger maxMembers, CallInformation call)
        {
            return this.DB.GetRecruitmentAds(regionID, skillPoints, typeMask, raceMask, isInAlliance, minMembers, maxMembers);
        }

        public PyTuple GetAllCorpMedals(PyInteger corporationID, CallInformation call)
        {
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                {"_corporationID", corporationID}
            };
            
            // TODO: IMPLEMENT CACHING FOR THIS ANSWER (THE CLIENT SEEMS TO EXPECT IT, ALTHOUGH IT DOESN'T ENFORCE IT)
            // TODO: THAT WILL REQUIRE TO SEND NOTIFICATIONS ON DATA CHANGES LIKE NEW MEDALS OR MEDALS BEING ISSUED
            // TODO: OnCorporationMedalAdded, OnMedalIssued, OnMedalStatusChanged
            return new PyTuple(2)
            {
                [0] = Database.Rowset(CorporationDB.LIST_MEDALS, values),
                [1] = Database.CRowset(CorporationDB.LIST_MEDALS_DETAILS, values)
            };
        }

        public PyDataType GetCorpInfo(PyInteger corporationID, CallInformation call)
        {
            DBRowDescriptor descriptor = new DBRowDescriptor();

            descriptor.Columns.Add(new DBRowDescriptor.Column("corporationID", FieldType.I4));
            descriptor.Columns.Add(new DBRowDescriptor.Column("typeID", FieldType.I4));
            descriptor.Columns.Add(new DBRowDescriptor.Column("buyDate", FieldType.FileTime));
            descriptor.Columns.Add(new DBRowDescriptor.Column("buyPrice", FieldType.CY));
            descriptor.Columns.Add(new DBRowDescriptor.Column("buyQuantity", FieldType.I4));
            descriptor.Columns.Add(new DBRowDescriptor.Column("buyStationID", FieldType.I4));
            descriptor.Columns.Add(new DBRowDescriptor.Column("sellDate", FieldType.FileTime));
            descriptor.Columns.Add(new DBRowDescriptor.Column("sellPrice", FieldType.CY));
            descriptor.Columns.Add(new DBRowDescriptor.Column("sellQuantity", FieldType.I4));
            descriptor.Columns.Add(new DBRowDescriptor.Column("sellStationID", FieldType.I4));
            descriptor.Columns.Add(new DBRowDescriptor.Column("agtBuyPrice", FieldType.CY));
            descriptor.Columns.Add(new DBRowDescriptor.Column("agtSellPrice", FieldType.CY));
            
            return new CRowset(descriptor);
        }

        private void ValidateMedal(PyString title, PyString description, PyList parts)
        {
            if (title.Length < 3) throw new MedalNameInvalid();
            if (title.Length > 100) throw new MedalNameTooLong();
            if (description.Length > 1000) throw new MedalDescriptionTooLong();
            if (false) throw new MedalDescriptionInvalid(); // TODO: CHECK FOR BANNED WORDS!
            
            // TODO: VALIDATE PART NAMES TO ENSURE THEY'RE VALID
        }
        
        public PyDataType CreateMedal(PyString title, PyString description, PyList parts, PyBool pay, CallInformation call)
        {
            int characterID = call.Client.EnsureCharacterIsSelected();
            
            if (CorporationRole.PersonnelManager.Is(call.Client.CorporationRole) == false && CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT);
            
            this.ValidateMedal(title, description, parts);

            if (pay == false)
                throw new ConfirmCreatingMedal(this.Container.Constants[Constants.medalCost]);
            
            using (Wallet wallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, call.Client.CorpAccountKey, true))
            {
                wallet.EnsureEnoughBalance(this.Container.Constants[Constants.medalCost]);
                wallet.CreateJournalRecord(MarketReference.MedalCreation, this.Container.Constants[Constants.medalTaxCorporation], null, -this.Container.Constants[Constants.medalCost]);
            }

            this.DB.CreateMedal(call.Client.CorporationID, characterID, title, description, parts.GetEnumerable<PyList>());
            
            return null;
        }

        public PyDataType CreateMedal(PyString title, PyString description, PyList parts, CallInformation call)
        {
            if (CorporationRole.PersonnelManager.Is(call.Client.CorporationRole) == false && CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT);

            this.ValidateMedal(title, description, parts);
            
            throw new ConfirmCreatingMedal(this.Container.Constants[Constants.medalCost]);
        }

        public PyDataType GetRecipientsOfMedal(PyInteger medalID, CallInformation call)
        {
            // TODO: CHECK IF THERE'S ANY KIND OF PERMISSION CHECK NEEDED (LIKE CORPORATION ID?)
            // TODO: CACHE THIS ANSWER TOO
            return this.DB.GetRecipientsOfMedal(medalID);
        }

        public PyDataType GetMedalStatuses(CallInformation call)
        {
            return new Rowset(
                new PyList<PyString>() {"statusID", "statusName"},
                new PyList<PyList>()
                {
                    new PyList() {1, "Remove"},
                    new PyList() {2, "Private"},
                    new PyList() {3, "Public"}
                }
            );
        }

        public PyDataType GiveMedalToCharacters(PyInteger medalID, PyList characterIDs, PyString reason, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (CorporationRole.PersonnelManager.Is(call.Client.CorporationRole) == false && CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT);
            
            throw new ConfirmCreatingMedal(this.Container.Constants[Constants.medalCost]);
        }

        public PyDataType GiveMedalToCharacters(PyInteger medalID, PyList characterIDs, PyString reason, PyBool pay, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (CorporationRole.PersonnelManager.Is(call.Client.CorporationRole) == false && CorporationRole.Director.Is(call.Client.CorporationRole) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_NEED_ROLE_PERS_MAN_OR_DIRECT);

            if (pay == false)
                throw new ConfirmGivingMedal(this.Container.Constants[Constants.medalCost]);
            
            using (Wallet wallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, call.Client.CorpAccountKey, true))
            {
                wallet.EnsureEnoughBalance(this.Container.Constants[Constants.medalCost]);
                wallet.CreateJournalRecord(MarketReference.MedalIssuing, this.Container.Constants[Constants.medalTaxCorporation], null, -this.Container.Constants[Constants.medalCost]);
            }
            
            // create the records for all the characters that have that medal
            foreach (PyInteger characterID in characterIDs.GetEnumerable<PyInteger>())
                this.DB.GrantMedal(medalID, characterID, callerCharacterID, reason, 2);

            // notify all the characters
            this.NotificationManager.NotifyCharacters(characterIDs.GetEnumerable<PyInteger>(), new OnMedalIssued());
            
            // increase recipients for medals
            this.DB.IncreaseRecepientsForMedal(medalID, characterIDs.Count);
            
            // no return data required
            return null;
        }

        public PyDataType SetMedalStatus(PyDictionary newStatuses, CallInformation call)
        {
            int characterID = call.Client.EnsureCharacterIsSelected();
            
            PyDictionary<PyInteger, PyInteger> newStatus = newStatuses.GetEnumerable<PyInteger, PyInteger>();

            foreach ((PyInteger medalID, PyInteger status) in newStatus)
            {
                // MEDAL DELETION
                if (status == 1)
                {
                    this.DB.RemoveMedalFromCharacter(medalID, characterID);
                }
                else
                {
                    // set whatever status was requested by the user
                    this.DB.UpdateMedalForCharacter(medalID, characterID, status);
                }
            }
            
            return null;
        }

        public PyDataType GetMedalDetails(PyInteger medalID, CallInformation call)
        {
            return KeyVal.FromDictionary(
                new PyDictionary()
                {
                    ["info"] = new PyList() {this.DB.GetMedalDetails(medalID)}
                }
            );
        }
    }
}