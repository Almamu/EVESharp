using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.EVE;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.corpRegistry;
using EVESharp.Node.Inventory;
using EVESharp.Node.Market;
using EVESharp.Node.Network;
using EVESharp.Node.Sessions;
using EVESharp.Node.StaticData.Corporation;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Account;

public class account : Service
{
    public override AccessLevel         AccessLevel         => AccessLevel.None;
    private         CharacterDB         DB                  { get; }
    private         WalletManager       WalletManager       { get; }
    private         ItemManager         ItemManager         { get; }
    private         CacheStorage        CacheStorage        { get; }
    private         NotificationManager NotificationManager { get; }
    private         DatabaseConnection  Database            { get; }

    public account (
        DatabaseConnection  databaseConnection, CharacterDB db, WalletManager walletManager, ItemManager itemManager, CacheStorage cacheStorage,
        NotificationManager notificationManager
    )
    {
        Database            = databaseConnection;
        DB                  = db;
        WalletManager       = walletManager;
        ItemManager         = itemManager;
        CacheStorage        = cacheStorage;
        NotificationManager = notificationManager;
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

        if (WalletManager.IsAccessAllowed (call.Session, WalletKeys.MAIN_WALLET, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.MAIN_WALLET);
        if (WalletManager.IsAccessAllowed (call.Session, WalletKeys.SECOND_WALLET, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.SECOND_WALLET);
        if (WalletManager.IsAccessAllowed (call.Session, WalletKeys.THIRD_WALLET, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.THIRD_WALLET);
        if (WalletManager.IsAccessAllowed (call.Session, WalletKeys.FOURTH_WALLET, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.FOURTH_WALLET);
        if (WalletManager.IsAccessAllowed (call.Session, WalletKeys.FIFTH_WALLET, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.FIFTH_WALLET);
        if (WalletManager.IsAccessAllowed (call.Session, WalletKeys.SIXTH_WALLET, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.SIXTH_WALLET);
        if (WalletManager.IsAccessAllowed (call.Session, WalletKeys.SEVENTH_WALLET, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.SEVENTH_WALLET);

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
        int accountKey = WalletKeys.MAIN_WALLET;

        if (call.NamedPayload.TryGetValue ("toAccountKey", out PyInteger namedAccountKey))
            accountKey = namedAccountKey;

        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        // acquire the origin wallet, subtract quantity
        // TODO: CHECK IF THE WALLETKEY IS INDICATED IN SOME WAY
        using (Wallet originWallet = WalletManager.AcquireWallet (callerCharacterID, WalletKeys.MAIN_WALLET))
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
        using (Wallet destinationWallet = WalletManager.AcquireWallet (destinationID, WalletKeys.MAIN_WALLET))
        {
            destinationWallet.CreateJournalRecord (MarketReference.CorporationPayment, call.Session.CorporationID, destinationID, -1, quantity);
        }

        return null;
    }
}