using System;
using System.Collections.Generic;
using EVESharp.Node.StaticData.Inventory;

namespace EVESharp.Node.Inventory.Items
{
    public class ItemInventoryByOwnerID : ItemInventory
    {
        private int mOwnerID;
        private Flags mInventoryFlag;

        public override int OwnerID
        {
            get => this.mOwnerID;
            set => this.mOwnerID = value;
        }

        public Flags InventoryFlag => this.mInventoryFlag;

        public ItemInventoryByOwnerID(int ownerID, Flags flag, ItemInventory @from) : base(@from)
        {
            this.mOwnerID = ownerID;
            this.mInventoryFlag = flag;
        }

        protected override void LoadContents(Flags ignoreFlags = Flags.None)
        {
            lock (this)
            {
                this.mItems = this.ItemFactory.LoadItemsLocatedAtByOwner(this, this.OwnerID, this.InventoryFlag);
                
                this.ContentsLoaded = true;
            }
        }

        public override void Destroy()
        {
            throw new NotSupportedException("Meta Inventories cannot be destroyed!");
        }
    }
}