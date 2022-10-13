using System.IO;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Dogma.Exception;
using EVESharp.EVE.Sessions;

namespace EVESharp.EVE.Dogma.Interpreter.Opcodes;

public class OpcodeSET : OpcodeRunnable
{
    public Opcode LeftSide { get; private set; }
    public Opcode Value    { get; private set; }

    public OpcodeSET (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        this.LeftSide = this.Interpreter.Step (reader);
        this.Value    = this.Interpreter.Step (reader);

        return this;
    }

    public override void Execute ()
    {
        if (this.LeftSide is OpcodeATT att)
        {
            if (this.Value is OpcodeDEFINT defint)
            {
                ItemEntity     item      = att.ItemToAffect.GetItem ();
                AttributeTypes attribute = att.AttributeToAffect.Attribute;

                item.Attributes [att.AttributeToAffect.Attribute].Integer = defint.Value;

                // notify the character
                this.Interpreter.Environment.DogmaNotifications.NotifyAttributeChange (
                    this.Interpreter.Environment.Session.EnsureCharacterIsSelected (),
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