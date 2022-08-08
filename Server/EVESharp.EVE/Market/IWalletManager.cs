using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Market;
using EVESharp.EVE.Sessions;

namespace EVESharp.Node.Market;

public interface IWalletManager
{
    /// <summary>
    /// Provides access to the specified wallet
    /// </summary>
    /// <param name="ownerID">The ownerID</param>
    /// <param name="walletKey">The wallet to get for the given ownerID</param>
    /// <param name="isCorporation">Whether the ownerID is a corporation or not</param>
    /// <returns></returns>
    public IWallet AcquireWallet (int ownerID, int walletKey, bool isCorporation = false);

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
    /// <param name="walletKey">The account key where the transaction was recorded</param>
    public void CreateTransactionRecord (
        int ownerID,   TransactionType type, int characterID, int otherID, int typeID, int quantity, double amount,
        int stationID, int             walletKey
    );

    public void CreateJournalForOwner (
        MarketReference reference, int  characterID, int    ownerID1,
        int?            ownerID2,  int? referenceID, double amount, double finalBalance, string reason, int walletKey
    );

    /// <summary>
    /// Creates a new wallet for the given owner with the specified walletKey
    /// </summary>
    /// <param name="ownerID">The ownerID</param>
    /// <param name="walletKey">The walletKey</param>
    /// <param name="startingBalance">The starting balance of the wallet</param>
    public void CreateWallet (int ownerID, int walletKey, double startingBalance);

    /// <summary>
    /// Checks if the given session is allowed to read the given account
    /// </summary>
    /// <param name="session">The session to check permissions for</param>
    /// <param name="accountKey">The wallet key</param>
    /// <param name="ownerID">The owner of the wallet</param>
    /// <returns></returns>
    public bool IsAccessAllowed (Session session, int accountKey, int ownerID)
    {
        return EVE.Permissions.Market.Wallet.IsAccessAllowed (session, accountKey, ownerID);
    }

    /// <summary>
    /// Checks if the given session is allowed to take from the given account
    /// </summary>
    /// <param name="session">The session to check permissions for</param>
    /// <param name="accountKey">The wallet key</param>
    /// <param name="ownerID">The owner of the wallet</param>
    /// <returns></returns>
    public bool IsTakeAllowed (Session session, int accountKey, int ownerID)
    {
        return EVE.Permissions.Market.Wallet.IsTakeAllowed (session, accountKey, ownerID);
    }

    /// <summary>
    /// Obtains the wallet balance for the given owner and wallet
    /// </summary>
    /// <param name="ownerID">The owner to get the wallet balance for</param>
    /// <param name="walletKey">The wallet to get balance for</param>
    /// <returns>The wallet's balance</returns>
    public double GetWalletBalance (int ownerID, int walletKey = WalletKeys.MAIN);
}