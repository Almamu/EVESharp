using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.EVE.Data.Inventory;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Database.Inventory;

public static class CharacterInformationDB
{
    /// <summary>
    /// Obtains the information of all the bloodlines
    /// </summary>
    /// <returns></returns>
    public static Dictionary <int, Bloodline> ChrLoadBloodlines (this IDatabaseConnection Database, ITypes types)
    {
        Dictionary <int, Bloodline> result     = new Dictionary <int, Bloodline> ();
        IDbConnection               connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT " +
            " bloodlineTypes.bloodlineID, typeID, bloodlineName, raceID, description, maleDescription, " +
            " femaleDescription, shipTypeID, corporationID, perception, willpower, charisma, memory, " +
            " intelligence, graphicID, shortDescription, shortMaleDescription, shortFemaleDescription " +
            " FROM bloodlineTypes, chrBloodlines " +
            " WHERE chrBloodlines.bloodlineID = bloodlineTypes.bloodlineID"
        );

        using (connection)
        using (reader)
        {
            while (reader.Read ())
            {
                Bloodline bloodline = new Bloodline (
                    reader.GetInt32 (0),
                    types [reader.GetInt32 (1)],
                    reader.GetString (2),
                    reader.GetInt32 (3),
                    reader.GetString (4),
                    reader.GetString (5),
                    reader.GetString (6),
                    types [reader.GetInt32 (7)],
                    reader.GetInt32 (8),
                    reader.GetInt32 (9),
                    reader.GetInt32 (10),
                    reader.GetInt32 (11),
                    reader.GetInt32 (12),
                    reader.GetInt32 (13),
                    reader.GetInt32OrDefault (14),
                    reader.GetString (15),
                    reader.GetString (16),
                    reader.GetString (17)
                );

                result [bloodline.ID] = bloodline;
            }
        }

        return result;
    }

    /// <summary>
    /// Obtains the information of all the ancestries
    /// </summary>
    /// <param name="bloodlines">Loaded bloodlines used to store the ancestry information into</param>
    /// <returns></returns>
    public static Dictionary <int, Ancestry> ChrLoadAncestries (this IDatabaseConnection Database, IBloodlines bloodlines)
    {
        Dictionary <int, Ancestry> result     = new Dictionary <int, Ancestry> ();
        IDbConnection              connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT " +
            " ancestryID, ancestryName, bloodlineID, description, perception, willpower, charisma," +
            " memory, intelligence, graphicID, shortDescription " +
            " FROM chrAncestries "
        );

        using (connection)
        using (reader)
        {
            while (reader.Read ())
            {
                Ancestry ancestry = new Ancestry (
                    reader.GetInt32 (0),
                    reader.GetString (1),
                    bloodlines [reader.GetInt32 (2)],
                    reader.GetString (3),
                    reader.GetInt32 (4),
                    reader.GetInt32 (5),
                    reader.GetInt32 (6),
                    reader.GetInt32 (7),
                    reader.GetInt32 (8),
                    reader.GetInt32OrDefault (9),
                    reader.GetString (10)
                );

                result [ancestry.ID] = ancestry;
            }
        }

        return result;
    }
}