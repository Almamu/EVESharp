using System;
using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Inventory.Items;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class ChatDB : DatabaseAccessor
    {
        public const int CHATROLE_CREATOR = 8 + 4 + 2 + 1;
        public const int CHATROLE_OPERATOR = 4 + CHATROLE_SPEAKER + CHATROLE_LISTENER;
        public const int CHATROLE_CONVERSATIONALIST = CHATROLE_SPEAKER + CHATROLE_LISTENER;
        public const int CHATROLE_SPEAKER = 2;
        public const int CHATROLE_LISTENER = 1;
        public const int CHATROLE_NOTSPECIFIED = -1;

        public const int CHANNEL_ROOKIECHANNELID = 1;
        
        public ChatDB(DatabaseConnection db) : base(db)
        {
        }

        public ulong CreateChannel(ItemEntity owner, ItemEntity relatedEntity, string name, bool maillist)
        {
            return Database.PrepareQueryLID(
                "INSERT INTO channels(channelID, ownerID, relatedEntityID, displayName, motd, comparisonKey, memberless, password, mailingList, cspa, temporary, estimatedMemberCount)VALUES(@relatedEntityID, @ownerID, @relatedEntityID, @displayName, '', NULL, 0, NULL, @maillist, 100, 0, 1)",
                new Dictionary<string, object>()
                {
                    {"@ownerID", owner.ID},
                    {"@relatedEntityID", relatedEntity?.ID},
                    {"@displayName", name},
                    {"@maillist", maillist}
                }
            );
        }

        public void JoinChannel(int channelID, int characterID, int role)
        {
            Database.PrepareQuery(
                "INSERT INTO channelChars(channelID, charID, role)VALUES(@channelID, @characterID, @role)",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID},
                    {"@characterID", characterID},
                    {"@role", role}
                }
            );
        }

        public Rowset GetChannelsForCharacter(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT" + 
                " channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," + 
                " mailingList, cspa, temporary, role AS mode, !ISNULL(channelChars.charID) AS subscribed, estimatedMemberCount " +
                " FROM channels" +
                " LEFT JOIN channelChars USING (channelID)" +
                " WHERE channelChars.charID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public PyDataType GetChannelInfo(int channelID, int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT" +
                " channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
                " mailingList, cspa, temporary, !ISNULL(channelChars.charID) AS subscribed, 0 AS languageRestriction " +
                " FROM channels" +
                " LEFT JOIN channelChars USING (channelID)" +
                " WHERE channelChars.charID = @characterID AND channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@channelID", channelID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    throw new Exception($"Cannot find channel information for channelID {channelID} and characterID {characterID}");

                return Row.FromMySqlDataReader(reader);
            }
        }

        public Rowset GetChannelMembers(int channelID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT charID, corporationID AS corpID, allianceID, 0 AS warFactionID, account.role AS role, 0 AS extra FROM channelChars LEFT JOIN chrInformation ON charID = characterID LEFT JOIN corporation USING(corporationID) LEFT JOIN account ON account.accountID = chrInformation.accountID WHERE channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
        }

        public Rowset GetChannelMods(int channelID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT accessor, mode, untilWhen, originalMode, admin, reason FROM channelMods WHERE channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
        }
        public PyDataType GetExtraInfo(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT itemID AS ownerID, itemName AS ownerName, typeID FROM entity WHERE itemID = @characterID",
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

                return Row.FromMySqlDataReader(reader);
            }
        }
    }
}