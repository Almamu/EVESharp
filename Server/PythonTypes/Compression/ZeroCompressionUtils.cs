using System;
using System.Collections.Generic;
using System.IO;

namespace PythonTypes.Compression
{
    public class ZeroCompressionUtils
    {
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
                    int bytes = (int)(Math.Min(8 - opcode.FirstLength, max - reader.BaseStream.Position));
                    for(int n = 0; n < bytes; n ++)
                        outputWriter.Write(reader.ReadByte());
                }

                if(opcode.SecondIsZero)
                {
                    for(int n = 0; n < (opcode.SecondLength + 1); n ++)
                        outputWriter.Write((byte) 0);
                }
                else
                {
                    int bytes = (int)(Math.Min(8 - opcode.SecondLength, max - reader.BaseStream.Position));
                    for(int n = 0; n < bytes; n ++)
                        outputWriter.Write(reader.ReadByte());
                }
            }

            // go to the beginning of the stream to properly parse the data
            return outputStream;
        }

        public static void ZeroCompress(BinaryReader reader, BinaryWriter output)
        {
            MemoryStream newStream = new MemoryStream();
            BinaryWriter newWriter = new BinaryWriter(newStream);

            byte b = reader.ReadByte();
            
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                ZeroCompressionOpcode opcode = (byte) 0;
                int opcodeStartShift = 1;

                // Reserve space for opcode
                newWriter.Write(opcode);

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
                        opcode.FirstLength = (byte)(firstLen + 1);
                    else
                        opcode.FirstLength = (byte)(firstLen);
                }
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
                        {
                            b = reader.ReadByte();
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                if (reader.BaseStream.Position == reader.BaseStream.Length)
                {
                    opcode.SecondIsZero = true;
                    opcode.SecondLength = 0;
                }
                else if (b == 0x00)
                {
                    opcode.SecondIsZero = true;
                    int secondLength = -1;

                    while ((b == 0x00) && (opcode.SecondLength < 7) && (reader.BaseStream.Position < reader.BaseStream.Length))
                    {
                        secondLength++;
                        b = reader.ReadByte();
                    }

                    opcode.SecondLength = (byte)(secondLength);
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
                        {
                            b = reader.ReadByte();
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                newWriter.Seek(-opcodeStartShift, SeekOrigin.Current);
                newWriter.Write(opcode);
                newWriter.Seek(opcodeStartShift - 1, SeekOrigin.Current);
            }

            output.WriteSizeEx((int)(newStream.Length));
            
            if (newStream.Length > 0)
            {
                newStream.WriteTo(output.BaseStream);
            }
        }
    }
}