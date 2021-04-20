using System;
using Common.Services;
using EVE.Packets.Complex;
using EVE.Packets.Exceptions;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Exceptions;
using Node.Inventory;
using Node.Market;
using Node.Network;
using Node.Notifications.Client.Character;
using PythonTypes.Types.Primitives;

namespace Node.Services.Account
{
    public class account : IService
    {
        private CharacterDB DB { get; }
        private WalletDB WalletDB { get; }
        private ItemManager ItemManager { get; }
        private CacheStorage CacheStorage { get; }
        private NotificationManager NotificationManager { get; }
        
        public account(CharacterDB db, WalletDB walletDB, ItemManager itemManager, CacheStorage cacheStorage, NotificationManager notificationManager)
        {
            this.DB = db;
            this.WalletDB = walletDB;
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
            return this.GetCashBalance(isCorpWallet ? 1 : 0, 1000, call);
        }
        
        public PyDataType GetCashBalance(PyInteger isCorpWallet, CallInformation call)
        {
            return this.GetCashBalance(isCorpWallet, 1000, call);
        }

        public PyDataType GetCashBalance(PyInteger isCorpWallet, PyInteger walletKey, CallInformation call)
        {
            if (isCorpWallet == 0)
                return this.GetCashBalance(call.Client);
            
            throw new CustomError("This function is not supported yet");
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

        public PyDataType GetJournal(PyInteger marketKey, PyInteger fromDate, PyInteger entryTypeID,
            PyBool isCorpWallet, PyInteger transactionID, PyInteger rev, CallInformation call)
        {
            int? transactionIDint = null;

            if (transactionID != null)
                transactionIDint = transactionID;
            
            return this.DB.GetJournal(call.Client.EnsureCharacterIsSelected(), entryTypeID, marketKey, fromDate, transactionIDint);
        }
    }
}