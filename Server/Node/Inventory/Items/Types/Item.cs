using Node.Inventory.Items.Attributes;

namespace Node.Inventory.Items.Types
{
    public class Item : ItemEntity
    {
        public Item(
            string entityName, int entityId, ItemType type, int entityOwnerID, int entityLocationID,
            int entityFlag, bool entityContraband, bool entitySingleton, int entityQuantity,
            double entityX, double entityY, double entityZ,
            string entityCustomInfo, AttributeList attributes, ItemFactory itemFactory
        ) : base(
            entityName, entityId, type, entityOwnerID, entityLocationID, entityFlag,
            entityContraband, entitySingleton, entityQuantity, entityX, entityY, entityZ,
            entityCustomInfo, attributes, itemFactory)
        {
        }
    }
}