using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Common.Services;
using EVESharp.Database;
using EVESharp.EVE;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.corpRegistry;
using EVESharp.Node.Inventory;
using EVESharp.Node.Market;
using EVESharp.Node.Network;
using EVESharp.Node.StaticData.Corporation;
using MySql.Data.MySqlClient;
using EVESharp.Node.Exceptions;
using EVESharp.Node.Exceptions.corpStationMgr;
using EVESharp.Node.Notifications.Client.Character;
using EVESharp.Node.Services.Characters;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Account
{
    public class account : IService
    {
        private CharacterDB DB { get; }
        private WalletManager WalletManager { get; init; }
        private ItemManager ItemManager { get; }
        private CacheStorage CacheStorage { get; }
        private NotificationManager NotificationManager { get; }
        private DatabaseConnection Database { get; init; }
        
        public account(DatabaseConnection databaseConnection, CharacterDB db, WalletManager walletManager, ItemManager itemManager, CacheStorage cacheStorage, NotificationManager notificationManager)
        {
            this.Database = databaseConnection;
            this.DB = db;
            this.WalletManager = walletManager;
            this.ItemManager = itemManager;
            this.CacheStorage = cacheStorage;
            this.NotificationManager = notificationManager;
        }

        private PyDataType GetCashBalance(Client client)
        {
            return this.WalletManager.GetWalletBalance(client.EnsureCharacterIsSelected());
        }

        public PyDataType GetCashBalance(PyBool isCorpWallet, CallInformation call)
        {
            return this.GetCashBalance(isCorpWallet ? 1 : 0, call.Client.CorpAccountKey, call);
        }
        
        public PyDataType GetCashBalance(PyInteger isCorpWallet, CallInformation call)
        {
            return this.GetCashBalance(isCorpWallet, call.Client.CorpAccountKey, call);
        }

        public PyDataType GetCashBalance(PyInteger isCorpWallet, PyInteger walletKey, CallInformation call)
        {
            if (isCorpWallet == 0)
                return this.GetCashBalance(call.Client);

            if (this.WalletManager.IsAccessAllowed(call.Client, walletKey, call.Client.CorporationID) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_ACCESSTOWALLETDIVISIONDENIED);

            return this.WalletManager.GetWalletBalance(call.Client.CorporationID, walletKey);
        }

        public PyDataType GetKeyMap(CallInformation call)
        {
            return this.DB.GetKeyMap();
        }

        public PyDataType GetEntryTypes(CallInformation call)
        {
            this.CacheStorage.Load(
                "account",
                "GetEntryTypes",
                "SELECT refTypeID AS entryTypeID, refTypeText AS entryTypeName, description FROM mktRefTypes",
                CacheStorage.CacheObjectType.Rowset
            );

            PyDataType cacheHint = this.CacheStorage.GetHint("account", "GetEntryTypes");

            return CachedMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetJournal(PyInteger accountKey, PyInteger fromDate, PyInteger entryTypeID,
            PyBool isCorpWallet, PyInteger transactionID, PyInteger rev, CallInformation call)
        {
            int? transactionIDint = null;

            if (transactionID != null)
                transactionIDint = transactionID;

            int entityID = call.Client.EnsureCharacterIsSelected();

            if (isCorpWallet == true)
                entityID = call.Client.CorporationID;

            if (this.WalletManager.IsAccessAllowed(call.Client, accountKey, entityID) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_ACCESSTOWALLETDIVISIONDENIED);
            
            // journal requires accountant roles for corporation
            if (entityID == call.Client.CorporationID && (CorporationRole.Accountant.Is(call.Client.CorporationRole) == false || CorporationRole.JuniorAccountant.Is(call.Client.CorporationRole) == false))
                throw new CrpAccessDenied(MLS.UI_SHARED_WALLETHINT8);

            return this.DB.GetJournal(entityID, entryTypeID, accountKey, fromDate, transactionIDint);
        }

        public PyDataType GetWalletDivisionsInfo(CallInformation call)
        {
            // build a list of divisions the user can access
            List<int> walletKeys = new List<int>();

            if (this.WalletManager.IsAccessAllowed(call.Client, WalletKeys.MAIN_WALLET, call.Client.CorporationID) == true)
                walletKeys.Add(WalletKeys.MAIN_WALLET);
            if (this.WalletManager.IsAccessAllowed(call.Client, WalletKeys.SECOND_WALLET, call.Client.CorporationID) == true)
                walletKeys.Add(WalletKeys.SECOND_WALLET);
            if (this.WalletManager.IsAccessAllowed(call.Client, WalletKeys.THIRD_WALLET, call.Client.CorporationID) == true)
                walletKeys.Add(WalletKeys.THIRD_WALLET);
            if (this.WalletManager.IsAccessAllowed(call.Client, WalletKeys.FOURTH_WALLET, call.Client.CorporationID) == true)
                walletKeys.Add(WalletKeys.FOURTH_WALLET);
            if (this.WalletManager.IsAccessAllowed(call.Client, WalletKeys.FIFTH_WALLET, call.Client.CorporationID) == true)
                walletKeys.Add(WalletKeys.FIFTH_WALLET);
            if (this.WalletManager.IsAccessAllowed(call.Client, WalletKeys.SIXTH_WALLET, call.Client.CorporationID) == true)
                walletKeys.Add(WalletKeys.SIXTH_WALLET);
            if (this.WalletManager.IsAccessAllowed(call.Client, WalletKeys.SEVENTH_WALLET, call.Client.CorporationID) == true)
                walletKeys.Add(WalletKeys.SEVENTH_WALLET);
            
            return Database.PackedRowList(
                WalletDB.GET_WALLETS,
                new Dictionary<string, object>()
                {
                    {"_ownerID", call.Client.CorporationID},
                    {"_walletKeyKeys", string.Join(',', walletKeys)}
                }
            );
        }

        public PyDataType GiveCash(PyInteger destinationID, PyDecimal quantity, PyString reason, CallInformation call)
        {
            int accountKey = WalletKeys.MAIN_WALLET;

            if (call.NamedPayload.TryGetValue("toAccountKey", out PyInteger namedAccountKey) == true)
                accountKey = namedAccountKey;
            
            // acquire the origin wallet, subtract quantity
            // TODO: CHECK IF THE WALLETKEY IS INDICATED IN SOME WAY
            using (Wallet originWallet = this.WalletManager.AcquireWallet(call.Client.EnsureCharacterIsSelected(), WalletKeys.MAIN_WALLET))
            {
                originWallet.EnsureEnoughBalance(quantity);
                originWallet.CreateJournalRecord(MarketReference.CorporationPayment, destinationID, null, -quantity, reason);
            }
            
            // acquire the destination wallet, add quantity
            using (Wallet destinationWallet = this.WalletManager.AcquireWallet(destinationID, accountKey, true))
            {
                destinationWallet.CreateJournalRecord(MarketReference.CorporationPayment, (int) call.Client.CharacterID, destinationID, -1, quantity, reason);
            }
            
            return null;
        }

        public PyDataType GiveCashFromCorpAccount(PyInteger destinationID, PyDecimal quantity, PyInteger accountKey, CallInformation call)
        {
            // ensure the character can take from the account in question
            if (this.WalletManager.IsTakeAllowed(call.Client, accountKey, call.Client.CorporationID) == false)
                throw new CrpAccessDenied(MLS.UI_CORP_ACCESSTOWALLETDIVISIONDENIED);
            
            // acquire the origin wallet, subtract quantity
            // TODO: CHECK IF THE WALLETKEY IS INDICATED IN SOME WAY
            using (Wallet originWallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, accountKey, true))
            {
                originWallet.EnsureEnoughBalance(quantity);
                originWallet.CreateJournalRecord(MarketReference.CorporationPayment, destinationID, call.Client.CharacterID, -quantity);
            }
            
            // TODO: CHECK IF THE DESTINATION IS A CORPORATION OR NOT
            // acquire the destination wallet, add quantity
            using (Wallet destinationWallet = this.WalletManager.AcquireWallet(destinationID, WalletKeys.MAIN_WALLET))
            {
                destinationWallet.CreateJournalRecord(MarketReference.CorporationPayment, call.Client.CorporationID, destinationID, -1, quantity);
            }

            return null;
        }
    }
}