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
        public const int MIN_CHANNEL_ENTITY_ID = 1000;
        public const int MAX_CHANNEL_ENTITY_ID = 2100000000;
        
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

        public void GrantAccessToStandardChannels(int characterID)
        {
            Database.PrepareQuery(
                "INSERT INTO lscChannelPermissions(channelID, accessor, mode) SELECT channelID, @characterID AS accessor, @mode AS `mode` FROM lscGeneralChannels WHERE channelID < 1000",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@mode", CHATROLE_CONVERSATIONALIST}
                }
            );
        }

        public long CreateChannel(ItemEntity owner, ItemEntity relatedEntity, string name, bool maillist)
        {
            return this.CreateChannel(owner.ID, relatedEntity?.ID, name, maillist);
        }

        public long CreateChannel(int owner, int? relatedEntity, string name, bool maillist)
        {
            if (relatedEntity == null)
            {
                return -(long) Database.PrepareQueryLID(
                    "INSERT INTO lscPrivateChannels(channelID, ownerID, displayName, motd, comparisonKey, memberless, password, mailingList, cspa, temporary, estimatedMemberCount)VALUES(NULL, @ownerID, @displayName, '', NULL, 0, NULL, @mailinglist, 0, 0, 0)",
                    new Dictionary<string, object>()
                    {
                        {"@ownerID", owner},
                        {"@displayName", name},
                        {"@mailinglist", maillist}
                    }
                );
            }
            else
            {
                // maillist 
                if (maillist == true)
                {
                    Database.PrepareQuery(
                        "INSERT INTO lscGeneralChannels(channelID, ownerID, relatedEntityID, displayName, motd, comparisonKey, memberless, password, mailingList, cspa, temporary, estimatedMemberCount)VALUES(@channelID, @ownerID, @relatedEntityID, @displayName, '', NULL, 0, NULL, 1, 0, 0, 0)",
                        new Dictionary<string, object>()
                        {
                            {"@channelID", relatedEntity},
                            {"@ownerID", owner},
                            {"@relatedEntityID", relatedEntity},
                            {"@displayName", name}
                        }
                    );

                    return (long) relatedEntity;
                }
                else
                {
                    return (long) Database.PrepareQueryLID(
                        "INSERT INTO lscGeneralChannels(channelID, ownerID, relatedEntityID, displayName, motd, comparisonKey, memberless, password, mailingList, cspa, temporary, estimatedMemberCount)VALUES(NULL, @ownerID, @relatedEntityID, @displayName, '', NULL, 0, NULL, 0, 0, 0, 0)",
                        new Dictionary<string, object>()
                        {
                            {"@channelID", relatedEntity},
                            {"@ownerID", owner},
                            {"@relatedEntityID", relatedEntity},
                            {"@displayName", name}
                        }
                    );
                }
            }
        }

        public void JoinEntityMailingList(int relatedEntityID, int characterID, int role = CHATROLE_CONVERSATIONALIST)
        {
            int channelID = this.GetChannelIDFromRelatedEntity(relatedEntityID, true);

            this.JoinChannel(channelID, characterID, role);
        }

        public void JoinEntityChannel(int relatedEntityID, int characterID, int role = CHATROLE_CONVERSATIONALIST)
        {
            int channelID = GetChannelIDFromRelatedEntity(relatedEntityID);
            
            this.JoinChannel(channelID, characterID, role);
        }

        public void JoinChannel(int channelID, int characterID, int role = CHATROLE_CONVERSATIONALIST)
        {
            Database.PrepareQuery(
                "INSERT INTO lscChannelPermissions(channelID, accessor, `mode`, untilWhen, originalMode, admin, reason)VALUES(@channelID, @characterID, @role, NULL, @role, @admin, '')",
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
            if (channelID < 0)
            {
                Database.PrepareQuery(
                    "DELETE FROM lscPrivateChannels WHERE channelID = @channelID",
                    new Dictionary<string, object>()
                    {
                        {"@channelID", -channelID}
                    }
                );
            }
            else
            {
                Database.PrepareQuery(
                    "DELETE FROM lscGeneralChannels WHERE channelID = @channelID",
                    new Dictionary<string, object>()
                    {
                        {"@channelID", channelID}
                    }
                );
            }
            
            Database.PrepareQuery(
                "DELETE FROM lscChannelPermissions WHERE channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
        }

        public void LeaveChannel(int channelID, int characterID)
        {
            Database.PrepareQuery(
                "DELETE FROM lscChannelPermissions WHERE accessor = @characterID AND channelID = @channelID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@channelID", channelID}
                }
            );
        }

        public Rowset GetChannelsForCharacter(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT" + 
                " lscChannelPermissions.channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," + 
                " mailingList, cspa, temporary, 1 AS subscribed, estimatedMemberCount " +
                " FROM lscPrivateChannels" +
                " LEFT JOIN lscChannelPermissions ON lscPrivateChannels.channelID = -lscChannelPermissions.channelID" +
                " WHERE accessor = @characterID AND `mode` > 0 AND lscChannelPermissions.channelID < 0 " +
                "UNION " +
                "SELECT" + 
                " lscChannelPermissions.channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," + 
                " mailingList, cspa, temporary, 1 AS subscribed, estimatedMemberCount " +
                " FROM lscGeneralChannels" +
                " LEFT JOIN lscChannelPermissions ON lscGeneralChannels.channelID = lscChannelPermissions.channelID" +
                $" WHERE accessor = @characterID AND `mode` > 0 AND lscChannelPermissions.channelID > {MIN_CHANNEL_ENTITY_ID} AND lscChannelPermissions.channelID < {MAX_CHANNEL_ENTITY_ID}",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public Row GetChannelInfo(int channelID, int characterID)
        {
            string query;

            if (channelID < 0)
            {
                query = 
                    "SELECT" +
                    " lscChannelPermissions.channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
                    " mailingList, cspa, temporary, !ISNULL(lscChannelPermissions.accessor) AS subscribed, 0 AS languageRestriction " +
                    " FROM lscPrivateChannels" +
                    " LEFT JOIN lscChannelPermissions ON lscPrivateChannels.channelID = -lscChannelPermissions.channelID" +
                    " WHERE lscChannelPermissions.accessor = @characterID AND lscChannelPermissions.channelID = @channelID";
            }
            else
            {
                query = 
                    "SELECT" +
                    " channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
                    " mailingList, cspa, temporary, !ISNULL(lscChannelPermissions.accessor) AS subscribed, 0 AS languageRestriction " +
                    " FROM lscGeneralChannels" +
                    " LEFT JOIN lscChannelPermissions USING (channelID)" +
                    " WHERE lscChannelPermissions.accessor = @characterID AND channelID = @channelID";
            }
            
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection, query, 
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

        public Row GetChannelInfoByRelatedEntity(int relatedEntityID, int characterID, bool maillist = false)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT" +
                " channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
                " mailingList, cspa, temporary, !ISNULL(lscChannelPermissions.accessor) AS subscribed, 0 AS languageRestriction " +
                " FROM lscGeneralChannels" +
                " LEFT JOIN lscChannelPermissions USING (channelID)" +
                " WHERE accessor = @characterID AND relatedEntityID = @relatedEntityID AND mailingList = @mailingList",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@relatedEntityID", relatedEntityID},
                    {"@mailingList", maillist}
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
                "SELECT accessor AS charID, corporationID AS corpID, allianceID, 0 AS warFactionID, account.role AS role, 0 AS extra FROM lscChannelPermissions LEFT JOIN chrInformation ON accessor = characterID LEFT JOIN corporation USING(corporationID) LEFT JOIN account ON account.accountID = chrInformation.accountID WHERE channelID = @channelID AND account.online = 1",
                new Dictionary<string, object>()
                {
                    {"@channelID", channelID}
                }
            );
        }

        public Rowset GetChannelMods(int channelID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT accessor, `mode`, untilWhen, originalMode, admin, reason FROM lscChannelPermissions WHERE channelID = @channelID",
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
                "SELECT accessor FROM lscChannelPermissions LEFT JOIN chrInformation ON accessor = characterID WHERE channelID = @channelID AND online = 1 AND `mode` > 0",
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
                "SELECT `mode` FROM lscChannelPermissions WHERE channelID = @channelID AND accessor = @characterID",
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
                "SELECT `mode` FROM lscChannelPermissions, lscGeneralChannels WHERE lscGeneralChannels.channelID = lscChannelPermissions.channelID AND lscGeneralChannels.relatedEntityID = @relatedEntityID AND accessor = @characterID",
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

        public int GetChannelIDFromRelatedEntity(int relatedEntityID, bool isMailingList = false)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT channelID FROM lscGeneralChannels WHERE relatedEntityID = @itemID AND mailingList = @mailingList",
                new Dictionary<string, object>()
                {
                    {"@itemID", relatedEntityID},
                    {"@mailingList", isMailingList}
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
            if (channelID < 0)
                return "normal";
            
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT displayName FROM lscGeneralChannels WHERE channelID = @channelID",
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
            string query;

            if (channelID < 0)
            {
                query = "SELECT displayName FROM lscPrivateChannels WHERE channelID = @channelID";
            }
            else
            {
                query = "SELECT displayName FROM lscGeneralChannels WHERE channelID = @channelID";
            }
            
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection, query,
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
                "SELECT 0 AS extra FROM lscChannelPermissions WHERE channelID = @channelID AND accessor = @characterID",
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
                "SELECT `mode` FROM lscChannelPermissions WHERE channelID = @channelID AND accessor = @characterID",
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
                "SELECT `mode` FROM lscChannelPermissions WHERE channelID = @channelID AND accessor = @characterID",
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
                "REPLACE INTO lscChannelPermissions(channelID, accessor, mode, untilWhen, originalMode, admin, reason)VALUES(@channelID, @characterID, @mode, NULL, @mode, @admin, '')",
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