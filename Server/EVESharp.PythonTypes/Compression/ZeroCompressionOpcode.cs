namespace EVESharp.PythonTypes.Compression;

/// <summary>
/// Easier representation of the opcode for the zero compression which has information on the block
/// like how much zeros were compressed 
/// </summary>
public struct ZeroCompressionOpcode
{
    /// <summary>
    /// The length of the first part of the block
    /// </summary>
    public byte FirstLength;

    /// <summary>
    /// Indicates if the first part of the block is zero
    /// </summary>
    public bool FirstIsZero;

    /// <summary>
    /// The length of the second part of the block
    /// </summary>
    public byte SecondLength;

    /// <summary>
    /// Indicates if the second part of the block is zero
    /// </summary>
    public bool SecondIsZero;

    public ZeroCompressionOpcode (byte firstLength, bool firstIsZero, byte secondLength, bool secondIsZero)
    {
        this.FirstLength  = firstLength;
        this.FirstIsZero  = firstIsZero;
        this.SecondLength = secondLength;
        this.SecondIsZero = secondIsZero;
    }

    public static implicit operator byte (ZeroCompressionOpcode opcode)
    {
        byte value = 0;

        value |= (byte) (opcode.FirstLength & 0x07);
        value |= (byte) (opcode.FirstIsZero ? 0x08 : 0x00);
        value |= (byte) ((opcode.SecondLength & 0x07) << 4);
        value |= (byte) (opcode.SecondIsZero ? 0x80 : 0x00);

        return value;
    }

    public static implicit operator ZeroCompressionOpcode (byte source)
    {
        return new ZeroCompressionOpcode (
            (byte) (source & 0x07),
            (source & 0x08) == 0x08,
            (byte) ((source & 0x70) >> 4),
            (source & 0x80) == 0x80
        );
    }
}