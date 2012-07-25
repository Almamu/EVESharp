using System;
using System.Collections.Generic;
using System.IO;
using Marshal.Database;
using System.Linq;
using System.Text;

namespace Marshal
{

    public class PyPackedRow : PyObject
    {
        public PyObject Header { get; set; }
        public byte[] RawData { get; private set; }

        public List<Column> Columns { get; private set; }

        public PyPackedRow()
            : base(PyObjectType.PackedRow)
        {
            
        }

        public PyPackedRow(DBRowDescriptor from)
            : base(PyObjectType.PackedRow)
        {
            Header = from.Encode();
            
            Columns = new List<Column>();

            for (int i = 0; i < from.ColumnCount(); i++)
            {
                Columns.Insert(i, new Column(from.GetColumnName(i).Value, from.GetColumnType(i)));
            }
        }

        public PyObject Get(string key)
        {
            var col = Columns.Where(c => c.Name == key).FirstOrDefault();
            return col == null ? null : col.Value;
        }

        public void SetValue(string key, PyObject value)
        {
            var col = Columns.Where(c => c.Name == key).FirstOrDefault();

            if (col != null)
            {
                int index = Columns.IndexOf(col);
                Columns[index].Value = value;
            }
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            context.NeedObjectEx = true;
            Header = context.ReadObject(source);
            context.NeedObjectEx = false;
            RawData = LoadZeroCompressed(source);

            if (!ParseRowData(context, source))
                 throw new InvalidDataException("Could not fully unpack PackedRow, stream integrity is broken");
        }

        private bool ParseRowData(Unmarshal context, BinaryReader source)
        {
            var objex = Header as PyObjectEx;
            if (objex == null)
                return false;

            var header = objex.Header as PyTuple;
            if (header == null || header.Items.Count < 2)
                return false;

            var columns = header.Items[1] as PyTuple;
            if (columns == null)
                return false;

            /*
            columns = columns.Items[0] as PyTuple;
            if (columns == null)
                return false;
            */
            Columns = new List<Column>(columns.Items.Count);

            foreach (var obj in columns)
            {
                var fieldData = obj as PyTuple;
                if (fieldData == null || fieldData.Items.Count < 2)
                    continue;

                var name = fieldData.Items[0] as PyString;
                if (name == null)
                    continue;

                Columns.Add(new Column(name.Value, (FieldType) fieldData.Items[1].IntValue));
            }

            var sizeList = Columns.OrderByDescending(c => FieldTypeHelper.GetTypeBits(c.Type));
            var sizeSum = sizeList.Sum(c => FieldTypeHelper.GetTypeBits(c.Type));
            // align
            sizeSum = (sizeSum + 7) >> 3;
            var rawStream = new MemoryStream();
            // fill up
            rawStream.Write(RawData, 0, RawData.Length);
            for (int i = 0; i < (sizeSum - RawData.Length); i++)
                rawStream.WriteByte(0);
            rawStream.Seek(0, SeekOrigin.Begin);
            var reader = new BinaryReader(rawStream);

            int bitOffset = 0;
            foreach (var column in sizeList)
            {
                switch (column.Type)
                {
                    case FieldType.I8:
                    case FieldType.UI8:
                    case FieldType.CY:
                    case FieldType.FileTime:
                        column.Value = new PyLongLong(reader.ReadInt64());
                        break;

                    case FieldType.I4:
                    case FieldType.UI4:
                        column.Value = new PyInt(reader.ReadInt32());
                        break;

                    case FieldType.I2:
                    case FieldType.UI2:
                        column.Value = new PyInt(reader.ReadInt16());
                        break;

                    case FieldType.I1:
                    case FieldType.UI1:
                        column.Value = new PyInt(reader.ReadByte());
                        break;

                    case FieldType.R8:
                        column.Value = new PyFloat(reader.ReadDouble());
                        break;

                    case FieldType.R4:
                        column.Value = new PyFloat(reader.ReadSingle());
                        break;

                    case FieldType.Bytes:
                    case FieldType.Str:
                    case FieldType.WStr:
                        column.Value = context.ReadObject(source);
                        break;

                    case FieldType.Bool:
                        {
                            if (7 < bitOffset)
                            {
                                bitOffset = 0;
                                reader.ReadByte();
                            }

                            var b = reader.ReadByte();
                            reader.BaseStream.Seek(-1, SeekOrigin.Current);
                            column.Value = new PyInt((b >> bitOffset++) & 0x01);
                            break;
                        }

                    default:
                        throw new Exception("No support for " + column.Type);
                }
            }

            return true;
        }

        public static byte[] LoadZeroCompressed(BinaryReader reader)
        {
            MemoryStream retStream = new MemoryStream();
            BinaryWriter ret = new BinaryWriter(retStream);

            uint packedLen = reader.ReadSizeEx();
            long max = reader.BaseStream.Position + packedLen;

            while (reader.BaseStream.Position < max)
            {
                var opcode = new ZeroCompressOpcode(reader.ReadByte());

                if (opcode.FirstIsZero)
                {
                    byte len = (byte)(opcode.FirstLength + 1);
                    while (0 < len--)
                        ret.Write((byte)(0x00));
                }
                else
                {
                    int bytes = (int)Math.Min(8 - opcode.FirstLength, max - reader.BaseStream.Position);
                    for (int n = 0; n < bytes; n++)
                        ret.Write(reader.ReadByte());
                }

                if (opcode.SecondIsZero)
                {
                    byte len = (byte)(opcode.SecondLength + 1);
                    while (0 < len--)
                        ret.Write((byte)(0x00));
                }
                else
                {
                    int bytes = (int)Math.Min(8 - opcode.SecondLength, max - reader.BaseStream.Position);
                    for (int n = 0; n < bytes; n++)
                        ret.Write(reader.ReadByte());
                }
            }

            return retStream.ToArray();
        }

        public static void SaveZeroCompressed(BinaryReader reader, BinaryWriter output)
        {
            MemoryStream newStream = new MemoryStream();
            BinaryWriter newWriter = new BinaryWriter(newStream);

            byte b = reader.ReadByte();

            while(reader.BaseStream.Position < reader.BaseStream.Length)
            {
                ZeroCompressOpcode opcode = new ZeroCompressOpcode(0);
                int opcodeStartShift = 1;

                // Reserve space for opcode
                newWriter.Write(opcode.Value);

                if (b == 0x00)
                {
                    opcode.FirstIsZero = true;
                    int firstLen = -1;

                    while ((b == 0x00) && (firstLen < 7) && (reader.BaseStream.Position < reader.BaseStream.Length))
                    {
                        firstLen++;
                        b = reader.ReadByte();
                    }

                    opcode.FirstLength = (byte)(firstLen);
                }
                else
                {
                    opcode.FirstIsZero = false;
                    opcode.FirstLength = 8;

                    while ((b > 0x00) && (opcode.FirstLength > 0))
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
                newWriter.Write(opcode.Value);
                newWriter.Seek(opcodeStartShift - 1, SeekOrigin.Current);
            }

            output.WriteSizeEx((int)(newStream.Length));
            if(newStream.Length > 0)
            {
                output.Write(newStream.ToArray());
            }
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.Write((byte)MarshalOpcode.PackedRow);
            Header.Encode(output);

            int cc = Columns.Count;

            var rawStream = new MemoryStream();
            var writer = new BinaryWriter(rawStream);
            var reader = new BinaryReader(rawStream);

            var sizeList = Columns.OrderByDescending(c => FieldTypeHelper.GetTypeBits(c.Type));

            foreach (Column col in sizeList)
            {
                switch (col.Type)
                {
                    case FieldType.I8:
                    case FieldType.UI8:
                    case FieldType.CY:
                    case FieldType.FileTime:
                        writer.Write(col.Value.IntValue);
                        break;

                    case FieldType.I4:
                    case FieldType.UI4:
                        writer.Write((int)(col.Value.IntValue));
                        break;

                    case FieldType.I2:
                    case FieldType.UI2:
                        writer.Write((short)(col.Value.IntValue));
                        break;

                    case FieldType.I1:
                    case FieldType.UI1:
                        writer.Write((byte)(col.Value.IntValue));
                        break;

                    case FieldType.R8:
                        writer.Write((double)(col.Value.FloatValue));
                        break;

                    case FieldType.R4:
                        writer.Write((float)(col.Value.FloatValue));
                        break;

                    case FieldType.Bool: // These are always the last data added
                        // writer.Write((byte)(col.Value.IntValue));
                        break;

                    case FieldType.Bytes:
                    case FieldType.Str:
                    case FieldType.WStr:
                        col.Value.Encode(output);
                        break;

                    default:
                        throw new Exception("Unsupported FieldType");
                }
            }

            long bitByte = 0;
            byte bitOffset = 0;
            byte[] unpacked = rawStream.ToArray();
            rawStream.Close();

            foreach (Column col in Columns)
            {
                if (FieldTypeHelper.GetTypeBits(col.Type) != 1)
                {
                    continue;
                }

                PyBool value = col.Value as PyBool;

                if (bitOffset > 7)
                    bitOffset = 0;

                if (bitOffset == 0)
                {
                    bitByte = unpacked.Length + 1;
                    Array.Resize<byte>(ref unpacked, (int)(bitByte));
                }

                unpacked[bitByte] |= (byte)(((value.Value == true) ? 1 : 0) << bitOffset++);
            }

            rawStream = new MemoryStream(unpacked);
            reader = new BinaryReader(rawStream);

            SaveZeroCompressed(reader, output);

            foreach (Column col in Columns)
            {
                col.Value.Encode(output);
            }

            reader.Close();
        }
    }
}