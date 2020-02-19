using System;
using System.IO;

namespace PythonTypes.Compression
{
    public class ZeroCompressionUtils
    {
        /// <summary>
        /// Decompress the zero-compressed data present in the reader's position.
        /// Every zero-compressed data begins with an extended size indicator
        /// <seealso cref="Extensions.WriteSizeEx(BinaryWriter,uint)"/>
        /// </summary>
        /// <param name="reader">The reader to read the compressed data from</param>
        /// <returns>The decompressed data as a MemoryStream for further usage</returns>
        public static MemoryStream LoadZeroCompressed(BinaryReader reader)
        {
            MemoryStream outputStream = new MemoryStream();
            BinaryWriter outputWriter = new BinaryWriter(outputStream);

            uint packedLen = reader.ReadSizeEx();
            long max = reader.BaseStream.Position + packedLen;

            while (reader.BaseStream.Position < max)
            {
                ZeroCompressionOpcode opcode = reader.ReadByte();

                if (opcode.FirstIsZero)
                {
                    for (int n = 0; n < (opcode.FirstLength + 1); n++)
                        outputWriter.Write((byte) 0);
                }
                else
                {
                    int bytes = (int) (Math.Min(8 - opcode.FirstLength, max - reader.BaseStream.Position));
                    
                    for (int n = 0; n < bytes; n++)
                        outputWriter.Write(reader.ReadByte());
                }

                if (opcode.SecondIsZero)
                {
                    for (int n = 0; n < (opcode.SecondLength + 1); n++)
                        outputWriter.Write((byte) 0);
                }
                else
                {
                    int bytes = (int) (Math.Min(8 - opcode.SecondLength, max - reader.BaseStream.Position));
                    
                    for (int n = 0; n < bytes; n++)
                        outputWriter.Write(reader.ReadByte());
                }
            }

            // go to the beginning of the stream to properly parse the data
            return outputStream;
        }

        /// <summary>
        /// Compresses all the data present in the <paramref name="reader" /> and saves it to the writer on <paramref name="output" />
        /// </summary>
        /// <param name="reader">The reader to read the data from</param>
        /// <param name="output">The writer to write the data to</param>
        public static void ZeroCompress(BinaryReader reader, BinaryWriter output)
        {
            // create temporal memory buffer to store the compressed data as we require
            // seek capability and not all the streams have it
            MemoryStream newStream = new MemoryStream();
            BinaryWriter newWriter = new BinaryWriter(newStream);

            byte b = reader.ReadByte();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                ZeroCompressionOpcode opcode = (byte) 0;
                int opcodeStartShift = 1;

                // reserve space for opcode
                newWriter.Write(opcode);

                // when the first byte found is a zero we actually do compression
                // count the number of zeros present up to 7 and update the opcode accordingly
                if (b == 0x00)
                {
                    opcode.FirstIsZero = true;
                    int firstLen = -1;

                    while ((b == 0x00) && (firstLen < 7) && (reader.BaseStream.Position < reader.BaseStream.Length))
                    {
                        firstLen++;
                        b = reader.ReadByte();
                    }

                    // Very stupid, but fixes a big problem with them
                    if (reader.BaseStream.Position == reader.BaseStream.Length)
                        opcode.FirstLength = (byte) (firstLen + 1);
                    else
                        opcode.FirstLength = (byte) (firstLen);
                }
                // when the first byte read is not 0 the only option is to update the opcode and write
                // to the stream the bytes that are not being compressed
                else
                {
                    opcode.FirstIsZero = false;
                    opcode.FirstLength = 8;

                    while ((b != 0x00) && (opcode.FirstLength > 0))
                    {
                        opcode.FirstLength--;
                        opcodeStartShift++;

                        newWriter.Write(b);
                        if (reader.BaseStream.Position < reader.BaseStream.Length)
                            b = reader.ReadByte();
                        else
                            break;
                    }
                }

                // special situation, if the input stream is shorter than a double block do not try to compress further
                // mark the second part as zeros, with zero length and finish the compression
                if (reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    opcode.SecondIsZero = true;
                    opcode.SecondLength = 0;
                }
                else if (b == 0x00)
                {
                    opcode.SecondIsZero = true;
                    int secondLength = -1;

                    while ((b == 0x00) && (opcode.SecondLength < 7) &&
                           (reader.BaseStream.Position < reader.BaseStream.Length))
                    {
                        secondLength++;
                        b = reader.ReadByte();
                    }

                    opcode.SecondLength = (byte) (secondLength);
                }
                else
                {
                    opcode.SecondIsZero = false;
                    opcode.SecondLength = 8;

                    while ((b != 0x00) && (opcode.SecondLength > 0))
                    {
                        opcode.SecondLength--;
                        opcodeStartShift++;

                        newWriter.Write(b);
                        if (reader.BaseStream.Position < reader.BaseStream.Length)
                            b = reader.ReadByte();
                        else
                            break;
                    }
                }

                // seek back to where the opcode position was reserved
                newWriter.Seek(-opcodeStartShift, SeekOrigin.Current);
                // write the updated opcode
                newWriter.Write(opcode);
                // seek back to where the last write ended
                newWriter.Seek(opcodeStartShift - 1, SeekOrigin.Current);
            }

            // once all the data is compressed write it to the actual output stream
            output.WriteSizeEx((int) (newStream.Length));

            if (newStream.Length > 0)
                newStream.WriteTo(output.BaseStream);
        }
    }
}