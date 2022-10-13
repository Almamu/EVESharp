using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace EVESharp.Database.Old;

public class ReprocessingDB : DatabaseAccessor
{
    public ReprocessingDB (IDatabase db) : base (db) { }

    public List <Recoverables> GetRecoverables (int typeID)
    {
        DbDataReader reader = this.Database.Select (
            "SELECT requiredTypeID, MIN(quantity) FROM typeActivityMaterials LEFT JOIN invBlueprintTypes ON typeID = blueprintTypeID WHERE damagePerJob = 1 AND ((activityID = 6 AND typeID = @typeID) OR (activityID = 1 AND productTypeID = @typeID)) GROUP BY requiredTypeID",
            new Dictionary <string, object> {{"@typeID", typeID}}
        );

        using (reader)
        {
            List <Recoverables> result = new List <Recoverables> ();

            while (reader.Read ())
                result.Add (
                    new Recoverables
                    {
                        TypeID         = reader.GetInt32 (0),
                        AmountPerBatch = reader.GetInt32 (1)
                    }
                );

            return result;
        }
    }

    public class Recoverables
    {
        public int TypeID         { get; set; }
        public int AmountPerBatch { get; set; }
    }
}