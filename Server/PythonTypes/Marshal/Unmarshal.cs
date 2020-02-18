using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Org.BouncyCastle.Utilities.Zlib;
using PythonTypes.Compression;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Marshal
{
    public class Unmarshal
    {
        public const byte HeaderByte = 0x7E;

        // not a real magic since zlib just doesn't include one..
        public const byte ZlibMarker = 0x78;
        
        public const byte SaveMask = 0x40;
        public const byte OpcodeMask = 0x3F;

        public static PyDataType ReadFromByteArray(byte[] data, bool expectHeader = true)
        {
            MemoryStream stream = new MemoryStream(data);

            return ReadFromStream(stream, expectHeader);
        }

        public static PyDataType ReadFromStream(Stream stream, bool expectHeader = true)
        {
            Unmarshal processor = new Unmarshal(stream);

            return processor.Process();
        }

        private Stream mStream;
        private BinaryReader mReader;
        private PyDataType[] mSavedList;
        private int mCurrentSavedIndex;
        private int[] mSavedElementsMap;
        
        public Unmarshal(Stream stream)
        {
            mStream = stream;
        }

        public PyDataType Process(bool expectHeader = true)
        {
            byte header;
            
            if (expectHeader)
            {
                // check the header and ensure we use the correct stream to read from it
                header = (byte) this.mStream.ReadByte();

                if (header == ZlibMarker)
                {
                    this.mStream.Seek(-1, SeekOrigin.Current);
                    this.mStream = new ZInputStream(this.mStream);
                    header = (byte) this.mStream.ReadByte();
                }
            
                if(header != HeaderByte)
                    throw new InvalidDataException($"Expected Marshal header, but got {header}");
            
                // create the reader
                this.mReader = new BinaryReader(this.mStream);
                // read the save list information
                int saveCount = this.mReader.ReadInt32();

                if (saveCount > 0)
                {
                    // compressed streams cannot be seek'd so ensure that the stream is decompressed first
                    if (this.mStream is ZInputStream)
                    {
                        MemoryStream newStream = new MemoryStream();
                        this.mStream.CopyTo(newStream);
                        this.mStream = newStream;
                        this.mReader = new BinaryReader(this.mStream);
                    }
                    
                    // reserve space for the savelist map and the actual elements in the save list
                    this.mSavedElementsMap = new int[saveCount];
                    this.mSavedList = new PyDataType[saveCount];

                    long currentPosition = this.mStream.Position;
                    // read at the end of the stream to get the correct index to element mapping
                    this.mStream.Seek(-saveCount * 4, SeekOrigin.End);

                    for (int i = 0; i < saveCount; i++)
                        this.mSavedElementsMap[i] = this.mReader.ReadInt32();

                    // go back to where the stream was after reading the amount of saved elements
                    this.mStream.Seek(currentPosition, SeekOrigin.Begin);
                }
            }

            header = mReader.ReadByte();
            Opcode opcode = (Opcode) (header & OpcodeMask);
            bool save = (header & SaveMask) == SaveMask;

            PyDataType data;
            
            // perform decode
            switch (opcode)
            {
                case Opcode.IntegerLongLong:
                case Opcode.IntegerSignedShort:
                case Opcode.IntegerByte:
                case Opcode.IntegerMinusOne:
                case Opcode.IntegerOne:
                case Opcode.IntegerZero:
                case Opcode.IntegerLong:
                case Opcode.IntegerVar:
                    data = ProcessInteger(opcode);
                    break;
                
                case Opcode.None:
                    data = ProcessNone(opcode);
                    break;
                
                case Opcode.Buffer:
                    data = ProcessBuffer(opcode);
                    break;
                
                case Opcode.BoolFalse:
                case Opcode.BoolTrue:
                    data = ProcessBool(opcode);
                    break;
                
                case Opcode.Real:
                case Opcode.RealZero:
                    data = ProcessDecimal(opcode);
                    break;
                
                case Opcode.Token:
                    data = ProcessToken(opcode);
                    break;
                
                case Opcode.StringEmpty:
                case Opcode.StringChar:
                case Opcode.StringShort:
                case Opcode.StringTable:
                case Opcode.StringLong:
                case Opcode.WStringEmpty:
                case Opcode.WStringUCS2:
                case Opcode.WStringUCS2Char:
                case Opcode.WStringUTF8:
                    data = ProcessString(opcode);
                    break;
                case Opcode.Tuple:
                case Opcode.TupleOne:
                case Opcode.TupleTwo:
                case Opcode.TupleEmpty:
                    data = ProcessTuple(opcode);
                    break;
                
                case Opcode.List:
                case Opcode.ListOne:
                case Opcode.ListEmpty:
                    data = ProcessList(opcode);
                    break;
                
                case Opcode.Dict:
                    data = ProcessDictionary(opcode);
                    break;
                
                case Opcode.PackedRow:
                    data = ProcessPackedRow(opcode);
                    break;

                case Opcode.ObjectData:
                    data = ProcessObjectData(opcode);
                    break;
                
                case Opcode.ObjectType1:
                case Opcode.ObjectType2:
                    data = ProcessObjectEx(opcode);
                    break;
                
                case Opcode.SavedStreamElement:
                    data = this.mSavedList[
                        this.mSavedElementsMap[this.mReader.ReadSizeEx() - 1] - 1
                    ];
                    break;
                
                case Opcode.SubStruct:
                    data = ProcessSubStruct(opcode);
                    break;
                
                case Opcode.SubStream:
                    data = ProcessSubStream(opcode);
                    break;
                
                case Opcode.ChecksumedStream:
                    data = ProcessChecksumedStream(opcode);
                    break;
                
                default:
                    throw new Exception($"Unknown python type for opcode {opcode.ToString("X")}");
            }
            
            // check if the element has to be saved
            if (save)
            {
                this.mSavedList[this.mSavedElementsMap[this.mCurrentSavedIndex++] - 1] = data;
            }

            return data;
        }

        private PyDataType ProcessInteger(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.IntegerLongLong:
                    return new PyInteger(mReader.ReadInt64());
                case Opcode.IntegerLong:
                    return new PyInteger(mReader.ReadInt32());
                case Opcode.IntegerSignedShort:
                    return new PyInteger(mReader.ReadInt16());
                case Opcode.IntegerByte:
                    return new PyInteger(mReader.ReadByte());
                case Opcode.IntegerOne:
                    return new PyInteger(1);
                case Opcode.IntegerZero:
                    return new PyInteger(0);
                case Opcode.IntegerMinusOne:
                    return new PyInteger(-1);
                case Opcode.IntegerVar:
                {
                    // read the size
                    uint length = mReader.ReadSizeEx();
                    // emergency fallback, for longer integers read it as a PyBuffer
                    if(length > 8)
                        return new PyBuffer(mReader.ReadBytes((int) length));
                    if(length == 1)
                        return new PyInteger(mReader.ReadByte());
                    if(length == 2)
                        return new PyInteger(mReader.ReadInt16());
                    if (length == 4)
                        return new PyInteger(mReader.ReadInt32());
                    if(length == 8)
                        return new PyInteger(mReader.ReadInt64());
                    
                    throw new InvalidDataException($"IntegerVar of odd size ({length})");
                }
                default:
                    throw new InvalidDataException($"Trying to parse a {opcode} as Integer");
            }
        }

        private PyDataType ProcessNone(Opcode opcode)
        {
            if(opcode != Opcode.None)
                throw new InvalidDataException($"Trying to parse a {opcode} as None");

            return new PyNone();
        }

        private PyDataType ProcessBuffer(Opcode opcode)
        {
            if (opcode != Opcode.Buffer)
                throw new InvalidDataException($"Trying to parse a {opcode} as Buffer");

            uint length = this.mReader.ReadSizeEx();
            
            return new PyBuffer(this.mReader.ReadBytes((int) length));
        }

        private PyDataType ProcessBool(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.BoolTrue:
                    return new PyBool(true);
                case Opcode.BoolFalse:
                    return new PyBool(false);
                default:
                    throw new InvalidDataException($"Trying to parse a {opcode} as Boolean");
            }
        }

        private PyDataType ProcessDecimal(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.RealZero:
                    return new PyDecimal(0.0);
                case Opcode.Real:
                    return new PyDecimal(this.mReader.ReadDouble());
                default:
                    throw new InvalidDataException($"Trying to parse a {opcode} as Decimal");
            }
        }

        private PyDataType ProcessToken(Opcode opcode)
        {
            if (opcode != Opcode.Token)
                throw new InvalidDataException($"Trying to parse a {opcode} as Token");

            return new PyToken(
                Encoding.ASCII.GetString(
                    this.mReader.ReadBytes(this.mReader.ReadByte())
                )
            );
        }

        private PyDataType ProcessString(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.StringEmpty:
                    return new PyString("");
                case Opcode.StringChar:
                    return new PyString(new string(this.mReader.ReadChar(), 1));
                case Opcode.StringShort:
                    return new PyString(
                        Encoding.ASCII.GetString(
                            this.mReader.ReadBytes(this.mReader.ReadByte())
                        )
                    );
                case Opcode.StringTable:
                    return new PyString(
                        StringTable.Entries [this.mReader.ReadByte() - 1]
                    );
                case Opcode.StringLong:
                    return new PyString(
                        Encoding.ASCII.GetString(
                            this.mReader.ReadBytes((int) this.mReader.ReadSizeEx())
                        )
                    );
                case Opcode.WStringEmpty:
                    return new PyString("");
                case Opcode.WStringUCS2:
                    return new PyString(
                        Encoding.Unicode.GetString(
                            this.mReader.ReadBytes((int) this.mReader.ReadSizeEx())
                        )
                    );
                case Opcode.WStringUCS2Char:
                    return new PyString(
                        Encoding.Unicode.GetString(
                            this.mReader.ReadBytes(2)
                        )
                    );
                case Opcode.WStringUTF8:
                    return new PyString(
                        Encoding.UTF8.GetString(
                            this.mReader.ReadBytes((int) this.mReader.ReadSizeEx())
                        )
                    );
                
                default:
                    throw new InvalidDataException($"Trying to parse a {opcode} as String");
            }
        }

        private PyDataType ProcessTuple(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.Tuple:
                {
                    uint count = this.mReader.ReadSizeEx();
                    PyTuple tuple = new PyTuple((int) count);

                    for (int i = 0; i < count; i++)
                        tuple[i] = this.Process(false);

                    return tuple;
                }
                case Opcode.TupleOne:
                    return new PyTuple(new PyDataType[]
                    {
                        this.Process(false)
                    });
                case Opcode.TupleTwo:
                    return new PyTuple(new PyDataType[]
                    {
                        this.Process(false),
                        this.Process(false)
                    });
                case Opcode.TupleEmpty:
                    return new PyTuple(0);
                default:
                    throw new InvalidDataException($"Trying to parse a {opcode} as Tuple");
            }
        }

        private PyDataType ProcessList(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.ListEmpty:
                    return new PyList();
                case Opcode.ListOne:
                    return new PyList( new PyDataType[]
                    {
                        this.Process(false)
                    });
                case Opcode.List:
                {
                    uint count = this.mReader.ReadSizeEx();
                    PyList list = new PyList((int) count);

                    for (int i = 0; i < count; i++)
                        list.Add(this.Process(false));

                    return list;
                }
                default:
                    throw new InvalidDataException($"Trying to parse a {opcode} as List");
            }
        }

        private PyDataType ProcessDictionary(Opcode opcode)
        {
            if(opcode != Opcode.Dict)
                throw new InvalidDataException($"Trying to parse a {opcode} as Dictionary");

            PyDictionary dictionary = new PyDictionary();
            uint size = this.mReader.ReadSizeEx();

            while (size-- > 0)
            {
                PyDataType value = this.Process(false);
                PyDataType key = this.Process(false);
                
                if(key is PyString == false)
                    throw new InvalidDataException($"Expected String as Dictionary key, but gor {key.GetType()}");

                dictionary[key as PyString] = value;
            }

            return dictionary;
        }

        private PyDataType ProcessPackedRow(Opcode opcode)
        {
            if(opcode != Opcode.PackedRow)
                throw new InvalidDataException($"Trying to parse a {opcode} as PackedRow");

            DBRowDescriptor descriptor = this.Process(false);
            PyDataType[] data = new PyDataType[descriptor.Columns.Count];

            MemoryStream decompressedStream = ZeroCompressionUtils.LoadZeroCompressed(this.mReader);
            BinaryReader decompressedReader = new BinaryReader(decompressedStream);

            // sort columns by the bit size
            IEnumerable<DBRowDescriptor.Column> enumerator = descriptor.Columns.OrderByDescending(c => Utils.GetTypeBits(c.Type));
            
            int bitOffset = 8;
            byte buffer = 0;
            int index = 0;
            
            foreach (DBRowDescriptor.Column column in enumerator)
            {
                switch (column.Type)
                {
                    case FieldType.I8:
                    case FieldType.UI8:
                    case FieldType.CY:
                    case FieldType.FileTime:
                        data[index++] = new PyInteger(decompressedReader.ReadInt64());
                        break;
                    case FieldType.I4:
                    case FieldType.UI4:
                        data[index++] = new PyInteger(decompressedReader.ReadInt32());
                        break;
                    case FieldType.I2:
                    case FieldType.UI2:
                        data[index++] = new PyInteger(decompressedReader.ReadInt16());
                        break;
                    case FieldType.I1:
                    case FieldType.UI1:
                        data[index++] = new PyInteger(decompressedReader.ReadByte());
                        break;
                    case FieldType.R8:
                        data[index++] = new PyDecimal(decompressedReader.ReadDouble());
                        break;
                    case FieldType.R4:
                        data[index++] = new PyDecimal(decompressedReader.ReadSingle());
                        break;
                    case FieldType.Bool:               
                        // read a byte from the buffer if needed
                        if (bitOffset == 8)
                        {
                            buffer = decompressedReader.ReadByte();
                            bitOffset = 0;
                        }

                        data[index++] = new PyBool(((buffer >> bitOffset++) & 0x01) == 0x01);
                        break;
                    case FieldType.Bytes:
                    case FieldType.WStr:
                    case FieldType.Str:
                        data[index++] = this.Process(false);
                        break;
                }
            }

            return new PyPackedRow(descriptor, data);
        }

        private PyDataType ProcessObjectData(Opcode opcode)
        {
            if(opcode != Opcode.ObjectData)
                throw new Exception($"Trying to parse a {opcode} as ObjectData");
            
            return new PyObjectData(
                this.Process(false) as PyString, this.Process(false)
            );
        }
        
        private PyDataType ProcessObjectEx(Opcode opcode)
        {
            if(opcode != Opcode.ObjectType1 && opcode != Opcode.ObjectType2)
                throw new Exception($"Trying to parse a {opcode} as ObjectEx");

            PyTuple header = this.Process(false) as PyTuple;
            PyList list = new PyList();
            PyDictionary dict = new PyDictionary();

            while (this.mReader.PeekChar() != Marshal.PackedTerminator)
            {
                list.Add(this.Process(false));
            }

            // ignore packed terminator
            this.mReader.ReadByte();
            
            while (this.mReader.PeekChar() != Marshal.PackedTerminator)
            {
                PyString key = this.Process(false) as PyString;
                PyDataType value = this.Process(false);

                dict[key] = value;
            }

            // ignore packed terminator
            this.mReader.ReadByte();
            
            return new PyObject(header, list, dict);
        }

        private PyDataType ProcessSubStruct(Opcode opcode)
        {
            if(opcode != Opcode.SubStruct)
                throw new Exception($"Trying to parse a {opcode} as SubStruct");
            
            return new PySubStruct(
                this.Process(false)
            );
        }

        private PyDataType ProcessSubStream(Opcode opcode)
        {
            if(opcode != Opcode.SubStream)
                throw new Exception($"Trying to parse a {opcode} as SubStream");

            uint length = this.mReader.ReadSizeEx();
            PyDataType result = null;
            
            // on compressed streams it's impossible to do boundary-checks, so ensure that is taken into account
            if (this.mReader.BaseStream is ZInputStream)
            {
                byte[] buffer = new byte[length];

                this.mStream.Read(buffer, 0, buffer.Length);

                result = ReadFromByteArray(buffer);
            }
            else
            {
                long start = this.mStream.Position;
            
                // this substream has it's own savelist etc, the best way is to create a new unmarshaler using the same
                // stream so the data can be properly read without much issue
                result = ReadFromStream(this.mStream);
            
                if((start + length) != this.mStream.Position)
                    throw new Exception($"Read past the boundaries of the PySubStream");
   
            }
            
            return new PySubStream(result);
        }

        private PyDataType ProcessChecksumedStream(Opcode opcode)
        {
            if (opcode != Opcode.ChecksumedStream)
                throw new Exception($"Trying to parse a {opcode} as ChecksumedStream");

            uint checksum = this.mReader.ReadUInt32();
            
            return new PyChecksumedStream(
                this.Process(false)
            );
        }
    }
}