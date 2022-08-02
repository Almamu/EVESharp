using System;
using System.IO;
using EVESharp.EVE.Dogma.Exception;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Inventory.Items;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

public class OpcodeDEFENVIDX : Opcode
{
    public EVE.StaticData.Dogma.Environment Environment { get; private set; }

    public OpcodeDEFENVIDX (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        if (Enum.TryParse (reader.ReadString (), out EVE.StaticData.Dogma.Environment environment) == false)
            throw new DogmaMachineException ("Cannot determine environment id");

        Environment = environment;

        return this;
    }

    public ItemEntity GetItem ()
    {
        ItemEntity item = null;

        switch (Environment)
        {
            case EVE.StaticData.Dogma.Environment.Self:
                item = Interpreter.Environment.Self;
                break;

            case EVE.StaticData.Dogma.Environment.Char:
                item = Interpreter.Environment.Character;
                break;

            case EVE.StaticData.Dogma.Environment.Ship:
                item = Interpreter.Environment.Ship;
                break;

            case EVE.StaticData.Dogma.Environment.Target:
            case EVE.StaticData.Dogma.Environment.Area:
            case EVE.StaticData.Dogma.Environment.Other:
            case EVE.StaticData.Dogma.Environment.Charge:
            default:
                throw new CustomError ("Unsupported target");
        }

        return item;
    }
}