using EVESharp.Node.Exceptions.character;

namespace EVESharp.Node.Inventory.Items.Types;

public class Implant : ItemEntity
{
    public Implant (Information.Item info) : base (info) { }

    // check prerequirements for this item
    public override void CheckPrerequisites (Character character)
    {
        base.CheckPrerequisites (character);

        // check if the implant requires other implant used
        if (Attributes.AttributeExists (StaticData.Inventory.Attributes.prereqimplant) == false)
            return;

        int typeID = (int) Attributes [StaticData.Inventory.Attributes.prereqimplant].Integer;

        if (character.PluggedInImplantsByTypeID.ContainsKey (typeID) == false)
            throw new PrereqImplantMissing (typeID);
    }
}