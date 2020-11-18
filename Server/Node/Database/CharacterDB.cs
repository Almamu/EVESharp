using System.Collections.Generic;
using System.Threading;
using Common.Database;
using MySql.Data.MySqlClient;
using Node.Data;
using Node.Inventory;
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

        public List<Bloodline> GetBloodlineInformation()
        {
            List<Bloodline> result = new List<Bloodline>();
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
    }
}