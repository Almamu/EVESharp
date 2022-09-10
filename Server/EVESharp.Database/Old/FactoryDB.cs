using System.Collections.Generic;
using EVESharp.EVE.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Old;

public class FactoryDB : DatabaseAccessor
{
    public FactoryDB (IDatabaseConnection db) : base (db) { }

    public PyDictionary GetBlueprintAttributes (int blueprintID, int characterID)
    {
        // TODO: IMPROVE PERMISSIONS CHECK ON THE ITEM, CAN BLUEPRINTS BE CHECKED REGARDLESS OF OWNERSHIP?
        // TODO: MOST LIKELY YES, FOR CONTRACT STUFF AND OTHER THINGS
        return this.Database.PrepareDictionary (
            "SELECT copy, productionTime AS manufacturingTime, productivityLevel, materialLevel, maxProductionLimit, researchMaterialTime, researchCopyTime, researchProductivityTime, researchTechTime, wasteFactor AS wastageFactor, productTypeID FROM invItems LEFT JOIN invBlueprints USING(itemID) LEFT JOIN invBlueprintTypes ON invBlueprintTypes.blueprintTypeID = invItems.typeID WHERE itemID = @itemID AND ownerID = @characterID",
            new Dictionary <string, object>
            {
                {"@itemID", blueprintID},
                {"@characterID", characterID}
            }
        );
    }

    public Rowset GetMaterialsForTypeWithActivity (int blueprintTypeID)
    {
        return this.Database.PrepareRowset (
            "SELECT requiredTypeID, quantity, damagePerJob, activityID FROM typeActivityMaterials WHERE typeID = @blueprintTypeID",
            new Dictionary <string, object> {{"@blueprintTypeID", blueprintTypeID}}
        );
    }

    public Rowset GetMaterialCompositionOfItemType (int typeID)
    {
        return this.Database.PrepareRowset (
            "SELECT requiredTypeID AS typeID, quantity FROM typeActivityMaterials LEFT JOIN invBlueprintTypes ON productTypeID = @typeID WHERE typeID = invBlueprintTypes.blueprintTypeID AND activityID = 1 AND damagePerJob = 1",
            new Dictionary <string, object> {{"@typeID", typeID}}
        );
    }

    public Rowset GetBlueprintInformationAtLocationWithFlag (int locationID, int flag)
    {
        return this.Database.PrepareRowset (
            "SELECT itemID, typeID, singleton, licensedProductionRunsRemaining, productivityLevel, materialLevel, copy FROM invItems LEFT JOIN invBlueprints USING(itemID) WHERE locationID = @locationID AND flag = @flag",
            new Dictionary <string, object>
            {
                {"@locationID", locationID},
                {"@flag", flag}
            }
        );
    }

    public Rowset GetBlueprintInformationAtLocation (int locationID)
    {
        return this.Database.PrepareRowset (
            "SELECT itemID, typeID, singleton, licensedProductionRunsRemaining, productivityLevel, materialLevel, copy FROM invItems LEFT JOIN invBlueprints USING(itemID) WHERE locationID = @locationID",
            new Dictionary <string, object> {{"@locationID", locationID}}
        );
    }
}