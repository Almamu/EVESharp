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

                this.mItems = this.mItemFactory.ItemManager.LoadItemsLocatedAtByOwner(this, this.ItemOwnerID);
            }
        }
    }
}