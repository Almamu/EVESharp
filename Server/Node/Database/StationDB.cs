using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Data;

namespace Node.Database
{
    public class StationDB : DatabaseAccessor
    {
        public StationDB(DatabaseConnection db) : base(db)
        {
        }

        public Dictionary<int, StationOperations> LoadOperations()
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Query(ref connection, "SELECT operationID, operationName, description FROM staOperations");
            
            using (connection)
            using (reader)
            {
                Dictionary<int, StationOperations> operations = new Dictionary<int, StationOperations>();
                
                while (reader.Read() == true)
                {
                    List<int> services = new List<int>();
                    MySqlConnection connectionServices = null;
                    MySqlDataReader readerServices = Database.PrepareQuery(ref connectionServices,
                        "SELECT serviceID FROM staOperationServices WHERE operationID = @operationID",
                        new Dictionary<string, object>()
                        {
                            {"@operationID", reader.GetUInt32(0)}
                        }
                    );
                    
                    using (connectionServices)
                    using (readerServices)
                    {
                        while (readerServices.Read() == true)
                            services.Add(readerServices.GetInt32(0));
                    }
                    
                    StationOperations operation = new StationOperations(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        services
                    );

                    operations[operation.OperationID] = operation;
                }

                return operations;
            }
        }

        public Dictionary<int, StationType> LoadStationTypes()
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Query(
                ref connection,
                "SELECT stationTypeID, hangarGraphicID, dockEntryX," + 
                " dockEntryY, dockEntryZ, dockOrientationX, dockOrientationY," +
                " dockOrientationZ, operationID, officeSlots, reprocessingEfficiency, conquerable " +
                "FROM staStationTypes"
            );
            
            using (connection)
            using (reader)
            {
                Dictionary<int, StationType> result = new Dictionary<int, StationType>();
                
                while (reader.Read() == true)
                {
                    StationType stationType = new StationType(
                        reader.GetInt32(0),
                        reader.GetInt32OrNull(1),
                        reader.GetDouble(2),
                        reader.GetDouble(3),
                        reader.GetDouble(4),
                        reader.GetDouble(5),
                        reader.GetDouble(6),
                        reader.GetDouble(7),
                        reader.GetInt32OrNull(8),
                        reader.GetInt32OrNull(9),
                        reader.GetDoubleOrNull(10),
                        reader.GetBoolean(11)
                    );

                    result[stationType.ID] = stationType;
                }

                return result;
            }
        }

        public Dictionary<int, string> LoadServices()
        {
            MySqlConnection connection = null;
            MySqlCommand command =
                Database.PrepareQuery(ref connection, "SELECT serviceID, serviceName FROM staServices");
            
            using (connection)
            using (command)
            {
                MySqlDataReader reader = command.ExecuteReader();

                using (reader)
                {
                    Dictionary<int, string> result = new Dictionary<int, string>();

                    while (reader.Read() == true)
                        result[reader.GetInt32(0)] = reader.GetString(1);

                    return result;
                }
            }
        }
    }
}