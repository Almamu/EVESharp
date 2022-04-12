using System.IO;
using EVESharp.EVE.StaticData.Dogma;

namespace EVESharp.EVE.Dogma;

public static class Extensions
{
    /// <summary>
    /// Writes a marshal opcode to the stream
    /// </summary>
    /// <param name="w">The binary writer in use</param>
    /// <param name="op">The opcode to write</param>
    public static void WriteOperand (this BinaryWriter w, EffectOperand op)
    {
        w.Write ((byte) op);
    }
}