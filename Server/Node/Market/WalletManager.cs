using Node.Database;
using Node.Network;

namespace Node.Market
{
    public class WalletManager
    {
        private WalletDB DB { get; init; }
        private NotificationManager NotificationManager { get; init; }
        
        public Wallet AcquireWallet(int ownerID, int walletKey)
        {
            // TODO: CHECK PERMISSIONS
            return new Wallet()
            {
                Connection = this.DB.AcquireLock(ownerID, walletKey, out double balance),
                OwnerID = ownerID,
                WalletKey = walletKey,
                Balance = balance,
                OriginalBalance = balance,
                DB = this.DB,
                NotificationManager = this.NotificationManager
            };
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
            this.DB.CreateTransactionForCharacter(
                ownerID, otherID, type, typeID, quantity, amount, stationID
            );
        }

        public WalletManager(WalletDB db, NotificationManager notificationManager)
        {
            this.DB = db;
            this.NotificationManager = notificationManager;
        }
    }
}