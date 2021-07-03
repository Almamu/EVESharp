using System.Collections.Generic;
using Common.Database;
using Node.Market;
using PythonTypes.Types.Database;

namespace Node.Database
{
    public class BillsDB : DatabaseAccessor
    {
        /// <summary>
        /// Lists receivable bills for the given corporation
        /// </summary>
        /// <param name="corporationID"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// List payable bills for the given corporation
        /// </summary>
        /// <param name="corporationID"></param>
        /// <returns></returns>
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
        /// <param name="debtorID">Who has to pay the bill</param>
        /// <param name="creditorID">Who the bill be paid to</param>
        /// <param name="amount">The total amount the bill is</param>
        /// <param name="dueDateTime">The date the bill has to be paid by</param>
        /// <param name="interest">The amount of interests</param>
        /// <param name="externalID">Extra information for the EVE Client</param>
        /// <param name="externalID2">Extra information for the EVE Client</param>
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