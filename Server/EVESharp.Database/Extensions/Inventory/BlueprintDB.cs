using System.Collections.Generic;
using System.Data;
using EVESharp.Database.Inventory.Types.Information;

namespace EVESharp.Database.Extensions.Inventory;

public static class BlueprintDB
{
    public static Blueprint InvBpLoad (this IDatabase Database, Item item)
    {
        IDataReader reader = Database.Select (
            "SELECT copy, materialLevel, productivityLevel, licensedProductionRunsRemaining FROM invBlueprints WHERE itemID = @itemID",
            new Dictionary <string, object> {{"@itemID", item.ID}}
        );

        using (reader)
        {
            if (reader.Read () == false)
                return null;

            return new Blueprint
            {
                Information                     = item,
                IsCopy                          = reader.GetBoolean (0),
                MaterialLevel                   = reader.GetInt32 (1),
                ProductivityLevel               = reader.GetInt32 (2),
                LicensedProductionRunsRemaining = reader.GetInt32 (3)
            };
        }
    }

    public static void InvBpPersist (this IDatabase Database, Blueprint info)
    {
        Database.Prepare (
            "UPDATE invBlueprints SET copy = @copy, materialLevel = @materialLevel, productivityLevel = @productivityLevel, licensedProductionRunsRemaining = @licensedProductionRunsRemaining WHERE itemID = @itemID",
            new Dictionary <string, object>
            {
                {"@itemID", info.Information.ID},
                {"@copy", info.IsCopy},
                {"@materialLevel", info.MaterialLevel},
                {"@productivityLevel", info.ProductivityLevel},
                {"@licensedProductionRunsRemaining", info.LicensedProductionRunsRemaining}
            }
        );
    }
}