using System;
using Common.Services;
using MySql.Data.MySqlClient;
using Node.Database;
using Node.Exceptions;
using Node.Inventory;
using Node.Market;
using Node.Network;
using Node.Notifications.Client.Character;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
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

            return PyCacheMethodCallResult.FromCacheHint(cacheHint);
        }

        public PyDataType GetJournal(PyInteger marketKey, PyInteger fromDate, PyInteger entryTypeID,
            PyBool isCorpWallet, PyInteger transactionID, PyInteger rev, CallInformation call)
        {
            int? transactionIDint = null;

            if (transactionID != null)
                transactionIDint = transactionID;
            
            return this.DB.GetJournal(call.Client.EnsureCharacterIsSelected(), entryTypeID, marketKey, fromDate, transactionIDint);
        }

        /// <summary>
        /// Represents status of a wallet that has a lock acquired
        /// </summary>
        public class WalletLock
        {
            public int OwnerID { get; init; }
            public int WalletKey { get; init; }
            public MySqlConnection Connection { get; init; }
            public double Balance { get; set; }
            public double OriginalBalance { get; init; }
        }

        /// <summary>
        /// Acquires a lock for the given ownerID and wallet key
        /// </summary>
        /// <param name="ownerID">The owner to acquire the lock for</param>
        /// <param name="walletKey">The wallet to use</param>
        /// <returns>A lock with the required information</returns>
        public WalletLock AcquireLock(int ownerID, int walletKey)
        {
            return new WalletLock()
            {
                Connection = this.WalletDB.AcquireLock(ownerID, walletKey, out double balance),
                OwnerID = ownerID,
                WalletKey = walletKey,
                Balance = balance,
                OriginalBalance = balance
            };
        }

        /// <summary>
        /// Checks that the wallet has enough balance to perform whatever operations
        /// </summary>
        /// <param name="walletLock">The wallet to look into</param>
        /// <param name="required">The amount required</param>
        /// <exception cref="NotEnoughMoney"></exception>
        public void EnsureEnoughBalance(WalletLock walletLock, double required)
        {
            if (walletLock.Balance < required)
                throw new NotEnoughMoney(walletLock.Balance, required);
        }

        /// <summary>
        /// Creates a journal record for the given wallet and adds to the balance
        /// </summary>
        /// <param name="walletLock">The acquired lock for the wallet</param>
        /// <param name="reference">The type of market reference</param>
        /// <param name="ownerID1">Character involved</param>
        /// <param name="ownerID2">Other character involved</param>
        /// <param name="referenceID"></param>
        /// <param name="amount">The amount of ISK to add</param>
        /// <param name="reason">Extra information for the EVE Client</param>
        public void CreateJournalRecord(WalletLock walletLock, MarketReference reference, int ownerID1, int? ownerID2, int? referenceID, double amount, string reason = "")
        {
            // subtract balance
            walletLock.Balance += amount;
            
            // create journal entry
            this.WalletDB.CreateJournalForCharacter(
                reference, walletLock.OwnerID, ownerID1, ownerID2, referenceID, amount,
                walletLock.Balance, reason, walletLock.WalletKey
            );
        }
        

        /// <summary>
        /// Creates a journal record for the given wallet and adds to the balance
        /// </summary>
        /// <param name="walletLock">The acquired lock for the wallet</param>
        /// <param name="reference">The type of market reference</param>
        /// <param name="ownerID2">Other character involved</param>
        /// <param name="referenceID"></param>
        /// <param name="amount">The amount of ISK to add</param>
        /// <param name="reason">Extra information for the EVE Client</param>
        public void CreateJournalRecord(WalletLock walletLock, MarketReference reference, int? ownerID2, int? referenceID, double amount, string reason = "")
        {
            // subtract balance
            walletLock.Balance += amount;
            
            // create journal entry
            this.WalletDB.CreateJournalForCharacter(
                reference, walletLock.OwnerID, walletLock.OwnerID, ownerID2, referenceID, amount,
                walletLock.Balance, reason, walletLock.WalletKey
            );
        }

        /// <summary>
        /// Creates a transaction record in the wallet without modifying the wallet balance
        /// </summary>
        /// <param name="ownerID">The owner of the wallet</param>
        /// <param name="type">The type of transaction</param>
        /// <param name="otherID">The other character's ID</param>
        /// <param name="typeID">The type of item</param>
        /// <param name="quantity">The amount of items</param>
        /// <param name="amount">The amount of ISK</param>
        /// <param name="stationID">The place where the transaction was recorded</param>
        public void CreateTransactionRecord(int ownerID, TransactionType type, int otherID, int typeID, int quantity, double amount, int stationID)
        {
            // market transactions do not affect the wallet value because these are paid either when placing the sell/buy order
            // or when fullfiling it
            this.WalletDB.CreateTransactionForCharacter(
                ownerID, otherID, type, typeID, quantity, amount, stationID
            );
        }

        /// <summary>
        /// Creates a transaction record in the wallet modifying the wallet balance
        /// </summary>
        /// <param name="walletLock">The wallet to update</param>
        /// <param name="type">The type of transaction</param>
        /// <param name="otherID">The other character's ID</param>
        /// <param name="typeID">The type of item</param>
        /// <param name="quantity">The amount of items</param>
        /// <param name="amount">The amount of ISK</param>
        /// <param name="stationID">The place where the transaction was recorded</param>
        public void CreateTransactionRecord(WalletLock walletLock, TransactionType type, int otherID, int typeID, int quantity, double amount, int stationID)
        {
            walletLock.Balance += amount;
            // market transactions do not affect the wallet value because these are paid either when placing the sell/buy order
            // or when fullfiling it
            this.WalletDB.CreateTransactionForCharacter(
                walletLock.OwnerID, otherID, type, typeID, quantity, amount, stationID
            );
        }

        /// <summary>
        /// Frees the given wallet, closes it and saves the current balance
        /// </summary>
        /// <param name="walletLock">The lock to free</param>
        public void FreeLock(WalletLock walletLock)
        {
            // if the balance changed, update the record in the database
            if (Math.Abs(walletLock.Balance - walletLock.OriginalBalance) > 0.01)
            {
                // TODO: PROPERLY SUPPORT WALLET KEYS AND OWNERIDS
                this.WalletDB.SetCharacterBalance(walletLock.Connection, walletLock.OwnerID, walletLock.Balance);
                // send notification to the client
                this.NotificationManager.NotifyCharacter(walletLock.OwnerID, 
                    new OnAccountChange(walletLock.WalletKey, walletLock.OwnerID, walletLock.Balance)
                );
            }

            this.WalletDB.ReleaseLock(walletLock.Connection, walletLock.OwnerID, walletLock.WalletKey);
        }
    }
}