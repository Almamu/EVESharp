/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Configuration;

namespace Node.Inventory.Items
{
    public abstract class ItemInventory : ItemEntity
    {
        public ItemInventory(ItemEntity from) : base(from.Name, from.ID, from.Type, from.OwnerID, from.LocationID,
            from.Flag, from.Contraband, from.Singleton, from.Quantity, from.X, from.Y, from.Z, from.CustomInfo,
            from.Attributes, from.ItemFactory)
        {
        }

        protected virtual void LoadContents(ItemFlags ignoreFlags = ItemFlags.None)
        {
            lock (this)
            {
                this.mContentsLoaded = true;

                this.mItems = this.ItemFactory.ItemManager.LoadItemsLocatedAt(this, ignoreFlags);
            }
        }

        public bool ContentsLoaded
        {
            get => this.mContentsLoaded;
            set => this.mContentsLoaded = value;
        }

        public Dictionary<int, ItemEntity> Items
        {
            get
            {
                if (this.mContentsLoaded == false)
                    this.LoadContents();

                return this.mItems;
            }
            
            protected set => this.mItems = value;
        }

        public virtual void AddItem(ItemEntity item)
        {
            // do not add anything if the inventory is not loaded
            // this prevents loading the full inventory for operations
            // that don't really need it
            if (this.mContentsLoaded == false)
                return;
            
            this.Items[item.ID] = item;
        }

        public void RemoveItem(ItemEntity item)
        {
            if (this.mContentsLoaded == false)
                return;

            this.Items.Remove(item.ID);
        }

        protected override void SaveToDB()
        {
            base.SaveToDB();
            
            if (this.mContentsLoaded == true)
            {
                // persist all the items
                lock(this.Items)
                    foreach (KeyValuePair<int, ItemEntity> pair in this.Items)
                        pair.Value.Persist();
            }
        }

        public override void Destroy()
        {
            // first destroy all the items inside this inventory
            // this might trigger the item loading mechanism but it's needed to ensure
            // that all the childs are removed off the database too
            this.ItemFactory.ItemManager.DestroyItems(this.Items);

            // finally call our base destroy method as this will get rid of the item from the database
            // for good
            base.Destroy();
        }

        public override void Unload()
        {
            base.Unload();
            
            // unload all the items loaded in this inventory if they're loaded
            if (this.ContentsLoaded == true)
            {
                this.ContentsLoaded = false;
                
                lock(this.Items)
                    foreach (KeyValuePair<int, ItemEntity> pair in this.Items)
                        if(this.ItemFactory.ItemManager.IsItemLoaded(pair.Key) == true)
                            pair.Value.Unload();

                // set the item list to null again
                this.mItems = null;
            }
        }

        protected Dictionary<int, ItemEntity> mItems;
        private bool mContentsLoaded = false;
    }
}