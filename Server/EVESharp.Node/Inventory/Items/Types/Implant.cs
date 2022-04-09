using EVESharp.EVE.Client.Exceptions.character;
using EVESharp.EVE.StaticData.Inventory;

namespace EVESharp.Node.Inventory.Items.Types;

public class Implant : ItemEntity
{
    public Implant (Information.Item info) : base (info) { }

    // check prerequirements for this item
    public override void CheckPrerequisites (Character character)
    {
        base.CheckPrerequisites (character);

        // check if the implant requires other implant used
        if (Attributes.AttributeExists (AttributeTypes.prereqimplant) == false)
            return;

        int typeID = (int) Attributes [AttributeTypes.prereqimplant].Integer;

        if (character.PluggedInImplantsByTypeID.ContainsKey (typeID) == false)
            throw new PrereqImplantMissing (typeID);
    }
}