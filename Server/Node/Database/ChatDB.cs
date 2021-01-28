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
            return this.CreateChannel(owner.ID, relatedEntity?.ID, name, maillist);
        }

        public ulong CreateChannel(int owner, int? relatedEntity, string name, bool maillist)
        {
            return Database.PrepareQueryLID(
                "INSERT INTO channels(ownerID, relatedEntityID, displayName, motd, comparisonKey, memberless, password, mailingList, cspa, temporary, estimatedMemberCount)VALUES(@ownerID, @relatedEntityID, @displayName, '', NULL, 0, NULL, @maillist, 1, 0, 1)",
                new Dictionary<string, object>()
                {
                    {"@ownerID", owner},
                    {"@relatedEntityID", relatedEntity},
                    {"@displayName", name},
                    {"@maillist", maillist}
                }
            );
        }

        public void JoinEntityChannel(int relatedEntityID, int characterID, int role = CHATROLE_CONVERSATIONALIST)
        {
            int channelID = GetChannelIDFromRelatedEntity(relatedEntityID);
            
            this.JoinChannel(channelID, characterID, role);
        }

        public void JoinChannel(int channelID, int characterID, int role = CHATROLE_CONVERSATIONALIST)
        {
            Database.PrepareQuery(
                "INSERT INTO channelMods(channelID, accessor, `mode`, untilWhen, originalMode, admin, reason)VALUES(@channelID, @characterID, @role, NULL, @role, @admin, '')",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID},
                    {"@characterID", characterID},
                    {"@role", role},
                    {"@admin", role == CHATROLE_CREATOR}
                }
            );
        }

        public void DestroyChannel(int channelID)
        {
            Database.PrepareQuery(
                "DELETE FROM channels WHERE channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
            Database.PrepareQuery(
                "DELETE FROM channelMods WHERE channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
        }

        public void LeaveChannel(int channelID, int characterID)
        {
            Database.PrepareQuery(
                "DELETE FROM channelMods WHERE accessor = @characterID AND channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@channelID", channelID}
                }
            );
        }

        public Rowset GetChannelsForCharacter(int characterID)
        {
            // TODO: IMPLEMENT THE OPTION TO HAVE GLOBAL CHANNELS SO ALL THE PLAYERS CAN SEE THEM
            return Database.PrepareRowsetQuery(
                "SELECT" + 
                " channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," + 
                " mailingList, cspa, temporary, 1 AS subscribed, estimatedMemberCount " +
                " FROM channels" +
                " LEFT JOIN channelMods USING (channelID)" +
                " WHERE accessor = @characterID AND `mode` > 0",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public Row GetChannelInfo(int channelID, int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT" +
                " channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
                " mailingList, cspa, temporary, !ISNULL(channelMods.accessor) AS subscribed, 0 AS languageRestriction " +
                " FROM channels" +
                " LEFT JOIN channelMods USING (channelID)" +
                " WHERE channelMods.accessor = @characterID AND channelID = @channelID",
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

        public Row GetChannelInfoByRelatedEntity(int relatedEntityID, int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT" +
                " channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
                " mailingList, cspa, temporary, !ISNULL(channelMods.accessor) AS subscribed, 0 AS languageRestriction " +
                " FROM channels" +
                " LEFT JOIN channelMods USING (channelID)" +
                " WHERE channelMods.accessor = @characterID AND channels.relatedEntityID = @relatedEntityID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@relatedEntityID", relatedEntityID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    throw new Exception($"Cannot find channel information for channel related to the entity {relatedEntityID} and characterID {characterID}");

                return Row.FromMySqlDataReader(reader);
            }
        }

        public Rowset GetChannelMembers(int channelID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT accessor AS charID, corporationID AS corpID, allianceID, 0 AS warFactionID, account.role AS role, 0 AS extra FROM channelMods LEFT JOIN chrInformation ON accessor = characterID LEFT JOIN corporation USING(corporationID) LEFT JOIN account ON account.accountID = chrInformation.accountID WHERE channelID = @channelID AND account.online = 1",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
        }

        public Rowset GetChannelMods(int channelID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT accessor, `mode`, untilWhen, originalMode, admin, reason FROM channelMods WHERE channelID = @channelID",
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

        public PyList GetOnlineCharsOnChannel(int channelID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT accessor FROM channelMods LEFT JOIN chrInformation ON accessor = characterID WHERE channelID = @channelID AND online = 1 AND `mode` > 0",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
            
            using (connection)
            using (reader)
            {
                PyList result = new PyList();

                while (reader.Read() == true)
                    result.Add(reader.GetInt32(0));

                return result;
            }
        }

        public bool IsPlayerAllowedToChat(int channelID, int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT `mode` FROM channelMods WHERE channelID = @channelID AND accessor = @characterID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID},
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return false;

                return (reader.GetInt32(0) & CHATROLE_SPEAKER) == CHATROLE_SPEAKER;
            }
        }

        public bool IsPlayerAllowedToChatOnRelatedEntity(int relatedEntityID, int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT `mode` FROM channelMods, channels WHERE channels.channelID = channelMods.channelID AND channels.relatedEntityID = @relatedEntityID AND accessor = @characterID",
                new Dictionary<string, object>()
                {
                    {"@relatedEntityID", relatedEntityID},
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return false;

                return (reader.GetInt32(0) & CHATROLE_SPEAKER) == CHATROLE_SPEAKER;
            }
        }

        public int GetChannelIDFromRelatedEntity(int relatedEntityID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT channelID FROM channels WHERE relatedEntityID = @itemID",
                new Dictionary<string, object>()
                {
                    {"@itemID", relatedEntityID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return 0;

                return reader.GetInt32(0);
            }
        }

        public string GetChannelType(int channelID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT displayName FROM channels WHERE channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return "normal";

                return this.ChannelNameToChannelType(reader.GetString(0));
            }
        }

        public string GetChannelName(int channelID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT displayName FROM channels WHERE channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return "";

                return reader.GetString(0);
            }
        }

        public string ChannelNameToChannelType(string channelName)
        {
            if (channelName == "System Channels\\Corp")
                return "corpid";
            if (channelName == "System Channels\\Region")
                return "regionid";
            if (channelName == "System Channels\\Constellation")
                return "constellationid";
            if (channelName == "System Channels\\Local")
                return "solarsystemid2";
            if (channelName == "System Channels\\Alliance")
                return "allianceid";
            if (channelName == "System Channels\\Gang")
                return "gangid";
            if (channelName == "System Channels\\Squad")
                return "squadid";
            if (channelName == "System Channels\\Wing")
                return "wingid";
            if (channelName == "System Channels\\War Faction")
                return "warfactionid";
            if (channelName == "System Channels\\Global")
                return "global";

            return "normal";
        }

        public bool IsCharacterMemberOfChannel(int channelID, int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT 0 AS extra FROM channelMods WHERE channelID = @channelID AND accessor = @characterID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID},
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                return reader.Read();
            }
        }

        public bool IsCharacterAdminOfChannel(int channelID, int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT `mode` FROM channelMods WHERE channelID = @channelID AND accessor = @characterID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID},
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return false;

                return (reader.GetInt32(0) & CHATROLE_CREATOR) == CHATROLE_CREATOR;
            }
        }

        public bool IsCharacterOperatorOrAdminOfChannel(int channelID, int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT `mode` FROM channelMods WHERE channelID = @channelID AND accessor = @characterID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID},
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return false;

                return (reader.GetInt32(0) & (CHATROLE_CREATOR | CHATROLE_OPERATOR)) > 0;
            }
        }

        public void UpdatePermissionsForCharacterOnChannel(int channelID, int characterID, sbyte permissions)
        {
            Database.PrepareQuery(
                "REPLACE INTO channelMods(channelID, accessor, mode, untilWhen, originalMode, admin, reason)VALUES(@channelID, @characterID, @mode, NULL, @mode, @admin, '')",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID},
                    {"@characterID", characterID},
                    {"@mode", permissions},
                    {"@admin", permissions == CHATROLE_OPERATOR}
                }
            );
        }
    }
}