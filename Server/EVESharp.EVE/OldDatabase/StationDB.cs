using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Database.Extensions;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Old;

public class StationDB : DatabaseAccessor
{
    public StationDB (IDatabase db) : base (db) { }

    public int CountRentedOffices (int stationID)
    {
        DbDataReader reader = this.Database.Select (
            "SELECT COUNT(*) FROM crpOffices WHERE stationID = @stationID AND impounded = 0",
            new Dictionary <string, object> {{"@stationID", stationID}}
        );

        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt32 (0);
        }
    }

    public void RentOffice (int corporationID, int stationID, int officeFolderID, long dueDate, double periodCost, int nextBillID)
    {
        this.Database.Prepare (
            "INSERT INTO crpOffices(corporationID, stationID, officeID, officeFolderID, startDate, rentPeriodInDays, periodCost, balanceDueDate, nextBillID)VALUES(@corporationID, @stationID, @officeFolderID, @officeFolderID, @startDate, @rentPeriodInDays, @periodCost, @dueDate, @nextBillID)",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@stationID", stationID},
                {"@officeFolderID", officeFolderID},
                {"@startDate", DateTime.UtcNow.ToFileTimeUtc ()},
                {"@rentPeriodInDays", (DateTime.FromFileTimeUtc (dueDate) - DateTime.UtcNow).TotalDays},
                {"@periodCost", periodCost},
                {"@dueDate", dueDate},
                {"@nextBillID", nextBillID}
            }
        );
    }

    public PyList <PyPackedRow> GetOfficesList (int stationID)
    {
        return this.Database.PreparePackedRowList (
            "SELECT corporationID, officeID AS itemID, officeFolderID FROM crpOffices WHERE stationID = @stationID AND impounded = 0",
            new Dictionary <string, object> {{"@stationID", stationID}}
        );
    }

    public PyDataType GetOfficesOwners (int stationID)
    {
        return this.Database.PrepareRowset (
            "SELECT corporationID AS ownerID, itemName AS ownerName, eveNames.typeID FROM crpOffices LEFT JOIN eveNames ON eveNames.itemID = corporationID WHERE stationID = @stationID AND impounded = 0",
            new Dictionary <string, object> {{"@stationID", stationID}}
        );
    }

    public PyDataType GetCorporations (int stationID)
    {
        // TODO: TAKE INTO ACCOUNT CORPORATION'S HEADQUARTERS TOO!
        return this.Database.PrepareRowset (
            "SELECT corporationID, itemName AS corporationName, corporation.stationID FROM crpOffices LEFT JOIN corporation USING (corporationID) LEFT JOIN eveNames ON eveNames.itemID = corporationID WHERE crpOffices.stationID = @stationID",
            new Dictionary <string, object> {{"@stationID", stationID}}
        );
    }
}