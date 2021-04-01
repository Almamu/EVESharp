using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Node.Exceptions.Internal;
using Node.Exceptions.ship;
using Node.Inventory.Items.Attributes;

namespace Node.Inventory.Items.Types
{
    public class Ship : ItemInventory
    {
        public Dictionary<ItemFlags, ItemEntity> ActiveModules =>
            this.Items
                .Where(x =>
                    x.Value.Flag == ItemFlags.HiSlot0 || x.Value.Flag == ItemFlags.HiSlot1 || x.Value.Flag == ItemFlags.HiSlot2 ||
                    x.Value.Flag == ItemFlags.HiSlot3 || x.Value.Flag == ItemFlags.HiSlot4 || x.Value.Flag == ItemFlags.HiSlot5 ||
                    x.Value.Flag == ItemFlags.HiSlot6 || x.Value.Flag == ItemFlags.HiSlot7 ||
                    x.Value.Flag == ItemFlags.MedSlot0 || x.Value.Flag == ItemFlags.MedSlot1 || x.Value.Flag == ItemFlags.MedSlot2 ||
                    x.Value.Flag == ItemFlags.MedSlot3 || x.Value.Flag == ItemFlags.MedSlot4 || x.Value.Flag == ItemFlags.MedSlot5 ||
                    x.Value.Flag == ItemFlags.MedSlot6 || x.Value.Flag == ItemFlags.MedSlot7 ||
                    x.Value.Flag == ItemFlags.LoSlot0 || x.Value.Flag == ItemFlags.LoSlot1 || x.Value.Flag == ItemFlags.LoSlot2 ||
                    x.Value.Flag == ItemFlags.LoSlot3 || x.Value.Flag == ItemFlags.LoSlot4 || x.Value.Flag == ItemFlags.LoSlot5 ||
                    x.Value.Flag == ItemFlags.LoSlot6 || x.Value.Flag == ItemFlags.LoSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Dictionary<ItemFlags, ItemEntity> RigSlots =>
            this.Items
                .Where(x =>
                    x.Value.Flag == ItemFlags.RigSlot0 || x.Value.Flag == ItemFlags.RigSlot1 || x.Value.Flag == ItemFlags.RigSlot2 ||
                    x.Value.Flag == ItemFlags.RigSlot3 || x.Value.Flag == ItemFlags.RigSlot4 || x.Value.Flag == ItemFlags.RigSlot5 ||
                    x.Value.Flag == ItemFlags.RigSlot6 || x.Value.Flag == ItemFlags.RigSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Dictionary<ItemFlags, ItemEntity> HighSlotModules =>
            this.Items
                .Where(x =>
                    x.Value.Flag == ItemFlags.HiSlot0 || x.Value.Flag == ItemFlags.HiSlot1 || x.Value.Flag == ItemFlags.HiSlot2 ||
                     x.Value.Flag == ItemFlags.HiSlot3 || x.Value.Flag == ItemFlags.HiSlot4 || x.Value.Flag == ItemFlags.HiSlot5 ||
                     x.Value.Flag == ItemFlags.HiSlot6 || x.Value.Flag == ItemFlags.HiSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Dictionary<ItemFlags, ItemEntity> MediumSlotModules => 
            this.Items
                .Where(x =>
                    x.Value.Flag == ItemFlags.MedSlot0 || x.Value.Flag == ItemFlags.MedSlot1 || x.Value.Flag == ItemFlags.MedSlot2 ||
                     x.Value.Flag == ItemFlags.MedSlot3 || x.Value.Flag == ItemFlags.MedSlot4 || x.Value.Flag == ItemFlags.MedSlot5 ||
                     x.Value.Flag == ItemFlags.MedSlot6 || x.Value.Flag == ItemFlags.MedSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Dictionary<ItemFlags, ItemEntity> LowSlotModules => 
            this.Items
                .Where(x =>
                    x.Value.Flag == ItemFlags.LoSlot0 || x.Value.Flag == ItemFlags.LoSlot1 || x.Value.Flag == ItemFlags.LoSlot2 ||
                     x.Value.Flag == ItemFlags.LoSlot3 || x.Value.Flag == ItemFlags.LoSlot4 || x.Value.Flag == ItemFlags.LoSlot5 ||
                     x.Value.Flag == ItemFlags.LoSlot6 || x.Value.Flag == ItemFlags.LoSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Ship(ItemEntity from) : base(from)
        {
        }

        protected override void LoadContents(ItemFlags ignoreFlags = ItemFlags.None)
        {
            base.LoadContents(ItemFlags.Pilot);
        }

        public override void Destroy()
        {
            base.Destroy();
            
            // remove insurance off the database
            this.ItemFactory.InsuranceDB.UnInsureShip(this.ID);
        }
    }
}