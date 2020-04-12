using System.Collections.Generic;
using Common.Database;
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

        public int GetCharacterTypeByBloodline(int bloodlineID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                "SELECT typeID FROM bloodlineTypes WHERE bloodlineID = @bloodlineID",
                new Dictionary<string, object>()
                {
                    {"@bloodlineID", bloodlineID}
                }
            );
            
            using(connection)
            using (reader)
            {
                reader.Read();

                return reader.GetInt32(0);
            }
        }

        public PyDataType GetCharSelectInfo(int characterID)
        {
            MySqlConnection connection = null;
            MySqlDataReader reader = Database.PrepareQuery(
                ref connection,
                @"SELECT itemName AS shortName,bloodlineID,gender,bounty,character_.corporationID,allianceID, 0 AS allianceMemberStartDate,
                title,startDateTime,createDateTime,securityRating,character_.balance,character_.stationID,
                solarSystemID,constellationID,regionID,petitionMessage,logonMinutes,tickerName
                FROM character_
                    LEFT JOIN entity ON characterID = itemID
                    LEFT JOIN corporation USING (corporationID)
                    LEFT JOIN bloodlineTypes USING (typeID)
                WHERE characterID=@characterID",
                new Dictionary<string, object>()
                {
                    {"@characterID", characterID}
                }
            );

            using (connection)
            using (reader)
            {
                return Rowset.FromMySqlDataReader(reader);
            }
        }
    }
}