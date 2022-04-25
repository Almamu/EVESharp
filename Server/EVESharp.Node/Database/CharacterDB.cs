using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.EVE.StaticData;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using MySql.Data.MySqlClient;
using Type = EVESharp.EVE.StaticData.Inventory.Type;

namespace EVESharp.Node.Database;

public class CharacterDB : DatabaseAccessor
{
    private ItemDB      ItemDB      { get; }
    private TypeManager TypeManager { get; }

    public CharacterDB (IDatabaseConnection db, ItemDB itemDB, TypeManager typeManager) : base (db)
    {
        TypeManager = typeManager;
        ItemDB      = itemDB;
    }

    /// <summary>
    /// Returns the list of characters the given account has ready to be sent to the EVE Client
    /// </summary>
    /// <param name="accountID"></param>
    /// <returns></returns>
    public Rowset GetCharacterList (int accountID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT" +
            " characterID, itemName AS characterName, 0 as deletePrepareDateTime," +
            " gender, accessoryID, beardID, costumeID, decoID, eyebrowsID, eyesID, hairID," +
            " lipstickID, makeupID, skinID, backgroundID, lightID," +
            " headRotation1, headRotation2, headRotation3, eyeRotation1," +
            " eyeRotation2, eyeRotation3, camPos1, camPos2, camPos3," +
            " morph1e, morph1n, morph1s, morph1w, morph2e, morph2n," +
            " morph2s, morph2w, morph3e, morph3n, morph3s, morph3w," +
            " morph4e, morph4n, morph4s, morph4w" +
            " FROM chrInformation " +
            "	LEFT JOIN eveNames ON characterID = itemID" +
            " WHERE accountID = @accountID",
            new Dictionary <string, object> {{"@accountID", accountID}}
        );

        using (connection)
        using (reader)
        {
            return Rowset.FromDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Gets information for the character selection ready to be sent to the EVE Client
    /// </summary>
    /// <param name="characterID"></param>
    /// <param name="accountID"></param>
    /// <returns></returns>
    public Rowset GetCharacterSelectionInfo (int characterID, int accountID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT " +
            " itemName AS shortName,bloodlineID,gender,bounty,chrInformation.corporationID,allianceID,title,startDateTime,createDateTime," +
            " securityRating,IF(balance IS NULL, 0, balance) AS balance,chrInformation.stationID,solarSystemID,constellationID,regionID," +
            " petitionMessage,logonMinutes,tickerName, corporation.startDate AS allianceMemberStartDate" +
            " FROM chrInformation " +
            "	LEFT JOIN eveNames ON characterID = itemID" +
            "	LEFT JOIN corporation USING (corporationID)" +
            "	LEFT JOIN bloodlineTypes USING (typeID)" +
            "   LEFT JOIN mktWallets ON characterID = ownerID" +
            " WHERE characterID=@characterID AND accountID = @accountID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@accountID", accountID}
            }
        );

        using (connection)
        using (reader)
        {
            return Rowset.FromDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Obtains public information for a given character
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public PyDataType GetPublicInfo (int characterID)
    {
        return Database.PrepareKeyValQuery (
            "SELECT chrInformation.corporationID, raceID, bloodlineID, ancestryID, careerID," +
            " schoolID, careerSpecialityID, createDateTime, gender " +
            "FROM chrInformation " +
            "LEFT JOIN chrAncestries USING (ancestryID) " +
            "LEFT JOIN chrBloodlines USING (bloodlineID) " +
            "WHERE characterID=@characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    /// <summary>
    /// Similar to <seealso cref="GetPublicInfo"/> obtains basic information of a character
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public Rowset GetPublicInfo3 (int characterID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT bounty, title, startDateTime, description, corporationID FROM chrInformation WHERE characterID=@characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    /// <summary>
    /// Checks if the given character name is taken by anyone already
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns></returns>
    public bool IsCharacterNameTaken (string characterName)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT COUNT(*) FROM eveNames WHERE groupID = 1 AND itemName LIKE @characterName",
            new Dictionary <string, object> {{"@characterName", characterName}}
        );

        using (connection)
        using (reader)
        {
            reader.Read ();

            return reader.GetInt32 (0) > 0;
        }
    }

    /// <summary>
    /// Obtains the information of all the bloodlines
    /// </summary>
    /// <returns></returns>
    public Dictionary <int, Bloodline> GetBloodlineInformation ()
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
                    TypeManager [reader.GetInt32 (1)],
                    reader.GetString (2),
                    reader.GetInt32 (3),
                    reader.GetString (4),
                    reader.GetString (5),
                    reader.GetString (6),
                    TypeManager [reader.GetInt32 (7)],
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
    public Dictionary <int, Ancestry> GetAncestryInformation (Bloodlines bloodlines)
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

    /// <summary>
    /// Creates a new character record in the database
    /// </summary>
    /// <param name="from">The item type the character is</param>
    /// <param name="name">The character's name</param>
    /// <param name="owner">The item owner (usually EVESystem)</param>
    /// <param name="accountID">The account this character belongs to</param>
    /// <param name="securityRating">The default security rating</param>
    /// <param name="corporationID">The default corporation</param>
    /// <param name="corpRole">The role inside the corporation</param>
    /// <param name="rolesAtBase">Roles at base in the corporation</param>
    /// <param name="rolesAtHQ">Roles at HQ in the corporation</param>
    /// <param name="rolesAtOther">Roles at other in the corporation</param>
    /// <param name="corporationDateTime">The time the character joined the corporation</param>
    /// <param name="startDateTime">The time the character started</param>
    /// <param name="createDateTime">The time the character was created</param>
    /// <param name="ancestryID">The ancestry of the character</param>
    /// <param name="careerID">The career of the character</param>
    /// <param name="schoolID">The school of the character</param>
    /// <param name="careerSpecialityID">The speciality of the character</param>
    /// <param name="gender">The gender of the character</param>
    /// <param name="accessoryID"></param>
    /// <param name="beardID"></param>
    /// <param name="costumeID"></param>
    /// <param name="decoID"></param>
    /// <param name="eyebrowsID"></param>
    /// <param name="eyesID"></param>
    /// <param name="hairID"></param>
    /// <param name="lipstickID"></param>
    /// <param name="makeupID"></param>
    /// <param name="skinID"></param>
    /// <param name="backgroundID"></param>
    /// <param name="lightID"></param>
    /// <param name="headRotation1"></param>
    /// <param name="headRotation2"></param>
    /// <param name="headRotation3"></param>
    /// <param name="eyeRotation1"></param>
    /// <param name="eyeRotation2"></param>
    /// <param name="eyeRotation3"></param>
    /// <param name="camPos1"></param>
    /// <param name="camPos2"></param>
    /// <param name="camPos3"></param>
    /// <param name="morph1E"></param>
    /// <param name="morph1N"></param>
    /// <param name="morph1S"></param>
    /// <param name="morph1W"></param>
    /// <param name="morph2E"></param>
    /// <param name="morph2N"></param>
    /// <param name="morph2S"></param>
    /// <param name="morph2W"></param>
    /// <param name="morph3E"></param>
    /// <param name="morph3N"></param>
    /// <param name="morph3S"></param>
    /// <param name="morph3W"></param>
    /// <param name="morph4E"></param>
    /// <param name="morph4N"></param>
    /// <param name="morph4S"></param>
    /// <param name="morph4W"></param>
    /// <param name="stationID">At what station the character is at right now</param>
    /// <param name="solarSystemID">At what solar system the character is at right now</param>
    /// <param name="constellationID">At what constellation the character is at right now</param>
    /// <param name="regionID">At what region the character is at right now</param>
    /// <returns></returns>
    public int CreateCharacter (
        Type    from,                string  name,          ItemEntity owner,          int     accountID,     double  securityRating,
        int     corporationID,       int     corpRole,      int        rolesAtBase,    int     rolesAtHQ,     int     rolesAtOther,
        long    corporationDateTime, long    startDateTime, long       createDateTime, int     ancestryID,    int     careerID,  int  schoolID,
        int     careerSpecialityID,  int     gender,        int?       accessoryID,    int?    beardID,       int     costumeID, int? decoID, int eyebrowsID,
        int     eyesID,              int     hairID,        int?       lipstickID,     int?    makeupID,      int     skinID,    int  backgroundID, int lightID,
        double  headRotation1,       double  headRotation2, double     headRotation3,  double  eyeRotation1,  double  eyeRotation2,
        double  eyeRotation3,        double  camPos1,       double     camPos2,        double  camPos3,       double? morph1E,         double? morph1N,
        double? morph1S,             double? morph1W,       double?    morph2E,        double? morph2N,       double? morph2S,         double? morph2W,
        double? morph3E,             double? morph3N,       double?    morph3S,        double? morph3W,       double? morph4E,         double? morph4N,
        double? morph4S,             double? morph4W,       int        stationID,      int     solarSystemID, int     constellationID, int     regionID
    )
    {
        // create the item first
        int itemID = (int) ItemDB.CreateItem (
            name, from.ID, owner.ID, stationID, Flags.Connected, false,
            true, 1, 0, 0, 0, ""
        );

        // now create the character record in the database
        Database.PrepareQuery (
            "INSERT INTO chrInformation(" +
            "characterID, accountID, title, description, bounty, securityRating, petitionMessage, " +
            "logonMinutes, corporationID, roles, rolesAtBase, rolesAtHQ, rolesAtOther, " +
            "corporationDateTime, startDateTime, createDateTime, ancestryID, careerID, schoolID, careerSpecialityID, " +
            "gender, accessoryID, beardID, costumeID, decoID, eyebrowsID, eyesID, hairID, lipstickID, makeupID, " +
            "skinID, backgroundID, lightID, headRotation1, headRotation2, headRotation3, eyeRotation1, " +
            "eyeRotation2, eyeRotation3, camPos1, camPos2, camPos3, morph1e, morph1n, morph1s, morph1w, " +
            "morph2e, morph2n, morph2s, morph2w, morph3e, morph3n, morph3s, morph3w, " +
            "morph4e, morph4n, morph4s, morph4w, stationID, solarSystemID, constellationID, regionID, online," +
            "logonDateTime, logoffDateTime" +
            ")VALUES(" +
            "@characterID, @accountID, @title, @description, @bounty, @securityRating, @petitionMessage, " +
            "@logonMinutes, @corporationID, @corpRole, @rolesAtBase, @rolesAtHQ, @rolesAtOther, " +
            "@corporationDateTime, @startDateTime, @createDateTime, @ancestryID, @careerID, @schoolID, @careerSpecialityID, " +
            "@gender, @accessoryID, @beardID, @costumeID, @decoID, @eyebrowsID, @eyesID, @hairID, @lipstickID, @makeupID, " +
            "@skinID, @backgroundID, @lightID, @headRotation1, @headRotation2, @headRotation3, @eyeRotation1, " +
            "@eyeRotation2, @eyeRotation3, @camPos1, @camPos2, @camPos3, @morph1e, @morph1n, @morph1s, @morph1w, " +
            "@morph2e, @morph2n, @morph2s, @morph2w, @morph3e, @morph3n, @morph3s, @morph3w, " +
            "@morph4e, @morph4n, @morph4s, @morph4w, @stationID, @solarSystemID, @constellationID, @regionID, @online, " +
            "@createDateTime, @createDateTime" +
            ")"
            ,
            new Dictionary <string, object>
            {
                {"@characterID", itemID},
                {"@accountID", accountID},
                {"@title", ""},
                {"@description", ""},
                {"@bounty", 0},
                {"@securityRating", securityRating},
                {"@petitionMessage", ""},
                {"@logonMinutes", 0},
                {"@corporationID", corporationID},
                {"@corpRole", corpRole},
                {"@rolesAtBase", rolesAtBase},
                {"@rolesAtHQ", rolesAtHQ},
                {"@rolesAtOther", rolesAtOther},
                {"@corporationDateTime", corporationDateTime},
                {"@startDateTime", startDateTime},
                {"@createDateTime", createDateTime},
                {"@ancestryID", ancestryID},
                {"@careerID", careerID},
                {"@schoolID", schoolID},
                {"@careerSpecialityID", careerSpecialityID},
                {"@gender", gender},
                {"@accessoryID", accessoryID},
                {"@beardID", beardID},
                {"@costumeID", costumeID},
                {"@decoID", decoID},
                {"@eyebrowsID", eyebrowsID},
                {"@eyesID", eyesID},
                {"@hairID", hairID},
                {"@lipstickID", lipstickID},
                {"@makeupID", makeupID},
                {"@skinID", skinID},
                {"@backgroundID", backgroundID},
                {"@lightID", lightID},
                {"@headRotation1", headRotation1},
                {"@headRotation2", headRotation2},
                {"@headRotation3", headRotation3},
                {"@eyeRotation1", eyeRotation1},
                {"@eyeRotation2", eyeRotation2},
                {"@eyeRotation3", eyeRotation3},
                {"@camPos1", camPos1},
                {"@camPos2", camPos2},
                {"@camPos3", camPos3},
                {"@morph1e", morph1E},
                {"@morph1n", morph1N},
                {"@morph1s", morph1S},
                {"@morph1w", morph1W},
                {"@morph2e", morph2E},
                {"@morph2n", morph2N},
                {"@morph2s", morph2S},
                {"@morph2w", morph2W},
                {"@morph3e", morph3E},
                {"@morph3n", morph3N},
                {"@morph3s", morph3S},
                {"@morph3w", morph3W},
                {"@morph4e", morph4E},
                {"@morph4n", morph4N},
                {"@morph4s", morph4S},
                {"@morph4w", morph4W},
                {"@stationID", stationID},
                {"@solarSystemID", solarSystemID},
                {"@constellationID", constellationID},
                {"@regionID", regionID},
                {"@online", false}
            }
        );

        this.CreateEmploymentRecord (itemID, corporationID, createDateTime);

        // return the character's item id
        return itemID;
    }

    /// <summary>
    /// Creates an employment record for a character
    /// </summary>
    /// <param name="itemID">The character</param>
    /// <param name="corporationID">The corporation</param>
    /// <param name="createDateTime">When the employment change happened</param>
    public void CreateEmploymentRecord (int itemID, int corporationID, long createDateTime)
    {
        // create employment record
        Database.PrepareQuery (
            "INSERT INTO chrEmployment(characterID, corporationID, startDate)VALUES(@characterID, @corporationID, @startDate)",
            new Dictionary <string, object>
            {
                {"@characterID", itemID},
                {"@corporationID", corporationID},
                {"@startDate", createDateTime}
            }
        );
    }

    /// <summary>
    /// Gets a random career for the given race
    /// </summary>
    /// <param name="raceID">The reace to get the career information for</param>
    /// <param name="careerID">The choosen career</param>
    /// <param name="schoolID">The choosen school</param>
    /// <param name="careerSpecialityID">The choosen speciality</param>
    /// <param name="corporationID">The choosen corporation</param>
    /// <returns>Whether the information was found or not</returns>
    public bool GetRandomCareerForRace (int raceID, out int careerID, out int schoolID, out int careerSpecialityID, out int corporationID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT careerID, corporationID, schoolID FROM chrSchools WHERE raceID = @raceID ORDER BY RAND();",
            new Dictionary <string, object> {{"@raceID", raceID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
            {
                // set some defaults just in case
                careerID           = 11;
                schoolID           = 17;
                careerSpecialityID = 11;
                corporationID      = 1000167;

                return false;
            }

            careerID           = reader.GetInt32 (0);
            corporationID      = reader.GetInt32 (1);
            schoolID           = reader.GetInt32 (2);
            careerSpecialityID = careerID;

            return true;
        }
    }

    /// <summary>
    /// Gets the default location for the given corporation
    /// </summary>
    /// <param name="corporationID"></param>
    /// <param name="stationID">The station where the corporation is</param>
    /// <param name="solarSystemID">The solar system where the corporation is</param>
    /// <param name="constellationID">The constellation where the corporation is</param>
    /// <param name="regionID">The region where the corporation is</param>
    /// <returns>Whether the information was found or not</returns>
    public bool GetLocationForCorporation (
        int     corporationID,   out int stationID, out int solarSystemID,
        out int constellationID, out int regionID
    )
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT staStations.stationID, solarSystemID, constellationID, regionID" +
            " FROM staStations, corporation" +
            " WHERE staStations.stationID = corporation.stationID AND corporation.corporationID = @corporationID",
            new Dictionary <string, object> {{"@corporationID", corporationID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
            {
                stationID       = 0;
                solarSystemID   = 0;
                constellationID = 0;
                regionID        = 0;

                return false;
            }

            stationID       = reader.GetInt32 (0);
            solarSystemID   = reader.GetInt32 (1);
            constellationID = reader.GetInt32 (2);
            regionID        = reader.GetInt32 (3);

            return true;
        }
    }

    /// <summary>
    /// Gets the default skills assigned to the given race
    /// </summary>
    /// <param name="raceID"></param>
    /// <returns>Dictionary of skillTypeID related to the skill level</returns>
    public Dictionary <int, int> GetBasicSkillsByRace (int raceID)
    {
        Dictionary <int, int> skills = new Dictionary <int, int> ();

        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT skillTypeID, levels FROM chrRaceSkills WHERE raceID = @raceID",
            new Dictionary <string, object> {{"@raceID", raceID}}
        );

        using (connection)
        using (reader)
        {
            while (reader.Read ())
                skills [reader.GetInt32 (0)] = reader.GetInt32 (1);
        }

        return skills;
    }

    /// <summary>
    /// Obtains the keymap list for the wallet, ready for the EVE Client
    /// </summary>
    /// <returns></returns>
    public Rowset GetKeyMap ()
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT accountKey as keyID, accountType as keyType, accountName as keyName, description FROM market_keyMap"
        );

        using (connection)
        using (reader)
        {
            return Rowset.FromDataReader (Database, reader);
        }
    }

    /// <summary>
    /// Loads the current skill queue for the given character
    /// </summary>
    /// <param name="character">Character to get the queue for</param>
    /// <param name="skillsInTraining">The list of skills that are being trained (based on the inventory)</param>
    /// <returns></returns>
    public List <Character.SkillQueueEntry> LoadSkillQueue (Character character, Dictionary <int, Skill> skillsInTraining)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT skillItemID, level FROM chrSkillQueue WHERE characterID = @characterID ORDER BY orderIndex",
            new Dictionary <string, object> {{"@characterID", character.ID}}
        );

        using (connection)
        using (reader)
        {
            List <Character.SkillQueueEntry> result = new List <Character.SkillQueueEntry> ();

            while (reader.Read ())
                result.Add (
                    new Character.SkillQueueEntry
                    {
                        Skill       = skillsInTraining [reader.GetInt32 (0)],
                        TargetLevel = reader.GetInt32 (1)
                    }
                );

            return result;
        }
    }

    /// <summary>
    /// Obtains the notes created by the given character
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public Rowset GetOwnerNoteLabels (int characterID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT noteID, label FROM chrOwnerNote WHERE ownerID = @ownerID",
            new Dictionary <string, object> {{"@ownerID", characterID}}
        );
    }

    /// <summary>
    /// Checks if the given <paramref name="characterID"/> is online or not
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public bool IsOnline (int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT online FROM chrInformation WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return false;

            return reader.GetBoolean (0);
        }
    }

    /// <summary>
    /// Returns the list of characters that are online in the <paramref name="character"/>'s friend list
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public PyList <PyInteger> GetOnlineFriendList (Character character)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT accessor AS characterID FROM lscChannelPermissions, chrInformation WHERE lscChannelPermissions.channelID = @characterID AND chrInformation.characterID = lscChannelPermissions.accessor and chrInformation.online = 1",
            new Dictionary <string, object> {{"@characterID", character.ID}}
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
    /// Updates the given <paramref name="characterID"/> login date to the current time
    /// </summary>
    /// <param name="characterID"></param>
    public void UpdateCharacterLogonDateTime (int characterID)
    {
        Database.PrepareQuery (
            "UPDATE chrInformation SET logonDateTime = @date, online = 1 WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@date", DateTime.UtcNow.ToFileTimeUtc ()}
            }
        );
    }

    /// <summary>
    /// Saves various aspects of a character like online status, activeCloneID, attribute remap status, clone jump, skill queue, description...
    /// </summary>
    /// <param name="character"></param>
    public void UpdateCharacterInformation (Character character)
    {
        Database.PrepareQuery (
            "UPDATE chrInformation SET activeCloneID = @activeCloneID, freeRespecs = @freeRespecs, nextRespecTime = @nextRespecTime, timeLastJump = @timeLastJump, description = @description, warFactionID = @warFactionID, corporationID = @corporationID, corporationDateTime = @corporationDateTime, corpAccountKey = @corpAccountKey WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", character.ID},
                {"@activeCloneID", character.ActiveCloneID},
                {"@freeRespecs", character.FreeReSpecs},
                {"@nextRespecTime", character.NextReSpecTime},
                {"@timeLastJump", character.TimeLastJump},
                {"@description", character.Description},
                {"@warFactionID", character.WarFactionID},
                {"@corporationID", character.CorporationID},
                {"@corporationDateTime", character.CorporationDateTime},
                {"@corpAccountKey", character.CorpAccountKey}
            }
        );

        if (character.ContentsLoaded)
        {
            // ensure the skill queue is saved too
            Database.PrepareQuery (
                "DELETE FROM chrSkillQueue WHERE characterID = @characterID",
                new Dictionary <string, object> {{"@characterID", character.ID}}
            );

            // re-create the whole skill queue
            IDbConnection connection = null;

            MySqlCommand create = (MySqlCommand) Database.PrepareQuery (
                ref connection,
                "INSERT INTO chrSkillQueue(orderIndex, characterID, skillItemID, level) VALUE (@orderIndex, @characterID, @skillItemID, @level)"
            );

            using (connection)
            using (create)
            {
                int index = 0;

                foreach (Character.SkillQueueEntry entry in character.SkillQueue)
                {
                    create.Parameters.Clear ();
                    create.Parameters.AddWithValue ("@orderIndex",  index++);
                    create.Parameters.AddWithValue ("@characterID", character.ID);
                    create.Parameters.AddWithValue ("@skillItemID", entry.Skill.ID);
                    create.Parameters.AddWithValue ("@level",       entry.TargetLevel);
                    create.ExecuteNonQuery ();
                }
            }
        }
    }

    /// <summary>
    /// Obtains the journal for the <paramref name="characterID"/> ready to be sent to the EVE Client
    /// </summary>
    /// <param name="characterID"></param>
    /// <param name="refTypeID">Filter by the given reference type (if not null)</param>
    /// <param name="accountKey">The account to get the journal for</param>
    /// <param name="maxDate">The maximum date to be fetched</param>
    /// <param name="startTransactionID">The record to start from (if any, used for pagination)</param>
    /// <returns></returns>
    public Rowset GetJournal (int characterID, int? refTypeID, int accountKey, long maxDate, int? startTransactionID)
    {
        // get the last 30 days of journal
        long minDate = DateTime.FromFileTimeUtc (maxDate).AddDays (-30).ToFileTimeUtc ();

        Dictionary <string, object> parameters = new Dictionary <string, object>
        {
            {"@characterID", characterID},
            {"@accountKey", accountKey},
            {"@maxDate", maxDate},
            {"@minDate", minDate}
        };

        string query =
            "SELECT transactionID, transactionDate, referenceID, entryTypeID," +
            " ownerID1, ownerID2, accountKey, amount, balance, description " +
            "FROM mktJournal " +
            "WHERE charID = @characterID AND accountKey=@accountKey AND transactionDate >= @minDate AND transactionDate <= @maxDate";

        if (refTypeID is not null)
        {
            query                       += " AND entryTypeID=@entryTypeID";
            parameters ["@entryTypeID"] =  refTypeID;
        }

        if (startTransactionID is not null)
        {
            query                              += " AND transactionID > @startTransactionID";
            parameters ["@startTransactionID"] =  startTransactionID;
        }

        query += " ORDER BY transactionID DESC";

        return Database.PrepareRowsetQuery (query, parameters);
    }

    /// <summary>
    /// Gets the history of ship kills and losses related to the given <paramref name="characterID"/>
    /// </summary>
    /// <param name="characterID"></param>
    /// <param name="count">The maximum amount of records to be returned</param>
    /// <param name="startIndex">The amount of records to ignore from the begining of the result</param>
    /// <returns></returns>
    public Rowset GetRecentShipKillsAndLosses (int characterID, int count, int? startIndex)
    {
        // TODO: WRITE A GENERATOR FOR THE KILL LOGS, THESE SEEM TO BE KIND OF AN XML FILE WITH ALL THE INFORMATION
        // TODO: FOR MORE INFORMATION CHECK CombatLog_CopyText ON eveCommonUtils.py
        return Database.PrepareRowsetQuery (
            "SELECT killID, solarSystemID, moonID, victimCharacterID, victimCorporationID, victimAllianceID, victimFactionID, victimShipTypeID, victimDamageTaken, finalCharacterID, finalCorporationID, finalAllianceID, finalFactionID, finalDamageDone, finalSecurityStatus, finalShipTypeID, finalWeaponTypeID, killTime, killBlob FROM chrCombatLogs WHERE victimCharacterID = @characterID OR finalCharacterID = @characterID LIMIT @startIndex, @limit",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@startIndex", startIndex ?? 0},
                {"@limit", count}
            }
        );
    }

    /// <summary>
    /// Gets the top 100 bounties on the server ready for the EVE Client
    /// </summary>
    /// <returns></returns>
    public Rowset GetTopBounties ()
    {
        // return the 100 topmost bounties
        return Database.PrepareRowsetQuery (
            "SELECT characterID, itemName AS ownerName, SUM(bounty) AS bounty, 0 AS online FROM chrBounties, eveNames WHERE eveNames.itemID = chrBounties.ownerID GROUP BY characterID ORDER BY bounty DESC LIMIT 100"
        );
    }

    /// <summary>
    /// Adds a new bounty to the <paramref name="characterID"/>
    /// </summary>
    /// <param name="characterID">The player to place the bounty on</param>
    /// <param name="ownerID">Who placed the bounty</param>
    /// <param name="bounty">The amount</param>
    public void AddToBounty (int characterID, int ownerID, double bounty)
    {
        // create bounty record
        Database.PrepareQuery (
            "INSERT INTO chrBounties(characterID, ownerID, bounty)VALUES(@characterID, @ownerID, @bounty)",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@ownerID", ownerID},
                {"@bounty", bounty}
            }
        );

        // add the bounty to the player
        Database.PrepareQuery (
            "UPDATE chrInformation SET bounty = bounty + @bounty WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@bounty", bounty},
                {"@characterID", characterID}
            }
        );
    }

    /// <summary>
    /// Fetches some private information for the given <paramref name="characterID"/>
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public PyDataType GetPrivateInfo (int characterID)
    {
        return Database.PrepareKeyValQuery (
            "SELECT gender, createDateTime, itemName AS charName, bloodlineName, raceName " +
            "FROM chrInformation " +
            "LEFT JOIN eveNames ON eveNames.itemID = chrInformation.characterID " +
            "LEFT JOIN chrAncestries USING (ancestryID) " +
            "LEFT JOIN chrBloodlines USING (bloodlineID) " +
            "LEFT JOIN chrRaces USING (raceID) " +
            "WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    /// <summary>
    /// Obtains information about the character's appearance ready for the EVE Client
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public Rowset GetCharacterAppearanceInfo (int characterID)
    {
        return Database.PrepareRowsetQuery (
            "SELECT accessoryID, beardID, costumeID, decoID, eyebrowsID, eyesID, hairID," +
            " lipstickID, makeupID, skinID, backgroundID, lightID," +
            " headRotation1, headRotation2, headRotation3, eyeRotation1," +
            " eyeRotation2, eyeRotation3, camPos1, camPos2, camPos3," +
            " morph1e, morph1n, morph1s, morph1w, morph2e, morph2n," +
            " morph2s, morph2w, morph3e, morph3n, morph3s, morph3w," +
            " morph4e, morph4n, morph4s, morph4w " +
            "FROM chrInformation WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );
    }

    /// <summary>
    /// Gets the note written by <paramref name="ownerID"/> for the given <paramref name="itemID"/>
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="ownerID"></param>
    /// <returns></returns>
    public string GetNote (int itemID, int ownerID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT note FROM chrNotes WHERE itemID = @itemID AND ownerID = @ownerID",
            new Dictionary <string, object>
            {
                {"@itemID", itemID},
                {"@ownerID", ownerID}
            }
        );

        using (connection)
        using (reader)
        {
            // if no record exists, return an empty string so the player can create it's own
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
    public void SetNote (int itemID, int ownerID, string note)
    {
        // remove the note if no text is present
        if (note.Length == 0)
            Database.PrepareQuery (
                "DELETE FROM chrNotes WHERE itemID = @itemID AND ownerID = @ownerID",
                new Dictionary <string, object>
                {
                    {"@itemID", itemID},
                    {"@ownerID", ownerID}
                }
            );
        else
            Database.PrepareQuery (
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
    /// Obtains the character name of the given <paramref name="characterID"/>
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public string GetCharacterName (int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT itemName FROM eveNames WHERE itemID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
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
    /// Obtains the skill level for the given character
    /// </summary>
    /// <param name="skill">The skill type</param>
    /// <param name="characterID">The character to get skill level from</param>
    /// <returns></returns>
    public int GetSkillLevelForCharacter (Types skill, int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT valueInt FROM invItemsAttributes LEFT JOIN invItems USING(itemID) WHERE typeID = @skillTypeID AND ownerID = @characterID",
            new Dictionary <string, object>
            {
                {"@skillTypeID", (int) skill},
                {"@characterID", characterID}
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
    /// Searches for characters with the given name
    /// </summary>
    /// <param name="namePart"></param>
    /// <returns></returns>
    public List <int> FindCharacters (string namePart)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT itemID FROM eveNames WHERE groupID = 1 AND itemName LIKE @name",
            new Dictionary <string, object> {{"@name", $"%{namePart}%"}}
        );

        using (connection)
        using (reader)
        {
            List <int> result = new List <int> ();

            while (reader.Read ())
                result.Add (reader.GetInt32 (0));

            return result;
        }
    }

    /// <summary>
    /// Gets the last date this character joined a faction (if any)
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public long GetLastFactionJoinDate (int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT startDate FROM chrEmployment WHERE corporationID IN (SELECT militiaCorporationID FROM chrFactions) AND characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                return 0;

            return reader.GetInt64 (0);
        }
    }

    /// <summary>
    /// Get's when the character's stasis timer started (if any)
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public long? GetCharacterStasisTimer (int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT corpStasisTime FROM chrInformation WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false || reader.IsDBNull (0))
                return null;

            return reader.GetInt64 (0);
        }
    }

    /// <summary>
    /// Sets when the character's stasis timer started (if any)
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public void SetCharacterStasisTimer (int characterID, long? start)
    {
        Database.PrepareQuery (
            "UPDATE chrInformation SET corpStasisTime = @timerStart WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@timerStart", start}
            }
        );
    }

    /// <summary>
    /// Obtains the character's role information
    /// </summary>
    /// <param name="characterID"></param>
    /// <param name="roles">The roles the character has</param>
    /// <param name="rolesAtBase">The roles at base the character has</param>
    /// <param name="rolesAtHQ">The roles at HQ the character has</param>
    /// <param name="rolesAtOther">The roles at other the character has</param>
    /// <param name="grantableRoles">The roles the character can grant</param>
    /// <param name="grantableRolesAtBase">The roles at base the character can grant</param>
    /// <param name="grantableRolesAtHQ">The roles at HQ the character can grant</param>
    /// <param name="grantableRolesAtOther">The roles at other the character can grant</param>
    /// <param name="blockRoles">If the character has blocked role changing</param>
    /// <param name="baseID">The baseID for the character</param>
    /// <exception cref="System.Exception"></exception>
    public void GetCharacterRoles (
        int      characterID,           out long roles,          out long rolesAtBase,          out long rolesAtHQ,
        out long rolesAtOther,          out long grantableRoles, out long grantableRolesAtBase, out long grantableRolesAtHQ,
        out long grantableRolesAtOther, out int? blockRoles,     out int? baseID,               out int  titleMask
    )
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT roles, rolesAtBase, rolesAtHQ, rolesAtOther, grantableRoles, grantableRolesAtBase, grantableRolesAtHQ, grantableRolesAtOther, blockRoles, baseID, titleMask FROM chrInformation WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
        );

        using (connection)
        using (reader)
        {
            if (reader.Read () == false)
                throw new Exception ("Cannot find given character!");

            roles                 = reader.GetInt64 (0);
            rolesAtBase           = reader.GetInt64 (1);
            rolesAtHQ             = reader.GetInt64 (2);
            rolesAtOther          = reader.GetInt64 (3);
            grantableRoles        = reader.GetInt64 (4);
            grantableRolesAtBase  = reader.GetInt64 (5);
            grantableRolesAtHQ    = reader.GetInt64 (6);
            grantableRolesAtOther = reader.GetInt64 (7);
            blockRoles            = reader.GetInt32OrNull (8);
            baseID                = reader.GetInt32OrNull (9);
            titleMask             = reader.GetInt32 (10);
        }
    }

    /// <summary>
    /// Updates the character role information
    /// </summary>
    /// <param name="characterID"></param>
    /// <param name="roles">The roles the character has</param>
    /// <param name="rolesAtBase">The roles at base the character has</param>
    /// <param name="rolesAtHQ">The roles at HQ the character has</param>
    /// <param name="rolesAtOther">The roles at other the character has</param>
    /// <param name="grantableRoles">The roles the character can grant</param>
    /// <param name="grantableRolesAtBase">The roles at base the character can grant</param>
    /// <param name="grantableRolesAtHQ">The roles at HQ the character can grant</param>
    /// <param name="grantableRolesAtOther">The roles at other the character can grant</param>
    /// <param name="titleMask">The titles this character has</param>
    public void UpdateCharacterRoles (
        int  characterID,    long roles,              long rolesAtHQ,            long rolesAtBase,           long rolesAtOther,
        long grantableRoles, long grantableRolesAtHQ, long grantableRolesAtBase, long grantableRolesAtOther, long titleMask
    )
    {
        Database.PrepareQuery (
            "UPDATE chrInformation SET roles = @roles, rolesAtBase = @rolesAtBase, rolesAtHQ = @rolesAtHQ, rolesAtOther = @rolesAtOther, grantableRoles = @grantableRoles, grantableRolesAtBase = @grantableRolesAtBase, grantableRolesAtHQ = @grantableRolesAtHQ, grantableRolesAtOther = @grantableRolesAtOther, titleMask = @titleMask WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@roles", roles},
                {"@rolesAtOther", rolesAtOther},
                {"@rolesAtBase", rolesAtBase},
                {"@rolesAtHQ", rolesAtHQ},
                {"@grantableRoles", grantableRoles},
                {"@grantableRolesAtBase", grantableRolesAtBase},
                {"@grantableRolesAtOther", grantableRolesAtOther},
                {"@grantableRolesAtHQ", grantableRolesAtHQ},
                {"@titleMask", titleMask}
            }
        );
    }

    /// <summary>
    /// Updates the character's block role status
    /// </summary>
    /// <param name="characterID"></param>
    /// <param name="blockRoles"></param>
    public void UpdateCharacterBlockRole (int characterID, int? blockRoles)
    {
        Database.PrepareQuery (
            "UPDATE chrInformation SET blockRoles = @blockRoles WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@characterID", characterID},
                {"@blockRoles", blockRoles}
            }
        );
    }

    /// <summary>
    /// Retrieves the corporationID of a given character
    /// </summary>
    /// <param name="characterID"></param>
    /// <returns></returns>
    public int GetCharacterCorporationID (int characterID)
    {
        IDbConnection connection = null;
        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT corporationID FROM chrInformation WHERE characterID = @characterID",
            new Dictionary <string, object> {{"@characterID", characterID}}
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
    /// Updates character's corporationID and the corporation join date time for that character
    /// </summary>
    /// <param name="characterID"></param>
    /// <param name="corporationID"></param>
    public void UpdateCorporationID (int characterID, int corporationID)
    {
        Database.PrepareQuery (
            "UPDATE chrInformation SET corporationID = @corporationID, corporationDateTime = @corporationDateTime WHERE characterID = @characterID",
            new Dictionary <string, object>
            {
                {"@corporationID", corporationID},
                {"@corporationDateTime", DateTime.UtcNow.ToFileTimeUtc ()},
                {"@characterID", characterID}
            }
        );
    }
}