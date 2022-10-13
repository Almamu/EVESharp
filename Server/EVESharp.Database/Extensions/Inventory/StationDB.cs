using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Database.Inventory.Stations;

namespace EVESharp.Database.Extensions.Inventory;

public static class StationDB
{
    public static Dictionary <int, Operation> StaLoadOperations (this IDatabase Database)
    {
        DbDataReader  reader     = Database.Select ("SELECT operationID, operationName, description FROM staOperations");

        using (reader)
        {
            Dictionary <int, Operation> operations = new Dictionary <int, Operation> ();

            while (reader.Read ())
            {
                List <int>    services           = new List <int> ();
                DbDataReader readerServices = Database.Select (
                    "SELECT serviceID FROM staOperationServices WHERE operationID = @operationID",
                    new Dictionary <string, object> {{"@operationID", reader.GetInt32 (0)}}
                );

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

    public static Dictionary <int, Type> StaLoadStationTypes (this IDatabase Database)
    {
        DbDataReader reader = Database.Select (
            "SELECT stationTypeID, hangarGraphicID, dockEntryX," +
            " dockEntryY, dockEntryZ, dockOrientationX, dockOrientationY," +
            " dockOrientationZ, operationID, officeSlots, reprocessingEfficiency, conquerable " +
            "FROM staStationTypes"
        );

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

    public static Dictionary <int, string> StaLoadServices (this IDatabase Database)
    {
        DbDataReader reader =
            Database.Select ("SELECT serviceID, serviceName FROM staServices");

        using (reader)
        {
            Dictionary <int, string> result = new Dictionary <int, string> ();

            while (reader.Read ())
                result [reader.GetInt32 (0)] = reader.GetString (1);

            return result;
        }
    }
}