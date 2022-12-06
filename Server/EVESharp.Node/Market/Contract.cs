using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.Database;
using EVESharp.Database.Corporations;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Categories;
using EVESharp.Database.Inventory.Types;
using EVESharp.Database.Market;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Dogma;
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
using Container = EVESharp.EVE.Data.Inventory.Items.Types.Container;

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
    /// Access to the dogma items management
    /// </summary>
    private IDogmaItems DogmaItems { get; }
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
    public int StartStationID => this.mInformation.StartStationID;
    public int EndStationID   => this.mInformation.EndStationID;
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

    private bool mDisposed = false;
    
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

        if (this.mInformation is null)
            throw new ConContractNotFound ();
    }

    public void CreateCrate ()
    {
        if (CrateID != 0)
            return;
        
        ItemEntity item = Items.CreateSimpleItem (
            EndStationID != 0 ? Items.GetItem <Station> (EndStationID).Name : null,
            (int) TypeID.PlasticWrap,
            Items.LocationSystem.ID,
            this.StartStationID,
            Flags.None, 1, false, true
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

        Volume += (preloadValue.Volume * quantity);
    }

    public void AddRequestedItem (int typeID, int quantity)
    {
        Lock.ConAddItem (ID, typeID, quantity, 0, null);
    }
    
    private (int bidderID, int amount, int walletKey) GetMaximumBid ()
    {
        return Lock.ConGetMaximumBid (ID);
    }
    
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

        if (maximumBidderID == 0)
            nextMinimumBid = (int) Price;
        
        if (quantity < nextMinimumBid)
            throw new ConBidTooLow (quantity, nextMinimumBid);

        // take the bid's money off the wallet
        using (IWallet bidderWallet = this.Wallets.AcquireWallet (bidderID, walletKey))
        {
            bidderWallet.EnsureEnoughBalance (quantity);
            bidderWallet.CreateJournalRecord (MarketReference.ContractAuctionBid, null, null, -quantity);
        }

        Notifications.NotifyOwners (this.GetOutbids (), new OnContractOutbid (ID));
        Notifications.NotifyOwner (ForCorp ? IssuerID : IssuerCorpID, new OnContractOutbid (ID));

        // finally place the bid
        ulong bidID = Lock.ConPlaceBid (ID, quantity, bidderID, forCorp, walletKey);

        if (maximumBidderID == 0)
            return bidID;
        
        // return the money for the player that was the highest bidder
        using (IWallet maximumBidderWallet = this.Wallets.AcquireWallet (maximumBidderID, bidderWalletKey))
        {
            maximumBidderWallet.CreateJournalRecord (MarketReference.ContractAuctionBidRefund, null, null, maximumBid);
        }

        return bidID;
    }

    public void CheckOwnership (Session session)
    {
        if (ForCorp == false && IssuerID != session.CharacterID)
            throw new ConNotYourContract ();
        if (ForCorp == true && IssuerCorpID != session.CorporationID)
            throw new ConNotYourContract ();
        if (ForCorp == true && CorporationRole.ContractManager.Is (session.CorporationRole) == false)
            throw new ConCorpContractRoleMissing ();
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
        foreach ((RequestedContractItem item, int quantity) in ValidateRequestedItems (this.StartStationID, acceptorID))
        {
            ItemEntity loadedItem = Items.LoadItem (item.ItemID, out bool loadRequired);

            DogmaItems.SplitStack (loadedItem, quantity, StartStationID, contractCreator, ForCorp ? Flags.CorpMarket : Flags.Hangar);
        }
        
        // now move the items to the new owner's 
        foreach ((int typeID, int quantity, int? itemID) in Lock.ConGetItems (ID, 1))
        {
            ItemEntity loadedItem = Items.LoadItem ((int) itemID);

            DogmaItems.MoveItem (loadedItem, StartStationID, acceptorID, ForCorp ? Flags.CorpMarket : Flags.Hangar);
        }

        Status        = ContractStatus.Finished;
        CompletedDate = DateTime.Now.ToFileTimeUtc ();
    }

    private void AcceptCourierContract (int acceptorID)
    {
        // move the crate into the player's inventory
        Container crate = Items.LoadItem <Container> (CrateID);

        foreach ((int _, ItemEntity item) in crate.Items)
        {
            item.OwnerID = acceptorID;
            item.Flag    = Flags.None;
            item.Persist ();
        }
        
        DogmaItems.MoveItem (crate, StartStationID, acceptorID, ForCorp ? Flags.CorpMarket : Flags.Hangar);

        Status = ContractStatus.InProgress;
    }

    private void AcceptLoanContract (int acceptorID)
    {
        // move the crate into the player's inventory
        Container crate = Items.LoadItem <Container> (CrateID, out bool loadRequired);

        foreach ((int _, ItemEntity item) in crate.Items)
            DogmaItems.MoveItem (item, StartStationID, acceptorID, ForCorp ? Flags.CorpMarket : Flags.Hangar);

        DogmaItems.DestroyItem (crate);

        Status = ContractStatus.InProgress;
        
        // TODO: ON FINISHING THIS CONTRACT THERE'S SOME CHECKS REQUIRED:
        // TODO: ConReturnItemMissingShip CHECK FOR THE SAME SHIP WITH THE SAME ITEM
        // TODO: ConReturnItemsCurrentShip CHECK THE SHIP IS NOT BOARDED RIGHT NOW
        // TODO: ConReturnItemsDamaged NO DAMAGE ON ITEMS
        // TODO: ConReturnItemsMissing ANY OF THE ITEMS ARE MISSING
        // TODO: ConReturnItemsMissingNonSingleton SINGLETON STATUS FOR SPECIFIC ITEMS
        // TODO: ConReturnItemsStacked ENSURE STACKS ARE THE SAME SIZE (WHY THO? JUST CHECKING FOR QUANTITIES LIKE WE DO IN CONTRACT CREATION SHOULD BE FINE)
        // TODO: ConReturnItemsWrong WHEN A SHIP HAS MORE ITEMS THAN IT INITIALLY HAD
        
    }

    public void Accept (Session session, bool forCorp)
    {
        if (Status != ContractStatus.Outstanding)
            throw new ConContractNotOutstanding ();
        if (ExpireTime < DateTime.Now.ToFileTimeUtc ())
            throw new ConContractExpired ();
        // TODO: CHECK FOR SAME ACCEPTOR ConContractSameIssuerAndAcceptor
        // TODO: CHECK FOR ContractManager ROLE FOR CORPORATIONS ConCorpContractRoleMissing

        // TODO: CHECK FOR PERMISSIONS FOR ACCEPTING CONTRACTS FOR CORPORATION
        int acceptorID = forCorp ? session.CorporationID : session.CharacterID;
        
        switch (Type)
        {
            case ContractTypes.ItemExchange:
                this.AcceptItemExchangeContract (acceptorID);
                break;
            case ContractTypes.Courier:
                this.AcceptCourierContract (acceptorID);
                break;
            
            case ContractTypes.Loan:
                this.AcceptLoanContract (acceptorID);
                break;
            
            case ContractTypes.Auction:  throw new CustomError ("Auctions cannot be accepted!");
            case ContractTypes.Freeform: throw new CustomError ("Freeform contracts not supported yet!");
            default:                     throw new CustomError ("Unknown contract type to accept!");
        }
        
        // TODO: FINISHING A COURIER CONTRACT SHOULD CHECK FOR CRATE IN ENDSTATIONID AND THROW ConCrateNotFound IF NOT FOUND
        // TODO: SHOULD ALSO CHECK CRATE ITEMS AND THROW ConCrateNotFound IF THEY DON'T MATCH, IS THIS REALLY NEEDED?
        
        AcceptorID   = session.CharacterID;
        AcceptedDate = DateTime.Now.ToFileTimeUtc ();
        
        DogmaNotifications.QueueMultiEvent(
            ForCorp ? IssuerCorpID : IssuerID, new OnContractAccepted (ID)
        );
        DogmaNotifications.QueueMultiEvent (
            acceptorID, new OnContractAccepted (ID)
        );
    }
    
    public void Destroy ()
    {
        if (Status != ContractStatus.Outstanding)
            throw new ConContractNotOutstanding ();
        
        Lock.ConDestroy (ID);

        // finally dispose of the contract as it's not needed
        Dispose (false);
    }

    private string GenerateLockName ()
    {
        return $"contract_{this.ID}";
    }

    private void Dispose (bool save)
    {
        if (this.mDisposed)
            return;
        
        this.mDisposed = true;
        
        if (save)
            Lock.ConSaveInfo (this.mInformation);
        
        Lock.Dispose ();
    }

    public void Dispose ()
    {
        this.Dispose (true);
    }
}