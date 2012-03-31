using System;
using System.IO;

namespace Marshal
{
    
    public class PyIntegerVar : PyObject
    {
        public byte[] Raw { get; private set; }

        public PyIntegerVar()
            : base (PyObjectType.IntegerVar)
        {

        }

        public PyIntegerVar(byte[] data)
            : base(PyObjectType.IntegerVar)
        {
            Raw = data;
        }

        public PyIntegerVar(int data)
            : base(PyObjectType.IntegerVar)
        {
            Raw = GetData(data);
        }

        public PyIntegerVar(long data)
            : base(PyObjectType.IntegerVar)
        {
            Raw = GetData(data);
        }

        public PyIntegerVar(short data)
            : base(PyObjectType.IntegerVar)
        {
            Raw = GetData(data);
        }

        public PyIntegerVar(byte data)
            : base(PyObjectType.IntegerVar)
        {
            Raw = new []{data};
        }
        
        private static byte[] GetData(long value)
        {
            if (value < 128)
                return new[]{(byte)value};
            if (value < Math.Pow(2, 15))
                return BitConverter.GetBytes((short)value);
            if (value < Math.Pow(2, 31))
                return BitConverter.GetBytes((int)value);
            return BitConverter.GetBytes(value);
        }

        public int Value
        {
            get
            {
                if (Raw.Length == 1)
                    return Raw[0];
                if (Raw.Length == 2)
                    return BitConverter.ToInt16(Raw, 0);
                if (Raw.Length == 4)
                    return BitConverter.ToInt32(Raw, 0);
                return -1;
            }
        }

        public override void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source)
        {
            var len = source.ReadSizeEx();
            Raw = source.ReadBytes((int) len);
        }

        protected override void EncodeInternal(BinaryWriter output)
        {
            output.WriteOpcode(MarshalOpcode.IntegerVar);
            output.WriteSizeEx(Raw.Length);
            output.Write(Raw);
        }
    }

}