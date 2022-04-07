using EVESharp.Node.Exceptions.character;
using EVESharp.Node.StaticData.Inventory;

namespace EVESharp.Node.Inventory.Items.Types;

public class Implant : ItemEntity
{
    public Implant(Information.Item info) : base(info)
    {
    }
        
    // check prerequirements for this item
    public override void CheckPrerequisites(Character character)
    {
        base.CheckPrerequisites(character);

        // check if the implant requires other implant used
        if (this.Attributes.AttributeExists(StaticData.Inventory.Attributes.prereqimplant) == false)
            return;

        int typeID = (int) this.Attributes[StaticData.Inventory.Attributes.prereqimplant].Integer;

        if (character.PluggedInImplantsByTypeID.ContainsKey(typeID) == false)
            throw new PrereqImplantMissing(typeID);
    }
}