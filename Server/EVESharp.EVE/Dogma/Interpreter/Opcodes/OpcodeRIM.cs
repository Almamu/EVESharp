using System.IO;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Dogma.Exception;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Dogma.Interpreter.Opcodes;

/// <summary>
/// RIM stands for RemoveItemModifier
/// </summary>
public class OpcodeRIM : OpcodeRunnable
{
    public OpcodeEFF          Change    { get; private set; }
    public OpcodeDEFATTRIBUTE Attribute { get; private set; }

    public OpcodeRIM (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        Opcode leftSide  = this.Interpreter.Step (reader);
        Opcode rightSide = this.Interpreter.Step (reader);

        // ensure that both sides can return a value
        if (leftSide is not OpcodeEFF left)
            throw new DogmaMachineException ("The left side of a RIM operand must be EFF");
        if (rightSide is not OpcodeDEFATTRIBUTE right)
            throw new DogmaMachineException ("The right side of a RIM operand must be DEFATTRIBUTE");

        this.Change    = left;
        this.Attribute = right;

        return this;
    }

    public override void Execute ()
    {
        ItemEntity     item      = this.Change.RightSide.ItemToAffect.GetItem ();
        ItemEntity     target    = this.Interpreter.Environment.Self;
        AttributeTypes attribute = this.Change.RightSide.AttributeToAffect.Attribute;

        // add the modifier to the attribute
        item.Attributes [attribute].RemoveModifier (this.Change.LeftSide.Association, target.Attributes [this.Attribute.Attribute]);

        // notify the character
        this.Interpreter.Environment.DogmaNotifications.NotifyAttributeChange (
            this.Interpreter.Environment.Session.EnsureCharacterIsSelected (),
            attribute,
            item
        );
    }
}