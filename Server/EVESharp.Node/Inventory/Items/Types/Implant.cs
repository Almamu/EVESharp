using EVESharp.EVE.Client.Exceptions.character;

namespace EVESharp.Node.Inventory.Items.Types;

public class Implant : ItemEntity
{
    public Implant (Information.Item info) : base (info) { }

    // check prerequirements for this item
    public override void CheckPrerequisites (Character character)
    {
        base.CheckPrerequisites (character);

        // check if the implant requires other implant used
        if (Attributes.AttributeExists (EVE.StaticData.Inventory.Attributes.prereqimplant) == false)
            return;

        int typeID = (int) Attributes [EVE.StaticData.Inventory.Attributes.prereqimplant].Integer;

        if (character.PluggedInImplantsByTypeID.ContainsKey (typeID) == false)
            throw new PrereqImplantMissing (typeID);
    }
}