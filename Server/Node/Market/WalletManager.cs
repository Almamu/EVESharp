using Node.Database;
using Node.Network;
using Node.StaticData.Corporation;

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
        /// <param name="characterID">The character that performed the transaction</param>
        /// <param name="otherID">The other character's ID</param>
        /// <param name="typeID">The type of item</param>
        /// <param name="quantity">The amount of items</param>
        /// <param name="amount">The amount of ISK</param>
        /// <param name="stationID">The place where the transaction was recorded</param>
        /// <param name="accountKey">The account key where the transaction was recorded</param>
        public void CreateTransactionRecord(int ownerID, TransactionType type, int characterID, int otherID, int typeID, int quantity, double amount, int stationID, int accountKey)
        {
            // market transactions do not affect the wallet value because these are paid either when placing the sell/buy order
            // or when fullfiling it
            this.DB.CreateTransactionForOwner(
                ownerID, characterID, otherID, type, typeID, quantity, amount, stationID, accountKey
            );
        }

        /// <summary>
        /// Creates a new wallet for the given owner with the specified walletKey
        /// </summary>
        /// <param name="ownerID">The ownerID</param>
        /// <param name="accountKey">The accountKey</param>
        /// <param name="startingBalance">The starting balance of the wallet</param>
        public void CreateWallet(int ownerID, int accountKey, double startingBalance)
        {
            this.DB.CreateWallet(ownerID, accountKey, startingBalance);
        }

        public bool IsAccessAllowed(Client client, int accountKey, int ownerID)
        {
            if (ownerID == client.CharacterID)
                return true;
            
            if (ownerID == client.CorporationID)
            {
                // check for permissions
                // check if the character has any accounting roles and set the correct accountKey based on the data
                if (CorporationRole.AccountCanQuery1.Is(client.CorporationRole) && accountKey == 1000)
                    return true;
                if (CorporationRole.AccountCanQuery2.Is(client.CorporationRole) && accountKey == 1001)
                    return true;
                if (CorporationRole.AccountCanQuery3.Is(client.CorporationRole) && accountKey == 1002)
                    return true;
                if (CorporationRole.AccountCanQuery4.Is(client.CorporationRole) && accountKey == 1003)
                    return true;
                if (CorporationRole.AccountCanQuery5.Is(client.CorporationRole) && accountKey == 1004)
                    return true;
                if (CorporationRole.AccountCanQuery6.Is(client.CorporationRole) && accountKey == 1005)
                    return true;
                if (CorporationRole.AccountCanQuery7.Is(client.CorporationRole) && accountKey == 1006)
                    return true;
                // last chance, accountant role
                if (CorporationRole.Accountant.Is(client.CorporationRole))
                    return true;
                if (CorporationRole.JuniorAccountant.Is(client.CorporationRole))
                    return true;
            }

            return false;
        }

        public bool IsTakeAllowed(Client client, int accountKey, int ownerID)
        {
            if (ownerID == client.CharacterID)
                return true;
            
            if (ownerID == client.CorporationID)
            {
                // check for permissions
                // check if the character has any accounting roles and set the correct accountKey based on the data
                if (CorporationRole.AccountCanTake1.Is(client.CorporationRole) && accountKey == 1000)
                    return true;
                if (CorporationRole.AccountCanTake2.Is(client.CorporationRole) && accountKey == 1001)
                    return true;
                if (CorporationRole.AccountCanTake3.Is(client.CorporationRole) && accountKey == 1002)
                    return true;
                if (CorporationRole.AccountCanTake4.Is(client.CorporationRole) && accountKey == 1003)
                    return true;
                if (CorporationRole.AccountCanTake5.Is(client.CorporationRole) && accountKey == 1004)
                    return true;
                if (CorporationRole.AccountCanTake6.Is(client.CorporationRole) && accountKey == 1005)
                    return true;
                if (CorporationRole.AccountCanTake7.Is(client.CorporationRole) && accountKey == 1006)
                    return true;
                if (CorporationRole.Accountant.Is(client.CorporationRole))
                    return true;
            }

            return false;
        }
        
        public WalletManager(WalletDB db, NotificationManager notificationManager)
        {
            this.DB = db;
            this.NotificationManager = notificationManager;
        }
    }
}