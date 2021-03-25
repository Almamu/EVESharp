using Node.Inventory.Items.Dogma;

namespace Node.Inventory.Items.Attributes
{
    public class AttributeModifier
    {
        public Association Modification { get; init; }
        public ItemAttribute Value { get; init; }
    }
}