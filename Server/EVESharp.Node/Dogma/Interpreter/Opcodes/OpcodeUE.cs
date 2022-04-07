using System.IO;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Dogma.Exception;

namespace EVESharp.Node.Dogma.Interpreter.Opcodes;

/// <summary>
/// UE stands for UserError
/// </summary>
public class OpcodeUE : OpcodeWithBooleanOutput
{
    public OpcodeDEFSTRING LeftSide { get; private set; }
        
    public OpcodeUE(Interpreter interpreter) : base(interpreter)
    {
    }

    public override Opcode LoadOpcode(BinaryReader reader)
    {
        Opcode leftSide = this.Interpreter.Step(reader);

        if (leftSide is not OpcodeDEFSTRING defstring)
            throw new DogmaMachineException("OpcodeUE (user error) must specify a exception name to throw");

        this.LeftSide = defstring;

        return this;
    }

    public override bool Execute()
    {
        throw new CustomError(this.LeftSide.Definition);
    }
}