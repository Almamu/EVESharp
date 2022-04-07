using System.Collections.Generic;
using EVESharp.Common.Database;
using MySql.Data.MySqlClient;

namespace EVESharp.Node.Database;

public class ReprocessingDB : DatabaseAccessor
{
    public class Recoverables
    {
        public int TypeID         { get; set; }
        public int AmountPerBatch { get; set; }
    }

    public List<Recoverables> GetRecoverables(int typeID)
    {
        MySqlConnection connection = null;
        MySqlDataReader reader = Database.Select(ref connection,
                                                 "SELECT requiredTypeID, MIN(quantity) FROM typeActivityMaterials LEFT JOIN invBlueprintTypes ON typeID = blueprintTypeID WHERE damagePerJob = 1 AND ((activityID = 6 AND typeID = @typeID) OR (activityID = 1 AND productTypeID = @typeID)) GROUP BY requiredTypeID",
                                                 new Dictionary<string, object>()
                                                 {
                                                     {"@typeID", typeID}
                                                 }
        );
            
        using (connection)
        using (reader)
        {
            List<Recoverables> result = new List<Recoverables>();

            while (reader.Read() == true)
            {
                result.Add(new Recoverables
                    {
                        TypeID         = reader.GetInt32(0),
                        AmountPerBatch = reader.GetInt32(1)
                    }
                );
            }

            return result;
        }
    }

    public ReprocessingDB(DatabaseConnection db) : base(db)
    {
    }
}