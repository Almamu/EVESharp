using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.EVE;
using EVESharp.EVE.Account;
using EVESharp.EVE.Client.Exceptions.corpRegistry;
using EVESharp.EVE.Client.Messages;
using EVESharp.EVE.Market;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Corporation;
using EVESharp.EVE.Wallet;
using EVESharp.Node.Cache;
using EVESharp.Node.Database;
using EVESharp.Node.Market;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Account;

public class account : Service
{
    public override AccessLevel        AccessLevel   => AccessLevel.None;
    private         CharacterDB        DB            { get; }
    private         WalletManager      WalletManager { get; }
    private         CacheStorage       CacheStorage  { get; }
    private         DatabaseConnection Database      { get; }

    public account (DatabaseConnection databaseConnection, CharacterDB db, WalletManager walletManager, CacheStorage cacheStorage)
    {
        Database      = databaseConnection;
        DB            = db;
        WalletManager = walletManager;
        CacheStorage  = cacheStorage;
    }

    [RequiredRole (Roles.ROLE_PLAYER)]
    private PyDataType GetCashBalance (Session session)
    {
        return WalletManager.GetWalletBalance (session.EnsureCharacterIsSelected ());
    }

    public PyDataType GetCashBalance (PyBool isCorpWallet, CallInformation call)
    {
        return this.GetCashBalance (isCorpWallet ? 1 : 0, call.Session.CorpAccountKey, call);
    }

    public PyDataType GetCashBalance (PyInteger isCorpWallet, CallInformation call)
    {
        return this.GetCashBalance (isCorpWallet, call.Session.CorpAccountKey, call);
    }

    public PyDataType GetCashBalance (PyInteger isCorpWallet, PyInteger walletKey, CallInformation call)
    {
        if (isCorpWallet == 0)
            return this.GetCashBalance (call.Session);

        if (WalletManager.IsAccessAllowed (call.Session, walletKey, call.Session.CorporationID) == false)
            throw new CrpAccessDenied (MLS.UI_CORP_ACCESSTOWALLETDIVISIONDENIED);

        return WalletManager.GetWalletBalance (call.Session.CorporationID, walletKey);
    }

    public PyDataType GetKeyMap (CallInformation call)
    {
        return DB.GetKeyMap ();
    }

    public PyDataType GetEntryTypes (CallInformation call)
    {
        CacheStorage.Load (
            "account",
            "GetEntryTypes",
            "SELECT refTypeID AS entryTypeID, refTypeText AS entryTypeName, description FROM mktRefTypes",
            CacheStorage.CacheObjectType.Rowset
        );

        PyDataType cacheHint = CacheStorage.GetHint ("account", "GetEntryTypes");

        return CachedMethodCallResult.FromCacheHint (cacheHint);
    }

    public PyDataType GetJournal (
        PyInteger accountKey,   PyInteger fromDate,      PyInteger entryTypeID,
        PyBool    isCorpWallet, PyInteger transactionID, PyInteger rev, CallInformation call
    )
    {
        int? transactionIDint = null;

        if (transactionID != null)
            transactionIDint = transactionID;

        int entityID = call.Session.EnsureCharacterIsSelected ();

        if (isCorpWallet == true)
            entityID = call.Session.CorporationID;

        if (WalletManager.IsAccessAllowed (call.Session, accountKey, entityID) == false)
            throw new CrpAccessDenied (MLS.UI_CORP_ACCESSTOWALLETDIVISIONDENIED);

        // journal requires accountant roles for corporation
        if (entityID == call.Session.CorporationID && (CorporationRole.Accountant.Is (call.Session.CorporationRole) == false ||
                                                       CorporationRole.JuniorAccountant.Is (call.Session.CorporationRole) == false))
            throw new CrpAccessDenied (MLS.UI_SHARED_WALLETHINT8);

        return DB.GetJournal (entityID, entryTypeID, accountKey, fromDate, transactionIDint);
    }

    public PyDataType GetWalletDivisionsInfo (CallInformation call)
    {
        // build a list of divisions the user can access
        List <int> walletKeys = new List <int> ();

        if (WalletManager.IsAccessAllowed (call.Session, Keys.MAIN, call.Session.CorporationID))
            walletKeys.Add (Keys.MAIN);
        if (WalletManager.IsAccessAllowed (call.Session, Keys.SECOND, call.Session.CorporationID))
            walletKeys.Add (Keys.SECOND);
        if (WalletManager.IsAccessAllowed (call.Session, Keys.THIRD, call.Session.CorporationID))
            walletKeys.Add (Keys.THIRD);
        if (WalletManager.IsAccessAllowed (call.Session, Keys.FOURTH, call.Session.CorporationID))
            walletKeys.Add (Keys.FOURTH);
        if (WalletManager.IsAccessAllowed (call.Session, Keys.FIFTH, call.Session.CorporationID))
            walletKeys.Add (Keys.FIFTH);
        if (WalletManager.IsAccessAllowed (call.Session, Keys.SIXTH, call.Session.CorporationID))
            walletKeys.Add (Keys.SIXTH);
        if (WalletManager.IsAccessAllowed (call.Session, Keys.SEVENTH, call.Session.CorporationID))
            walletKeys.Add (Keys.SEVENTH);

        return Database.PackedRowList (
            WalletDB.GET_WALLETS,
            new Dictionary <string, object>
            {
                {"_ownerID", call.Session.CorporationID},
                {"_walletKeyKeys", string.Join (',', walletKeys)}
            }
        );
    }

    public PyDataType GiveCash (PyInteger destinationID, PyDecimal quantity, PyString reason, CallInformation call)
    {
        int accountKey = Keys.MAIN;

        if (call.NamedPayload.TryGetValue ("toAccountKey", out PyInteger namedAccountKey))
            accountKey = namedAccountKey;

        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        // acquire the origin wallet, subtract quantity
        // TODO: CHECK IF THE WALLETKEY IS INDICATED IN SOME WAY
        using (Wallet originWallet = WalletManager.AcquireWallet (callerCharacterID, Keys.MAIN))
        {
            originWallet.EnsureEnoughBalance (quantity);
            originWallet.CreateJournalRecord (MarketReference.CorporationPayment, destinationID, null, -quantity, reason);
        }

        // acquire the destination wallet, add quantity
        using (Wallet destinationWallet = WalletManager.AcquireWallet (destinationID, accountKey, true))
        {
            destinationWallet.CreateJournalRecord (MarketReference.CorporationPayment, callerCharacterID, destinationID, -1, quantity, reason);
        }

        return null;
    }

    public PyDataType GiveCashFromCorpAccount (PyInteger destinationID, PyDecimal quantity, PyInteger accountKey, CallInformation call)
    {
        // ensure the character can take from the account in question
        if (WalletManager.IsTakeAllowed (call.Session, accountKey, call.Session.CorporationID) == false)
            throw new CrpAccessDenied (MLS.UI_CORP_ACCESSTOWALLETDIVISIONDENIED);

        // acquire the origin wallet, subtract quantity
        // TODO: CHECK IF THE WALLETKEY IS INDICATED IN SOME WAY
        using (Wallet originWallet = WalletManager.AcquireWallet (call.Session.CorporationID, accountKey, true))
        {
            originWallet.EnsureEnoughBalance (quantity);
            originWallet.CreateJournalRecord (MarketReference.CorporationPayment, destinationID, call.Session.CharacterID, -quantity);
        }

        // TODO: CHECK IF THE DESTINATION IS A CORPORATION OR NOT
        // acquire the destination wallet, add quantity
        using (Wallet destinationWallet = WalletManager.AcquireWallet (destinationID, Keys.MAIN))
        {
            destinationWallet.CreateJournalRecord (MarketReference.CorporationPayment, call.Session.CorporationID, destinationID, -1, quantity);
        }

        return null;
    }
}