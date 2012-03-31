using System;
using System.IO;
using System.Text;

namespace Marshal
{

    public class PyString : PyObject
    {
        public string Value { get; private set; }
        public byte[] Raw { get; private set; }
        public bool ForceUTF8 { get; private set; }

        public PyString()
            : base(PyObjectType.String)
        {
            
        }

        public PyString(string data)
            : base(PyObjectType.String)
        {
            Raw = Encoding.ASCII.GetBytes(data);
            Value = data;
        }

        public PyString(string data, bool forceUTF8)
            : base(PyObjectType.String)
        {
            Raw = Encoding.UTF8.GetBytes(data);
            Value = data;
            ForceUTF8 = forceUTF8;
        }

        public PyString(byte[] raw)
            : base(PyObjectType.String)
        {
            Raw = raw;
            Value = Encoding.ASCII.GetString(raw);
        }

        private void Update(string data)
        {
            Raw = Encoding.ASCII.GetBytes(data);
            Value = data;
        }

        private void Update(byte[] data, bool isUnicode)
        {
            Raw = data;
            Value = isUnicode ? Encoding.Unicode.GetString(Raw) : Encoding.ASCII.GetString(Raw);
        }

        private void Update( byte[] data )
        {
            Update( data, false );
        }

        private void UpdateUTF8(byte[] data)
        {
            Raw = data;
            Value = Encoding.UTF8.GetString(Raw);
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            if (op == MarshalOpcode.StringEmpty)
                Update(new byte[0]);
            else if (op == MarshalOpcode.StringChar)
                Update(new[]{source.ReadByte()});
            else if (op == MarshalOpcode.WStringUTF8)
                UpdateUTF8(source.ReadBytes((int)source.ReadSizeEx()));
            else if (op == MarshalOpcode.WStringUCS2Char)
                Update(new[]{source.ReadByte(), source.ReadByte()}, true);
            else if (op == MarshalOpcode.WStringEmpty)
                Update(new byte[0]);
            else if (op == MarshalOpcode.WStringUCS2)
                Update(source.ReadBytes((int)source.ReadSizeEx()), true);
            else if (op == MarshalOpcode.StringShort)
                Update(source.ReadBytes(source.ReadByte()));
            else if (op == MarshalOpcode.StringLong)
                Update(source.ReadBytes((int)source.ReadSizeEx()));
            else if (op == MarshalOpcode.StringTable)
            {
                byte index = source.ReadByte();
                Update(StringTable.Entries[index-1]);
            }
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            if (ForceUTF8)
            {
                output.WriteOpcode(MarshalOpcode.WStringUTF8);
                output.WriteSizeEx(Raw.Length);
                output.Write(Raw);
                return;
            }

            int idx;
            if (Raw.Length == 0)
                output.WriteOpcode(MarshalOpcode.StringEmpty);
            else if (Raw.Length == 1)
            {
                output.WriteOpcode(MarshalOpcode.StringChar);
                output.Write(Raw[0]);
            }
            else if ((idx = StringTable.Entries.IndexOf(Value)) >= 0)
            {
                output.WriteOpcode(MarshalOpcode.StringTable);
                output.Write((byte)(idx+1));
            }
            else
            {
                /*if (Raw.Length < 0xFF)
                {
                    output.WriteOpcode(MarshalOpcode.StringShort);
                    output.Write((byte)Raw.Length);
                    output.Write(Raw);
                }
                else*/
                {
                    output.WriteOpcode(MarshalOpcode.StringLong);
                    output.WriteSizeEx(Raw.Length);
                    output.Write(Raw);
                }
            }
        }

        public override string ToString()
        {
            if (Value.Length <= 0)
                return "<empty string>";
            if (char.IsLetterOrDigit(Value[0]))
                return "<" + Value + ">";
            return "<" + BitConverter.ToString(Raw) + ">";
        }
    }

}