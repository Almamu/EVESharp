using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class StandingDB : DatabaseAccessor
    {
        public StandingDB(DatabaseConnection db) : base(db)
        {
        }

        public PyDataType GetCharStandings(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT toID, standing FROM chrStandings WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public PyDataType GetCharPrime(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT itemID as ownerID, itemName as ownerName, typeID FROM chrStandings, entity WHERE characterID = @characterID AND entity.itemID = chrStandings.toID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public PyDataType GetCharNPCStandings(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT fromID, standing FROM chrNPCStandings WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public PyDataType GetStandingTransactions(int? fromID, int? toID, int? direction, int? eventID, int? eventTypeID,
            long? eventDateTime)
        {
            // to understand what int_1, int_2 and int_3 mean check FmtStandingTransaction on eveFormat.py
            // to see how they're used
            
            // use the old 1=1 trick to make it easier to append things
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string query =
                "SELECT eventID, fromID, toID, direction, eventTypeID, msg, modification, int_1, int_2, int_3 FROM chrStandingTransactions WHERE 1=1";

            if (fromID != null)
            {
                query += " AND fromID=@fromID";
                parameters["@fromID"] = (int) fromID;
            }

            if (toID != null)
            {
                query += " AND toID=@toID";
                parameters["@toID"] = (int) toID;
            }

            if (direction != null)
            {
                query += " AND direction=@direction";
                parameters["@direction"] = (int) direction;
            }

            if (eventID != null)
            {
                query += " AND eventID=@eventID";
                parameters["@eventID"] = (int) eventID;
            }

            if (eventTypeID != null)
            {
                query += " AND eventTypeID=@eventTypeID";
                parameters["@eventTypeID"] = (int) eventTypeID;
            }

            if (eventDateTime != null)
            {
                query += " AND eventDateTime=@eventDateTime";
                parameters["@eventDateTime"] = (long) eventDateTime;
            }

            return Database.PrepareRowsetQuery(query, parameters);
        }

        public double? GetSecurityRating(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT securityRating FROM chrInformation WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return null;

                return reader.GetDouble(0);
            }
        }
    }
}