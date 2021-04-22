using System;
using System.IO;
using System.Runtime.CompilerServices;

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
        /// <param name="length">The expected length of the decompressed data</param>
        /// <returns>The decompressed data as a MemoryStream for further usage</returns>
        public static MemoryStream LoadZeroCompressed(BinaryReader reader, int length)
        {
            MemoryStream outputStream = new MemoryStream(new byte [length], 0, length, true, true);
            BinaryWriter outputWriter = new BinaryWriter(outputStream);

            uint packedLen = reader.ReadSizeEx();
            long max = reader.BaseStream.Position + packedLen;
            bool nibble = false;
            byte nibbleByte = 0;

            while (reader.BaseStream.Position < max)
            {
                int count = 0;
                
                nibble = !nibble;

                if (nibble == true)
                {
                    nibbleByte = reader.ReadByte();
                    count = (nibbleByte & 0x0F) - 8;
                }
                else
                {
                    count = (nibbleByte >> 4) - 8;
                }

                if (count >= 0)
                {
                    if (outputStream.Capacity < outputStream.Position + count + 1)
                        throw new InvalidDataException("The ZeroCompressed data size does not match the allocated space");
                    
                    while (count-- >= 0)
                        outputWriter.Write((byte) 0);
                }
                else
                {
                    if (outputStream.Capacity < outputStream.Position - count)
                        throw new InvalidDataException("The ZeroCompressed data size does not match the allocated space");

                    while (count++ < 0 && reader.BaseStream.Position < max)
                        outputWriter.Write(reader.ReadByte());
                }
            }
            
            // ensure the rest of the bytes are written as 0
            if (outputStream.Capacity < outputStream.Position)
                outputStream.Write(new byte [outputStream.Capacity - outputStream.Position]);
            
            // and go back to the begining of the stream
            outputStream.Seek(0, SeekOrigin.Begin);
            
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

            bool nibble = false;
            long nibblePosition = 0;
            byte nibbleByte = 0;
            int zerochains = 0;
            long start, end, count;

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                if (nibble == false)
                {
                    nibbleByte = 0;
                    nibblePosition = newWriter.BaseStream.Position;
                    // write the new nibble
                    newWriter.Write(nibbleByte);
                }

                // define the boundaries for the read
                start = reader.BaseStream.Position;
                end = start + 8;
                if (end > reader.BaseStream.Length)
                    end = reader.BaseStream.Length;

                byte current = reader.PeekByte();

                if (current > 0)
                {
                    zerochains = 0;

                    do
                    {
                        newWriter.Write(reader.ReadByte());
                    } while (reader.BaseStream.Position < end && reader.PeekByte() > 0);
                    
                    // calculate the count
                    count = (start - reader.BaseStream.Position) + 8;
                }
                else
                {
                    zerochains++;

                    // ignore all zero bytes
                    while (reader.BaseStream.Position < end && reader.PeekByte() == 0)
                        reader.ReadByte();
                    
                    // count the ignored bytes
                    count = (reader.BaseStream.Position - start) + 7;
                }

                if (nibble == true)
                {
                    nibbleByte |= (byte) (count << 4);
                }
                else
                {
                    nibbleByte = (byte) count;
                }
                
                // write the nibble value and seek back to the end of the writer
                newWriter.BaseStream.Seek(nibblePosition, SeekOrigin.Begin);
                newWriter.Write(nibbleByte);
                newWriter.BaseStream.Seek(0, SeekOrigin.End);

                nibble = !nibble;
            }

            if (nibble == true && zerochains > 0)
                zerochains++;

            long finalLength = newWriter.BaseStream.Length;
            
            while (zerochains > 1)
            {
                zerochains -= 2;
                // update length
                finalLength--;
            }

            // go to the begining and remove the extra bytes
            newWriter.BaseStream.Position = 0;
            newWriter.BaseStream.SetLength(finalLength);
            
            // once all the data is compressed write it to the actual output stream
            output.WriteSizeEx((int) (newStream.Length));

            if (newStream.Length > 0)
                newStream.WriteTo(output.BaseStream);
        }
    }
}