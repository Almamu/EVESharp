using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Node.Exceptions.Internal;
using Node.Exceptions.ship;
using Node.Inventory.Items.Attributes;
using Node.StaticData.Inventory;

namespace Node.Inventory.Items.Types
{
    public class Ship : ItemInventory
    {
        public Dictionary<Flags, ItemEntity> ActiveModules =>
            this.Items
                .Where(x =>
                    x.Value.Flag == Flags.HiSlot0 || x.Value.Flag == Flags.HiSlot1 || x.Value.Flag == Flags.HiSlot2 ||
                    x.Value.Flag == Flags.HiSlot3 || x.Value.Flag == Flags.HiSlot4 || x.Value.Flag == Flags.HiSlot5 ||
                    x.Value.Flag == Flags.HiSlot6 || x.Value.Flag == Flags.HiSlot7 ||
                    x.Value.Flag == Flags.MedSlot0 || x.Value.Flag == Flags.MedSlot1 || x.Value.Flag == Flags.MedSlot2 ||
                    x.Value.Flag == Flags.MedSlot3 || x.Value.Flag == Flags.MedSlot4 || x.Value.Flag == Flags.MedSlot5 ||
                    x.Value.Flag == Flags.MedSlot6 || x.Value.Flag == Flags.MedSlot7 ||
                    x.Value.Flag == Flags.LoSlot0 || x.Value.Flag == Flags.LoSlot1 || x.Value.Flag == Flags.LoSlot2 ||
                    x.Value.Flag == Flags.LoSlot3 || x.Value.Flag == Flags.LoSlot4 || x.Value.Flag == Flags.LoSlot5 ||
                    x.Value.Flag == Flags.LoSlot6 || x.Value.Flag == Flags.LoSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Dictionary<Flags, ItemEntity> RigSlots =>
            this.Items
                .Where(x =>
                    x.Value.Flag == Flags.RigSlot0 || x.Value.Flag == Flags.RigSlot1 || x.Value.Flag == Flags.RigSlot2 ||
                    x.Value.Flag == Flags.RigSlot3 || x.Value.Flag == Flags.RigSlot4 || x.Value.Flag == Flags.RigSlot5 ||
                    x.Value.Flag == Flags.RigSlot6 || x.Value.Flag == Flags.RigSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Dictionary<Flags, ItemEntity> HighSlotModules =>
            this.Items
                .Where(x =>
                    x.Value.Flag == Flags.HiSlot0 || x.Value.Flag == Flags.HiSlot1 || x.Value.Flag == Flags.HiSlot2 ||
                     x.Value.Flag == Flags.HiSlot3 || x.Value.Flag == Flags.HiSlot4 || x.Value.Flag == Flags.HiSlot5 ||
                     x.Value.Flag == Flags.HiSlot6 || x.Value.Flag == Flags.HiSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Dictionary<Flags, ItemEntity> MediumSlotModules => 
            this.Items
                .Where(x =>
                    x.Value.Flag == Flags.MedSlot0 || x.Value.Flag == Flags.MedSlot1 || x.Value.Flag == Flags.MedSlot2 ||
                     x.Value.Flag == Flags.MedSlot3 || x.Value.Flag == Flags.MedSlot4 || x.Value.Flag == Flags.MedSlot5 ||
                     x.Value.Flag == Flags.MedSlot6 || x.Value.Flag == Flags.MedSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Dictionary<Flags, ItemEntity> LowSlotModules => 
            this.Items
                .Where(x =>
                    x.Value.Flag == Flags.LoSlot0 || x.Value.Flag == Flags.LoSlot1 || x.Value.Flag == Flags.LoSlot2 ||
                     x.Value.Flag == Flags.LoSlot3 || x.Value.Flag == Flags.LoSlot4 || x.Value.Flag == Flags.LoSlot5 ||
                     x.Value.Flag == Flags.LoSlot6 || x.Value.Flag == Flags.LoSlot7)
                .ToDictionary(x => x.Value.Flag, x => x.Value);
        
        public Ship(ItemEntity from) : base(from)
        {
        }

        protected override void LoadContents(Flags ignoreFlags = Flags.None)
        {
            base.LoadContents(Flags.Pilot);
        }

        public override void Destroy()
        {
            base.Destroy();
            
            // remove insurance off the database
            this.ItemFactory.InsuranceDB.UnInsureShip(this.ID);
        }
    }
}