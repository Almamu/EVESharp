using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.EVE.Data.Chat;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Database;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Old;

public class ChatDB : DatabaseAccessor
{
    public const int MIN_CHANNEL_ENTITY_ID = 1000;
    public const int MAX_CHANNEL_ENTITY_ID = 2100000000;

    public const int CHANNEL_ROOKIECHANNELID = 1;

    public ChatDB (IDatabaseConnection db) : base (db) { }

    /// <summary>
    /// Grants access to the standard channels to the given player
    /// </summary>
    /// <param name="characterID"></param>
    public void GrantAccessToStandardChannels (int characterID)
    {
        this.Database.Prepare (
            $"INSERT INTO lscChannelPermissions(channelID, accessor, mode) SELECT channelID, @characterID AS accessor, @mode AS `mode` FROM lscGeneralChannels WHERE channelID < {MIN_CHANNEL_ENTITY_ID}",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@mode", Roles.CONVERSATIONALIST}
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
            return -(long) this.Database.PrepareLID (
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
            this.Database.Prepare (
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

        return (long) this.Database.PrepareLID (
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
    public void JoinEntityMailingList (int relatedEntityID, int characterID, int role = Roles.CONVERSATIONALIST)
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
    public void JoinEntityChannel (int relatedEntityID, int characterID, int role = Roles.CONVERSATIONALIST)
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
    public void JoinChannel (int channelID, int characterID, int role = Roles.CONVERSATIONALIST)
    {
        this.Database.Prepare (
            "REPLACE INTO lscChannelPermissions(channelID, accessor, `mode`, untilWhen, originalMode, admin, reason)VALUES(@channelID, @characterID, @role, NULL, @role, @admin, '')",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@characterID", characterID},
                {"@role", role},
                {"@admin", role == Roles.CREATOR}
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
            this.Database.Prepare (
                "DELETE FROM lscPrivateChannels WHERE channelID = @channelID",
                new Dictionary <string, object> {{"@channelID", -channelID}}
            );
        else
            this.Database.Prepare (
                "DELETE FROM lscGeneralChannels WHERE channelID = @channelID",
                new Dictionary <string, object> {{"@channelID", channelID}}
            );

        this.Database.Prepare (
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
        this.Database.Prepare (
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
        Rowset firstQuery = this.Database.PrepareRowset (
            "SELECT" +
            " lscChannelPermissions.channelID, ownerID, displayName, motd, comparisonKey, memberless, !ISNULL(password) AS password," +
            " mailingList, cspa, temporary, 1 AS subscribed, estimatedMemberCount " +
            " FROM lscPrivateChannels" +
            " LEFT JOIN lscChannelPermissions ON lscPrivateChannels.channelID = -lscChannelPermissions.channelID" +
            " WHERE accessor = @characterID AND `mode` > 0 AND lscChannelPermissions.channelID < 0 ",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        Rowset secondQuery = this.Database.PrepareRowset (
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

        DbDataReader reader = this.Database.Select (
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

            return reader.Row();
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

        DbDataReader reader = this.Database.Select (
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

            return reader.Row();
        }
    }

    /// <summary>
    /// Obtains the list of members of the character's address book
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public CRowset GetAddressBookMembers (int characterID)
    {
        return this.Database.PrepareCRowset (
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
            return this.Database.PrepareRowset (
                "SELECT accessor AS charID, corporationID AS corpID, allianceID, 0 AS warFactionID, account.role AS role, 0 AS extra FROM lscChannelPermissions LEFT JOIN chrInformation ON accessor = characterID LEFT JOIN corporation USING(corporationID) LEFT JOIN account ON account.accountID = chrInformation.accountID WHERE channelID = @channelID AND accessor != @characterID",
                new Dictionary <string, object>
                {
                    {"@channelID", channelID},
                    {"@characterID", characterID}
                }
            );

        return this.Database.PrepareRowset (
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
        return this.Database.PrepareRowset (
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

        DbDataReader reader = this.Database.Select (
            ref connection,
            "SELECT itemID AS ownerID, itemName AS ownerName, typeID FROM eveNames WHERE itemID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return reader.Row();
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

        DbDataReader reader = this.Database.Select (
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

        DbDataReader reader = this.Database.Select (
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

            return (reader.GetInt32 (0) & Roles.SPEAKER) == Roles.SPEAKER;
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

        DbDataReader reader = this.Database.Select (
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

            return (reader.GetInt32 (0) & Roles.LISTENER) == Roles.LISTENER;
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

        DbDataReader reader = this.Database.Select (
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

            return (reader.GetInt32 (0) & Roles.SPEAKER) == Roles.SPEAKER;
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

        DbDataReader reader = this.Database.Select (
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
            return ChannelType.NORMAL;

        IDbConnection connection = null;

        DbDataReader reader = this.Database.Select (
            ref connection,
            "SELECT displayName FROM lscGeneralChannels WHERE channelID = @channelID",
            new Dictionary <string, object> {{"@channelID", channelID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return ChannelType.NORMAL;

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

        DbDataReader reader = this.Database.Select (
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
        return channelName switch
        {
            "System Channels\\Corp"          => ChannelType.CORPID,
            "System Channels\\Region"        => ChannelType.REGIONID,
            "System Channels\\Constellation" => ChannelType.CONSTELLATIONID,
            "System Channels\\Local"         => ChannelType.SOLARSYSTEMID2,
            "System Channels\\Alliance"      => ChannelType.ALLIANCEID,
            "System Channels\\Gang"          => ChannelType.GANGID,
            "System Channels\\Squad"         => ChannelType.SQUADID,
            "System Channels\\Wing"          => ChannelType.WINGID,
            "System Channels\\War Faction"   => ChannelType.WARFACTIONID,
            "System Channels\\Global"        => ChannelType.GLOBAL,
            _                                => ChannelType.NORMAL
        };
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

        DbDataReader reader = this.Database.Select (
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

        DbDataReader reader = this.Database.Select (
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

            return (reader.GetInt32 (0) & Roles.CREATOR) == Roles.CREATOR;
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

        DbDataReader reader = this.Database.Select (
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

            return (reader.GetInt32 (0) & (Roles.CREATOR | Roles.OPERATOR)) > 0;
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
        this.Database.Prepare (
            "REPLACE INTO lscChannelPermissions(channelID, accessor, mode, untilWhen, originalMode, admin, reason)VALUES(@channelID, @characterID, @mode, NULL, @mode, @admin, '')",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@characterID", characterID},
                {"@mode", permissions},
                {"@admin", permissions == Roles.OPERATOR}
            }
        );
    }
}