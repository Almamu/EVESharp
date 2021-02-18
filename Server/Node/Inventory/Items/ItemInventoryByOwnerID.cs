using System.Collections.Generic;

namespace Node.Inventory.Items
{
    public class ItemInventoryByOwnerID : ItemInventory
    {
        public int ItemOwnerID { get; }
        
        public ItemInventoryByOwnerID(int ownerID, ItemInventory @from) : base(@from)
        {
            this.ItemOwnerID = ownerID;
        }

        protected override void LoadContents(ItemFlags ignoreFlags = ItemFlags.None)
        {
            lock (this)
            {
                this.ContentsLoaded = true;

                this.mItems = this.ItemFactory.ItemManager.LoadItemsLocatedAtByOwner(this, this.ItemOwnerID);
            }
        }

        public override void Dispose()
        {
            // meta inventories should not unload the original item
            if (this.ContentsLoaded == true)
            {
                this.ContentsLoaded = false;
                
                lock (this.Items)
                    foreach (KeyValuePair<int, ItemEntity> pair in this.Items)
                        if(this.ItemFactory.ItemManager.IsItemLoaded(pair.Key) == true)
                            pair.Value.Dispose();

                this.Items = null;
            }
        }
    }
}