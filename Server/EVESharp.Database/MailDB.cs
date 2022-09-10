using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database;

public static class MailDB
{
    public static Rowset EveMailGetHeaders (this IDatabaseConnection Database, int characterID)
    {
        return Database.PrepareRowset (
            "SELECT channelID, messageID, senderID, subject, created, `read` FROM eveMail LEFT JOIN lscChannelPermissions USING(channelID) WHERE accessor = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    public static PyTuple EveMailGetMessages (this IDatabaseConnection Database, int channelID, int messageID)
    {
        // TODO: SIMPLIFY TABLE STRUCTURE, ATTACHMENTS ARE NOT SUPPORTED
        IDataReader reader = Database.Select (
            "SELECT channelID, messageID, senderID, subject, body, mimeTypeID, mimeType, `binary`, created FROM eveMail LEFT JOIN eveMailMimeType USING(mimeTypeID) WHERE messageID = @messageID AND channelID = @channelID",
            new Dictionary <string, object>
            {
                {"@messageID", messageID},
                {"@channelID", channelID}
            }
        );

        using (reader)
        {
            if (reader.Read () == false)
                return new PyTuple (0);

            Row mimeTypeRow = new Row (
                new PyList <PyString> (3)
                {
                    [0] = "mimeTypeID",
                    [1] = "mimeType",
                    [2] = "binary"
                },
                new PyList (3)
                {
                    [0] = reader.GetInt32 (5),
                    [1] = reader.GetString (6),
                    [2] = reader.GetInt32 (7)
                }
            );

            Row entryRow = new Row (
                new PyList <PyString> (8)
                {
                    [0] = "channelID",
                    [1] = "messageID",
                    [2] = "senderID",
                    [3] = "subject",
                    [4] = "body",
                    [5] = "created",
                    [6] = "mimeType",
                    [7] = "deleted"
                },
                new PyList (8)
                {
                    [0] = reader.GetInt32 (0),
                    [1] = reader.GetInt32 (1),
                    [2] = reader.GetInt32 (2),
                    [3] = reader.GetString (3),
                    [4] = reader.GetString (4),
                    [5] = reader.GetInt64 (8),
                    [6] = mimeTypeRow,
                    [7] = false
                }
            );

            return new PyTuple (2)
            {
                [0] = entryRow,
                [1] = new PyList () // attachments are not really supported by the client
            };
        }
    }

    public static void EveMailMarkMessagesRead (this IDatabaseConnection Database, int mailboxID, PyList <PyInteger> messageIDs)
    {
        Database.Query (
            $"UPDATE eveMail SET `read` = 1 WHERE messageID IN ({PyString.Join (',', messageIDs)}) AND channelID = @mailboxID",
            new Dictionary <string, object> ()
            {
                {"@mailboxID", mailboxID}
            }
        );
    }

    public static void EveMailDeleteMessages (this IDatabaseConnection Database, int mailboxID, PyList <PyInteger> messageIDs)
    {
        // TODO: CHECK PERMISSIONS
        Database.Prepare (
            $"DELETE FROM eveMail WHERE messageID IN ({PyString.Join (',', messageIDs)} AND channelID = @mailboxID",
            new Dictionary <string, object> {{"@mailboxID", mailboxID}}
        );
    }

    public static ulong EveMailStore (this IDatabaseConnection Database, int channelID, int senderID, string subject, string message, out string mailboxType)
    {
        ulong messageID = Database.PrepareLID (
            "INSERT INTO eveMail (channelID, senderID, subject, body, mimeTypeID, created, `read`)VALUES(@channelID, @senderID, @subject, @body, 2, @created, 0)",
            new Dictionary <string, object>
            {
                {"@channelID", channelID},
                {"@senderID", senderID},
                {"@subject", subject},
                {"@body", message},
                {"@created", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );

        mailboxType = "";

        IDataReader reader = Database.Select (
            "SELECT groupID FROM invItems LEFT JOIN invTypes USING(typeID) WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", channelID}}
        );

        using (reader)
        {
            // bail out if no data was found with some default garbage value
            if (reader.Read ())
            {
                mailboxType = reader.GetInt32 (0) switch
                {
                    (int) GroupID.Character   => "charid",
                    (int) GroupID.Corporation => "corpid",
                    _                         => mailboxType
                };
            }
            else
                mailboxType = "charid";
        }

        return messageID;
    }
}