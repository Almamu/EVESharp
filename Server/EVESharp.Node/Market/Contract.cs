using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Categories;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Market;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.contractMgr;
using EVESharp.EVE.Market;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Contracts;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Notifications;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Market;

public class Contract : IContract
{
    /// <summary>
    /// The database lock to have access to the contract information
    /// </summary>
    private DbLock Lock { get; }
    /// <summary>
    /// Access to the items in the cluster
    /// </summary>
    private IItems Items { get; }
    /// <summary>
    /// Access to the types in the cluster
    /// </summary>
    private ITypes Types { get; }
    /// <summary>
    /// Access to the wallets in the cluster
    /// </summary>
    private IWallets Wallets { get; }
    /// <summary>
    /// Access to the notification sender
    /// </summary>
    private INotificationSender Notifications { get; }
    /// <summary>
    /// Access to the dogma notifications
    /// </summary>
    private IDogmaNotifications DogmaNotifications { get; }
    /// <summary>
    /// Contains the database information fetched for this contract
    /// </summary>
    private readonly EVESharp.Database.Market.Contract mInformation;

    public int     ID         { get; }
    public double? Price      => this.mInformation.Price;
    public int     Collateral => this.mInformation.Collateral;
    public long    ExpireTime => this.mInformation.ExpireTime;
    public int CrateID
    {
        get => this.mInformation.CrateID;
        set => this.mInformation.CrateID = value;
    }
    public int            StationID    => this.mInformation.StationID;
    public ContractStatus Status
    {
        get => this.mInformation.Status;
        set => this.mInformation.Status = value;
    }
    public ContractTypes  Type         => this.mInformation.Type;
    public int            IssuerID     => this.mInformation.IssuerID;
    public int            IssuerCorpID => this.mInformation.IssuerCorpID;
    public bool           ForCorp      => this.mInformation.ForCorp;
    public double?        Reward       => this.mInformation.Reward;
    public double? Volume
    {
        get => this.mInformation.Volume;
        set => this.mInformation.Volume = value;
    }
    public int AcceptorID
    {
        get => this.mInformation.AcceptorID;
        set => this.mInformation.AcceptorID = value;
    }
    public long AcceptedDate
    {
        get => this.mInformation.AcceptedDate;
        set => this.mInformation.AcceptedDate = value;
    }
    public long CompletedDate
    {
        get => this.mInformation.CompletedDate;
        set => this.mInformation.CompletedDate = value;
    }
    
    public Contract (int contractID, IDatabase Database, ITypes types, IItems Items, IWallets wallets, INotificationSender notificationSender, IDogmaNotifications dogmaNotifications)
    {
        this.ID                 = contractID;
        this.Lock               = Database.GetLock (this.GenerateLockName ());
        this.mInformation       = this.Lock.ConGet (contractID);
        this.Items              = Items;
        this.Types              = types;
        this.Wallets            = wallets;
        this.Notifications      = notificationSender;
        this.DogmaNotifications = dogmaNotifications;
    }

    public void CreateCrate ()
    {
        if (CrateID != 0)
            return;
        
        // TODO: WRITE A CREATE CONTAINER FUNCTION
        ItemEntity item = Items.CreateSimpleItem (
            Types [TypeID.PlasticWrap],
            Items.LocationSystem.ID,
            StationID,
            Flags.None
        );

        // store the crateID and initialize the volume
        this.CrateID = item.ID;
        this.Volume  = 0;

        // unload the container as it's not really needed for anything
        Items.UnloadItem (item);
    }

    public void AddItem (int itemID, int quantity, int ownerID, int stationID)
    {
        PreloadedContractItem preloadValue = Lock.ConGetItemPreloadValues (itemID, ownerID, stationID);

        if (preloadValue.Damage > 0)
            throw new ConCannotTradeDamagedItem (this.Types [preloadValue.TypeID]);
        if (preloadValue.Contraband == true)
            throw new ConCannotTradeContraband (this.Types [preloadValue.TypeID]);
        if (quantity != preloadValue.Quantity)
            throw new ConCannotTradeItemSanity ();
        if (preloadValue.CategoryID == (int) CategoryID.Ship && preloadValue.Singleton == false)
            throw new ConCannotTradeNonSingletonShip (this.Types [preloadValue.TypeID], stationID);
            
        Lock.ConAddItem (ID, preloadValue.TypeID, preloadValue.Quantity, 1, itemID);

        Volume += preloadValue.Volume;
    }

    public void AddRequestedItem (int typeID, int quantity)
    {
        Lock.ConAddItem (ID, typeID, quantity, 0, null);
    }

    /// <summary>
    /// Returns the information about the maximum bid placed
    /// </summary>
    /// <returns></returns>
    private (int bidderID, int amount, int walletKey) GetMaximumBid ()
    {
        return Lock.ConGetMaximumBid (ID);
    }

    /// <summary>
    /// Returns the ids of players that are going to be outbid by the next bid placed
    /// </summary>
    /// <returns></returns>
    private PyList <PyInteger> GetOutbids ()
    {
        return Lock.ConGetOutbids (ID);
    }

    public ulong PlaceBid (int quantity, Session session, bool forCorp)
    {
        int bidderID  = forCorp ? session.CorporationID : session.CharacterID;
        int walletKey = forCorp ? session.CorpAccountKey : WalletKeys.MAIN;
        
        // ensure the contract is still in progress
        if (Status != ContractStatus.Outstanding)
            throw new ConAuctionAlreadyClaimed ();

        (int maximumBidderID, int maximumBid, int bidderWalletKey) = this.GetMaximumBid ();

        // calculate next bid slot
        int nextMinimumBid = maximumBid + (int) Math.Max (0.1 * (double) Price, 1000);

        if (quantity < nextMinimumBid)
            throw new ConBidTooLow (quantity, nextMinimumBid);

        // take the bid's money off the wallet
        using (IWallet bidderWallet = this.Wallets.AcquireWallet (bidderID, walletKey))
        {
            bidderWallet.EnsureEnoughBalance (quantity);
            bidderWallet.CreateJournalRecord (MarketReference.ContractAuctionBid, null, null, -quantity);
        }

        Notifications.NotifyOwners (this.GetOutbids (), new OnContractOutbid (ID));

        // finally place the bid
        ulong bidID = Lock.ConPlaceBid (ID, quantity, bidderID, forCorp, walletKey);

        // return the money for the player that was the highest bidder
        using (IWallet maximumBidderWallet = this.Wallets.AcquireWallet (maximumBidderID, bidderWalletKey))
        {
            maximumBidderWallet.CreateJournalRecord (MarketReference.ContractAuctionBidRefund, null, null, maximumBid);
        }

        return bidID;
    }

    private List <(RequestedContractItem item, int quantity)> ValidateRequestedItems (int locationID, int ownerID)
    {
        List<(RequestedContractItem item, int quantity)> result = new List <(RequestedContractItem item, int quantity)> ();
        
        foreach ((int typeID, int quantity, int? itemID) in Lock.ConGetItems (ID, 0))
        {
            int quantityLeft = quantity;

            foreach (RequestedContractItem item in Lock.ConGetRequestedItemsAtLocation (typeID, locationID, ownerID))
            {
                // contraband items cannot be traded
                if (item.Contraband)
                    continue;
                if (item.Damage > 0)
                    continue;
                // ships must be singleton
                if (this.Types [typeID].Group.Category.ID == (int) CategoryID.Ship && item.Singleton == false)
                    continue;
                // TODO: HANDLE MODULES? THOSE SHOULD BE SINGLETON TOO, RIGHT?
                int quantityRequested = Math.Min (quantityLeft, item.Quantity);
                
                // add the item to the result
                result.Add ((item, quantityRequested));
                
                quantityLeft -= quantityRequested;

                if (quantityLeft <= 0)
                    break;
            }

            if (quantityLeft > 0)
                throw new ConReturnItemsMissingNonSingleton (this.Types [typeID], locationID);
        }

        return result;
    }

    private void AcceptItemExchangeContract (int acceptorID)
    {
        // the destination of the requested items
        int contractCreator = ForCorp ? IssuerCorpID : IssuerID;
        
        // change the ownership of all the player's items
        foreach ((RequestedContractItem item, int quantity) in ValidateRequestedItems (StationID, acceptorID))
        {
            ItemEntity loadedItem = Items.LoadItem (item.ItemID, out bool loadRequired);

            if (loadedItem.Quantity == quantity)
            {
                // remove the item from the meta inventories
                Items.MetaInventories.OnItemDestroyed (loadedItem);
                // the item can be moved directly
                loadedItem.LocationID = Items.LocationRecycler.ID;
                // notify the old owner that the item got destroyed
                DogmaNotifications.QueueMultiEvent (
                    acceptorID, OnItemChange.BuildLocationChange (loadedItem, StationID)
                );
                // now move the item to the final place
                loadedItem.LocationID = StationID;
                loadedItem.OwnerID    = contractCreator;
                // notify the new owner
                DogmaNotifications.QueueMultiEvent (
                    contractCreator, OnItemChange.BuildNewItemChange (loadedItem)
                );
                // finally re-add it to the meta inventories
                // TODO: ADD SUPPORT FOR REFERENCE COUNTING ITEMS!
                Items.MetaInventories.OnItemLoaded (loadedItem);
            }
            else
            {
                // the item is not completely moved, so update quantities and create a new one for destination
                loadedItem.Quantity -= quantity;
                
                DogmaNotifications.QueueMultiEvent (
                    acceptorID, OnItemChange.BuildQuantityChange (loadedItem, loadedItem.Quantity + quantity)
                );
                
                // create the destination item for the new owner
                ItemEntity newItem = Items.CreateSimpleItem (
                    loadedItem.Type, contractCreator, loadedItem.LocationID,
                    Flags.Hangar, quantity, false, loadedItem.Singleton
                );
                
                // notify the new owner about the new item
                DogmaNotifications.QueueMultiEvent (
                    contractCreator, OnItemChange.BuildNewItemChange (newItem)
                );
                
                // finally unload the item
                // TODO: ADD SUPPORT FOR REFERENCE COUNTING ITEMS!
                Items.UnloadItem (newItem);
            }

            loadedItem.Persist ();

            if (loadRequired)
                Items.UnloadItem (loadedItem);
        }
        
        // now move the items to the new owner's 
        foreach ((int typeID, int quantity, int? itemID) in Lock.ConGetItems (ID, 1))
        {
            ItemEntity loadedItem = Items.LoadItem ((int) itemID, out bool loadRequired);

            loadedItem.LocationID = StationID;
            loadedItem.OwnerID    = acceptorID;

            DogmaNotifications.QueueMultiEvent (
                acceptorID, OnItemChange.BuildLocationChange (loadedItem, CrateID)
            );
            
            loadedItem.Persist ();
            
            // finally unload the item
            // TODO: ADD SUPPORT FOR REFERENCE COUNTING ITEMS!
            if (loadRequired)
                Items.UnloadItem (loadedItem);
        }

        Status        = ContractStatus.Finished;
        CompletedDate = DateTime.Now.ToFileTimeUtc ();
    }

    public void Accept (Session session)
    {
        if (Status != ContractStatus.Outstanding)
            throw new ConContractNotOutstanding ();
        if (ExpireTime < DateTime.Now.ToFileTimeUtc ())
            throw new ConContractExpired ();
        
        switch (Type)
        {
            case ContractTypes.ItemExchange:
                this.AcceptItemExchangeContract (session.CharacterID);
                break;
            
            case ContractTypes.Auction:  throw new CustomError ("Auctions cannot be accepted!");
            case ContractTypes.Courier:  throw new CustomError ("Courier contracts not supported yet!");
            case ContractTypes.Loan:     throw new CustomError ("Loan contracts not supported yet!");
            case ContractTypes.Freeform: throw new CustomError ("Freeform contracts not supported yet!");
            default:                     throw new CustomError ("Unknown contract type to accept!");
        }
        
        AcceptorID   = session.CharacterID;
        AcceptedDate = DateTime.Now.ToFileTimeUtc ();
        
        DogmaNotifications.QueueMultiEvent(
            ForCorp ? IssuerCorpID : IssuerID, new OnContractAccepted (ID)
        );
    }
    
    public void Destroy ()
    {
        // TODO: IMPLEMENT THIS
    }

    private string GenerateLockName ()
    {
        return $"contract_{this.ID}";
    }

    public void Dispose ()
    {
        Lock.ConSaveInfo (this.mInformation);
        Lock.Dispose ();
    }
}