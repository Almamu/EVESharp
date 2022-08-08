using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Market;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Database;

public static class BillsDB
{
    public static CRowset MktBillsGetReceivable (this IDatabaseConnection Database, int corporationID)
    {
        return Database.CRowset (
            "MktBillsGetReceivable",
            new Dictionary <string, object> {{"_creditorID", corporationID}}
        );
    }

    public static CRowset MktBillsGetPayable (this IDatabaseConnection Database, int corporationID)
    {
        return Database.CRowset (
            "MktBillsGetPayable",
            new Dictionary <string, object> {{"_debtorID", corporationID}}
        );
    }

    public static ulong MktBillsCreate (this IDatabaseConnection Database, BillTypes type, int debtorID, int creditorID, double amount, long dueDateTime, double interest, int? externalID = null, int? externalID2 = null)
    {
        return Database.ProcedureLID ("MktBillsCreate",
            new Dictionary <string, object>
            {
                {"_billTypeID", (int) type},
                {"_debtorID", debtorID},
                {"_creditorID", creditorID},
                {"_amount", amount},
                {"_dueDateTime", dueDateTime},
                {"_interest", interest},
                {"_externalID", externalID ?? -1},
                {"_externalID2", externalID2 ?? -1}
            }
        );
    }
}