using EVESharp.EVE;
using EVESharp.EVE.Services;
using EVESharp.EVE.Wallet;
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

    public charmgr (CharacterDB db, MarketDB marketDB, ItemFactory itemFactory, WalletManager WalletManager)
    {
        DB                 = db;
        MarketDB           = marketDB;
        ItemFactory        = itemFactory;
        this.WalletManager = WalletManager;
    }

    public PyDataType GetPublicInfo (PyInteger characterID, CallInformation call)
    {
        return DB.GetPublicInfo (characterID);
    }

    public PyDataType GetPublicInfo3 (PyInteger characterID, CallInformation call)
    {
        return DB.GetPublicInfo3 (characterID);
    }

    public PyDataType GetTopBounties (CallInformation call)
    {
        return DB.GetTopBounties ();
    }

    public PyDataType AddToBounty (PyInteger characterID, PyInteger bounty, CallInformation call)
    {
        // get character's object
        Character character = ItemFactory.GetItem <Character> (call.Session.EnsureCharacterIsSelected ());

        // access the wallet and do the required changes
        using Wallet wallet = WalletManager.AcquireWallet (character.ID, Keys.MAIN);
        {
            // ensure the character has enough balance
            wallet.EnsureEnoughBalance (bounty);
            // take the balance from the wallet
            wallet.CreateJournalRecord (MarketReference.Bounty, null, characterID, -bounty, "Added to bounty price");
        }

        // create the bounty record and update the information in the database
        DB.AddToBounty (character.ID, characterID, bounty);

        return null;
    }

    public PyDataType GetPrivateInfo (PyInteger characterID, CallInformation call)
    {
        return DB.GetPrivateInfo (characterID);
    }
}