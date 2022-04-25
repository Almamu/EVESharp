using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.Node.Inventory.Items;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;

namespace EVESharp.Node.Database;

public class ChatDB : DatabaseAccessor
{
    public const string CHANNEL_TYPE_NORMAL          = "normal";
    public const string CHANNEL_TYPE_GLOBAL          = "global";
    public const string CHANNEL_TYPE_SOLARSYSTEMID2  = "solarsystemid2";
    public const string CHANNEL_TYPE_REGIONID        = "regionid";
    public const string CHANNEL_TYPE_CORPID          = "corpid";
    public const string CHANNEL_TYPE_CONSTELLATIONID = "constellationid";
    public const string CHANNEL_TYPE_WARFACTIONID    = "warfactionid";
    public const string CHANNEL_TYPE_ALLIANCEID      = "allianceid";

    public const int MIN_CHANNEL_ENTITY_ID = 1000;
    public const int MAX_CHANNEL_ENTITY_ID = 2100000000;

    public const int CHATROLE_CREATOR           = 8 + 4 + 2 + 1;
    public const int CHATROLE_OPERATOR          = 4 + CHATROLE_SPEAKER + CHATROLE_LISTENER;
    public const int CHATROLE_CONVERSATIONALIST = CHATROLE_SPEAKER + CHATROLE_LISTENER;
    public const int CHATROLE_SPEAKER           = 2;
    public const int CHATROLE_LISTENER          = 1;
    public const int CHATROLE_NOTSPECIFIED      = -1;

    public const int CHANNEL_ROOKIECHANNELID = 1;

    public ChatDB (IDatabaseConnection db) : base (db) { }

    /// <summary>
    /// Grants access to the standard channels to the given player
    /// </summary>
    /// <param name="characterID"></param>
    public void GrantAccessToStandardChannels (int characterID)
    {
        Database.PrepareQuery (
            $"INSERT INTO lscChannelPermissions(channelID, accessor, mode) SELECT channelID, @characterID AS accessor, @mode AS `mode` FROM lscGeneralChannels WHERE channelID < {MIN_CHANNEL_ENTITY_ID}",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@mode", CHATROLE_CONVERSATIONALIST}
            }
        );
    }

    /// <summary>
    /// Creates a new chat channel
    /// </summary>
    /// <param name="owner">The owner of the channel</param>
    /// <param name="relatedEntity">The related entity for the channel (if any)</param>
    /// <param name="name">The name of the channel</param>
    /// <param name="maillist">If it's a maillist or not</param>
    /// <returns></returns>
    public long CreateChannel (ItemEntity owner, ItemEntity relatedEntity, string name, bool maillist)
    {
        return this.CreateChannel (owner.ID, relatedEntity?.ID, name, maillist);
    }

    /// <summary>
    /// Creates a new chat channel
    /// </summary>
    /// <param name="owner">The owner of the channel</param>
    /// <param name="relatedEntity">The related entity for the channel (if any)</param>
    /// <param name="name">The name of the channel</param>
    /// <param name="maillist">If it's a maillist or not</param>
    /// <returns></returns>
    public long CreateChannel (int owner, int? relatedEntity, string name, bool maillist)
    {
        if (relatedEntity == null)
            return -(long) Database.PrepareQueryLID (
                "INSERT INTO lscPrivateChannels(channelID, ownerID, displayName, motd, comparisonKey, memberless, password, mailingList, cspa, temporary, estimatedMemberCount)VALUES(NULL, @ownerID, @displayName, '', NULL, 0, NULL, @mailinglist, 0, 0, 0)",
                new Dictionary <string, object>
                {
                    {"@ownerID", owner},
                    {"@displayName", name},
                    {"@mailinglist", maillist}
                }
            );

        // maillist 
        if (maillist)
        {
            Database.PrepareQuery (
                "INSERT INTO lscGeneralChannels(channelID, ownerID, relatedEntityID, displayName, motd, comparisonKey, memberless, password, mailingList, cspa, temporary, estimatedMemberCount)VALUES(@channelID, @ownerID, @relatedEntityID, @displayName, '', NULL, 0, NULL, 1, 0, 0, 0)",
                new Dictionary <string, object>
                {
                    {"@channelID", relatedEntity},
                    {"@ownerID", owner},
                    {"@relatedEntityID", relatedEntity},
                    {"@displayName", name}
                }
            );

            return (long) relatedEntity;
        }

        return (long) Database.PrepareQueryLID (
            "INSERT INTO lscGeneralChannels(channelID, ownerID, relatedEntityID, displayName, motd, comparisonKey, memberless, password, mailingList, cspa, temporary, estimatedMemberCount)VALUES(NULL, @ownerID, @relatedEntityID, @displayName, '', NULL, 0, NULL, 0, 0, 0, 0)",
            new Dictionary <string, object>
            {
                {"@channelID", relatedEntity},
                {"@ownerID", owner},
                {"@relatedEntityID", relatedEntity},
                {"@displayName", name}
            }
        );
    }

    /// <summary>
    /// Joins the given <paramref name="characterID"/> to the maillist channel for the <paramref name="relatedEntityID"/>
    /// </summary>
    /// <param name="relatedEntityID"></param>
    /// <param name="characterID"></param>
    /// <param name="role">Role for the player</param>
    public void JoinEntityMailingList (int relatedEntityID, int characterID, int role = CHATROLE_CONVERSATIONALIST)
    {
        int channelID = this.GetChannelIDFromRelatedEntity (relatedEntityID, true);

        this.JoinChannel (channelID, characterID, role);
    }

    /// <summary>
    /// Joins the given <paramref name="characterID"/> to the chat channel for the <paramref name="relatedEntityID"/>
    /// </summary>
    /// <param name="relatedEntityID"></param>
    /// <param name="characterID"></param>
    /// <param name="role">Role for the player</param>
    public void JoinEntityChannel (int relatedEntityID, int characterID, int role = CHATROLE_CONVERSATIONALIST)
    {
        int channelID = this.GetChannelIDFromRelatedEntity (relatedEntityID);

        this.JoinChannel (channelID, characterID, role);
    }

    /// <summary>
    /// Joins the given <paramref name="characterID"/> to the chat channel with the given <paramref name="channelID"/>
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    /// <param name="role">Role for the player</param>
    public void JoinChannel (int channelID, int characterID, int role = CHATROLE_CONVERSATIONALIST)
    {
        Database.PrepareQuery (
            "REPLACE INTO lscChannelPermissions(channelID, accessor, `mode`, untilWhen, originalMode, admin, reason)VALUES(@channelID, @characterID, @role, NULL, @role, @admin, '')",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@characterID", characterID},
                {"@role", role},
                {"@admin", role == CHATROLE_CREATOR}
            }
        );
    }

    /// <summary>
    /// Destroys the given channel and cleans up the permissions table
    /// </summary>
    /// <param name="channelID"></param>
    public void DestroyChannel (int channelID)
    {
        if (channelID < 0)
            Database.PrepareQuery (
                "DELETE FROM lscPrivateChannels WHERE channelID = @channelID",
                new Dictionary <string, object> {{"@channelID", -channelID}}
            );
        else
            Database.PrepareQuery (
                "DELETE FROM lscGeneralChannels WHERE channelID = @channelID",
                new Dictionary <string, object> {{"@channelID", channelID}}
            );

        Database.PrepareQuery (
            "DELETE FROM lscChannelPermissions WHERE channelID = @channelID",
            new Dictionary <string, object> {{"@channelID", channelID}}
        );
    }

    /// <summary>
    /// Removes the character from the given channel
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    public void LeaveChannel (int channelID, int characterID)
    {
        Database.PrepareQuery (
            "DELETE FROM lscChannelPermissions WHERE accessor = @characterID AND channelID = @channelID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@channelID", channelID}
            }
        );
    }

    /// <summary>
    /// Removes the character from the given entity-related channel
    /// </summary>
    /// <param name="relatedEntityID"></param>
    /// <param name="characterID"></param>
    public void LeaveEntityChannel (int relatedEntityID, int characterID)
    {
        int channelID = this.GetChannelIDFromRelatedEntity (relatedEntityID);

        this.LeaveChannel (channelID, characterID);
    }

    /// <summary>
    /// Obtains the list of channels the character is currently allowed into ready for the EVE Client
    /// </summary>
    /// <param name="characterID"></param>
    /// <param name="corporationID"></param>
    /// <returns></returns>
    public Rowset GetChannelsForCharacter (int characterID, int corporationID)
    {
        Rowset firstQuery = Database.PrepareRowsetQuery (
            "SELECT" +
            " lscChannelPermissions.channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
            " mailingList, cspa, temporary, 1 AS subscribed, estimatedMemberCount " +
            " FROM lscPrivateChannels" +
            " LEFT JOIN lscChannelPermissions ON lscPrivateChannels.channelID = -lscChannelPermissions.channelID" +
            " WHERE accessor = @characterID AND `mode` > 0 AND lscChannelPermissions.channelID < 0 ",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        Rowset secondQuery = Database.PrepareRowsetQuery (
            "SELECT" +
            " lscChannelPermissions.channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
            " mailingList, cspa, temporary, 1 AS subscribed, estimatedMemberCount " +
            " FROM lscGeneralChannels" +
            " LEFT JOIN lscChannelPermissions ON lscGeneralChannels.channelID = lscChannelPermissions.channelID" +
            $" WHERE accessor = @characterID AND `mode` > 0 AND lscChannelPermissions.channelID > 0 AND ((lscChannelPermissions.channelID < {MIN_CHANNEL_ENTITY_ID} AND lscChannelPermissions.channelID != @characterID) OR lscChannelPermissions.channelID = @corporationID)",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID}
            }
        );

        secondQuery.Rows.AddRange (firstQuery.Rows);

        return secondQuery;
        /*
         * This is a more elegant solution, but the charset-guessing code goes nuts with the UNIONS as the tables are different
         * and as such it's impossible to know what table it comes from
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
            $" WHERE accessor = @characterID AND `mode` > 0 AND lscChannelPermissions.channelID > 0 AND ((lscChannelPermissions.channelID < {MIN_CHANNEL_ENTITY_ID} AND lscChannelPermissions.channelID != @characterID) OR lscChannelPermissions.channelID = @corporationID)",
            new Dictionary<string, object>()
            {
                {"@characterID", characterID},
                {"@corporationID", corporationID}
            }
        );
        */
    }

    /// <summary>
    /// Obtains information for the given channel and character ready to be used by the EVE Client
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Row GetChannelInfo (int channelID, int characterID)
    {
        string query;

        if (channelID < 0)
            query =
                "SELECT" +
                " lscChannelPermissions.channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
                " mailingList, cspa, temporary, !ISNULL(lscChannelPermissions.accessor) AS subscribed, 0 AS languageRestriction " +
                " FROM lscPrivateChannels" +
                " LEFT JOIN lscChannelPermissions ON lscPrivateChannels.channelID = -lscChannelPermissions.channelID" +
                " WHERE lscChannelPermissions.accessor = @characterID AND lscChannelPermissions.channelID = @channelID";
        else
            query =
                "SELECT" +
                " channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
                " mailingList, cspa, temporary, !ISNULL(lscChannelPermissions.accessor) AS subscribed, 0 AS languageRestriction " +
                " FROM lscGeneralChannels" +
                " LEFT JOIN lscChannelPermissions USING (channelID)" +
                " WHERE lscChannelPermissions.accessor = @characterID AND channelID = @channelID";

        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection, query,
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@channelID", channelID}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                throw new Exception ($"Cannot find channel information for channelID {channelID} and characterID {characterID}");

            return Row.FromDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Obtains information for the given channel and character ready to be used by the EVE Client
    /// </summary>
    /// <param name="relatedEntityID"></param>
    /// <param name="characterID"></param>
    /// <param name="maillist"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Row GetChannelInfoByRelatedEntity (int relatedEntityID, int characterID, bool maillist = false)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT" +
            " channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
            " mailingList, cspa, temporary, !ISNULL(lscChannelPermissions.accessor) AS subscribed, 0 AS languageRestriction " +
            " FROM lscGeneralChannels" +
            " LEFT JOIN lscChannelPermissions USING (channelID)" +
            " WHERE accessor = @characterID AND relatedEntityID = @relatedEntityID AND mailingList = @mailingList",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@relatedEntityID", relatedEntityID},
                {"@mailingList", maillist}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                throw new Exception ($"Cannot find channel information for channel related to the entity {relatedEntityID} and characterID {characterID}");

            return Row.FromDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Obtains the list of members of the character's address book
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public CRowset GetAddressBookMembers (int characterID)
    {
        return Database.PrepareCRowsetQuery (
            "SELECT accessor AS characterID, online FROM lscChannelPermissions, chrInformation WHERE channelID = @characterID AND chrInformation.characterID = lscChannelPermissions.accessor",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    /// <summary>
    /// Obtains the list of channel members ready for the EVE Client
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public Rowset GetChannelMembers (int channelID, int characterID)
    {
        // TODO: SEEMS THAT CHANNELS ARE USED FOR ADDRESSBOOK TOO?! WTF CCP?! TAKE THAT INTO ACCOUNT
        if (channelID == characterID)
            return Database.PrepareRowsetQuery (
                "SELECT accessor AS charID, corporationID AS corpID, allianceID, 0 AS warFactionID, account.role AS role, 0 AS extra FROM lscChannelPermissions LEFT JOIN chrInformation ON accessor = characterID LEFT JOIN corporation USING(corporationID) LEFT JOIN account ON account.accountID = chrInformation.accountID WHERE channelID = @channelID AND accessor != @characterID",
                new Dictionary <string, object>
                {
                    {"@channelID", channelID},
                    {"@characterID", characterID}
                }
            );

        return Database.PrepareRowsetQuery (
            "SELECT accessor AS charID, corporationID AS corpID, allianceID, 0 AS warFactionID, account.role AS role, 0 AS extra FROM lscChannelPermissions LEFT JOIN chrInformation ON accessor = characterID LEFT JOIN corporation USING(corporationID) LEFT JOIN account ON account.accountID = chrInformation.accountID WHERE channelID = @channelID AND account.online = 1",
            new Dictionary <string, object> {{"@channelID", channelID}}
        );
    }

    /// <summary>
    /// Obtains the list of channel mods ready for the EVE Client
    /// </summary>
    /// <param name="channelID"></param>
    /// <returns></returns>
    public Rowset GetChannelMods (int channelID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT accessor, `mode`, untilWhen, originalMode, admin, reason FROM lscChannelPermissions WHERE channelID = @channelID",
            new Dictionary <string, object> {{"@channelID", channelID}}
        );
    }

    /// <summary>
    /// Obtains extra information for the given <paramref name="characterID"/> for the chat system
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public Row GetExtraInfo (int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT itemID AS ownerID, itemName AS ownerName, typeID FROM eveNames WHERE itemID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return Row.FromDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Lists the online characters in the given channel
    /// </summary>
    /// <param name="channelID"></param>
    /// <returns></returns>
    public PyList <PyInteger> GetOnlineCharsOnChannel (int channelID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT accessor FROM lscChannelPermissions LEFT JOIN chrInformation ON accessor = characterID WHERE channelID = @channelID AND online = 1 AND `mode` > 0",
            new Dictionary <string, object> {{"@channelID", channelID}}
        );

        using (connection)
        using (reader)
        {
            PyList <PyInteger> result = new PyList <PyInteger> ();

            while (reader.Read ())
                result.Add (reader.GetInt32 (0));

            return result;
        }
    }

    /// <summary>
    /// Checks whether the <paramref name="characterID"/> is allowed to chat on the given <paramref name="channelID"/>
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public bool IsPlayerAllowedToChat (int channelID, int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT `mode` FROM lscChannelPermissions WHERE channelID = @channelID AND accessor = @characterID",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@characterID", characterID}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return false;

            return (reader.GetInt32 (0) & CHATROLE_SPEAKER) == CHATROLE_SPEAKER;
        }
    }

    /// <summary>
    /// Checks whether the <paramref name="characterID"/> is allowed to read the given <paramref name="channelID"/>
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public bool IsPlayerAllowedToRead (int channelID, int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT `mode` FROM lscChannelPermissions WHERE channelID = @channelID AND accessor = @characterID",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@characterID", characterID}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return false;

            return (reader.GetInt32 (0) & CHATROLE_LISTENER) == CHATROLE_LISTENER;
        }
    }

    /// <summary>
    /// Checks whether the <paramref name="characterID"/> is allowed to chat on the given <paramref name="relatedEntityID"/>
    /// </summary>
    /// <param name="relatedEntityID"></param>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public bool IsPlayerAllowedToChatOnRelatedEntity (int relatedEntityID, int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT `mode` FROM lscChannelPermissions, lscGeneralChannels WHERE lscGeneralChannels.channelID = lscChannelPermissions.channelID AND lscGeneralChannels.relatedEntityID = @relatedEntityID AND accessor = @characterID",
            new Dictionary <string, object>
            {
                {"@relatedEntityID", relatedEntityID},
                {"@characterID", characterID}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return false;

            return (reader.GetInt32 (0) & CHATROLE_SPEAKER) == CHATROLE_SPEAKER;
        }
    }

    /// <summary>
    /// Obtains the channelID based on the <paramref name="relatedEntityID"/>
    /// </summary>
    /// <param name="relatedEntityID"></param>
    /// <param name="isMailingList">Whether we're looking for a normal chat or a mailing list</param>
    /// <returns></returns>
    public int GetChannelIDFromRelatedEntity (int relatedEntityID, bool isMailingList = false)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT channelID FROM lscGeneralChannels WHERE relatedEntityID = @itemID AND mailingList = @mailingList",
            new Dictionary <string, object>
            {
                {"@itemID", relatedEntityID},
                {"@mailingList", isMailingList}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt32 (0);
        }
    }

    /// <summary>
    /// Obtains the type of channel
    /// </summary>
    /// <param name="channelID"></param>
    /// <returns></returns>
    public string GetChannelType (int channelID)
    {
        if (channelID < 0)
            return CHANNEL_TYPE_NORMAL;

        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT displayName FROM lscGeneralChannels WHERE channelID = @channelID",
            new Dictionary <string, object> {{"@channelID", channelID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return CHANNEL_TYPE_NORMAL;

            return this.ChannelNameToChannelType (reader.GetString (0));
        }
    }

    /// <summary>
    /// Obtains the channel name
    /// </summary>
    /// <param name="channelID"></param>
    /// <returns></returns>
    public string GetChannelName (int channelID)
    {
        string query;

        if (channelID < 0)
            query = "SELECT displayName FROM lscPrivateChannels WHERE channelID = @channelID";
        else
            query = "SELECT displayName FROM lscGeneralChannels WHERE channelID = @channelID";

        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection, query,
            new Dictionary <string, object> {{"@channelID", channelID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return "";

            return reader.GetString (0);
        }
    }

    /// <summary>
    /// Converts a standard channel name into a channel type
    /// </summary>
    /// <param name="channelName"></param>
    /// <returns></returns>
    public string ChannelNameToChannelType (string channelName)
    {
        if (channelName == "System Channels\\Corp")
            return CHANNEL_TYPE_CORPID;
        if (channelName == "System Channels\\Region")
            return CHANNEL_TYPE_REGIONID;
        if (channelName == "System Channels\\Constellation")
            return CHANNEL_TYPE_CONSTELLATIONID;
        if (channelName == "System Channels\\Local")
            return CHANNEL_TYPE_SOLARSYSTEMID2;
        if (channelName == "System Channels\\Alliance")
            return CHANNEL_TYPE_ALLIANCEID;
        if (channelName == "System Channels\\Gang")
            return "gangid";
        if (channelName == "System Channels\\Squad")
            return "squadid";
        if (channelName == "System Channels\\Wing")
            return "wingid";
        if (channelName == "System Channels\\War Faction")
            return CHANNEL_TYPE_WARFACTIONID;
        if (channelName == "System Channels\\Global")
            return CHANNEL_TYPE_GLOBAL;

        return CHANNEL_TYPE_NORMAL;
    }

    /// <summary>
    /// Checks whether the given <paramref name="characterID"/> is member of the given <paramref name="channelID"/>
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public bool IsCharacterMemberOfChannel (int channelID, int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT 0 AS extra FROM lscChannelPermissions WHERE channelID = @channelID AND accessor = @characterID",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@characterID", characterID}
            }
        );

        using (connection)
        using (reader)
        {
            return reader.Read ();
        }
    }

    /// <summary>
    /// Checks whether the given <paramref name="characterID"/> is admin of the given <paramref name="channelID"/>
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public bool IsCharacterAdminOfChannel (int channelID, int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT `mode` FROM lscChannelPermissions WHERE channelID = @channelID AND accessor = @characterID",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@characterID", characterID}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return false;

            return (reader.GetInt32 (0) & CHATROLE_CREATOR) == CHATROLE_CREATOR;
        }
    }

    /// <summary>
    /// Checks whether the given <paramref name="characterID"/> is admin or operator of the given <paramref name="channelID"/>
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public bool IsCharacterOperatorOrAdminOfChannel (int channelID, int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT `mode` FROM lscChannelPermissions WHERE channelID = @channelID AND accessor = @characterID",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@characterID", characterID}
            }
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return false;

            return (reader.GetInt32 (0) & (CHATROLE_CREATOR | CHATROLE_OPERATOR)) > 0;
        }
    }

    /// <summary>
    /// Updates the permissions for the given <paramref name="characterID"/> on the <paramref name="channelID"/>
    /// </summary>
    /// <param name="channelID"></param>
    /// <param name="characterID"></param>
    /// <param name="permissions">The new permissions for that character</param>
    public void UpdatePermissionsForCharacterOnChannel (int channelID, int characterID, sbyte permissions)
    {
        Database.PrepareQuery (
            "REPLACE INTO lscChannelPermissions(channelID, accessor, mode, untilWhen, originalMode, admin, reason)VALUES(@channelID, @characterID, @mode, NULL, @mode, @admin, '')",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@characterID", characterID},
                {"@mode", permissions},
                {"@admin", permissions == CHATROLE_OPERATOR}
            }
        );
    }
}