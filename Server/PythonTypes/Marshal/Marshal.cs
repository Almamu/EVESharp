using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using PythonTypes.Compression;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using Ubiety.Dns.Core;

namespace PythonTypes.Marshal
{
    public class Marshal
    {
        public const byte PackedTerminator = 0x2D;
        
        public static byte[] ToByteArray(PyDataType data, bool writeHeader = true)
        {
            MemoryStream stream = new MemoryStream();

            WriteToStream(stream, data, writeHeader);

            return stream.ToArray();
        }
        
        public static void WriteToStream(Stream stream, PyDataType data, bool writeHeader = true)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            if (writeHeader)
            {
                writer.Write(Specification.PACKET_MAGIC_VALUE);
                writer.Write(0);
            }

            Process(writer, data);
        }

        private static void Process(BinaryWriter writer, PyDataType data)
        {
            if (data == null || data is PyNone)
            {
                ProcessNone(writer);
            }
            else if (data is PyInteger)
            {
                ProcessInteger(writer, data as PyInteger);
            }
            else if (data is PyDecimal)
            {
                ProcessDecimal(writer, data as PyDecimal);
            }
            else if (data is PyToken)
            {
                ProcessToken(writer, data as PyToken);
            }
            else if (data is PyBool)
            {
                ProcessBool(writer, data as PyBool);
            }
            else if (data is PyBuffer)
            {
                ProcessBuffer(writer, data as PyBuffer);
            }
            else if (data is PyDictionary)
            {
                ProcessDictionary(writer, data as PyDictionary);
            }
            else if (data is PyList)
            {
                ProcessList(writer, data as PyList);
            }
            else if (data is PyObjectData)
            {
                ProcessObjectData(writer, data as PyObjectData);
            }
            else if (data is PyObject)
            {
                ProcessObject(writer, data as PyObject);
            }
            else if (data is PyString)
            {
                ProcessString(writer, data as PyString);
            }
            else if (data is PySubStream)
            {
                ProcessSubStream(writer, data as PySubStream);
            }
            else if (data is PyChecksumedStream)
            {
                ProcessChecksumedStream(writer, data as PyChecksumedStream);
            }
            else if (data is PySubStruct)
            {
                ProcessSubStruct(writer, data as PySubStruct);
            }
            else if (data is PyTuple)
            {
                ProcessTuple(writer, data as PyTuple);
            }
            else if (data is PyPackedRow)
            {
                ProcessPackedRow(writer, data as PyPackedRow);
            }
            else
            {
                throw new InvalidDataException($"Unexpected type {data.GetType()}");
            }
        }

        private static void ProcessInteger(BinaryWriter writer, PyInteger data)
        {
            if (data == 1)
                writer.WriteOpcode(Opcode.IntegerOne);
            else if (data == 0)
                writer.WriteOpcode(Opcode.IntegerZero);
            else if (data == -1)
                writer.WriteOpcode(Opcode.IntegerMinusOne);
            else if (data < sbyte.MaxValue)
            {
                writer.WriteOpcode(Opcode.IntegerByte);
                writer.Write((byte) data.Value);
            }
            else if (data < short.MaxValue)
            {
                writer.WriteOpcode(Opcode.IntegerSignedShort);
                writer.Write((short) data.Value);
            }
            else if (data < int.MaxValue)
            {
                writer.WriteOpcode(Opcode.IntegerLong);
                writer.Write((int) data.Value);
            }
            else
            {
                writer.WriteOpcode(Opcode.IntegerLongLong);
                writer.Write(data.Value);
            }
        }

        private static void ProcessDecimal(BinaryWriter writer, PyDecimal data)
        {
            if(data.Value == 0.0)
                writer.WriteOpcode(Opcode.RealZero);
            else
            {
                writer.WriteOpcode(Opcode.Real);
                writer.Write(data.Value);
            }
        }

        private static void ProcessToken(BinaryWriter writer, PyToken token)
        {
            writer.WriteOpcode(Opcode.Token);
            writer.Write((byte) token.Token.Length);
            writer.Write(Encoding.ASCII.GetBytes(token));
        }

        private static void ProcessBool(BinaryWriter writer, PyBool boolean)
        {
            if (boolean)
                writer.WriteOpcode(Opcode.BoolTrue);
            else
                writer.WriteOpcode(Opcode.BoolFalse);
        }

        private static void ProcessBuffer(BinaryWriter writer, PyBuffer buffer)
        {
            writer.WriteOpcode(Opcode.Buffer);
            writer.WriteSizeEx(buffer.Value.Length);
            writer.Write(buffer);
        }

        private static void ProcessDictionary(BinaryWriter writer, PyDictionary dictionary)
        {
            writer.WriteOpcode(Opcode.Dict);
            writer.WriteSizeEx(dictionary.Length);

            foreach (KeyValuePair<string, PyDataType> pair in dictionary)
            {
                Process(writer, pair.Value);
                Process(writer, pair.Key);
            }
        }

        private static void ProcessList(BinaryWriter writer, PyList list)
        {
            if(list.Count == 0)
                writer.WriteOpcode(Opcode.ListEmpty);
            else if (list.Count == 1)
            {
                writer.WriteOpcode(Opcode.ListOne);
                Process(writer, list[0]);
            }
            else
            {
                writer.WriteOpcode(Opcode.List);
                writer.WriteSizeEx(list.Count);

                foreach (PyDataType entry in list)
                    Process(writer, entry);
            }
        }

        private static void ProcessObjectData(BinaryWriter writer, PyObjectData data)
        {
            writer.WriteOpcode(Opcode.ObjectData);
            Process(writer, data.Name);
            Process(writer, data.Arguments);
        }

        private static void ProcessNone(BinaryWriter writer)
        {
            writer.WriteOpcode(Opcode.None);
        }

        private static void ProcessObject(BinaryWriter writer, PyObject data)
        {
            if (data.Header.IsType2 == true)
                writer.WriteOpcode(Opcode.ObjectType2);
            else
                writer.WriteOpcode(Opcode.ObjectType1);
            
            Process(writer, data.Header);

            if (data.List.Count > 0)
            {
                foreach (PyDataType entry in data.List)
                    Process(writer, entry);
            }

            writer.Write(PackedTerminator);

            if (data.Dictionary.Length > 0)
            {
                foreach (KeyValuePair<string, PyDataType> entry in data.Dictionary)
                {
                    Process(writer, entry.Key);
                    Process(writer, entry.Value);
                }
            }

            writer.Write(PackedTerminator);
        }

        private static void ProcessString(BinaryWriter writer, PyString data)
        {
            if (data.Length == 0)
                writer.WriteOpcode(Opcode.WStringEmpty);
            else if (data.Length == 1)
            {
                writer.WriteOpcode(Opcode.StringChar);
                writer.Write(Encoding.ASCII.GetBytes(data)[0]);
            }
            else if (data.IsStringTableEntry)
            {
                writer.WriteOpcode(Opcode.StringTable);
                writer.Write(((byte) data.StringTableEntryIndex) + 1);
            }
            else
            {
                // TODO: ON SOME SITUATIONS A NORMAL STRING IS EXPECTED, PROVIDE A MECHANISM TO FORCE MARSHALING SUPPORT
                writer.WriteOpcode(Opcode.StringLong);
                writer.WriteSizeEx(data.Length);
                // writer.Write(Encoding.UTF8.GetBytes(data.Value));
                writer.Write(Encoding.ASCII.GetBytes(data.Value));
            }
        }

        private static void ProcessSubStream(BinaryWriter writer, PySubStream data)
        {
            byte[] buffer = ToByteArray(data.Stream);
            
            writer.WriteOpcode(Opcode.SubStream);
            writer.WriteSizeEx(buffer.Length);
            writer.Write(buffer);
        }

        private static void ProcessChecksumedStream(BinaryWriter writer, PyChecksumedStream data)
        {
            byte[] buffer = ToByteArray(data.Data, false);

            uint checksum = Adler32.Checksum(buffer);

            writer.WriteOpcode(Opcode.ChecksumedStream);
            writer.Write(checksum);
            writer.Write(buffer);
        }

        private static void ProcessSubStruct(BinaryWriter writer, PySubStruct data)
        {
            writer.WriteOpcode(Opcode.SubStruct);
            Process(writer, data.Definition);
        }

        private static void ProcessTuple(BinaryWriter writer, PyTuple tuple)
        {
            if (tuple.Count == 0)
                writer.WriteOpcode(Opcode.TupleEmpty);
            else if (tuple.Count == 1)
                writer.WriteOpcode(Opcode.TupleOne);
            else if (tuple.Count == 2)
                writer.WriteOpcode(Opcode.TupleTwo);
            else
            {
                writer.WriteOpcode(Opcode.Tuple);
                writer.WriteSizeEx(tuple.Count);
            }

            foreach (PyDataType entry in tuple)
                Process(writer, entry);
        }
        
        private static void ProcessPackedRow(BinaryWriter writer, PyPackedRow packedRow)
        {
            writer.WriteOpcode(Opcode.PackedRow);
            Process(writer, packedRow.Header);
            
            // prepare the zero-compression stream
            MemoryStream wholeByteStream = new MemoryStream();
            MemoryStream bitPacketStream = new MemoryStream();
            MemoryStream objectStream = new MemoryStream();

            BinaryWriter wholeByteWriter = new BinaryWriter(wholeByteStream);
            BinaryWriter bitPacketWriter = new BinaryWriter(bitPacketStream);
            BinaryWriter objectWriter = new BinaryWriter(objectStream);
            
            // sort the columns by size
            IOrderedEnumerable<DBRowDescriptor.Column> enumerator = packedRow.Header.Columns.OrderByDescending(c => Utils.GetTypeBits(c.Type));
            byte bitOffset = 0;
            byte toWrite = 0;

            foreach (DBRowDescriptor.Column column in enumerator)
            {
                switch (column.Type)
                {
                    case FieldType.I8:
                    case FieldType.UI8:
                    case FieldType.CY:
                    case FieldType.FileTime:
                        wholeByteWriter.Write((long) (packedRow[column.Name] as PyInteger));
                        break;
                    
                    case FieldType.I4:
                    case FieldType.UI4:
                        wholeByteWriter.Write((int) (packedRow[column.Name] as PyInteger));
                        break;
                    
                    case FieldType.I2:
                    case FieldType.UI2:
                        wholeByteWriter.Write((short) (packedRow[column.Name] as PyInteger));
                        break;
                    
                    case FieldType.I1:
                    case FieldType.UI1:
                        wholeByteWriter.Write((byte) (packedRow[column.Name] as PyInteger));
                        break;
                    
                    case FieldType.R8:
                        wholeByteWriter.Write((double) (packedRow[column.Name] as PyDecimal));
                        break;
                    
                    case FieldType.R4:
                        wholeByteWriter.Write((float) (packedRow[column.Name] as PyDecimal));
                        break;

                    // bools, bytes and str are handled differently
                    case FieldType.Bool:
                        PyBool value = packedRow[column.Name] as PyBool;

                        if (value)
                        {
                            // bytes are written from right to left in the buffer
                            toWrite |= (byte) (1 << bitOffset);
                        }

                        bitOffset++;

                        if (bitOffset > 7)
                        {
                            // byte is full, write the byte to the stream
                            bitPacketWriter.Write(toWrite);
                            // reset the byte to keep using it as buffer
                            toWrite = 0;
                            // do the same for the next bit offset
                            bitOffset = 0;
                        }
                        break;
                    
                    case FieldType.Bytes:
                    case FieldType.Str:
                    case FieldType.WStr:
                        // write the object to the proper memory stream
                        Process(objectWriter, packedRow[column.Name]);
                        break;
                    
                    default:
                        throw new Exception($"Unknown field type {column.Type}");
                }
            }

            // after the column loop is done there might be some leftover compressed bits
            // that have to be written to the bit stream too
            if (bitOffset > 0)
                bitPacketWriter.Write(toWrite);
            
            // append the bitStream to the to the wholeByteWriter
            bitPacketStream.WriteTo(wholeByteStream);
            // create a reader for the stream
            wholeByteStream.Seek(0, SeekOrigin.Begin);
            // create the reader used to compress the buffer
            BinaryReader reader = new BinaryReader(wholeByteStream);
            // finally compress the data into the output
            ZeroCompressionUtils.ZeroCompress(reader, writer);
            // as last step write the encoded objects after the packed data
            objectStream.WriteTo(writer.BaseStream);
        }
    }
}