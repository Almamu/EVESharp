using System;
using System.IO;

namespace Marshal
{
    
    public abstract class PyObject
    {
        /// <summary>
        /// Enables recording RawSource and RawOffset data. More than doubles memory footprint of eveMarshal.
        /// </summary>
        public static bool EnableInspection;

        public byte[] RawSource { get; set; }
        public long RawOffset { get; set; }

        public PyObjectType Type
        {
            get; private set;
        }

        protected PyObject(PyObjectType type)
        {
            Type = type;
        }

        public abstract void Decode(Unmarshal context, MarshalOpcode op, BinaryReader source);

        public void Encode(BinaryWriter output)
        {
            RawOffset = output.BaseStream.Position;

            EncodeInternal(output);

            if (EnableInspection)
            {
                var postOffset = output.BaseStream.Position;
                output.BaseStream.Seek(RawOffset, SeekOrigin.Begin);
                RawSource = new byte[postOffset - RawOffset];
                output.BaseStream.Read(RawSource, 0, (int)(postOffset - RawOffset));
            }
        }

        protected abstract void EncodeInternal(BinaryWriter output);

        public T As<T>() where T : PyObject
        {
            return this as T;
        }

        public virtual PyObject this[int index]
        {
            get
            {
                throw new NotSupportedException("Can't subscript a PyObject");
            }
            set
            {
                throw new NotSupportedException("Can't subscript a PyObject");
            }
        }

        public virtual PyObject this[string key]
        {
            get
            {
                throw new NotSupportedException("Can't subscript a PyObject");
            }
            set
            {
                throw new NotSupportedException("Can't subscript a PyObject");
            }
        }

        public double FloatValue
        {
            get
            {
                if (this is PyFloat)
                    return (this as PyFloat).Value;
                return IntValue;
            }
        }

        public string StringValue
        {
            get
            {
                if (this is PyString)
                    return (this as PyString).Value;
                if (this is PyToken)
                    return (this as PyToken).Token;
                return null;
            }
        }

        public long IntValue
        {
            get
            {
                if (this is PyNone)
                    return 0;

                if (this is PyFloat)
                    return (long)As<PyFloat>().Value;

                if (this is PyBool)
                    return As<PyBool>().Value ? 1 : 0;

                if (this is PyInt)
                    return As<PyInt>().Value;

                if (this is PyLongLong)
                    return As<PyLongLong>().Value;

                if (this is PyIntegerVar)
                {
                    var iv = this as PyIntegerVar;
                    if (iv.Raw.Length <= 8)
                    {
                        var copy = new byte[8];
                        iv.Raw.CopyTo(copy, 0);
                        return BitConverter.ToInt64(copy, 0);
                    }
                }

                throw new InvalidDataException("Not an integer");
            }
        }

        public override string ToString()
        {
            return "<" + Type + ">";
        }
    }

}