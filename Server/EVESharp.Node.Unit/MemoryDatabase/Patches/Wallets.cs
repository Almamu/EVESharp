using System.Linq;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Market;
using HarmonyLib;
using NUnit.Framework;

namespace EVESharp.Node.Unit.MemoryDatabase.Patches;

public class Wallets
{
    [HarmonyPatch(typeof(WalletDB), nameof(WalletDB.MktWalletCreate))]
    static bool MktWalletCreate (IDatabase Database, double balance, int ownerID, int walletKey)
    {
        // ensure the wallet doesn't exist before
        Assert.False (MemoryDatabase.Wallets.Data.ContainsKey ((walletKey, ownerID)));
        // set the starting balance
        MemoryDatabase.Wallets.Data [(walletKey, ownerID)] = balance;
        
        return false;
    }
    
    [HarmonyPatch(typeof(WalletDB), nameof(WalletDB.MktWalletGetBalance))]
    static bool MktWalletGetBalance (DbLock DbLock, int walletKey, int ownerID, ref double __result)
    {
        // ensure the wallet exists
        Assert.True (MemoryDatabase.Wallets.Data.ContainsKey ((walletKey, ownerID)));
        // obtain the value
        __result = MemoryDatabase.Wallets.Data.First (x => x.Key.walletKey == walletKey && x.Key.ownerID == ownerID).Value;
        
        return false;
    }

    [HarmonyPatch(typeof(WalletDB), nameof(WalletDB.MktWalletSetBalance))]
    static bool MktWalletSetBalance (DbLock DbLock, double balance, int ownerID, int walletKey)
    {
        // ensure the wallet exists
        Assert.True (MemoryDatabase.Wallets.Data.ContainsKey ((walletKey, ownerID)));
        // store the value
        MemoryDatabase.Wallets.Data [(walletKey, ownerID)] = balance;
        
        return false;
    }

    [HarmonyPatch(typeof(WalletDB), nameof(WalletDB.MktRecordTransaction))]
    static bool MktRecordTransaction
    (
        IDatabase Database, int ownerID,   TransactionType type, int characterID, int otherID, int typeID, int quantity,
        double                   amount,   int stationID, int             walletKey
    )
    {
        return false;
    }

    [HarmonyPatch(typeof(WalletDB), nameof(WalletDB.MktCreateJournalEntry))]
    static bool MktCreateJournalEntry
    (
        IDatabase Database,     MarketReference reference, int characterID, int ownerID1, int? ownerID2, int? referenceID, double amount,
        double                   finalBalance, string          reason,    int walletKey
    )
    {
        return false;
    }
    
}