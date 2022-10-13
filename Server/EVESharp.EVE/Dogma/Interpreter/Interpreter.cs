using System;
using System.Collections.Generic;
using System.IO;
using EVESharp.Database.Dogma;
using EVESharp.EVE.Dogma.Exception;
using EVESharp.EVE.Dogma.Interpreter.Opcodes;

namespace EVESharp.EVE.Dogma.Interpreter;

public class Interpreter
{
    private readonly Dictionary <EffectOperand, Type> mOpcodes = new Dictionary <EffectOperand, Type> ();
    public           Environment                      Environment { get; }

    public Interpreter (Environment environment)
    {
        this.Environment = environment;

        // register all the opcode handlers implemented for now
        this.mOpcodes [EffectOperand.DEFSTRING]      = typeof (OpcodeDEFSTRING);
        this.mOpcodes [EffectOperand.SKILLCHECK]     = typeof (OpcodeSKILLCHECK);
        this.mOpcodes [EffectOperand.AND]            = typeof (OpcodeAND);
        this.mOpcodes [EffectOperand.DEFENVIDX]      = typeof (OpcodeDEFENVIDX);
        this.mOpcodes [EffectOperand.GET]            = typeof (OpcodeGET);
        this.mOpcodes [EffectOperand.DEFATTRIBUTE]   = typeof (OpcodeDEFATTRIBUTE);
        this.mOpcodes [EffectOperand.GTE]            = typeof (OpcodeGTE);
        this.mOpcodes [EffectOperand.ADD]            = typeof (OpcodeADD);
        this.mOpcodes [EffectOperand.IF]             = typeof (OpcodeIF);
        this.mOpcodes [EffectOperand.ATT]            = typeof (OpcodeATT);
        this.mOpcodes [EffectOperand.SET]            = typeof (OpcodeSET);
        this.mOpcodes [EffectOperand.DEFINT]         = typeof (OpcodeDEFINT);
        this.mOpcodes [EffectOperand.OR]             = typeof (OpcodeOR);
        this.mOpcodes [EffectOperand.COMBINE]        = typeof (OpcodeCOMBINE);
        this.mOpcodes [EffectOperand.AIM]            = typeof (OpcodeAIM);
        this.mOpcodes [EffectOperand.RIM]            = typeof (OpcodeRIM);
        this.mOpcodes [EffectOperand.EFF]            = typeof (OpcodeEFF);
        this.mOpcodes [EffectOperand.DEFASSOCIATION] = typeof (OpcodeDEFASSOCIATION);
        this.mOpcodes [EffectOperand.UE]             = typeof (OpcodeUE);
        this.mOpcodes [EffectOperand.GT]             = typeof (OpcodeGT);
    }

    public Opcode Step (BinaryReader reader)
    {
        EffectOperand operand = (EffectOperand) reader.ReadByte ();

        if (this.mOpcodes.TryGetValue (operand, out Type opcodeType) == false)
            throw new DogmaMachineException ($"Unknown opcode {operand}");

        // create a new instance for this opcode
        Opcode handler = (Opcode) Activator.CreateInstance (opcodeType, this);

        // load it with the required data
        return handler.LoadOpcode (reader);
    }

    public Opcode Run (byte [] code)
    {
        MemoryStream stream = new MemoryStream (code);
        BinaryReader reader = new BinaryReader (stream);

        return this.Step (reader);
    }
}