using System.IO;
using EVESharp.EVE.Dogma.Exception;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Sessions;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeSET : OpcodeRunnable
{
    public Opcode LeftSide { get; private set; }
    public Opcode Value    { get; private set; }

    public OpcodeSET (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        LeftSide = Interpreter.Step (reader);
        Value    = Interpreter.Step (reader);

        return this;
    }

    public override void Execute ()
    {
        if (LeftSide is OpcodeATT att)
        {
            if (Value is OpcodeDEFINT defint)
            {
                ItemEntity     item      = att.ItemToAffect.GetItem ();
                AttributeTypes attribute = att.AttributeToAffect.Attribute;

                item.Attributes [att.AttributeToAffect.Attribute].Integer = defint.Value;

                // notify the character
                Interpreter.Environment.ItemFactory.DogmaUtils.NotifyAttributeChange (
                    Interpreter.Environment.Session.EnsureCharacterIsSelected (),
                    attribute,
                    item
                );
            }
        }
        else
        {
            throw new DogmaMachineException ("Unexpected parameter for left side of SET opcode");
        }
    }
}