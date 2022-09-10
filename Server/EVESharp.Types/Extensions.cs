using System;
using System.IO;
using EVESharp.Types.Serialization;

namespace EVESharp.Types;

public static class Extensions
{
    /// <summary>
    /// Writes a marshal opcode to the stream
    /// </summary>
    /// <param name="w">The binary writer in use</param>
    /// <param name="op">The opcode to write</param>
    public static void WriteOpcode (this BinaryWriter w, Opcode op)
    {
        w.Write ((byte) op);
    }
}