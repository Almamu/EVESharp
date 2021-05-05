using System;
using System.Collections.Generic;
using Common.Services;
using EVE;
using EVE.Packets.Complex;
using EVE.Packets.Exceptions;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.corpRegistry;
using Node.Exceptions.corpStationMgr;
using Node.Inventory;
using Node.Market;
using Node.Network;
using Node.Notifications.Client.Character;
using Node.Services.Characters;
using Node.StaticData.Corporation;
using PythonTypes.Types.Primitives;

namespace Node.Services.Account
{
    public class account : IService
    {
        private CharacterDB DB { get; }
        private WalletDB WalletDB { get; }
        private WalletManager WalletManager { get; init; }
        private ItemManager ItemManager { get; }
        private CacheStorage CacheStorage { get; }
        private NotificationManager NotificationManager { get; }
        
        public account(CharacterDB db, WalletDB walletDB, WalletManager walletManager, ItemManager itemManager, CacheStorage cacheStorage, NotificationManager notificationManager)
        {
            this.DB = db;
            this.WalletDB = walletDB;
            this.WalletManager = walletManager;
            this.ItemManager = itemManager;
            this.CacheStorage = cacheStorage;
            this.NotificationManager = notificationManager;
        }

        private PyDataType GetCashBalance(Client client)
        {
            return this.WalletDB.GetCharacterBalance(client.EnsureCharacterIsSelected());
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
                throw new CrpAccessDenied("You are not allowed to see the corporation's accounts");

            return this.WalletDB.GetWalletBalance(call.Client.CorporationID, walletKey);
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
                throw new CrpAccessDenied("You are not allowed to access that division's wallet");
            
            // journal requires accountant roles for corporation
            if (entityID == call.Client.CorporationID && (CorporationRole.Accountant.Is(call.Client.CorporationRole) == false || CorporationRole.JuniorAccountant.Is(call.Client.CorporationRole) == false))
                throw new CrpAccessDenied(MLS.UI_SHARED_WALLETHINT8);

            return this.DB.GetJournal(entityID, entryTypeID, accountKey, fromDate, transactionIDint);
        }

        public PyDataType GetWalletDivisionsInfo(CallInformation call)
        {
            // build a list of divisions the user can access
            List<int> walletKeys = new List<int>();

            if (CorporationRole.AccountCanQuery1.Is(call.Client.CorporationRole) == true)
                walletKeys.Add(1000);
            if (CorporationRole.AccountCanQuery2.Is(call.Client.CorporationRole) == true)
                walletKeys.Add(1001);
            if (CorporationRole.AccountCanQuery3.Is(call.Client.CorporationRole) == true)
                walletKeys.Add(1002);
            if (CorporationRole.AccountCanQuery4.Is(call.Client.CorporationRole) == true)
                walletKeys.Add(1003);
            if (CorporationRole.AccountCanQuery5.Is(call.Client.CorporationRole) == true)
                walletKeys.Add(1004);
            if (CorporationRole.AccountCanQuery6.Is(call.Client.CorporationRole) == true)
                walletKeys.Add(1005);
            if (CorporationRole.AccountCanQuery7.Is(call.Client.CorporationRole) == true)
                walletKeys.Add(1006);

            return this.WalletDB.GetWalletDivisionsForOwner(call.Client.CorporationID, walletKeys);
        }

        public PyDataType GiveCash(PyInteger destinationID, PyDecimal quantity, PyString reason, CallInformation call)
        {
            int accountKey = 1000;

            if (call.NamedPayload.TryGetValue("toAccountKey", out PyInteger namedAccountKey) == true)
                accountKey = namedAccountKey;
            
            // acquire the origin wallet, subtract quantity
            // TODO: CHECK IF THE WALLETKEY IS INDICATED IN SOME WAY
            using (Wallet originWallet = this.WalletManager.AcquireWallet(call.Client.EnsureCharacterIsSelected(), 1000))
            {
                originWallet.EnsureEnoughBalance(quantity);
                originWallet.CreateJournalRecord(MarketReference.CorporationPayment, destinationID, null, -quantity, reason);
            }
            
            // acquire the destination wallet, add quantity
            using (Wallet destinationWallet = this.WalletManager.AcquireWallet(destinationID, accountKey))
            {
                destinationWallet.CreateJournalRecord(MarketReference.CorporationPayment, (int) call.Client.CharacterID, destinationID, -1, quantity, reason);
            }
            
            return null;
        }

        public PyDataType GiveCashFromCorpAccount(PyInteger destinationID, PyDecimal quantity, PyInteger accountKey, CallInformation call)
        {
            // ensure the character can take from the account in question
            if (this.WalletManager.IsTakeAllowed(call.Client, accountKey, call.Client.CorporationID) == false)
                throw new CrpAccessDenied("You are not allowed to access that division's wallet");
            
            // acquire the origin wallet, subtract quantity
            // TODO: CHECK IF THE WALLETKEY IS INDICATED IN SOME WAY
            using (Wallet originWallet = this.WalletManager.AcquireWallet(call.Client.CorporationID, accountKey))
            {
                originWallet.EnsureEnoughBalance(quantity);
                originWallet.CreateJournalRecord(MarketReference.CorporationPayment, destinationID, call.Client.CharacterID, -quantity);
            }
            
            // acquire the destination wallet, add quantity
            using (Wallet destinationWallet = this.WalletManager.AcquireWallet(destinationID, 1000))
            {
                destinationWallet.CreateJournalRecord(MarketReference.CorporationPayment, call.Client.CorporationID, destinationID, -1, quantity);
            }

            return null;
        }
    }
}