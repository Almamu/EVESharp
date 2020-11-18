/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
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
using Node.Database;
using Node.Inventory.Items.Attributes;

namespace Node.Inventory.Items
{
    public abstract class ItemInventory : ItemEntity
    {
        public ItemInventory(ItemEntity from) : base(from.Name, from.ID, from.Type, from.Owner, from.Location, from.Flag, from.Contraband, from.Singleton, from.Quantity, from.X, from.Y, from.Z, from.CustomInfo, from.Attributes, from.mItemFactory)
        {
        }

        protected virtual void LoadContents()
        {
            this.mContentsLoaded = true;
        }

        public List<ItemEntity> Items
        {
            get
            {
                if (this.mContentsLoaded == false)
                    this.LoadContents();

                return this.mItems;
            }
        }
        
        private List<ItemEntity> mItems;
        private bool mContentsLoaded = false;
    }
}