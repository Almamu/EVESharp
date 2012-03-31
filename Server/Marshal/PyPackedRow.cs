using System;
using System.Collections.Generic;
using System.IO;
using Marshal.Database;
using System.Linq;

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

            columns = columns.Items[0] as PyTuple;
            if (columns == null)
                return false;

            Columns = new List<Column>(columns.Items.Count);

            foreach (var obj in columns.Items)
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

        private static byte[] LoadZeroCompressed(BinaryReader reader)
        {
            var ret = new List<byte>();
            uint packedLen = reader.ReadSizeEx();
            long max = reader.BaseStream.Position + packedLen;
            while (reader.BaseStream.Position < max)
            {
                var opcode = new ZeroCompressOpcode(reader.ReadByte());

                if (opcode.FirstIsZero)
                {
                    for (int n = 0; n < (opcode.FirstLength+1); n++)
                        ret.Add(0x00);
                }
                else
                {
                    int bytes = (int)Math.Min(8 - opcode.FirstLength, max - reader.BaseStream.Position);
                    for (int n = 0; n < bytes; n++)
                        ret.Add(reader.ReadByte());
                }

                if (opcode.SecondIsZero)
                {
                    for (int n = 0; n < (opcode.SecondLength+1); n++)
                        ret.Add(0x00);
                }
                else
                {
                    int bytes = (int)Math.Min(8 - opcode.SecondLength, max - reader.BaseStream.Position);
                    for (int n = 0; n < bytes; n++)
                        ret.Add(reader.ReadByte());
                }
            }
            return ret.ToArray();
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.Write((byte)MarshalOpcode.PackedRow);
            Header.Encode(output);

            int cc = Columns.Count;

            Dictionary<int, byte> sizeMap = new Dictionary<int, byte>();

            var rawStream = new MemoryStream();
            var writer = new BinaryWriter(rawStream);

            var bitOffset = 0;

            var sizeList = Columns.OrderByDescending(c => FieldTypeHelper.GetTypeBits(c.Type));
            var sum = sizeList.Sum(c => FieldTypeHelper.GetTypeBits(c.Type));

            sum = (sum + 7) >> 3;

            foreach (int i in sizeMap.Keys)
            {
                switch (Columns[i].Type)
                {
                    case FieldType.I8:
                    case FieldType.UI8:
                    case FieldType.R8:
                    case FieldType.CY:
                    case FieldType.FileTime:
                        writer.Write(Columns[i].Value.IntValue);
                        break;

                    case FieldType.I4:
                    case FieldType.UI4:
                    case FieldType.R4:
                        writer.Write((int)Columns[i].Value.IntValue);
                        break;

                    case FieldType.I2:
                    case FieldType.UI2:
                        writer.Write((short)Columns[i].Value.IntValue);
                        break;

                    case FieldType.I1:
                    case FieldType.UI1:
                        writer.Write((byte)Columns[i].Value.IntValue);
                        break;

                    case FieldType.Bool:
                        if (7 < bitOffset)
                        {
                            bitOffset = 0;
                            writer.Write((byte)0);
                        }

                        var b = (byte)(1 << bitOffset++);
                        writer.Write(b);
                        writer.BaseStream.Seek(-1, SeekOrigin.Current);
                        break;

                    case FieldType.Bytes:
                    case FieldType.Str:
                    case FieldType.WStr:
                        Columns[i].Value.Encode(writer);
                        break;

                    default:
                        throw new Exception("Unsupported FieldType");
                }
            }

            long bitByte = 0;

            foreach (var col in Columns)
            {
                if(col.Type != FieldType.Bool)
                    continue;

                if (7 < bitOffset)
                    bitOffset = 0;

                if (0 == bitOffset)
                    bitByte = rawStream.Length;

                bitByte |= ( col.Value.IntValue << bitOffset++);
            }

            // Ported from EVEmu
            /*
    uint8 bitOffset = 0;
    Buffer::iterator<uint8> bitByte;

    cur = sizeMap.lower_bound( 1 );
    end = sizeMap.lower_bound( 0 );
    for(; cur != end; ++cur)
    {
        const uint32 index = cur->second;
        const PyBool* r = rep->GetField( index )->AsBool();

        if( 7 < bitOffset )
            bitOffset = 0;
        if( 0 == bitOffset )
        {
            bitByte = unpacked.end<uint8>();
            unpacked.ResizeAt( bitByte, 1 );
        }

        *bitByte |= ( r->value() << bitOffset++ );
    }

    //pack the bytes with the zero compression algorithm.
    if( !SaveZeroCompressed( unpacked ) )
        return false;

    // Append fields that are not packed:
    cur = sizeMap.lower_bound( 0 );
    end = sizeMap.end();
    for(; cur != end; cur++)
    {
        const uint32 index = cur->second;
        const PyRep* r = rep->GetField( index );

        if( !r->visit( *this ) )
            return false;
    }

    return true;
             * */
        }

    }

}