using System.Collections.Generic;
using Common.Database;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;

namespace Node.Database
{
    public class FactoryDB : DatabaseAccessor
    {
        public PyDictionary GetBlueprintAttributes(int blueprintID, int characterID)
        {
            // TODO: IMPROVE PERMISSIONS CHECK ON THE ITEM, CAN BLUEPRINTS BE CHECKED REGARDLESS OF OWNERSHIP?
            return Database.PrepareDictionaryQuery(
                "SELECT copy, productionTime AS manufacturingTime, productivityLevel, materialLevel, maxProductionLimit, researchMaterialTime, researchCopyTime, researchProductivityTime, researchTechTime, wasteFactor AS wastageFactor, productTypeID FROM invItems LEFT JOIN invBlueprints USING(itemID) LEFT JOIN invBlueprintTypes ON invBlueprintTypes.blueprintTypeID = invItems.typeID WHERE itemID = @itemID AND ownerID = @characterID",
                new Dictionary<string, object>()
                {
                    {"@itemID", blueprintID},
                    {"@characterID", characterID}
                }
            );
        }

        public Rowset GetMaterialsForTypeWithActivity(int blueprintTypeID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT requiredTypeID, quantity, damagePerJob, activityID FROM typeActivityMaterials WHERE typeID = @blueprintTypeID",
                new Dictionary<string, object>()
                {
                    {"@blueprintTypeID", blueprintTypeID}
                }
            );
        }

        public Rowset GetMaterialCompositionOfItemType(int typeID)
        {
            return Database.PrepareRowsetQuery(
                "SELECT requiredTypeID AS typeID, quantity FROM typeActivityMaterials LEFT JOIN invBlueprintTypes ON productTypeID = @typeID WHERE typeID = invBlueprintTypes.blueprintTypeID AND activityID = 1 AND damagePerJob = 1",
                new Dictionary<string, object>()
                {
                    {"@typeID", typeID}
                }
            );
        }
        
        public FactoryDB(DatabaseConnection db) : base(db)
        {
        }
    }
}