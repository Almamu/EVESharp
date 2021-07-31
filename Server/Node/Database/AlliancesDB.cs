using System;
using System.Collections.Generic;
using Common.Database;
using Node.Inventory;
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

        public void UpdateAlliance(int allianceID, string description, string url)
        {
            Database.PrepareQuery(
                "UPDATE crpAlliances SET description = @description, url = @url WHERE allianceID = @allianceID",
                new Dictionary<string, object>()
                {
                    {"@description", description},
                    {"@url", url},
                    {"@allianceID", allianceID}
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

        public PyDataType GetApplicationsToAlliance(int allianceID)
        {
            return Database.PrepareIndexRowsetQuery(
                0, "SELECT allianceID, corporationID, applicationText, applicationDateTime, state FROM crpApplications WHERE allianceID = @allianceID",
                new Dictionary<string, object>()
                {
                    {"@allianceID", allianceID}
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