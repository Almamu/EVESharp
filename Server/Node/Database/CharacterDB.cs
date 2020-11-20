using System.Collections.Generic;
using System.Threading;
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
        private ItemFactory mItemFactory = null;
        
        public CharacterDB(DatabaseConnection db, ItemFactory factory) : base(db)
        {
            this.mItemFactory = factory;
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
        public Character CreateCharacter(ItemType from, int accountId, double balance, double securityRating,
            int corporationId, int corpRole, int rolesAtAll, int rolesAtBase, int rolesAtHq, int rolesAtOther,
            long corporationDateTime, long startDateTime, long createDateTime, int ancestryId, int careerId, int schoolId,
            int careerSpecialityId, int gender, int accessoryId, int beardId, int costumeId, int decoId, int eyebrowsId,
            int eyesId, int hairId, int lipstickId, int makeupId, int skinId, int backgroundId, int lightId,
            double headRotation1, double headRotation2, double headRotation3, double eyeRotation1, double eyeRotation2,
            double eyeRotation3, double camPos1, double camPos2, double camPos3, double morph1E, double morph1N,
            double morph1S, double morph1W, double morph2E, double morph2N, double morph2S, double morph2W,
            double morph3E, double morph3N, double morph3S, double morph3W, double morph4E, double morph4N,
            double morph4S, double morph4W, int stationId, int solarSystemId, int constellationId, int regionId)
        {
            return null;
        }
    }
}