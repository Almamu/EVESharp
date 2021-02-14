using System;
using System.Collections.Generic;
using Node.Exceptions.character;
using Node.Exceptions.Internal;
using Node.Exceptions.ship;
using Node.Inventory.Items.Attributes;
using PythonTypes.Types.Exceptions;

namespace Node.Inventory.Items.Types
{
    public class Implant : ItemEntity
    {
        public Implant(ItemEntity from) : base(from)
        {
        }
        
        // check prerequirements for this item
        public override void CheckPrerequisites(Character character)
        {
            base.CheckPrerequisites(character);

            // check if the implant requires other implant used
            if (this.Attributes.AttributeExists(AttributeEnum.prereqimplant) == false)
                return;

            int typeID = (int) this.Attributes[AttributeEnum.prereqimplant].Integer;
            ItemType type = this.mItemFactory.TypeManager[typeID];

            if (character.PluggedInImplantsByTypeID.ContainsKey(typeID) == false)
                throw new PrereqImplantMissing(type);
        }
    }
}