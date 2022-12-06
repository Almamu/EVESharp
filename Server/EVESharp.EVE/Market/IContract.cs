using System;
using EVESharp.Database.Market;
using EVESharp.EVE.Sessions;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Market;

public interface IContract : IDisposable
{
    public int            ID             { get; }
    public double?        Price          { get; }
    public int            Collateral     { get; }
    public long           ExpireTime     { get; }
    public int            CrateID        { get; }
    public int            StartStationID { get; }
    public int            EndStationID   { get; }
    public ContractStatus Status         { get; }
    public ContractTypes  Type           { get; }
    public int            IssuerID       { get; }
    public int            IssuerCorpID   { get; }
    public bool           ForCorp        { get; }
    public double?        Reward         { get; }
    public double?        Volume         { get; }
    public int            AcceptorID     { get; }
    public long           AcceptedDate   { get; }
    public long           CompletedDate  { get; }

    /// <summary>
    /// Adds an item into the contract's item list checking for specific conditions
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="quantity"></param>
    /// <param name="ownerID"></param>
    /// <param name="stationID"></param>
    public void AddItem (int itemID, int quantity, int ownerID, int stationID);

    /// <summary>
    /// Adds an item type as requested into the contract's information
    /// </summary>
    /// <param name="typeID"></param>
    /// <param name="quantity"></param>
    public void AddRequestedItem (int typeID, int quantity);

    /// <summary>
    /// Places a new bid in this contract
    /// </summary>
    /// <param name="quantity"></param>
    /// <param name="session"></param>
    /// <param name="forCorp"></param>
    /// <returns></returns>
    public ulong PlaceBid (int quantity, Session session, bool forCorp);

    /// <summary>
    /// Ensures that the given session has permissions to modify this contract
    /// </summary>
    /// <param name="session"></param>
    public void CheckOwnership (Session session);
    
    /// <summary>
    /// Accepts the contract and handles all the item movement that has to happen
    /// </summary>
    /// <param name="session"></param>
    /// <param name="forCorp"></param>
    public void Accept (Session session, bool forCorp);
    
    /// <summary>
    /// Creates an item crate required to store items inside the contract
    /// </summary>
    public void CreateCrate ();

    /// <summary>
    /// Destroys the given contract and all the tracked information for it
    /// </summary>
    public void Destroy ();
}