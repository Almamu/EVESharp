using System.Collections.Generic;
using Common.Database;
using Node.Market;
using PythonTypes.Types.Database;

namespace Node.Database
{
    public class BillsDB : DatabaseAccessor
    {
        public CRowset GetCorporationBillsReceivable(int corporationID)
        {
            return Database.PrepareCRowsetQuery(
                "SELECT billID, billTypeID, debtorID, creditorID, amount, dueDateTime, interest, externalID, paid, externalID2 FROM mktBills WHERE creditorID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
        }
        
        public CRowset GetCorporationBillsPayable(int corporationID)
        {
            return Database.PrepareCRowsetQuery(
                "SELECT billID, billTypeID, debtorID, creditorID, amount, dueDateTime, interest, externalID, paid, externalID2 FROM mktBills WHERE debtorID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
        }

        /// <summary>
        /// Creates a bill with the given information
        /// </summary>
        /// <param name="type"></param>
        /// <param name="debtorID"></param>
        /// <param name="creditorID"></param>
        /// <param name="amount"></param>
        /// <param name="dueDateTime"></param>
        /// <param name="interest"></param>
        /// <param name="externalID"></param>
        /// <param name="externalID2"></param>
        public void CreateBill(BillTypes type, int debtorID, int creditorID, double amount, long dueDateTime, double interest, int? externalID = null, int? externalID2 = null)
        {
            Database.PrepareQuery(
                "INSERT INTO mktBills(billTypeID, debtorID, creditorID, amount, dueDateTime, interest, externalID, paid, externalID2)VALUES(@billTypeID, @debtorID, @creditorID, @amount, @dueDateTime, @interest, @externalID, 0, @externalID2)",
                new Dictionary<string, object>()
                {
                    {"@billTypeID", (int) type},
                    {"@debtorID", debtorID},
                    {"@creditorID", creditorID},
                    {"@amount", amount},
                    {"@dueDateTime", dueDateTime},
                    {"@interest", interest},
                    {"@externalID", externalID ?? -1},
                    {"@externalID2", externalID2 ?? -1}
                }
            );
        }
        
        public BillsDB(DatabaseConnection db) : base(db)
        {
        }
    }
}