using System;
using System.IO;
using System.Text;

namespace Marshal
{

    public class MarshalData
    {
        private readonly MemoryStream _stream = new MemoryStream();
        private readonly BinaryWriter _writer;

        public MarshalData()
        {
            _writer = new BinaryWriter(_stream);
        }

        public void PutHeader()
        {
            _writer.Write(Unmarshal.HeaderByte);
            _writer.Write((uint)0);
        }

        public byte[] GetData()
        {
            _writer.Flush();
            return _stream.ToArray();
        }

        private void WriteOpcode(MarshalOpcode op)
        {
            _writer.Write((byte)op);
        }

        private void WriteLength(int len)
        {
            if ((byte)len == 0xFF)
            {
                _writer.Write(0xFF);
                _writer.Write((uint)len);
            }
            else
                _writer.Write((byte)len);
        }

        private bool HandleSpecialInteger(long num)
        {
            if (num == -1)
                WriteOpcode(MarshalOpcode.IntegerMinusOne);
            else if (num == 1)
                WriteOpcode(MarshalOpcode.IntegerOne);
            else if (num == 0)
                WriteOpcode(MarshalOpcode.IntegerZero);
            return num == -1 || num == 1 || num == 0;
        }

        public void PutTuple(int count)
        {
            if (count == 0)
                WriteOpcode(MarshalOpcode.TupleEmpty);
            else if (count == 1)
                WriteOpcode(MarshalOpcode.TupleOne);
            else if (count == 2)
                WriteOpcode(MarshalOpcode.TupleTwo);
            else
            {
                WriteOpcode(MarshalOpcode.Tuple);
                WriteLength(count);
            }
        }

        public void PutList(int count)
        {
            if (count == 0)
                WriteOpcode(MarshalOpcode.ListEmpty);
            else if (count == 1)
                WriteOpcode(MarshalOpcode.ListOne);
            else
            {
                WriteOpcode(MarshalOpcode.List);
                WriteLength(count);
            }
        }

        public void PutDictionary(int count)
        {
            WriteOpcode(MarshalOpcode.Dict);
            WriteLength(count);
        }

        public void PutPair<T, U>(T key, U value)
        {
            Put(value);
            Put(key);
        }

        public void Put<T>(T data)
        {
            throw new ArgumentException("Unable to marshal " + data.GetType());
        }

        public void PutNone()
        {
            WriteOpcode(MarshalOpcode.None);
        }

        public void Put(float num)
        {
            Put((double)num);
        }

        public void Put(double num)
        {
            if (num == 0.0d)
                WriteOpcode(MarshalOpcode.RealZero);
            else
            {
                WriteOpcode(MarshalOpcode.Real);
                _writer.Write(num);
            }
        }

        public void Put(byte num)
        {
            if (!HandleSpecialInteger(num))
            {
                WriteOpcode(MarshalOpcode.IntegerByte);
                _writer.Write(num);
            }
        }

        public void Put(long num)
        {
            if (!HandleSpecialInteger(num))
            {
                WriteOpcode(MarshalOpcode.IntegerLongLong);
                _writer.Write(num);
            }
        }
        
        public void Put(short num)
        {
            if (!HandleSpecialInteger(num))
            {
                WriteOpcode(MarshalOpcode.IntegerSignedShort);
                _writer.Write(num);
            }
        }

        public void Put(uint num)
        {
            Put((int) num);
        }

        public void Put(int num)
        {
            if (!HandleSpecialInteger(num))
            {
                WriteOpcode(MarshalOpcode.IntegerLong);
                _writer.Write(num);
            }
        }

        public void Put(string str)
        {
            Put(Encoding.ASCII.GetBytes(str));
        }

        public void Put(byte[] str)
        {
            if (str.Length == 0)
                WriteOpcode(MarshalOpcode.StringEmpty);
            else
            {
                WriteOpcode(MarshalOpcode.StringLong);
                WriteLength(str.Length);
                _writer.Write(str);
            }
        }
    }

}