using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EVESharp.PythonTypes.Compression;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.PythonTypes.Marshal;

/// <summary>
/// Takes care of parsing marshalled data from the Python-side into C# objects that the
/// project can use to process the input data
/// </summary>
public class Marshal
{
    /// <summary>
    /// Separator for the PyObject's data
    /// </summary>
    public const byte PACKED_TERMINATOR = 0x2D;
    private readonly PyDataType mData;
    /// <summary>
    /// A key->value pair for saved elements so we know their index
    /// </summary>
    private readonly Dictionary <int, int> mHashToListPosition = new Dictionary <int, int> ();


    /// <summary>
    /// A key->value pair for saved elements so they can be updated
    /// </summary>
    private readonly Dictionary <int, long> mHashToPosition = new Dictionary <int, long> ();

    private readonly BinaryWriter mWriter;

    /// <summary>
    /// Creates a new Marshal context with the given stream as backing storage
    /// </summary>
    /// <param name="data">The data to marshal</param>
    /// <param name="output">The stream to write into</param>
    /// <exception cref="NotSupportedException">When the given stream does not support seeking</exception>
    protected Marshal (PyDataType data, Stream output)
    {
        this.mData = data;

        if (output.CanSeek == false)
            throw new NotSupportedException ("The stream must have seeking capabilities for the marshal to work");

        this.mWriter = new BinaryWriter (output);
    }

    /// <summary>
    /// Converts the given <paramref name="data" /> python type into a byte stream
    /// </summary>
    /// <param name="data">The Python type to convert into a byte stream</param>
    /// <param name="writeHeader">Whether the Marshal header has to be written or not</param>
    /// <returns>The full object converted into a byte stream</returns>
    public static byte [] ToByteArray (PyDataType data, bool writeHeader = true)
    {
        MemoryStream stream = new MemoryStream ();

        WriteToStream (stream, data, writeHeader);

        return stream.ToArray ();
    }

    /// <summary>
    /// Converts the given <paramref name="data" /> python type into a byte stream and writes it to <paramref name="stream" />
    /// </summary>
    /// <param name="stream">The stream to write the byte data into</param>
    /// <param name="data">The Python type to convert into a byte stream</param>
    /// <param name="writeHeader">Whether the Marshal header has to be written or not</param>
    /// <returns>The full object converted into a byte stream</returns>
    public static void WriteToStream (Stream stream, PyDataType data, bool writeHeader = true)
    {
        Marshal marshal = new Marshal (data, stream);

        marshal.Process (writeHeader);
    }

    /// <summary>
    /// Converts the given <see cref="mData"/> python type into a byte stream and writes it to <see name="mStream" />
    /// </summary>
    /// <param name="writeHeader">Whether the Marshal header has to be written or not</param>
    /// <returns>The full object converted into a byte stream</returns>
    public void Process (bool writeHeader = true)
    {
        if (writeHeader)
        {
            // write the marshal magic header
            this.mWriter.Write (Specification.MARSHAL_HEADER);
            // write a temporal save list to the stream so we reserve the position
            this.mWriter.Write (0);
        }

        this.Process (this.mWriter, this.mData);

        if (writeHeader && this.mHashToListPosition.Count > 0)
        {
            // order the map by the position
            IOrderedEnumerable <KeyValuePair <int, int>> ordered = this.mHashToListPosition.OrderBy (x => this.mHashToPosition [x.Key]);

            // write the saved element list
            foreach ((int _, int position) in ordered)
                this.mWriter.Write (position + 1);

            // finally go back to where the count is and write it too
            this.mWriter.Seek (1, SeekOrigin.Begin);
            this.mWriter.Write (this.mHashToListPosition.Count);
        }
    }

    /// <summary>
    /// Determines if the given PyDataType can be stored as a saved element
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static bool CanBeSaved (PyDataType data)
    {
        if (data is PyString {IsStringTableEntry: false} pyString && pyString.Value.Length > 1 && pyString.IsUTF8 == false)
            return true;

        switch (data)
        {
            case PyObject:
            case PyObjectData:
            case PyDictionary:
            case PyTuple:
            case PySubStruct:
            case PyBuffer:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// Processes the given <paramref name="data" /> python type and writes it's byte stream equivalent into.
    /// Every Python object converted must write the opcode in front as this is how it's identified on unmarshal
    /// <paramref name="writer" />
    /// </summary>
    /// <param name="writer">The writer were to write the data to</param>
    /// <param name="data">The python object to convert</param>
    /// <exception cref="InvalidDataException">If an unknown python data type is detected</exception>
    private void Process (BinaryWriter writer, PyDataType data)
    {
        long opcodePosition = writer.BaseStream.Position;
        int  hash           = data?.GetHashCode () ?? 0;
        bool canBeSaved     = CanBeSaved (data);

        // ignore specific values as they're not really worth it
        if (canBeSaved)
        {
            // check if the object was already saved and write it as such
            if (this.mHashToPosition.TryGetValue (hash, out long position))
            {
                // check if the hash is not already in there and add it
                if (this.mHashToListPosition.TryGetValue (hash, out int listPosition) == false)
                {
                    listPosition = this.mHashToListPosition.Count;
                    this.mHashToListPosition.Add (hash, listPosition);

                    // move to the opcode position and overwrite the value with the correct flag
                    writer.Seek ((int) position, SeekOrigin.Begin);
                    byte current = (byte) writer.BaseStream.ReadByte ();
                    // seek back again
                    writer.Seek (-1, SeekOrigin.Current);
                    // write the new value
                    writer.Write ((byte) (current | Specification.SAVE_MASK));
                    // seek back to where we were
                    writer.Seek (0, SeekOrigin.End);
                }

                // the object was already saved once, so that value can be used
                writer.WriteOpcode (Opcode.SavedStreamElement);
                // write the positional value
                writer.WriteSizeEx (listPosition + 1);

                // and done
                return;
            }

            // store the element in the map with it's opcode position if it's not already there
            this.mHashToPosition.Add (hash, opcodePosition);
        }

        switch (data)
        {
            case null:
            case PyNone _:
                this.ProcessNone (writer);

                break;
            case PyInteger pyInteger:
                this.ProcessInteger (writer, pyInteger);

                break;
            case PyDecimal pyDecimal:
                this.ProcessDecimal (writer, pyDecimal);

                break;
            case PyToken pyToken:
                this.ProcessToken (writer, pyToken);

                break;
            case PyBool pyBool:
                this.ProcessBool (writer, pyBool);

                break;
            case PyBuffer pyBuffer:
                this.ProcessBuffer (writer, pyBuffer);

                break;
            case PyDictionary pyDictionary:
                this.ProcessDictionary (writer, pyDictionary);

                break;
            case PyList pyList:
                this.ProcessList (writer, pyList);

                break;
            case PyObjectData pyObjectData:
                this.ProcessObjectData (writer, pyObjectData);

                break;
            case PyObject pyObject:
                this.ProcessObject (writer, pyObject);

                break;
            case PyString pyString:
                this.ProcessString (writer, pyString);

                break;
            case PySubStream pySubStream:
                this.ProcessSubStream (writer, pySubStream);

                break;
            case PyChecksumedStream pyChecksumedStream:
                this.ProcessChecksumedStream (writer, pyChecksumedStream);

                break;
            case PySubStruct pySubStruct:
                this.ProcessSubStruct (writer, pySubStruct);

                break;
            case PyTuple pyTuple:
                this.ProcessTuple (writer, pyTuple);

                break;
            case PyPackedRow pyPackedRow:
                this.ProcessPackedRow (writer, pyPackedRow);

                break;
            default:
                throw new InvalidDataException ($"Unexpected type {data.GetType ()}");
        }
    }

    /// <summary>
    /// Converts the given <paramref name="data"/> to it's byte array representation.
    /// Integers are encoded based on their value, which determines it's size in the byte stream
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.IntegerOne" /> 1 byte, value = 1
    /// <seealso cref="Opcode.IntegerZero" /> 1 byte, value = 0
    /// <seealso cref="Opcode.IntegerMinusOne" /> 1 byte, value = -1
    /// <seealso cref="Opcode.IntegerByte" /> 2 bytes, value is one byte after the opcode, signed byte
    /// <seealso cref="Opcode.IntegerSignedShort" /> 3 bytes, value is two bytes after the opcode, signed short
    /// <seealso cref="Opcode.IntegerLong" /> 5 bytes, value is four bytes after the opcode, signed integer
    /// <seealso cref="Opcode.IntegerLongLong" /> 9 bytes, value is eight bytes after the opcode, signed long
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="data">The value to write</param>
    private void ProcessInteger (BinaryWriter writer, PyInteger data)
    {
        if (data.Value == 1)
        {
            writer.WriteOpcode (Opcode.IntegerOne);
        }
        else if (data.Value == 0)
        {
            writer.WriteOpcode (Opcode.IntegerZero);
        }
        else if (data.Value == -1)
        {
            writer.WriteOpcode (Opcode.IntegerMinusOne);
        }
        else if (data.Value >= sbyte.MinValue && data.Value <= sbyte.MaxValue)
        {
            writer.WriteOpcode (Opcode.IntegerByte);
            writer.Write ((byte) data.Value);
        }
        else if (data.Value >= short.MinValue && data.Value <= short.MaxValue)
        {
            writer.WriteOpcode (Opcode.IntegerSignedShort);
            writer.Write ((short) data.Value);
        }
        else if (data.Value >= int.MinValue && data.Value <= int.MaxValue)
        {
            writer.WriteOpcode (Opcode.IntegerLong);
            writer.Write ((int) data.Value);
        }
        else
        {
            writer.WriteOpcode (Opcode.IntegerLongLong);
            writer.Write (data.Value);
        }
    }

    /// <summary>
    /// Converts the given <paramref name="data"/> to it's byte array representation.
    /// Decimals are encoded based on their value
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.RealZero" /> 1 byte, value = 0.0
    /// <seealso cref="Opcode.Real" /> 9 bytes, value is eight bytes after the opcode, double
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="data">The value to write</param>
    private void ProcessDecimal (BinaryWriter writer, PyDecimal data)
    {
        if (data == 0.0)
        {
            writer.WriteOpcode (Opcode.RealZero);
        }
        else
        {
            writer.WriteOpcode (Opcode.Real);
            writer.Write (data.Value);
        }
    }

    /// <summary>
    /// Converts the given <paramref name="token"/> to it's byte array representation.
    /// Tokens are basic ASCII strings with a variable length up to 255 bytes
    /// They represent the name of a python type on the other side of the wire
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.Token" /> 2 bytes minimum, the string data comes right after the length indicator
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="token">The value to write</param>
    private void ProcessToken (BinaryWriter writer, PyToken token)
    {
        if (token.Length > byte.MaxValue)
            throw new InvalidDataException ($"Token length cannot be greater than {byte.MaxValue}");

        writer.WriteOpcode (Opcode.Token);
        writer.Write ((byte) token.Token.Length);
        writer.Write (Encoding.ASCII.GetBytes (token));
    }

    /// <summary>
    /// Converts the given <paramref name="boolean"/> to it's byte array representation.
    /// Booleans are encoded to their specific opcode as they only have two possible values
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.BoolTrue" /> 1 byte, value = true
    /// <seealso cref="Opcode.BoolFalse" /> 1 byte, value = false
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="boolean">The value to write</param>
    private void ProcessBool (BinaryWriter writer, PyBool boolean)
    {
        if (boolean)
            writer.WriteOpcode (Opcode.BoolTrue);
        else
            writer.WriteOpcode (Opcode.BoolFalse);
    }

    /// <summary>
    /// Converts the given <paramref name="buffer"/> to it's byte array representation.
    /// Buffer are basic, binary byte arrays that do not follow any specific format
    /// Their length is variable and is indicated by an extended size indicator
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.Buffer" /> 2 bytes minimum, the data comes right after the length indicator
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="buffer">The value to write</param>
    private void ProcessBuffer (BinaryWriter writer, PyBuffer buffer)
    {
        writer.WriteOpcode (Opcode.Buffer);
        writer.WriteSizeEx (buffer.Value.Length);
        writer.Write (buffer);
    }

    /// <summary>
    /// Converts the given <paramref name="dictionary"/> to it's byte array representation.
    /// Dictionaries are complex, massive objects that encode other python objects
    /// Uses extended size indicator to specify the amount of key-value pairs available
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.Dictionary" /> 2 bytes minimum, extended size indicator, values are encoded as value-key values in python types
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="dictionary">The value to write</param>
    private void ProcessDictionary (BinaryWriter writer, PyDictionary dictionary)
    {
        writer.WriteOpcode (Opcode.Dictionary);
        writer.WriteSizeEx (dictionary.Length);

        foreach (PyDictionaryKeyValuePair pair in dictionary)
        {
            this.Process (writer, pair.Value);
            this.Process (writer, pair.Key);
        }
    }

    /// <summary>
    /// Converts the given <paramref name="list"/> to it's byte array representation.
    /// Lists are a bit simpler than dictionaries, the opcode can indicate the length of the list, and there is also
    /// support for extended size indicatos 
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.ListEmpty" /> 2 byte, no elements
    /// <seealso cref="Opcode.ListOne" /> 1 byte + encoded python element
    /// <seealso cref="Opcode.List" /> 1 byte + extended size indicator + python elements encoded
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="list">The value to write</param>
    private void ProcessList (BinaryWriter writer, PyList list)
    {
        if (list.Count == 0)
        {
            writer.WriteOpcode (Opcode.ListEmpty);
        }
        else if (list.Count == 1)
        {
            writer.WriteOpcode (Opcode.ListOne);
        }
        else
        {
            writer.WriteOpcode (Opcode.List);
            writer.WriteSizeEx (list.Count);
        }

        foreach (PyDataType entry in list)
            this.Process (writer, entry);
    }

    /// <summary>
    /// Converts the given <paramref name="data"/> to it's byte array representation.
    /// ObjectData are simple representations of Python objects.
    /// These objects are composed by one python token that indicates the type of the object (it's name)
    /// and an argument object that can be any kind of Python type
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.ObjectData" />
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="data">The value to write</param>
    private void ProcessObjectData (BinaryWriter writer, PyObjectData data)
    {
        writer.WriteOpcode (Opcode.ObjectData);
        this.Process (writer, data.Name);
        this.Process (writer, data.Arguments);
    }

    /// <summary>
    /// Nones are kind of special types that indicate No data
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.None" />
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    private void ProcessNone (BinaryWriter writer)
    {
        writer.WriteOpcode (Opcode.None);
    }

    /// <summary>
    /// Converts the given <paramref name="data"/> to it's byte array representation.
    /// Python Objects are a more flexible representation of actual class instances (similar to ObjectData)
    /// There are two types of Objects the marshal supports but both have almost the same structure, only varying
    /// on how the header data is created.  
    ///
    /// Both types have a Tuple as header, a list of items and a dictionary of items. The list and dictionary of items
    /// do not have any kind of size indication, instead their ends are marked by a specific flag <see cref="PACKED_TERMINATOR"/>
    /// and they are encoded one after the other. The Header, List elements and Dictionary elements are normal python types
    /// so anything can be inside them.
    ///
    /// The list elements are encoded one after the other.
    /// The dictionary follows the same principle, each key-value pair is encoded one after the other, in this case
    /// the key is encoded first and the value after it
    /// 
    /// The following opcodes are supported
    /// <seealso cref="Opcode.ObjectType1" />
    /// <seealso cref="Opcode.ObjectType2" />
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="data">The value to write</param>
    private void ProcessObject (BinaryWriter writer, PyObject data)
    {
        if (data.IsType2)
            writer.WriteOpcode (Opcode.ObjectType2);
        else
            writer.WriteOpcode (Opcode.ObjectType1);

        this.Process (writer, data.Header);

        if (data.List.Count > 0)
            foreach (PyDataType entry in data.List)
                this.Process (writer, entry);

        writer.Write (PACKED_TERMINATOR);

        if (data.Dictionary.Length > 0)
            foreach (PyDictionaryKeyValuePair <PyDataType, PyDataType> entry in data.Dictionary)
            {
                this.Process (writer, entry.Key);
                this.Process (writer, entry.Value);
            }

        writer.Write (PACKED_TERMINATOR);
    }

    /// <summary>
    /// Converts the given <paramref name="data"/> to it's byte array representation.
    /// There are different types of strings available in Python, normal (ASCII) strings, UTF8 Strings and
    /// Unicode strings (WStrings) with their corresponding opcodes. As an extra mechanism, a table of strings
    /// with commonly-used strings are also supported to minimize the length of the game packets.
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.StringEmpty" /> 1 byte, Empty, ascii string
    /// <seealso cref="Opcode.WStringEmpty" /> 1 byte, Empty, WString
    /// <seealso cref="Opcode.StringChar" /> 2 bytes, One-character string
    /// <seealso cref="Opcode.StringTable" /> 2 bytes, string table entry, the index starts on 1 instead of 0
    /// <seealso cref="Opcode.WStringUTF8" /> More than 2 bytes, contains an extended size indicator for the string length + the bytes that compose the string
    /// <seealso cref="Opcode.StringLong" /> More than 2 bytes, contains an extended size indicator for the string length + the bytes that compose the string
    ///
    /// There are some more available opcodes for string representation, but they are not really needed by the server as most strings will be
    /// UTF8 encoded, with only a few cases of non-UTF8-encoded strings
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="data">The value to write</param>
    private void ProcessString (BinaryWriter writer, PyString data)
    {
        if (data.Length == 0)
        {
            writer.WriteOpcode (data.IsUTF8 ? Opcode.WStringEmpty : Opcode.StringEmpty);
        }
        else if (data.Length == 1 && data.IsUTF8 == false)
        {
            writer.WriteOpcode (Opcode.StringChar);
            writer.Write (Encoding.ASCII.GetBytes (data) [0]);
        }
        else if (data.IsStringTableEntry)
        {
            writer.WriteOpcode (Opcode.StringTable);
            writer.Write ((byte) (data.StringTableEntryIndex + 1));
        }
        else
        {
            // no match found in the table, normal string found, write it to the marshal stream
            if (data.IsUTF8)
            {
                byte [] str = Encoding.UTF8.GetBytes (data.Value);
                writer.WriteOpcode (Opcode.WStringUTF8);
                writer.WriteSizeEx (str.Length);
                writer.Write (str);
            }
            else
            {
                // NOTE: ShortString doesn't seem to be used on Apocrypha
                writer.WriteOpcode (Opcode.StringLong);
                writer.WriteSizeEx (data.Length);
                // writer.Write(Encoding.UTF8.GetBytes(data.Value));
                writer.Write (Encoding.ASCII.GetBytes (data.Value));
            }
        }
    }

    /// <summary>
    /// Converts the given <paramref name="data"/> to it's byte array representation.
    /// A kind of object container without much usefulness, it just contains another python type in it and is prefixed
    /// by an extended size indicator. This is commonly used by EVE as the request body of a CallReq packet, and
    /// after some investigation looks like it might not be decoded by the proxy at all, just forwarded as is to the
    /// node the packet is destinated to, thus saving processing power on the node.
    ///
    /// TODO: CHECK IF THIS HYPOTHETICAL SITUATION IS REALLY WHAT'S GOING ON OR NOT (AND IF THE WHOLE PACKET IS ACTUALLY SENT OR JUST THE SUBSTREAM
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.SubStream" /> 2 bytes minimum, extended size indicator, data is encoded as a normal Marshal
    /// string with header and save lists
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="data">The value to write</param>
    private void ProcessSubStream (BinaryWriter writer, PySubStream data)
    {
        // this marshals the data only if the data changed from the original (or if it's a new PySubStream)
        byte [] buffer = data.ByteStream;

        writer.WriteOpcode (Opcode.SubStream);
        writer.WriteSizeEx (buffer.Length);
        writer.Write (buffer);
    }

    /// <summary>
    /// Converts the given <paramref name="data"/> to it's byte array representation.
    /// The ChecksumedStream is on the same situation as <seealso cref="ProcessSubStream"/> the only difference
    /// is that instead of an extended size indicator there is only a checksum of the data in the stream 
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.ChecksumedStream" /> 5 bytes minimum, Adler32 checksum, data is encoded as a normal Marshal string
    /// with header and save lists included
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="data">The value to write</param>
    private void ProcessChecksumedStream (BinaryWriter writer, PyChecksumedStream data)
    {
        byte [] buffer = ToByteArray (data.Data, false);

        uint checksum = Adler32.Checksum (buffer);

        writer.WriteOpcode (Opcode.ChecksumedStream);
        writer.Write (checksum);
        writer.Write (buffer);
    }

    /// <summary>
    /// Converts the given <paramref name="data"/> to it's byte array representation.
    /// Structs are some kind of container used to store other python objects. Each substruct only supports one element
    /// in it's definition.
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.SubStruct" /> 1 byte + encoded python type
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="data">The value to write</param>
    private void ProcessSubStruct (BinaryWriter writer, PySubStruct data)
    {
        writer.WriteOpcode (Opcode.SubStruct);
        this.Process (writer, data.Definition);
    }

    /// <summary>
    /// Converts the given <paramref name="tuple"/> to it's byte array representation.
    /// Tuples are a special kind of container, they do not grow automatically, could be seen as simple, static lists
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.TupleEmpty" /> 1 byte, empty tuple
    /// <seealso cref="Opcode.TupleOne" /> 1 byte + encoded python type
    /// <seealso cref="Opcode.TupleTwo"/> 1 byte + two encoded python types without any separator
    /// <seealso cref="Opcode.Tuple" /> 2 bytes minimun, contains an extended size indicator with the size of the
    /// tuple, all the elements are encoded after without any kind of separator
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="tuple">The value to write</param>
    private void ProcessTuple (BinaryWriter writer, PyTuple tuple)
    {
        if (tuple.Count == 0)
        {
            writer.WriteOpcode (Opcode.TupleEmpty);
        }
        else if (tuple.Count == 1)
        {
            writer.WriteOpcode (Opcode.TupleOne);
        }
        else if (tuple.Count == 2)
        {
            writer.WriteOpcode (Opcode.TupleTwo);
        }
        else
        {
            writer.WriteOpcode (Opcode.Tuple);
            writer.WriteSizeEx (tuple.Count);
        }

        foreach (PyDataType entry in tuple)
            this.Process (writer, entry);
    }

    /// <summary>
    /// Converts the given <paramref name="packedRow"/> to it's byte array representation.
    /// Packed rows are the big elephant in the room. These are a direct representation of a row in any of the tables
    /// the server has and are composed of 3 parts.
    ///
    /// The first is the header that contains information about the columns that the PackedRow has data for.
    /// This header is known as <see cref="DBRowDescriptor" /> which is composed of a tuple with a string (column name)
    /// and an integer (column type). The column order doesn't matter on the header.
    ///
    /// The second part is the compressible bit data. Contains part of the actual data in the row. The columns are sorted
    /// by bit size in descending order, this way the amount of zeros one after the another is kept to a minimum, allowing
    /// the zero compression algorithm (<see cref="ZeroCompressionUtils" />) to greatly reduce the size of this data.
    /// Strings are not encoded here and are part of the third section of the PackedRow.
    /// The 1-bit sized columns (booleans) are encoded a bit differently from the rest of the data. Once the multi-bit
    /// sized data is written (long, int, short, byte) the bool values are grouped into 1 byte chunks, storing up to 8 booleans
    /// in each byte. The booleans are added to the byte from right to left and the byte blocks are written one after the other.
    /// These resources might be useful to better understand this operation
    /// https://stackoverflow.com/questions/36829860/using-binary-to-compress-boolean-array
    /// https://en.wikipedia.org/wiki/Bit_array
    /// https://sakai.rutgers.edu/wiki/site/e07619c5-a492-4ebe-8771-179dfe450ae4/bit-to-boolean%20conversion.html
    ///
    /// The third and last section contains the byte arrays and strings. These are encoded as normal python types
    /// after the second part.
    ///
    /// The following opcodes are supported
    /// <seealso cref="Opcode.PackedRow" /> 
    /// </summary>
    /// <param name="writer">Where to write the encoded data to</param>
    /// <param name="packedRow">The value to write</param>
    private void ProcessPackedRow (BinaryWriter writer, PyPackedRow packedRow)
    {
        writer.WriteOpcode (Opcode.PackedRow);
        this.Process (writer, packedRow.Header);
        // bit where null flags will be written
        int booleanBits = 0;
        int nullBits    = 0;
        int wholeBytes  = 0;

        List <DBRowDescriptor.Column> booleanColumns = new List <DBRowDescriptor.Column> ();

        foreach (DBRowDescriptor.Column column in packedRow.Header.Columns)
        {
            int bitLength = Utils.GetTypeBits (column.Type);

            if (column.Type == FieldType.Bool)
            {
                booleanColumns.Add (column);
                booleanBits++;
            }

            nullBits++;

            if (bitLength >= 8)
                wholeBytes += bitLength >> 3;
        }

        // build byte buffers for the bitfields like booleans and nulls
        byte [] bitField = new byte[((booleanBits + nullBits) >> 3) + 1];

        // prepare the zero-compression stream
        MemoryStream wholeByteStream = new MemoryStream (wholeBytes + bitField.Length);
        MemoryStream objectStream    = new MemoryStream ();

        BinaryWriter wholeByteWriter = new BinaryWriter (wholeByteStream);
        BinaryWriter objectWriter    = new BinaryWriter (objectStream);

        // sort the columns by size and obtain some important statistics
        IOrderedEnumerable <DBRowDescriptor.Column> enumerator = packedRow.Header.Columns.OrderByDescending (c => Utils.GetTypeBits (c.Type));

        foreach (DBRowDescriptor.Column column in enumerator)
        {
            PyDataType value = packedRow [column.Name];

            switch (column.Type)
            {
                case FieldType.UI8:
                    wholeByteWriter.Write ((ulong) (value as PyInteger ?? 0));

                    break;

                case FieldType.I8:
                case FieldType.CY:
                case FieldType.FileTime:
                    wholeByteWriter.Write ((long) (value as PyInteger ?? 0));

                    break;

                case FieldType.I4:
                    wholeByteWriter.Write ((int) (value as PyInteger ?? 0));

                    break;
                case FieldType.UI4:
                    wholeByteWriter.Write ((uint) (value as PyInteger ?? 0));

                    break;
                case FieldType.I2:
                    wholeByteWriter.Write ((short) (value as PyInteger ?? 0));

                    break;
                case FieldType.UI2:
                    wholeByteWriter.Write ((ushort) (value as PyInteger ?? 0));

                    break;
                case FieldType.I1:
                    wholeByteWriter.Write ((sbyte) (value as PyInteger ?? 0));

                    break;
                case FieldType.UI1:
                    wholeByteWriter.Write ((byte) (value as PyInteger ?? 0));

                    break;

                case FieldType.R8:
                    wholeByteWriter.Write ((double) (value as PyDecimal ?? 0));

                    break;

                case FieldType.R4:
                    wholeByteWriter.Write ((float) (value as PyDecimal ?? 0));

                    break;

                // bools, bytes and str are handled differently
                case FieldType.Bool:
                    if (value is not null && value as PyBool)
                    {
                        int bit = booleanColumns.IndexOf (column);

                        bitField [bit >> 3] |= (byte) (1 << (bit & 0x7));
                    }

                    break;

                case FieldType.Bytes:
                case FieldType.Str:
                case FieldType.WStr:
                    // write the object to the proper memory stream
                    this.Process (objectWriter, packedRow [column.Name]);

                    continue;

                default:
                    throw new Exception ($"Unknown field type {column.Type}");
            }

            if (value is null)
            {
                int bit = packedRow.Header.Columns.IndexOf (column) + booleanBits;

                bitField [bit >> 3] |= (byte) (1 << (bit & 0x7));
            }
        }

        // write the bit field buffer into the wholeByteWriter
        wholeByteWriter.Write (bitField);
        // create a reader for the stream
        wholeByteStream.Seek (0, SeekOrigin.Begin);
        // create the reader used to compress the buffer
        BinaryReader reader = new BinaryReader (wholeByteStream);
        // finally compress the data into the output
        ZeroCompressionUtils.ZeroCompress (reader, writer);
        // as last step write the encoded objects after the packed data
        objectStream.WriteTo (writer.BaseStream);
    }
}