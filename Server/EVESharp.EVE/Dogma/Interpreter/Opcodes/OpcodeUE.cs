using System.IO;
using EVESharp.EVE.Dogma.Exception;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Packets.Exceptions;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

/// <summary>
/// UE stands for UserError
/// </summary>
public class OpcodeUE : OpcodeWithBooleanOutput
{
    public OpcodeDEFSTRING LeftSide { get; private set; }

    public OpcodeUE (Interpreter interpreter) : base (interpreter) { }

    public override Opcode LoadOpcode (BinaryReader reader)
    {
        Opcode leftSide = Interpreter.Step (reader);

        if (leftSide is not OpcodeDEFSTRING defstring)
            throw new DogmaMachineException ("OpcodeUE (user error) must specify a exception name to throw");

        LeftSide = defstring;

        return this;
    }

    public override bool Execute ()
    {
        throw new CustomError (LeftSide.Definition);
    }
}