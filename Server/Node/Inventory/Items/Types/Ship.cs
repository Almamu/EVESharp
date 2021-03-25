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
        public Dictionary<int, ItemEntity> ActiveModules =>
            this.Items
                .Where(x =>
                    (x.Value.Flag == ItemFlags.HiSlot0 || x.Value.Flag == ItemFlags.HiSlot1 || x.Value.Flag == ItemFlags.HiSlot2 ||
                    x.Value.Flag == ItemFlags.HiSlot3 || x.Value.Flag == ItemFlags.HiSlot4 || x.Value.Flag == ItemFlags.HiSlot5 ||
                    x.Value.Flag == ItemFlags.HiSlot6 || x.Value.Flag == ItemFlags.HiSlot7 ||
                    x.Value.Flag == ItemFlags.MedSlot0 || x.Value.Flag == ItemFlags.MedSlot1 || x.Value.Flag == ItemFlags.MedSlot2 ||
                    x.Value.Flag == ItemFlags.MedSlot3 || x.Value.Flag == ItemFlags.MedSlot4 || x.Value.Flag == ItemFlags.MedSlot5 ||
                    x.Value.Flag == ItemFlags.MedSlot6 || x.Value.Flag == ItemFlags.MedSlot7 ||
                    x.Value.Flag == ItemFlags.LoSlot0 || x.Value.Flag == ItemFlags.LoSlot1 || x.Value.Flag == ItemFlags.LoSlot2 ||
                    x.Value.Flag == ItemFlags.LoSlot3 || x.Value.Flag == ItemFlags.LoSlot4 || x.Value.Flag == ItemFlags.LoSlot5 ||
                    x.Value.Flag == ItemFlags.LoSlot6 || x.Value.Flag == ItemFlags.LoSlot7) && x.Value.Attributes[AttributeEnum.isOnline] == 1)
                .ToDictionary(x => x.Key, x => x.Value);
        
        public Dictionary<int, ItemEntity> HighSlotModules =>
            this.Items
                .Where(x =>
                    (x.Value.Flag == ItemFlags.HiSlot0 || x.Value.Flag == ItemFlags.HiSlot1 || x.Value.Flag == ItemFlags.HiSlot2 ||
                     x.Value.Flag == ItemFlags.HiSlot3 || x.Value.Flag == ItemFlags.HiSlot4 || x.Value.Flag == ItemFlags.HiSlot5 ||
                     x.Value.Flag == ItemFlags.HiSlot6 || x.Value.Flag == ItemFlags.HiSlot7) && x.Value.Attributes[AttributeEnum.isOnline] == 1)
                .ToDictionary(x => x.Key, x => x.Value);
        
        public Dictionary<int, ItemEntity> MediumSlotModules => 
            this.Items
                .Where(x =>
                    (x.Value.Flag == ItemFlags.MedSlot0 || x.Value.Flag == ItemFlags.MedSlot1 || x.Value.Flag == ItemFlags.MedSlot2 ||
                     x.Value.Flag == ItemFlags.MedSlot3 || x.Value.Flag == ItemFlags.MedSlot4 || x.Value.Flag == ItemFlags.MedSlot5 ||
                     x.Value.Flag == ItemFlags.MedSlot6 || x.Value.Flag == ItemFlags.MedSlot7) && x.Value.Attributes[AttributeEnum.isOnline] == 1)
                .ToDictionary(x => x.Key, x => x.Value);
        
        public Dictionary<int, ItemEntity> LowSlotModules => 
            this.Items
                .Where(x =>
                    (x.Value.Flag == ItemFlags.LoSlot0 || x.Value.Flag == ItemFlags.LoSlot1 || x.Value.Flag == ItemFlags.LoSlot2 ||
                     x.Value.Flag == ItemFlags.LoSlot3 || x.Value.Flag == ItemFlags.LoSlot4 || x.Value.Flag == ItemFlags.LoSlot5 ||
                     x.Value.Flag == ItemFlags.LoSlot6 || x.Value.Flag == ItemFlags.LoSlot7) && x.Value.Attributes[AttributeEnum.isOnline] == 1)
                .ToDictionary(x => x.Key, x => x.Value);
        
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

        public ItemAttribute CPULoad
        {
            get => this.Attributes[AttributeEnum.cpuLoad];
            set => this.Attributes[AttributeEnum.cpuLoad] = value;
        }
        public ItemAttribute CPUOutput
        {
            get => this.Attributes[AttributeEnum.cpuOutput];
            set => this.Attributes[AttributeEnum.cpuOutput] = value;
        }
        public ItemAttribute PowerLoad
        {
            get => this.Attributes[AttributeEnum.powerLoad];
            set => this.Attributes[AttributeEnum.powerLoad] = value;
        }
        public ItemAttribute PowerOutput
        {
            get => this.Attributes[AttributeEnum.powerOutput];
            set => this.Attributes[AttributeEnum.powerOutput] = value;
        }
    }
}