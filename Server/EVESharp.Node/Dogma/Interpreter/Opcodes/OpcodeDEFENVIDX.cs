using System;
using System.IO;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Dogma.Exception;
using EVESharp.Node.Inventory.Items;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeDEFENVIDX : Opcode
{
    public Inventory.Items.Dogma.Environment Environment { get; private set; }

    public OpcodeDEFENVIDX (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        if (Enum.TryParse (reader.ReadString (), out Inventory.Items.Dogma.Environment environment) == false)
            throw new DogmaMachineException ("Cannot determine environment id");

        Environment = environment;

        return this;
    }

    public ItemEntity GetItem ()
    {
        ItemEntity item = null;

        switch (Environment)
        {
            case Inventory.Items.Dogma.Environment.Self:
                item = Interpreter.Environment.Self;

                break;
            case Inventory.Items.Dogma.Environment.Char:
                item = Interpreter.Environment.Character;

                break;
            case Inventory.Items.Dogma.Environment.Ship:
                item = Interpreter.Environment.Ship;

                break;
            case Inventory.Items.Dogma.Environment.Target:
            case Inventory.Items.Dogma.Environment.Area:
            case Inventory.Items.Dogma.Environment.Other:
            case Inventory.Items.Dogma.Environment.Charge:
            default:
                throw new CustomError ("Unsupported target");
        }

        return item;
    }
}