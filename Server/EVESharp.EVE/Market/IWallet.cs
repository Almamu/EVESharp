using System;
using EVESharp.Database.Market;
using EVESharp.EVE.Exceptions;

namespace EVESharp.EVE.Market;

public interface IWallet : IDisposable
{
    public int    OwnerID         { get; }
    public int    WalletKey       { get; }
    public double Balance         { get; set; }
    public double OriginalBalance { get; }
    public bool   ForCorporation  { get; }


    /// <summary>
    /// Checks that the wallet has enough balance to perform whatever operations
    /// </summary>
    /// <param name="required">The amount required</param>
    /// <exception cref="NotEnoughMoney"></exception>
    public void EnsureEnoughBalance (double required);

    /// <summary>
    /// Creates a journal record for the given wallet and adds to the balance
    /// </summary>
    /// <param name="reference">The type of market reference</param>
    /// <param name="ownerID1">Character involved</param>
    /// <param name="ownerID2">Other character involved</param>
    /// <param name="referenceID"></param>
    /// <param name="amount">The amount of ISK to add</param>
    /// <param name="reason">Extra information for the EVE Client</param>
    public void CreateJournalRecord (MarketReference reference, int ownerID1, int? ownerID2, int? referenceID, double amount, string reason = "");

    /// <summary>
    /// Creates a journal record for the given wallet and adds to the balance
    /// </summary>
    /// <param name="reference">The type of market reference</param>
    /// <param name="ownerID2">Other character involved</param>
    /// <param name="referenceID"></param>
    /// <param name="amount">The amount of ISK to add</param>
    /// <param name="reason">Extra information for the EVE Client</param>
    public void CreateJournalRecord (MarketReference reference, int? ownerID2, int? referenceID, double amount, string reason = "");

    /// <summary>
    /// Creates a transaction record in the wallet modifying the wallet balance
    /// </summary>
    /// <param name="type">The type of transaction</param>
    /// <param name="characterID">The character performing the transaction</param>
    /// <param name="otherID">The other character's ID</param>
    /// <param name="typeID">The type of item</param>
    /// <param name="quantity">The amount of items</param>
    /// <param name="amount">The amount of ISK</param>
    /// <param name="stationID">The place where the transaction was recorded</param>
    public void CreateTransactionRecord (TransactionType type, int characterID, int otherID, int typeID, int quantity, double amount, int stationID);
}