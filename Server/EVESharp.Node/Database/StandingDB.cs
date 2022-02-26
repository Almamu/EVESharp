using System;
using System.Collections.Generic;
using EVESharp.Common.Database;
using MySql.Data.MySqlClient;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Database
{
    public class StandingDB : DatabaseAccessor
    {
        public Rowset GetStandings(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT toID, standing FROM chrStandings WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public Rowset GetPrime(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT itemID as ownerID, itemName as ownerName, typeID FROM chrStandings, eveNames WHERE characterID = @characterID AND eveNames.itemID = chrStandings.toID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public Rowset GetNPCStandings(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT fromID, standing FROM chrNPCStandings WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public Rowset GetStandingTransactions(int? fromID, int? toID, int? direction, int? eventID, int? eventTypeID,
            long? eventDateTime)
        {
            // to understand what int_1, int_2 and int_3 mean check FmtStandingTransaction on eveFormat.py
            // to see how they're used
            
            // use the old 1=1 trick to make it easier to append things
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            string query =
                "SELECT eventID, fromID, toID, direction, eventDateTime, eventTypeID, msg, modification, int_1, int_2, int_3 FROM chrStandingTransactions WHERE 1=1";

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
            MySqlDataReader reader = Database.Select(ref connection,
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

        public void CreateStandingTransaction(int eventTypeID, int fromID, int toID, double value, string message, int int_1 = 0, int int_2 = 0, int int_3 = 0)
        {
            Database.PrepareQuery(
                "INSERT INTO chrStandingTransactions (fromID, toID, modification, direction, msg, eventDateTime, eventTypeID, int_1, int_2, int_3)VALUES(@fromID, @toID, @modification, @direction, @msg, @eventDateTime, @eventTypeID, @int_1, @int_2, @int_3)",
                new Dictionary<string, object>()
                {
                    {"@fromID", fromID},
                    {"@toID", toID},
                    {"@direction", 1}, // direction seems to not be used anymore
                    {"@modification", value},
                    {"@msg", message},
                    {"@eventDateTime", DateTime.UtcNow.ToFileTimeUtc()},
                    {"@eventTypeID", eventTypeID},
                    {"@int_1", int_1},
                    {"@int_2", int_2},
                    {"@int_3", int_3}
                }
            );
        }

        public void SetPlayerStanding(int fromID, int toID, double value)
        {
            Database.PrepareQuery(
                "REPLACE INTO chrStandings(characterID, toID, standing)VALUES(@fromID, @toID, @value)",
                new Dictionary<string, object>()
                {
                    {"@fromID", fromID},
                    {"@toID", toID},
                    {"@value", value}
                }
            );
        }

        public double GetStanding(int from, int to)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Select(ref connection,
                "SELECT standing FROM chrStandings WHERE characterID = @fromID AND toID = @toID",
                new Dictionary<string, object>()
                {
                    {"@fromID", from},
                    {"@toID", to}
                }
            );
            
            using(connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return 0.0;

                return reader.GetDouble(0);
            }
        }

        public StandingDB(DatabaseConnection db) : base(db)
        {
        }
    }
}