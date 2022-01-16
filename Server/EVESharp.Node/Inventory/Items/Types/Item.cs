using EVESharp.Common.Database;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.StaticData;

namespace EVESharp.Node.Inventory.Items.Types
{
    public class Item : ItemEntity
    {
        public Item(
            DatabaseConnection databaseConnection, string entityName, int entityId, Type type,
            int ownerID, int locationID, Flags entityFlag, bool entityContraband, bool entitySingleton,
            int entityQuantity, double? entityX, double? entityY, double? entityZ, string entityCustomInfo,
            AttributeList attributes, ItemFactory itemFactory
        ) : base(
            databaseConnection, entityName, entityId, type, ownerID, locationID, entityFlag,
            entityContraband, entitySingleton, entityQuantity, entityX, entityY, entityZ,
            entityCustomInfo, attributes, itemFactory)
        {
        }
    }
}