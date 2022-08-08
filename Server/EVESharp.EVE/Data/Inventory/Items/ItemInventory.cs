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

using System.Collections.Concurrent;
using EVESharp.EVE.Data.Inventory.Items.Types.Information;

namespace EVESharp.EVE.Data.Inventory.Items;

public abstract class ItemInventory : ItemEntity
{
    public delegate ConcurrentDictionary <int, ItemEntity> InventoryLoadEventHandler (ItemInventory inventory, Flags ignoreFlags);

    public delegate void InventoryUnloadEventHandler (ItemInventory inventory);

    protected ConcurrentDictionary <int, ItemEntity> mItems;

    /// <summary>
    /// Event called by the item to request the inventory contents
    /// </summary>
    public InventoryLoadEventHandler OnInventoryLoad;
    /// <summary>
    /// Event called by the item to request the inventory to be unloaded
    /// </summary>
    public InventoryUnloadEventHandler OnInventoryUnload;

    public bool ContentsLoaded { get; set; }

    public new bool Singleton
    {
        get => base.Singleton;
        set
        {
            base.Singleton = value;

            // non-singleton inventories cannot be manipulated, so the items inside can be free'd
            if (base.Singleton == false)
                this.UnloadContents ();
        }
    }

    public ConcurrentDictionary <int, ItemEntity> Items
    {
        get
        {
            if (this.ContentsLoaded == false)
                this.LoadContents ();

            return this.mItems;
        }

        protected set => this.mItems = value;
    }

    protected ItemInventory (Item info) : base (info) { }

    public ItemInventory (ItemEntity from) : base (from) { }

    protected virtual void LoadContents (Flags ignoreFlags = Flags.None)
    {
        lock (this)
        {
            // ensure the list exists if no handler is there yet
            this.Items          = this.OnInventoryLoad?.Invoke (this, ignoreFlags) ?? new ConcurrentDictionary <int, ItemEntity> ();
            this.ContentsLoaded = true;
        }
    }

    protected virtual void UnloadContents ()
    {
        lock (this)
        {
            if (this.ContentsLoaded == false)
                return;

            this.OnInventoryUnload?.Invoke (this);
        }
    }

    public virtual void AddItem (ItemEntity item)
    {
        // do not add anything if the inventory is not loaded
        // this prevents loading the full inventory for operations
        // that don't really need it
        if (this.ContentsLoaded == false)
            return;

        lock (this.Items)
        {
            this.Items [item.ID] = item;
        }
    }

    public void RemoveItem (ItemEntity item)
    {
        if (this.ContentsLoaded == false)
            return;

        lock (this.Items)
        {
            this.Items.TryRemove (item.ID, out _);
        }
    }

    public override void Dispose ()
    {
        // trigger the unload of the contents of the inventory
        if (this.ContentsLoaded)
            this.UnloadContents ();

        base.Dispose ();
    }
}