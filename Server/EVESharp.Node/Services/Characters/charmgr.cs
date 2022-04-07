using EVESharp.EVE;
using EVESharp.EVE.Services;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Network;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

public class charmgr : Service
{
    public override AccessLevel   AccessLevel   => AccessLevel.None;
    private         CharacterDB   DB            { get; }
    private         MarketDB      MarketDB      { get; }
    private         ItemFactory   ItemFactory   { get; }
    private         WalletManager WalletManager { get; }
        
    public charmgr(CharacterDB db, MarketDB marketDB, ItemFactory itemFactory, WalletManager WalletManager)
    {
        this.DB            = db;
        this.MarketDB      = marketDB;
        this.ItemFactory   = itemFactory;
        this.WalletManager = WalletManager;
    }
        
    public PyDataType GetPublicInfo(PyInteger characterID, CallInformation call)
    {
        return this.DB.GetPublicInfo(characterID);
    }

    public PyDataType GetPublicInfo3(PyInteger characterID, CallInformation call)
    {
        return this.DB.GetPublicInfo3(characterID);
    }

    public PyDataType GetTopBounties(CallInformation call)
    {
        return this.DB.GetTopBounties();
    }

    public PyDataType AddToBounty(PyInteger characterID, PyInteger bounty, CallInformation call)
    {
        // get character's object
        Character character = this.ItemFactory.GetItem<Character>(call.Session.EnsureCharacterIsSelected());
            
        // access the wallet and do the required changes
        using Wallet wallet = this.WalletManager.AcquireWallet(character.ID, WalletKeys.MAIN_WALLET);
        {
            // ensure the character has enough balance
            wallet.EnsureEnoughBalance(bounty);
            // take the balance from the wallet
            wallet.CreateJournalRecord(
                MarketReference.Bounty, null, characterID, -bounty, "Added to bounty price"
            );
        }
            
        // create the bounty record and update the information in the database
        this.DB.AddToBounty(character.ID, characterID, bounty);
            
        return null;
    }

    public PyDataType GetPrivateInfo(PyInteger characterID, CallInformation call)
    {
        return this.DB.GetPrivateInfo(characterID);
    }
}