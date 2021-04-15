using System;
using System.Collections.Generic;
using Node.StaticData.Inventory;

namespace Node.Inventory.Items
{
    public class ItemInventoryByOwnerID : ItemInventory
    {
        public int ItemOwnerID { get; }
        
        public ItemInventoryByOwnerID(int ownerID, ItemInventory @from) : base(@from)
        {
            this.ItemOwnerID = ownerID;
        }

        protected override void LoadContents(Flags ignoreFlags = Flags.None)
        {
            lock (this)
            {
                this.mItems = this.ItemFactory.LoadItemsLocatedAtByOwner(this, this.ItemOwnerID);
                
                this.ContentsLoaded = true;
            }
        }

        public override void Destroy()
        {
            throw new NotSupportedException("Meta Inventories cannot be destroyed!");
        }
    }
}