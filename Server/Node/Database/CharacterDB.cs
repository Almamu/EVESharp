using System;
using System.Collections.Generic;
using System.Threading;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Data;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class CharacterDB : DatabaseAccessor
    {
        private readonly ItemFactory mItemFactory = null;
        private readonly ItemDB mItemDB = null;
        
        public CharacterDB(DatabaseConnection db, ItemFactory factory) : base(db)
        {
            this.mItemFactory = factory;
            this.mItemDB = new ItemDB(db, factory);
        }

        public PyDataType GetCharacterList(int accountID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT" +
                " characterID, itemName AS characterName, 0 as deletePrepareDateTime," +
                " gender, accessoryID, beardID, costumeID, decoID, eyebrowsID, eyesID, hairID," +
                " lipstickID, makeupID, skinID, backgroundID, lightID," +
                " headRotation1, headRotation2, headRotation3, eyeRotation1," +
                " eyeRotation2, eyeRotation3, camPos1, camPos2, camPos3," +
                " morph1e, morph1n, morph1s, morph1w, morph2e, morph2n," +
                " morph2s, morph2w, morph3e, morph3n, morph3s, morph3w," +
                " morph4e, morph4n, morph4s, morph4w" +
                " FROM character_ " +
                "	LEFT JOIN entity ON characterID = itemID" +
                " WHERE accountID = @accountID",
                new Dictionary<string, object>()
                {
                    {"@accountID", accountID}
                }
            );

            using (connection)
            using (reader)
            {
                return Rowset.FromMySqlDataReader(reader);
            }
        }

        public PyDataType GetCharacterSelectionInfo(int characterID, int accountID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT " +
                    " itemName AS shortName,bloodlineID,gender,bounty,character_.corporationID,allianceID,title,startDateTime,createDateTime," +
                    " securityRating,character_.balance,character_.stationID,solarSystemID,constellationID,regionID," +
                    " petitionMessage,logonMinutes,tickerName" +
                    " FROM character_ " +
                    "	LEFT JOIN entity ON characterID = itemID" +
                    "	LEFT JOIN corporation USING (corporationID)" +
                    "	LEFT JOIN bloodlineTypes USING (typeID)" +
                    " WHERE characterID=@characterID AND accountID = @accountID",
                    new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@accountID", accountID}
                }
            );
                 
            using(connection)
            using (reader)
            {
                return Rowset.FromMySqlDataReader(reader);
            }
        }

        public PyDataType GetPublicInfo(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT gender, bloodlineID, corporationID " +
                "FROM character_ " +
                "LEFT JOIN chrAncestries USING (ancestryID) " +
                "WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                    throw new CustomError("Cannot find the specified character");

                return KeyVal.FromMySqlDataReader(reader);
            }
        }

        public bool IsCharacterNameTaken(string characterName)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                $"SELECT COUNT(*) FROM character_ LEFT JOIN entity ON characterID = itemID WHERE itemName LIKE @characterName",
                new Dictionary<string, object>()
                {
                    {"@characterName", characterName}
                }
            );

            using (connection)
            using (reader)
            {
                reader.Read();

                return reader.GetInt32(0) > 0;
            }
        }

        public Dictionary<int, Bloodline> GetBloodlineInformation()
        {
            Dictionary<int, Bloodline> result = new Dictionary<int, Bloodline>();
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Query(
                ref connection,
                "SELECT " +
                " bloodlineTypes.bloodlineID, typeID, bloodlineName, raceID, description, maleDescription, " +
                " femaleDescription, shipTypeID, corporationID, perception, willpower, charisma, memory, " +
                " intelligence, graphicID, shortDescription, shortMaleDescription, shortFemaleDescription " +
                " FROM bloodlineTypes, chrBloodlines " + 
                " WHERE chrBloodlines.bloodlineID = bloodlineTypes.bloodlineID"
            );
            
            using(connection)
            using (reader)
            {
                while (reader.Read() == true)
                {
                    Bloodline bloodline = new Bloodline(
                        reader.GetInt32(0),
                        this.mItemFactory.TypeManager[reader.GetInt32(1)],
                        reader.GetString(2),
                        reader.GetInt32(3),
                        reader.GetString(4),
                        reader.GetString(5),
                        reader.GetString(6),
                        this.mItemFactory.TypeManager[reader.GetInt32(7)],
                        reader.GetInt32(8),
                        reader.GetInt32(9),
                        reader.GetInt32(10),
                        reader.GetInt32(11),
                        reader.GetInt32(12),
                        reader.GetInt32(13),
                        reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                        reader.GetString(15),
                        reader.GetString(16),
                        reader.GetString(17)
                    );

                    result[bloodline.ID] = bloodline;
                }
            }

            return result;
        }

        public Dictionary<int, Ancestry> GetAncestryInformation(Dictionary<int, Bloodline> bloodlines)
        {
            Dictionary<int, Ancestry> result = new Dictionary<int, Ancestry>();
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Query(
                ref connection,
                "SELECT " +
                " ancestryID, ancestryName, bloodlineID, description, perception, willpower, charisma," +
                " memory, intelligence, graphicID, shortDescription " +
                " FROM chrAncestries "
            );
            
            using(connection)
            using (reader)
            {
                while (reader.Read() == true)
                {
                    Ancestry ancestry = new Ancestry(
                        reader.GetInt32(0),
                        reader.GetString (1),
                        bloodlines [reader.GetInt32(2)],
                        reader.GetString(3),
                        reader.GetInt32(4),
                        reader.GetInt32(5),
                        reader.GetInt32(6),
                        reader.GetInt32(7),
                        reader.GetInt32(8),
                        reader.IsDBNull(9) ? 0 : reader.GetInt32(9),
                        reader.GetString(10)
                    );

                    result[ancestry.ID] = ancestry;
                }
            }

            return result;
        }
        public int CreateCharacter(ItemType from, string name, ItemEntity owner, int accountID, double balance, double securityRating,
            int corporationID, int corpRole, int rolesAtAll, int rolesAtBase, int rolesAtHQ, int rolesAtOther,
            long corporationDateTime, long startDateTime, long createDateTime, int ancestryID, int careerID, int schoolID,
            int careerSpecialityID, int gender, int? accessoryID, int? beardID, int costumeID, int? decoID, int eyebrowsID,
            int eyesID, int hairID, int? lipstickID, int? makeupID, int skinID, int backgroundID, int lightID,
            double headRotation1, double headRotation2, double headRotation3, double eyeRotation1, double eyeRotation2,
            double eyeRotation3, double camPos1, double camPos2, double camPos3, double? morph1E, double? morph1N,
            double? morph1S, double? morph1W, double? morph2E, double? morph2N, double? morph2S, double? morph2W,
            double? morph3E, double? morph3N, double? morph3S, double? morph3W, double? morph4E, double? morph4N,
            double? morph4S, double? morph4W, int stationID, int solarSystemID, int constellationID, int regionID)
        {
            // create the item first
            int itemID = (int) this.mItemDB.CreateItem(name, from.ID, owner.ID, stationID, ItemFlags.Connected, false,
                true, 1, 0, 0, 0, "");

            // now create the character record in the database
            Database.PrepareQuery(
                "INSERT INTO character_(" + 
                    "characterID, accountID, title, description, bounty, balance, securityRating, petitionMessage, " +
                    "logonMinutes, corporationID, corpRole, rolesAtAll, rolesAtBase, rolesAtHQ, rolesAtOther, " +
                    "corporationDateTime, startDateTime, createDateTime, ancestryID, careerID, schoolID, careerSpecialityID, " +
                    "gender, accessoryID, beardID, costumeID, decoID, eyebrowsID, eyesID, hairID, lipstickID, makeupID, " +
                    "skinID, backgroundID, lightID, headRotation1, headRotation2, headRotation3, eyeRotation1, " +
                    "eyeRotation2, eyeRotation3, camPos1, camPos2, camPos3, morph1e, morph1n, morph1s, morph1w, " +
                    "morph2e, morph2n, morph2s, morph2w, morph3e, morph3n, morph3s, morph3w, " +
                    "morph4e, morph4n, morph4s, morph4w, stationID, solarSystemID, constellationID, regionID, online" +
                ")VALUES(" +
                    "@characterID, @accountID, @title, @description, @bounty, @balance, @securityRating, @petitionMessage, " +
                    "@logonMinutes, @corporationID, @corpRole, @rolesAtAll, @rolesAtBase, @rolesAtHQ, @rolesAtOther, " +
                    "@corporationDateTime, @startDateTime, @createDateTime, @ancestryID, @careerID, @schoolID, @careerSpecialityID, " +
                    "@gender, @accessoryID, @beardID, @costumeID, @decoID, @eyebrowsID, @eyesID, @hairID, @lipstickID, @makeupID, " +
                    "@skinID, @backgroundID, @lightID, @headRotation1, @headRotation2, @headRotation3, @eyeRotation1, " +
                    "@eyeRotation2, @eyeRotation3, @camPos1, @camPos2, @camPos3, @morph1e, @morph1n, @morph1s, @morph1w, " +
                    "@morph2e, @morph2n, @morph2s, @morph2w, @morph3e, @morph3n, @morph3s, @morph3w, " +
                    "@morph4e, @morph4n, @morph4s, @morph4w, @stationID, @solarSystemID, @constellationID, @regionID, @online" +
                ")"
                ,
                new Dictionary<string, object>()
                {
                    {"@characterID", itemID},
                    {"@accountID", accountID},
                    {"@title", ""},
                    {"@description", ""},
                    {"@bounty", 0},
                    {"@balance", balance},
                    {"@securityRating", securityRating},
                    {"@petitionMessage", ""},
                    {"@logonMinutes", 0},
                    {"@corporationID", corporationID},
                    {"@corpRole", corpRole},
                    {"@rolesAtAll", rolesAtAll},
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
            
            // return the character's item id
            return itemID;
        }

        public bool GetLocationForCorporation(int corporationID, out int stationID, out int solarSystemID,
            out int constellationID, out int regionID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT staStations.stationID, solarSystemID, constellationID, regionID" +
                " FROM staStations, corporation" +
                " WHERE staStations.stationID = corporation.stationID AND corporation.corporationID = @corporationID",
                new Dictionary<string, object>()
                {
                    {"@corporationID", corporationID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                {
                    stationID = 0;
                    solarSystemID = 0;
                    constellationID = 0;
                    regionID = 0;
                    
                    return false;
                }

                stationID = reader.GetInt32(0);
                solarSystemID = reader.GetInt32(1);
                constellationID = reader.GetInt32(2);
                regionID = reader.GetInt32(3);

                return true;
            }
        }

        public Dictionary<int, int> GetBasicSkillsByRace(int raceID)
        {
            Dictionary<int, int> skills = new Dictionary<int, int>();
            
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT skillTypeID, levels FROM chrRaceSkills WHERE raceID = @raceID",
                new Dictionary<string, object> ()
                {
                    {"@raceID", raceID}
                }
            );
            
            using(connection)
            using (reader)
            {
                while (reader.Read() == true)
                    skills[reader.GetInt32(0)] = reader.GetInt32(1);
            }

            return skills;
        }
    }
}