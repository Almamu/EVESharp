using System.IO;
using EVESharp.EVE.Dogma.Exception;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Sessions;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

/// <summary>
/// AIM stands for AddItemModifier
/// </summary>
public class OpcodeAIM : OpcodeRunnable
{
    public OpcodeEFF          Change    { get; private set; }
    public OpcodeDEFATTRIBUTE Attribute { get; private set; }

    public OpcodeAIM (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        Opcode leftSide  = Interpreter.Step (reader);
        Opcode rightSide = Interpreter.Step (reader);

        // ensure that both sides can return a value
        if (leftSide is not OpcodeEFF left)
            throw new DogmaMachineException ("The left side of a AIM operand must be EFF");
        if (rightSide is not OpcodeDEFATTRIBUTE right)
            throw new DogmaMachineException ("The right side of a AIM operand must be DEFATTRIBUTE");

        Change    = left;
        Attribute = right;

        return this;
    }

    public override void Execute ()
    {
        ItemEntity     item      = Change.RightSide.ItemToAffect.GetItem ();
        ItemEntity     target    = Interpreter.Environment.Self;
        AttributeTypes attribute = Change.RightSide.AttributeToAffect.Attribute;

        // add the modifier to the attribute
        item.Attributes [attribute].AddModifier (Change.LeftSide.Association, target.Attributes [Attribute.Attribute]);

        // notify the character
        Interpreter.Environment.ItemFactory.DogmaUtils.NotifyAttributeChange (
            Interpreter.Environment.Session.EnsureCharacterIsSelected (),
            attribute,
            item
        );
    }
}