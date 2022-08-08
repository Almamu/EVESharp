using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using EVESharp.Common.Database;
using EVESharp.PythonTypes.Types.Database;

namespace EVESharp.Node.Database;

public class ReprocessingDB : DatabaseAccessor
{
    public ReprocessingDB (IDatabaseConnection db) : base (db) { }

    public List <Recoverables> GetRecoverables (int typeID)
    {
        IDbConnection connection = null;

        DbDataReader reader = Database.Select (
            ref connection,
            "SELECT requiredTypeID, MIN(quantity) FROM typeActivityMaterials LEFT JOIN invBlueprintTypes ON typeID = blueprintTypeID WHERE damagePerJob = 1 AND ((activityID = 6 AND typeID = @typeID) OR (activityID = 1 AND productTypeID = @typeID)) GROUP BY requiredTypeID",
            new Dictionary <string, object> {{"@typeID", typeID}}
        );

        using (connection)
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