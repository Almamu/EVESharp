using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.EVE.Data.Inventory.Station;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Type = EVESharp.EVE.Data.Inventory.Station.Type;

namespace EVESharp.Node.Database;

public class StationDB : DatabaseAccessor
{
    public StationDB (IDatabaseConnection db) : base (db) { }

    public Dictionary <int, Operation> LoadOperations ()
    {
        IDbConnection connection = null;
        DbDataReader  reader     = Database.Select (ref connection, "SELECT operationID, operationName, description FROM staOperations");

        using (connection)
        using (reader)
        {
            Dictionary <int, Operation> operations = new Dictionary <int, Operation> ();

            while (reader.Read ())
            {
                List <int>    services           = new List <int> ();
                IDbConnection connectionServices = null;
                DbDataReader readerServices = Database.Select (
                    ref connectionServices,
                    "SELECT serviceID FROM staOperationServices WHERE operationID = @operationID",
                    new Dictionary <string, object> {{"@operationID", reader.GetInt32 (0)}}
                );

                using (connectionServices)
                using (readerServices)
                {
                    while (readerServices.Read ())
                        services.Add (readerServices.GetInt32 (0));
                }

                Operation operation = new Operation (
                    reader.GetInt32 (0),
                    reader.GetString (1),
                    reader.GetString (2),
                    services
                );

                operations [operation.OperationID] = operation;
            }

            return operations;
        }
    }

    public Dictionary <int, Type> LoadStationTypes ()
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT stationTypeID, hangarGraphicID, dockEntryX," +
            " dockEntryY, dockEntryZ, dockOrientationX, dockOrientationY," +
            " dockOrientationZ, operationID, officeSlots, reprocessingEfficiency, conquerable " +
            "FROM staStationTypes"
        );

        using (connection)
        using (reader)
        {
            Dictionary <int, Type> result = new Dictionary <int, Type> ();

            while (reader.Read ())
            {
                Type stationType = new Type (
                    reader.GetInt32 (0),
                    reader.GetInt32OrNull (1),
                    reader.GetDouble (2),
                    reader.GetDouble (3),
                    reader.GetDouble (4),
                    reader.GetDouble (5),
                    reader.GetDouble (6),
                    reader.GetDouble (7),
                    reader.GetInt32OrNull (8),
                    reader.GetInt32OrNull (9),
                    reader.GetDoubleOrNull (10),
                    reader.GetBoolean (11)
                );

                result [stationType.ID] = stationType;
            }

            return result;
        }
    }

    public Dictionary <int, string> LoadServices ()
    {
        IDbConnection connection = null;
        DbDataReader reader =
            Database.Select (ref connection, "SELECT serviceID, serviceName FROM staServices");

        using (connection)
        using (reader)
        {
            Dictionary <int, string> result = new Dictionary <int, string> ();

            while (reader.Read ())
                result [reader.GetInt32 (0)] = reader.GetString (1);

            return result;
        }
    }

    public int CountRentedOffices (int stationID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT COUNT(*) FROM crpOffices WHERE stationID = @stationID",
            new Dictionary <string, object> {{"@stationID", stationID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt32 (0);
        }
    }

    public void RentOffice (int corporationID, int stationID, int officeFolderID, long dueDate, double periodCost, int nextBillID)
    {
        Database.Prepare (
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

    public PyList<PyPackedRow> GetOfficesList (int stationID)
    {
        return Database.PreparePackedRowList (
            "SELECT corporationID, officeID AS itemID, officeFolderID FROM crpOffices WHERE stationID = @stationID",
            new Dictionary <string, object> {{"@stationID", stationID}}
        );
    }

    public PyDataType GetOfficesOwners (int stationID)
    {
        return Database.PrepareRowset (
            "SELECT corporationID AS ownerID, itemName AS ownerName, eveNames.typeID FROM crpOffices LEFT JOIN eveNames ON eveNames.itemID = corporationID WHERE stationID = @stationID",
            new Dictionary <string, object> {{"@stationID", stationID}}
        );
    }

    public PyDataType GetCorporations (int stationID)
    {
        // TODO: TAKE INTO ACCOUNT CORPORATION'S HEADQUARTERS TOO!
        return Database.PrepareRowset (
            "SELECT corporationID, itemName AS corporationName, corporation.stationID FROM crpOffices LEFT JOIN corporation USING (corporationID) LEFT JOIN eveNames ON eveNames.itemID = corporationID WHERE crpOffices.stationID = @stationID",
            new Dictionary <string, object> {{"@stationID", stationID}}
        );
    }

    public bool CorporationHasOfficeRentedAt (int corporationID, int stationID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT corporationID FROM crpOffices WHERE stationID = @stationID AND corporationID = @corporationID",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@stationID", stationID}
            }
        );

        using (connection)
        using (reader)
        {
            return reader.Read ();
        }
    }
}