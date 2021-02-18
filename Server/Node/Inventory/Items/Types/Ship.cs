using System;
using System.Collections.Generic;
using Node.Exceptions.Internal;
using Node.Exceptions.ship;
using Node.Inventory.Items.Attributes;

namespace Node.Inventory.Items.Types
{
    public class Ship : ItemInventory
    {
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