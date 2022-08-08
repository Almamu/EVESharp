using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Market;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.Node.Data.Inventory;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using EVESharp.Node.Market;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

public class charmgr : Service
{
    public override AccessLevel    AccessLevel   => AccessLevel.None;
    private         CharacterDB    DB            { get; }
    private         MarketDB       MarketDB      { get; }
    private         IItems    Items   { get; }
    private         IWalletManager WalletManager { get; }

    public charmgr (CharacterDB db, MarketDB marketDB, IItems items, IWalletManager WalletManager)
    {
        DB                 = db;
        MarketDB           = marketDB;
        this.Items         = items;
        this.WalletManager = WalletManager;
    }

    public PyDataType GetPublicInfo (CallInformation call, PyInteger characterID)
    {
        return DB.GetPublicInfo (characterID);
    }

    [MustBeCharacter]
    public PyDataType GetPublicInfo3 (CallInformation call, PyInteger characterID)
    {
        return DB.GetPublicInfo3 (characterID);
    }

    [MustBeCharacter]
    public PyDataType GetTopBounties (CallInformation call)
    {
        return DB.GetTopBounties ();
    }

    [MustBeCharacter]
    public PyDataType AddToBounty (CallInformation call, PyInteger characterID, PyInteger bounty)
    {
        // get character's object
        Character character = this.Items.GetItem <Character> (call.Session.CharacterID);

        // access the wallet and do the required changes
        using IWallet wallet = WalletManager.AcquireWallet (character.ID, WalletKeys.MAIN);
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

    [MustBeCharacter]
    public PyDataType GetPrivateInfo (CallInformation call, PyInteger characterID)
    {
        return DB.GetPrivateInfo (characterID);
    }
}