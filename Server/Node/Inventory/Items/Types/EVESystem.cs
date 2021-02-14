using System;

namespace Node.Inventory.Items.Types
{
    public class EVESystem : ItemInventory
    {
        public EVESystem(ItemEntity @from) : base(@from)
        {
        }

        protected override void LoadContents(ItemFlags ignoreFlags = ItemFlags.None)
        {
            throw new NotSupportedException("EVE System Items cannot load any content in them");
        }
    }
}