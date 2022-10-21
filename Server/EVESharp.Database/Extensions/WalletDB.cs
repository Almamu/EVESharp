using System;
using System.Collections.Generic;
using EVESharp.Database.Market;
using EVESharp.Database.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Extensions;

public static class WalletDB
{
    public static double MktWalletGetBalance (this DbLock DbLock, int walletKey, int ownerID)
    {
        return DbLock.Creator.Scalar <double> (
            DbLock.Connection,
            "MktWalletGetBalance",
            new Dictionary <string, object>
            {
                {"_walletKey", walletKey},
                {"_ownerID", ownerID}
            }
        );
    }

    public static double MktWalletGetBalance (this IDatabase Database, int walletKey, int ownerID)
    {
        return Database.Scalar <double> (
            "MktWalletGetBalance",
            new Dictionary <string, object>
            {
                {"_walletKey", walletKey},
                {"_ownerID", ownerID}
            }
        );
    }

    public static void MktWalletSetBalance (this DbLock DbLock, double balance, int ownerID, int walletKey)
    {
        DbLock.Creator.QueryProcedure (
            DbLock.Connection,
            "MktWalletSetBalance",
            new Dictionary <string, object>
            {
                {"_balance", balance},
                {"_ownerID", ownerID},
                {"_walletKey", walletKey}
            }
        );
    }

    public static PyList <PyPackedRow> MktWalletGet (this IDatabase Database, int ownerID, List <int> walletKeys)
    {
        return Database.PackedRowList (
            "MktWalletGet",
            new Dictionary <string, object>
            {
                {"_ownerID", ownerID},
                {"_walletKeyKeys", string.Join (',', walletKeys)}
            }
        );
    }

    public static void MktWalletCreate (this IDatabase Database, double balance, int ownerID, int walletKey)
    {
        Database.QueryProcedure (
            "MktWalletCreate",
            new Dictionary <string, object>
            {
                {"_walletKey", walletKey},
                {"_ownerID", ownerID},
                {"_balance", balance}
            }
        );
    }
    
    public static void MktCreateJournalEntry (this IDatabase Database, MarketReference reference, int characterID, int ownerID1, int? ownerID2, int? referenceID, double amount, double finalBalance, string reason, int walletKey)
    {
        reason = reason.Substring (0, Math.Min (reason.Length, 43));

        Database.QueryProcedure (
            "MktCreateJournalEntry",
            new Dictionary <string, object>
            {
                {"_transactionDate", DateTime.UtcNow.ToFileTimeUtc ()},
                {"_entryTypeID", (int) reference},
                {"_charID", characterID},
                {"_ownerID1", ownerID1},
                {"_ownerID2", ownerID2},
                {"_referenceID", referenceID},
                {"_amount", amount},
                {"_balance", finalBalance},
                {"_description", reason},
                {"_accountKey", walletKey}
            }
        );
    }
    
    public static void MktRecordTransaction(this IDatabase Database, int ownerID, TransactionType type, int characterID, int otherID, int typeID, int quantity, double amount, int stationID, int walletKey)
    {
        // market transactions do not affect the wallet value because these are paid either when placing the sell/buy order
        // or when fullfiling it
        Database.QueryProcedure (
            "MktRecordTransaction",
            new Dictionary <string, object>
            {
                {"_transactionDateTime", DateTime.UtcNow.ToFileTimeUtc ()},
                {"_typeID", typeID},
                {"_quantity", quantity},
                {"_price", amount},
                {"_transactionType", (int) type},
                {"_characterID", characterID},
                {"_clientID", otherID},
                {"_stationID", stationID},
                {"_accountKey", walletKey},
                {"_entityID", ownerID}
            }
        );
    }
    
    /// <summary>
    /// Obtains the keymap list for the wallet, ready for the EVE Client
    /// </summary>
    /// <returns></returns>
    public static Rowset MktGetKeyMap (this IDatabase Database)
    {
        return Database.Rowset ("MktGetKeyMap");
    }
}