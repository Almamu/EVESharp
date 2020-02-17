using Common.Database;
using PythonTypes;
using MySql.Data.MySqlClient;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class CharacterDB : DatabaseAccessor
    {
        public CharacterDB(DatabaseConnection db) : base(db)
        {
        }

        public PyDataType GetCharacterList(int accountID)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;
            
            Database.Query(
                ref reader, ref connection,
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
                " WHERE accountID=" + accountID
            );
            
            using (connection)
            using (reader)
            {
                return Rowset.FromMySqlDataReader(reader);
            }
        }

        public bool IsCharacterNameTaken(string characterName)
        {
            MySqlDataReader reader = null;
            MySqlConnection connection = null;
            
            Database.Query(
                ref reader, ref connection,
                $"SELECT COUNT(*) FROM character_ LEFT JOIN entity ON characterID = itemID WHERE itemName LIKE '{Database.DoEscapeString(characterName)}'"
            );
            
            using (connection)
            using (reader)
            {
                reader.Read();
                
                return reader.GetInt32(0) > 0;
            }
        }
    }
}