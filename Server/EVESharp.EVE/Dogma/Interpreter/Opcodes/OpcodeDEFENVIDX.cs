using System;
using System.IO;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Dogma.Exception;
using EVESharp.EVE.Exceptions;

namespace EVESharp.EVE.Dogma.Interpreter.Opcodes;

public class OpcodeDEFENVIDX : Opcode
{
    public Data.Dogma.Environment Environment { get; private set; }

    public OpcodeDEFENVIDX (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        if (Enum.TryParse (reader.ReadString (), out Data.Dogma.Environment environment) == false)
            throw new DogmaMachineException ("Cannot determine environment id");

        this.Environment = environment;

        return this;
    }

    public ItemEntity GetItem ()
    {
        ItemEntity item = null;

        switch (this.Environment)
        {
            case Data.Dogma.Environment.Self:
                item = this.Interpreter.Environment.Self;
                break;

            case Data.Dogma.Environment.Char:
                item = this.Interpreter.Environment.Character;
                break;

            case Data.Dogma.Environment.Ship:
                item = this.Interpreter.Environment.Ship;
                break;

            case Data.Dogma.Environment.Target:
            case Data.Dogma.Environment.Area:
            case Data.Dogma.Environment.Other:
            case Data.Dogma.Environment.Charge:
            default:
                throw new CustomError ("Unsupported target");
        }

        return item;
    }
}