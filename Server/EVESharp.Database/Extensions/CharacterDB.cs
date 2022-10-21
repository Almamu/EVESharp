using System;
using System.Collections.Generic;
using System.Data;
using EVESharp.Database.Types;
using EVESharp.Types;

namespace EVESharp.Database.Extensions;

public static class CharacterDB
{
    /// <summary>
    /// Sets when the character's stasis timer started (if any)
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public static void ChrSetStasisTimer (this IDatabase Database, int characterID, long? start)
    {
        Database.Prepare (
            "UPDATE chrInformation SET corpStasisTime = @timerStart WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@timerStart", start}
            }
        );
    }

    /// <summary>
    /// Get's when the character's stasis timer started (if any)
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public static long? ChrGetStasisTimer (this IDatabase Database, int characterID)
    {
        IDataReader reader = Database.Select (
            "SELECT corpStasisTime FROM chrInformation WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (reader)
        {
            if (reader.Read () == false || reader.IsDBNull (0))
                return null;

            return reader.GetInt64 (0);
        }
    }

    /// <summary>
    /// Obtains public information for a given character
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public static PyDataType ChrGetPublicInfo (this IDatabase Database, int characterID)
    {
        return Database.KeyVal (
            "ChrGetPublicInfo",
            new Dictionary <string, object> ()
            {
                {"_characterID", characterID}
            }
        );
    }

    /// <summary>
    /// Similar to <seealso cref="ChrGetPublicInfo"/> obtains basic information of a character
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public static Rowset ChrGetPublicInfo3 (this IDatabase Database, int characterID)
    {
        return Database.Rowset (
            "ChrGetPublicInfo3",
            new Dictionary <string, object> ()
            {
                {"_characterID", characterID}
            }
        );
    }

    /// <summary>
    /// Obtains the character name of the given <paramref name="characterID"/>
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public static string ChrGetName (this IDatabase Database, int characterID)
    {
        IDataReader reader = Database.Select (
            "SELECT itemName FROM eveNames WHERE itemID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (reader)
        {
            if (reader.Read () == false)
                return "";

            return reader.GetString (0);
        }
    }

    /// <summary>
    /// Updates the note written by <paramref name="ownerID"/> for the given <paramref name="itemID"/>
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="ownerID"></param>
    /// <param name="note">The note's text</param>
    public static void ChrSetNote (this IDatabase Database, int itemID, int ownerID, string note)
    {
        // remove the note if no text is present
        if (note.Length == 0)
            Database.Prepare (
                "DELETE FROM chrNotes WHERE itemID = @itemID AND ownerID = @ownerID",
                new Dictionary <string, object>
                {
                    {"@itemID", itemID},
                    {"@ownerID", ownerID}
                }
            );
        else
            Database.Prepare (
                "REPLACE INTO chrNotes (itemID, ownerID, note)VALUES(@itemID, @ownerID, @note)",
                new Dictionary <string, object>
                {
                    {"@itemID", itemID},
                    {"@ownerID", ownerID},
                    {"@note", note}
                }
            );
    }

    /// <summary>
    /// Gets the note written by <paramref name="ownerID"/> for the given <paramref name="itemID"/>
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="ownerID"></param>
    /// <returns></returns>
    public static string ChrGetNote (this IDatabase Database, int itemID, int ownerID)
    {
        IDataReader reader = Database.Select (
            "SELECT note FROM chrNotes WHERE itemID = @itemID AND ownerID = @ownerID",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@ownerID", ownerID}
            }
        );

        using (reader)
        {
            // if no record exists, return an empty string so the player can create it's own
            if (reader.Read () == false)
                return "";

            return reader.GetString (0);
        }
    }

    /// <summary>
    /// Updates the given <paramref name="characterID"/> login date to the current time
    /// </summary>
    /// <param name="characterID"></param>
    public static void ChrUpdateLogonTime (this IDatabase Database, int characterID)
    {
        Database.Prepare (
            "UPDATE chrInformation SET logonDateTime = @date, online = 1 WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@date", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    /// <summary>
    /// Returns the amount of online players currently in the cluster
    /// </summary>
    /// <returns></returns>
    public static long ChrGetOnlineCount (this IDatabase Database)
    {
        return Database.Scalar <long> ("ChrGetOnlineCount");
    }

    public static void ChrClearLoginStatus (this IDatabase Database)
    {
        Database.QueryProcedure ("ChrClearLoginStatus");
    }
}