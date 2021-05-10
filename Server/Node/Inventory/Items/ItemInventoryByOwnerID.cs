using System;
using System.Collections.Generic;
using Node.StaticData.Inventory;

namespace Node.Inventory.Items
{
    public class ItemInventoryByOwnerID : ItemInventory
    {
        private int mOwnerID;

        public override int OwnerID
        {
            get => this.mOwnerID;
            set => this.mOwnerID = value;
        }

        public ItemInventoryByOwnerID(int ownerID, ItemInventory @from) : base(@from)
        {
            this.mOwnerID = ownerID;
        }

        protected override void LoadContents(Flags ignoreFlags = Flags.None)
        {
            lock (this)
            {
                this.mItems = this.ItemFactory.LoadItemsLocatedAtByOwner(this, this.OwnerID);
                
                this.ContentsLoaded = true;
            }
        }

        public override void Destroy()
        {
            throw new NotSupportedException("Meta Inventories cannot be destroyed!");
        }
    }
}