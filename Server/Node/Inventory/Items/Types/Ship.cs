using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items.Types
{
    public class Ship : ItemInventory
    {
        public Ship(ItemEntity from) : base(from)
        {
        }

        public PyDictionary GetEffects()
        {
	        // for now return no data
	        return new PyDictionary();
        }
    }
}