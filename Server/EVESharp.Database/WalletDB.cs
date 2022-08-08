using System;
using System.Collections.Generic;
using System.Data;
using EVESharp.Common.Database;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Market;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using MySql.Data.MySqlClient;

namespace EVESharp.Database;

public static class WalletDB
{
    public static double MktWalletGetBalance (this IDatabaseConnection Database, ref IDbConnection connection, int walletKey, int ownerID)
    {
        return Database.Scalar <double> (
            ref connection,
            "MktWalletGetBalance",
            new Dictionary <string, object>
            {
                {"_walletKey", walletKey},
                {"_ownerID", ownerID}
            }
        );
    }

    public static double MktWalletGetBalance (this IDatabaseConnection Database, int walletKey, int ownerID)
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

    public static void MktWalletSetBalance (this IDatabaseConnection Database, ref IDbConnection connection, double balance, int ownerID, int walletKey)
    {
        Database.Procedure (
            ref connection,
            "MktWalletSetBalance",
            new Dictionary <string, object>
            {
                {"_balance", balance},
                {"_ownerID", ownerID},
                {"_walletKey", walletKey}
            }
        );
    }

    public static PyList <PyPackedRow> MktWalletGet (this IDatabaseConnection Database, int ownerID, List <int> walletKeys)
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

    public static void MktWalletCreate (this IDatabaseConnection Database, double balance, int ownerID, int walletKey)
    {
        Database.Procedure (
            "MktWalletCreate",
            new Dictionary <string, object>
            {
                {"_walletKey", walletKey},
                {"_ownerID", ownerID},
                {"_balance", balance}
            }
        );
    }
    
    public static void MktCreateJournalEntry (this IDatabaseConnection Database, MarketReference reference, int characterID, int ownerID1, int? ownerID2, int? referenceID, double amount, double finalBalance, string reason, int walletKey)
    {
        reason = reason.Substring (0, Math.Min (reason.Length, 43));

        Database.Procedure (
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
    
    public static void MktRecordTransaction(this IDatabaseConnection Database, int ownerID, TransactionType type, int characterID, int otherID, int typeID, int quantity, double amount, int stationID, int walletKey)
    {
        // market transactions do not affect the wallet value because these are paid either when placing the sell/buy order
        // or when fullfiling it
        Database.Procedure (
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
    public static Rowset MktGetKeyMap (this IDatabaseConnection Database)
    {
        return Database.Rowset ("MktKeyMap");
    }
}