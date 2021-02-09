using System;
using System.Collections.Generic;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Data;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class CharacterDB : DatabaseAccessor
    {
        private ItemDB ItemDB { get; }
        private TypeManager TypeManager { get; }
        
        public CharacterDB(DatabaseConnection db, ItemDB itemDB, TypeManager typeManager) : base(db)
        {
            this.TypeManager = typeManager;
            this.ItemDB = itemDB;
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
                " FROM chrInformation " +
                "	LEFT JOIN evenames ON characterID = itemID" +
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
                    " itemName AS shortName,bloodlineID,gender,bounty,chrInformation.corporationID,allianceID,title,startDateTime,createDateTime," +
                    " securityRating,chrInformation.balance,chrInformation.stationID,solarSystemID,constellationID,regionID," +
                    " petitionMessage,logonMinutes,tickerName" +
                    " FROM chrInformation " +
                    "	LEFT JOIN evenames ON characterID = itemID" +
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
            return Database.PrepareKeyValQuery(
                "SELECT chrInformation.corporationID, raceID, bloodlineID, ancestryID, careerID," +
                " schoolID, careerSpecialityID, createDateTime, gender " +
                "FROM chrInformation " +
                "LEFT JOIN chrAncestries USING (ancestryID) " +
                "LEFT JOIN chrBloodlines USING (bloodlineID) " +
                "WHERE characterID=@characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public PyDataType GetPublicInfo3(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT bounty, title, startDateTime, description, corporationID FROM chrInformation WHERE characterID=@characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public bool IsCharacterNameTaken(string characterName)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                $"SELECT COUNT(*) FROM chrInformation LEFT JOIN evenames ON characterID = itemID WHERE itemName LIKE @characterName",
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
                        this.TypeManager[reader.GetInt32(1)],
                        reader.GetString(2),
                        reader.GetInt32(3),
                        reader.GetString(4),
                        reader.GetString(5),
                        reader.GetString(6),
                        this.TypeManager[reader.GetInt32(7)],
                        reader.GetInt32(8),
                        reader.GetInt32(9),
                        reader.GetInt32(10),
                        reader.GetInt32(11),
                        reader.GetInt32(12),
                        reader.GetInt32(13),
                        reader.GetInt32OrDefault(14),
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
                        reader.GetInt32OrDefault(9),
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
            int itemID = (int) this.ItemDB.CreateItem(name, from.ID, owner.ID, stationID, ItemFlags.Connected, false,
                true, 1, 0, 0, 0, "");

            // now create the character record in the database
            Database.PrepareQuery(
                "INSERT INTO chrInformation(" + 
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
            
            // create employment record
            Database.PrepareQuery(
                "INSERT INTO chrEmployment(characterID, corporationID, startDate)VALUES(@characterID, @corporationID, @startDate)",
                new Dictionary<string, object>()
                {
                    {"@characterID", itemID},
                    {"@corporationID", corporationID},
                    {"@startDate", createDateTime}
                }
            );
            
            // return the character's item id
            return itemID;
        }

        public bool GetRandomCareerForRace(int raceID, out int careerID, out int schoolID, out int careerSpecialityID, out int corporationID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT careerID, corporationID, schoolID FROM chrSchools WHERE raceID = @raceID ORDER BY RAND();",
                new Dictionary<string, object>()
                {
                    {"@raceID", raceID}
                }
            );
            
            using (connection)
            using (reader)
            {
                if (reader.Read() == false)
                {
                    // set some defaults just in case
                    careerID = 11;
                    schoolID = 17;
                    careerSpecialityID = 11;
                    corporationID = 1000167;
                    
                    return false;
                }

                careerID = reader.GetInt32(0);
                corporationID = reader.GetInt32(1);
                schoolID = reader.GetInt32(2);
                careerSpecialityID = careerID;

                return true;
            }
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

        public PyDataType GetKeyMap()
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.Query(ref connection,
                "SELECT accountKey as keyID, accountType as keyType, accountName as keyName, description FROM market_keyMap"
            );
            
            using (connection)
            using (reader)
            {
                return Rowset.FromMySqlDataReader(reader);
            }
        }

        public List<Character.SkillQueueEntry> LoadSkillQueue(Character character, Dictionary<int, Skill> skillsInTraining)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT skillItemID, level FROM chrSkillQueue WHERE characterID = @characterID ORDER BY orderIndex",
                new Dictionary<string, object>()
                {
                    {"@characterID", character.ID}
                }
            );
            
            using (connection)
            using (reader)
            {
                List<Character.SkillQueueEntry> result = new List<Character.SkillQueueEntry>();

                while (reader.Read() == true)
                {
                    result.Add(
                        new Character.SkillQueueEntry()
                        {
                            Skill = skillsInTraining [reader.GetInt32(0)],
                            TargetLevel = reader.GetInt32(1),
                        }
                    );
                }

                return result;
            }
        }

        public PyDataType GetOwnerNoteLabels(Character character)
        {
            return Database.PrepareRowsetQuery(
                "SELECT noteID, label FROM chrOwnerNote WHERE ownerID = @ownerID",
                new Dictionary<string, object>()
                {
                    {"@ownerID", character.ID}
                }
            );
        }

        public PyDataType IsOnline(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT online FROM chrInformation WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
            
            using(connection)
            using (reader)
            {
                if (reader.Read() == false)
                    return false;

                return reader.GetBoolean(0);
            }
        }

        public List<int> GetOnlineFriendList(Character character)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT accessor AS characterID FROM lscChannelPermissions, chrInformation WHERE lscChannelPermissions.channelID = @characterID AND chrInformation.characterID = lscChannelPermissions.accessor and chrInformation.online = 1",
                new Dictionary<string, object>()
                {
                    {"@characterID", character.ID}
                }
            );
            
            using (connection)
            using (reader)
            {
                List<int> result = new List<int>();
                
                while(reader.Read() == true)
                    result.Add(reader.GetInt32(0));

                return result;
            }
        }

        public void UpdateCharacterInformation(Character character)
        {
            Database.PrepareQuery(
                "UPDATE chrInformation SET online = @online, activeCloneID = @activeCloneID, freeRespecs = @freeRespecs, nextRespecTime = @nextRespecTime, timeLastJump = @timeLastJump, description = @description, balance = @balance WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", character.ID},
                    {"@online", character.Online},
                    {"@activeCloneID", character.ActiveCloneID},
                    {"@freeRespecs", character.FreeReSpecs},
                    {"@nextRespecTime", character.NextReSpecTime},
                    {"@timeLastJump", character.TimeLastJump},
                    {"@description", character.Description},
                    {"@balance", character.Balance}
                }
            );

            if (character.ContentsLoaded == true)
            {
                // ensure the skill queue is saved too
                Database.PrepareQuery("DELETE FROM chrSkillQueue WHERE characterID = @characterID",
                    new Dictionary<string, object>()
                    {
                        {"@characterID", character.ID}
                    }
                );
            
                // re-create the whole skill queue
                MySqlConnection connection = null;
            
                MySqlCommand create = Database.PrepareQuery(
                    ref connection,
                    "INSERT INTO chrSkillQueue(orderIndex, characterID, skillItemID, level) VALUE (@orderIndex, @characterID, @skillItemID, @level)"
                );

                using (connection)
                {
                    int index = 0;
                
                    foreach (Character.SkillQueueEntry entry in character.SkillQueue)
                    {
                        create.Parameters.Clear();
                        create.Parameters.AddWithValue("@orderIndex", index++);
                        create.Parameters.AddWithValue("@characterID", character.ID);
                        create.Parameters.AddWithValue("@skillItemID", entry.Skill.ID);
                        create.Parameters.AddWithValue("@level", entry.TargetLevel);
                        create.ExecuteNonQuery();
                    }
                }
            }
        }

        public PyDataType GetJournal(int characterID, int? refTypeID, int accountKey, long minDate)
        {
            // add one day to the minimum date to get the maximum date
            long maxDate = DateTime.FromFileTimeUtc(minDate).AddDays(-1).ToFileTimeUtc();
            
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                {"@characterID", characterID},
                {"@accountKey", accountKey},
                {"@maxDate", maxDate},
                {"@minDate", minDate}
            };
            
            string query =
                "SELECT transactionID, transactionDate, referenceID, entryTypeID," +
                " ownerID1, ownerID2, accountKey, amount, balance, description " +
                "FROM market_journal " +
                "WHERE (ownerID1=@characterID OR ownerID2=@characterID) AND accountKey=@accountKey AND transactionDate <= @minDate AND transactionDate >= @maxDate";

            if (refTypeID != null)
            {
                query += " AND entryTypeID=@entryTypeID";
                parameters["@entryTypeID"] = (int) refTypeID;
            }

            return Database.PrepareRowsetQuery(query, parameters);
        }

        public PyDataType GetRecentShipKillsAndLosses(int characterID, int count, int? startIndex)
        {
            // TODO: WRITE A GENERATOR FOR THE KILL LOGS, THESE SEEM TO BE KIND OF AN XML FILE WITH ALL THE INFORMATION
            // TODO: FOR MORE INFORMATION CHECK CombatLog_CopyText ON eveCommonUtils.py
            return Database.PrepareRowsetQuery(
                "SELECT killID, solarSystemID, moonID, victimCharacterID, victimCorporationID, victimAllianceID, victimFactionID, victimShipTypeID, victimDamageTaken, finalCharacterID, finalCorporationID, finalAllianceID, finalFactionID, finalDamageDone, finalSecurityStatus, finalShipTypeID, finalWeaponTypeID, killTime, killBlob FROM chrCombatLogs WHERE victimCharacterID = @characterID OR finalCharacterID = @characterID LIMIT @startIndex, @limit",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@startIndex", startIndex ?? 0},
                    {"@limit", count}
                }
            );
        }

        public Rowset GetTopBounties()
        {
            // return the 100 topmost bounties
            return Database.PrepareRowsetQuery(
                "SELECT characterID, itemName AS ownerName, bounty, 0 AS online FROM chrBounties, evenames WHERE evenames.itemID = chrBounties.ownerID ORDER BY bounty DESC LIMIT 100"
            );
        }

        public void AddToBounty(int characterID, int ownerID, double bounty)
        {
            // create bounty record
            Database.PrepareQuery(
                "INSERT INTO chrBounties(characterID, ownerID, bounty)VALUES(@characterID, @ownerID, @bounty)",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID},
                    {"@ownerID", ownerID},
                    {"@bounty", bounty}
                }
            );
            
            // add the bounty to the player
            Database.PrepareQuery(
                "UPDATE chrInformation SET bounty = bounty + @bounty",
                new Dictionary<string, object>()
                {
                    {"@bounty", bounty}
                }
            );
        }

        public PyDataType GetPrivateInfo(int characterID)
        {
            return Database.PrepareKeyValQuery(
                "SELECT gender, createDateTime, itemName AS charName, bloodlineName, raceName " +
                "FROM chrInformation " + 
                "LEFT JOIN evenames ON evenames.itemID = chrInformation.characterID " +
                "LEFT JOIN chrAncestries USING (ancestryID) " +
                "LEFT JOIN chrBloodlines USING (bloodlineID) " +
                "LEFT JOIN chrRaces USING (raceID) " +
                "WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public Rowset GetCharacterAppearanceInfo(int characterID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT accessoryID, beardID, costumeID, decoID, eyebrowsID, eyesID, hairID," +
                " lipstickID, makeupID, skinID, backgroundID, lightID," +
                " headRotation1, headRotation2, headRotation3, eyeRotation1," +
                " eyeRotation2, eyeRotation3, camPos1, camPos2, camPos3," +
                " morph1e, morph1n, morph1s, morph1w, morph2e, morph2n," +
                " morph2s, morph2w, morph3e, morph3n, morph3s, morph3w," +
                " morph4e, morph4n, morph4s, morph4w " +
                "FROM chrInformation WHERE characterID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );
        }

        public string GetNote(int itemID, int ownerID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT note FROM chrNotes WHERE itemID = @itemID AND ownerID = @ownerID",
                new Dictionary<string, object>()
                {
                    {"@itemID", itemID},
                    {"@ownerID", ownerID}
                }
            );
            
            using (connection)
            using (reader)
            {
                // if no record exists, return an empty string so the player can create it's own
                if (reader.Read() == false)
                    return "";

                return reader.GetString(0);
            }
        }

        public void SetNote(int itemID, int ownerID, string note)
        {
            // remove the note if no text is present
            if (note.Length == 0)
            {
                Database.PrepareQuery("DELETE FROM chrNotes WHERE itemID = @itemID AND ownerID = @ownerID",
                    new Dictionary<string, object>()
                    {
                        {"@itemID", itemID},
                        {"@ownerID", ownerID}
                    }
                );
            }
            else
            {
                Database.PrepareQuery("REPLACE INTO chrNotes (itemID, ownerID, note)VALUES(@itemID, @ownerID, @note)",
                    new Dictionary<string, object>()
                    {
                        {"@itemID", itemID},
                        {"@ownerID", ownerID},
                        {"@note", note}
                    }
                );
            }
        }

        public string GetCharacterName(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(ref connection,
                "SELECT itemName FROM evenames WHERE itemID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
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
    }
}