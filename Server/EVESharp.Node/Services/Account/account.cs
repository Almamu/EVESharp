using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Data.Messages;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Market;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Database;
using EVESharp.Types;

namespace EVESharp.Node.Services.Account;

public class account : Service
{
    public override AccessLevel         AccessLevel  => AccessLevel.None;
    private         OldCharacterDB      DB           { get; }
    private         IWallets            Wallets      { get; }
    private         ICacheStorage       CacheStorage { get; }
    private         IDatabaseConnection Database     { get; }

    public account (IDatabaseConnection databaseConnection, OldCharacterDB db, IWallets wallets, ICacheStorage cacheStorage)
    {
        Database     = databaseConnection;
        DB           = db;
        Wallets      = wallets;
        CacheStorage = cacheStorage;
    }

    [MustBeCharacter]
    private PyDataType GetCashBalance (Session session)
    {
        return this.Wallets.GetWalletBalance (session.CharacterID);
    }

    public PyDataType GetCashBalance (CallInformation call, PyBool isCorpWallet)
    {
        return this.GetCashBalance (call, isCorpWallet ? 1 : 0, call.Session.CorpAccountKey);
    }

    public PyDataType GetCashBalance (CallInformation call, PyInteger isCorpWallet)
    {
        return this.GetCashBalance (call, isCorpWallet, call.Session.CorpAccountKey);
    }

    public PyDataType GetCashBalance (CallInformation call, PyInteger isCorpWallet, PyInteger walletKey)
    {
        if (isCorpWallet == 0)
            return this.GetCashBalance (call.Session);

        if (this.Wallets.IsAccessAllowed (call.Session, walletKey, call.Session.CorporationID) == false)
            throw new CrpAccessDenied (MLS.UI_CORP_ACCESSTOWALLETDIVISIONDENIED);

        return this.Wallets.GetWalletBalance (call.Session.CorporationID, walletKey);
    }

    public PyDataType GetKeyMap (CallInformation call)
    {
        return Database.MktGetKeyMap ();
    }

    public PyDataType GetEntryTypes (CallInformation call)
    {
        CacheStorage.Load (
            "account",
            "GetEntryTypes",
            "SELECT refTypeID AS entryTypeID, refTypeText AS entryTypeName, description FROM mktRefTypes",
            CacheObjectType.Rowset
        );

        PyDataType cacheHint = CacheStorage.GetHint ("account", "GetEntryTypes");

        return CachedMethodCallResult.FromCacheHint (cacheHint);
    }

    [MustBeCharacter]
    public PyDataType GetJournal
    (
        CallInformation call,         PyInteger accountKey,    PyInteger fromDate, PyInteger entryTypeID,
        PyBool          isCorpWallet, PyInteger transactionID, PyInteger rev
    )
    {
        int? transactionIDint = null;

        if (transactionID != null)
            transactionIDint = transactionID;

        int entityID = call.Session.CharacterID;

        if (isCorpWallet == true)
            entityID = call.Session.CorporationID;

        if (this.Wallets.IsAccessAllowed (call.Session, accountKey, entityID) == false)
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

        if (this.Wallets.IsAccessAllowed (call.Session, WalletKeys.MAIN, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.MAIN);

        if (this.Wallets.IsAccessAllowed (call.Session, WalletKeys.SECOND, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.SECOND);

        if (this.Wallets.IsAccessAllowed (call.Session, WalletKeys.THIRD, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.THIRD);

        if (this.Wallets.IsAccessAllowed (call.Session, WalletKeys.FOURTH, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.FOURTH);

        if (this.Wallets.IsAccessAllowed (call.Session, WalletKeys.FIFTH, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.FIFTH);

        if (this.Wallets.IsAccessAllowed (call.Session, WalletKeys.SIXTH, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.SIXTH);

        if (this.Wallets.IsAccessAllowed (call.Session, WalletKeys.SEVENTH, call.Session.CorporationID))
            walletKeys.Add (WalletKeys.SEVENTH);

        return Database.MktWalletGet (call.Session.CorporationID, walletKeys);
    }

    [MustBeCharacter]
    public PyDataType GiveCash (CallInformation call, PyInteger destinationID, PyDecimal quantity, PyString reason)
    {
        int accountKey = WalletKeys.MAIN;

        if (call.NamedPayload.TryGetValue ("toAccountKey", out PyInteger namedAccountKey))
            accountKey = namedAccountKey;

        int callerCharacterID = call.Session.CharacterID;

        // acquire the origin wallet, subtract quantity
        // TODO: CHECK IF THE WALLETKEY IS INDICATED IN SOME WAY
        using (IWallet originWallet = this.Wallets.AcquireWallet (callerCharacterID, WalletKeys.MAIN))
        {
            originWallet.EnsureEnoughBalance (quantity);
            originWallet.CreateJournalRecord (MarketReference.CorporationPayment, destinationID, null, -quantity, reason);
        }

        // acquire the destination wallet, add quantity
        using (IWallet destinationWallet = this.Wallets.AcquireWallet (destinationID, accountKey, true))
        {
            destinationWallet.CreateJournalRecord (MarketReference.CorporationPayment, callerCharacterID, destinationID, -1, quantity, reason);
        }

        return null;
    }

    public PyDataType GiveCashFromCorpAccount (CallInformation call, PyInteger destinationID, PyDecimal quantity, PyInteger accountKey)
    {
        // ensure the character can take from the account in question
        if (this.Wallets.IsTakeAllowed (call.Session, accountKey, call.Session.CorporationID) == false)
            throw new CrpAccessDenied (MLS.UI_CORP_ACCESSTOWALLETDIVISIONDENIED);

        // acquire the origin wallet, subtract quantity
        // TODO: CHECK IF THE WALLETKEY IS INDICATED IN SOME WAY
        using (IWallet originWallet = this.Wallets.AcquireWallet (call.Session.CorporationID, accountKey, true))
        {
            originWallet.EnsureEnoughBalance (quantity);
            originWallet.CreateJournalRecord (MarketReference.CorporationPayment, destinationID, call.Session.CharacterID, -quantity);
        }

        // TODO: CHECK IF THE DESTINATION IS A CORPORATION OR NOT
        // acquire the destination wallet, add quantity
        using (IWallet destinationWallet = this.Wallets.AcquireWallet (destinationID, WalletKeys.MAIN))
        {
            destinationWallet.CreateJournalRecord (MarketReference.CorporationPayment, call.Session.CorporationID, destinationID, -1, quantity);
        }

        return null;
    }
}