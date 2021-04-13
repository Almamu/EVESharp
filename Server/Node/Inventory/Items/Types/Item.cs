using Node.Inventory.Items.Attributes;
using Node.StaticData;
using Node.StaticData.Inventory;

namespace Node.Inventory.Items.Types
{
    public class Item : ItemEntity
    {
        public Item(
            string entityName, int entityId, Type type, int ownerID, int locationID,
            Flags entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity,
            double? entityX, double? entityY, double? entityZ,
            string entityCustomInfo, AttributeList attributes, ItemFactory itemFactory
        ) : base(
            entityName, entityId, type, ownerID, locationID, entityFlag,
            entityContraband, entitySingleton, entityQuantity, entityX, entityY, entityZ,
            entityCustomInfo, attributes, itemFactory)
        {
        }
    }
}