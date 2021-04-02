using System;
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
                this.mItems = this.ItemFactory.ItemManager.LoadItemsLocatedAtByOwner(this, this.ItemOwnerID);
                
                this.ContentsLoaded = true;
            }
        }

        public override void Destroy()
        {
            throw new NotSupportedException("Meta Inventories cannot be destroyed!");
        }
    }
}