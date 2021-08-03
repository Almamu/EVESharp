using System;
using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.StaticData.Inventory;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class AlliancesDB : DatabaseAccessor
    {
        private ItemDB ItemDB { get; init; }
        private ItemFactory ItemFactory { get; init; }
        
        public ulong CreateAlliance(string name, string shortName, string url, string description, int creatorID, int creatorCharacterID)
        {
            ulong allianceID = this.ItemDB.CreateItem(name, (int) Types.Alliance, creatorID, this.ItemFactory.LocationSystem.ID, Flags.None, false,
                true, 1, 0, 0, 0, "");

            Database.PrepareQuery(
                "INSERT INTO crpAlliances(allianceID, shortName, description, url, executorCorpID, creatorCorpID, creatorCharID, dictatorial, startDate)VALUES(@allianceID, @shortName, @description, @url, @creatorID, @creatorID, @creatorCharacterID, @dictatorial, @startDate)",
                new Dictionary<string, object>()
                {
                    {"@allianceID", allianceID},
                    {"@shortName", shortName},
                    {"@description", description},
                    {"@url", url},
                    {"@creatorID", creatorID},
                    {"@creatorCharacterID", creatorCharacterID},
                    {"@dictatorial", false},
                    {"@startDate", DateTime.UtcNow.ToFileTimeUtc()}
                }
            );

            return allianceID;
        }

        public void UpdateAlliance(Alliance alliance)
        {
            Database.PrepareQuery(
                "UPDATE crpAlliances SET description = @description, url = @url, executorCorpID = @executorCorpID WHERE allianceID = @allianceID",
                new Dictionary<string, object>()
                {
                    {"@description", alliance.Description},
                    {"@url", alliance.Url},
                    {"@allianceID", alliance.ID},
                    {"@executorCorpID", alliance.ExecutorCorpID}
                }
            );
        }

        public Rowset GetAlliances()
        {
            return Database.PrepareRowsetQuery(
                "SELECT allianceID, itemName AS allianceName, shortName, 0 AS memberCount, executorCorpID, creatorCorpID, creatorCharID, dictatorial, startDate, deleted FROM crpAlliances LEFT JOIN eveNames ON itemID = allianceID"
            );
        }
        
        public Row GetAlliance(int allianceID)
        {
            return Database.PrepareRowQuery(
                "SELECT allianceID, shortName, description, url, executorCorpID, creatorCorpID, creatorCharID, dictatorial, deleted FROM crpAlliances WHERE allianceID = @allianceID",
                new Dictionary<string, object>()
                {
                    {"@allianceID", allianceID}
                }
            );
        }

        public PyDataType GetMembers(int allianceID, bool extraInfo = false)
        {
            if (extraInfo == false)
            {
                return Database.PrepareRowsetQuery(
                    "SELECT corporationID, startDate FROM corporation WHERE allianceID = @allianceID",
                    new Dictionary<string, object>()
                    {
                        {"@allianceID", allianceID}
                    }
                );                
            }
            else
            {
                return Database.PrepareIndexRowsetQuery(
                    0, "SELECT corporationID, allianceID, chosenExecutorID FROM corporation WHERE allianceID = @allianceID",
                    new Dictionary<string, object>()
                    {
                        {"@allianceID", allianceID}
                    }
                );
            }
        }
        
        public IndexRowset GetRelationships(int allianceID)
        {
            return Database.PrepareIndexRowsetQuery(
                0, "SELECT toID, relationship FROM allRelationships WHERE fromID = @allianceID",
                new Dictionary<string, object>()
                {
                    {"@allianceID", allianceID}
                }
            );
        }
        
        public void UpdateRelationship(int allianceID, int toID, int relationship)
        {
            Database.PrepareQuery(
                "REPLACE INTO allRelationships(fromID, toID, relationship)VALUES(@fromID, @toID, @relationship)",
                new Dictionary<string, object>()
                {
                    {"@fromID", allianceID},
                    {"@toID", toID},
                    {"@relationship", relationship}
                }
            );
        }

        public void RemoveRelationship(int fromID, int toID)
        {
            Database.PrepareQuery(
                "DELETE FROM allRelationships WHERE fromID = @fromID AND toID = @toID",
                new Dictionary<string, object>()
                {
                    {"@fromID", fromID},
                    {"@toID", toID}
                }
            );
        }


        public void UpdateSupportedExecutor(int corporationID, int chosenExecutorID)
        {
            Database.PrepareQuery(
                "UPDATE corporation SET chosenExecutorID = @chosenExecutorID WHERE corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@chosenExecutorID", chosenExecutorID},
                    {"@corporationID", corporationID}
                }
            );
        }

        public void CalculateNewExecutorCorp(int allianceID, out int? executorCorpID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT chosenExecutorID, COUNT(*) AS votes, (SELECT COUNT(*) FROM corporation WHERE allianceID = @allianceID) AS total FROM corporation WHERE allianceID = @allianceID GROUP BY chosenExecutorID ORDER BY votes DESC LIMIT 1",
                new Dictionary<string, object>()
                {
                    {"@allianceID", allianceID}
                }
            );

            executorCorpID = null;
            
            using (connection)
            using (reader)
            {
                // there has to be at least ONE record available here
                reader.Read();
                
                int corporationID = reader.GetInt32(0);
                int entryVotes = reader.GetInt32(1);
                int totalVotes = reader.GetInt32(2);
                
                // calculate percentage
                int percentage = entryVotes * 100 / totalVotes;

                if (percentage > 50)
                    executorCorpID = corporationID;
            }
        }

        public PyDataType GetApplicationsToAlliance(int allianceID)
        {
            return Database.PrepareIndexRowsetQuery(
                1, "SELECT allianceID, corporationID, applicationText, applicationDateTime, state FROM crpApplications WHERE allianceID = @allianceID",
                new Dictionary<string, object>()
                {
                    {"@allianceID", allianceID}
                }
            );
        }

        public void UpdateApplicationStatus(int allianceID, int corporationID, int newStatus)
        {
            Database.PrepareQuery(
                "UPDATE crpApplications SET state = @newStatus WHERE allianceID = @allianceID AND corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID},
                    {"@allianceID", allianceID},
                    {"@newStatus", newStatus}
                }
            );
        }
        
        public AlliancesDB(ItemFactory itemFactory, ItemDB itemDB, DatabaseConnection db) : base(db)
        {
            this.ItemDB = itemDB;
            this.ItemFactory = itemFactory;
        }
    }
}