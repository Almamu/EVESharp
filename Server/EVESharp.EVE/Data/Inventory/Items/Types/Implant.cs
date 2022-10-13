using EVESharp.Database.Inventory.Attributes;
using EVESharp.EVE.Exceptions.character;

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class Implant : ItemEntity
{
    public Implant (Database.Inventory.Types.Information.Item info) : base (info) { }

    // check prerequirements for this item
    public override void CheckPrerequisites (Character character)
    {
        base.CheckPrerequisites (character);

        // check if the implant requires other implant used
        if (this.Attributes.AttributeExists (AttributeTypes.prereqimplant) == false)
            return;

        int typeID = (int) this.Attributes [AttributeTypes.prereqimplant].Integer;

        if (character.PluggedInImplantsByTypeID.ContainsKey (typeID) == false)
            throw new PrereqImplantMissing (typeID);
    }
}