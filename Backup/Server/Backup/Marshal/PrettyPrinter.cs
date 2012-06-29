using System;
using System.Text;

namespace Marshal
{

    public static class PrettyPrinter
    {
        public const string Indention = "  ";

        public static string Print(PyObject obj)
        {
            var ret = new StringBuilder();
            Print(ret, 0, obj);
            return ret.ToString();
        }

        private static void Print(StringBuilder builder, int indention, PyObject obj)
        {
            var indent = "";
            for (int i = 0; i < indention; i++)
                indent += Indention;

            if (obj is PyString)
                builder.AppendLine(PrintString(obj as PyString, indent) + PrintRawData(obj));
            else if (obj is PyNone)
                builder.AppendLine(indent + PrintNone(obj as PyNone) + PrintRawData(obj));
            else if (obj is PyFloat)
                builder.AppendLine(indent + PrintFloat(obj as PyFloat) + PrintRawData(obj));
            else if (obj is PyInt)
                builder.AppendLine(indent + PrintInt(obj as PyInt) + PrintRawData(obj));
            else if (obj is PyIntegerVar)
                builder.AppendLine(indent + PrintIntegerVar(obj as PyIntegerVar) + PrintRawData(obj));
            else if (obj is PyTuple)
            {
                var tuple = obj as PyTuple;
                builder.AppendLine(indent + PrintTuple(tuple) + PrintRawData(obj));
                foreach (var item in tuple.Items)
                    Print(builder, indention + 1, item);
            }
            else if (obj is PyList)
            {
                var list = obj as PyList;
                builder.AppendLine(indent + PrintList(list) + PrintRawData(obj));
                foreach (var item in list.Items)
                    Print(builder, indention + 1, item);
            }
            else if (obj is PyLongLong)
                builder.AppendLine(indent + PrintLongLong(obj as PyLongLong) + PrintRawData(obj));
            else if (obj is PyBuffer)
                builder.AppendLine(indent + PrintBuffer(obj as PyBuffer) + PrintRawData(obj));
            else if (obj is PyObjectData)
            {
                var objdata = obj as PyObjectData;
                builder.AppendLine(indent + PrintObjectData(objdata) + PrintRawData(obj));
                Print(builder, indention + 1, objdata.Arguments);
            }
            else if (obj is PySubStream)
            {
                var sub = obj as PySubStream;
                builder.AppendLine(indent + PrintSubStream(sub) + PrintRawData(obj));
                Print(builder, indention + 1, sub.Data);
            }
            else if (obj is PyDict)
            {
                var dict = obj as PyDict;
                builder.AppendLine(indent + PrintDict(dict) + PrintRawData(obj));
                foreach (var kvp in dict.Dictionary)
                {
                    Print(builder, indention + 1, kvp.Key);
                    Print(builder, indention + 1, kvp.Value);
                }
            }
            else if (obj is PyObjectEx)
            {
                var objex = obj as PyObjectEx;
                builder.AppendLine(indent + PrintObjectEx(objex) + PrintRawData(obj));
                Print(builder, indention + 1, objex.Header);
                foreach (var item in objex.List)
                    Print(builder, indention + 1, item);
                foreach (var kvp in objex.Dictionary)
                {
                    Print(builder, indention + 1, kvp.Key);
                    Print(builder, indention + 1, kvp.Value);
                }
            }
            else if (obj is PyToken)
            {
                builder.AppendLine(indent + PrintToken(obj as PyToken) + PrintRawData(obj));
            }
            else if (obj is PyPackedRow)
            {
                var packedRow = obj as PyPackedRow;
                builder.AppendLine(indent + PrintPackedRow(packedRow));
                if (packedRow.Columns != null)
                {
                    foreach (var column in packedRow.Columns)
                    {
                        builder.AppendLine(indent + Indention + "[\"" + column.Name + "\" => " + column.Value +
                                           " [" + column.Type + "]]");
                    }
                }
                else
                    builder.AppendLine(indent + Indention + "[Columns parsing failed!]");
            }
            else if (obj is PyBool)
            {
                builder.AppendLine(indent + PrintBool(obj as PyBool) + PrintRawData(obj));
            }
            else if (obj is PySubStruct)
            {
                var subs = obj as PySubStruct;
                builder.AppendLine(indent + PrintSubStruct(subs) + PrintRawData(obj));
                Print(builder, indention + 1, subs.Definition);
            }
            else if (obj is PyChecksumedStream)
            {
                var chk = obj as PyChecksumedStream;
                builder.AppendLine(indent + PrintChecksumedStream(chk));
                Print(builder, indention + 1, chk.Data);
            }
            else
                builder.AppendLine(indent + "[Warning: unable to print " + obj.Type + "]");
        }

        private static string PrintChecksumedStream(PyChecksumedStream obj)
        {
            return "[PyChecksumedStream Checksum: " + obj.Checksum + "]";
        }

        private static string PrintRawData(PyObject obj)
        {
            if (obj.RawSource == null)
                return "";
            return " [" + BitConverter.ToString(obj.RawSource, 0, obj.RawSource.Length > 8 ? 8 : obj.RawSource.Length) + "]";
        }

        private static string PrintSubStruct(PySubStruct substruct)
        {
            return "[PySubStruct]";
        }

        private static string PrintBool(PyBool boolean)
        {
            return "[PyBool " + boolean.Value + "]";
        }

        private static string PrintPackedRow(PyPackedRow packedRow)
        {
            return "[PyPackedRow " + packedRow.RawData.Length + " bytes]";
        }

        private static string PrintToken(PyToken token)
        {
            return "[PyToken " + token.Token + "]";
        }

        private static string PrintObjectEx(PyObjectEx obj)
        {
            return "[PyObjectEx " + (obj.IsType2 ? "Type2" : "Normal") + "]";
        }

        private static string PrintDict(PyDict dict)
        {
            return "[PyDict " + dict.Dictionary.Count + " kvp]";
        }

        private static string PrintSubStream(PySubStream sub)
        {
            if (sub.RawData != null)
                return "[PySubStream " + sub.RawData.Length + " bytes]";
            return "[PySubStream]";
        }

        private static string PrintIntegerVar(PyIntegerVar intvar)
        {
            return "[PyIntegerVar " + intvar.IntValue + "]";
        }

        private static string PrintList(PyList list)
        {
            return "[PyList " + list.Items.Count + " items]";
        }

        private static string PrintObjectData(PyObjectData data)
        {
            return "[PyObjectData Name: " + data.Name + "]";
        }

        private static string PrintBuffer(PyBuffer buf)
        {
            return "[PyBuffer " + buf.Data.Length + " bytes]";
        }

        private static string PrintLongLong(PyLongLong ll)
        {
            return "[PyLongLong " + ll.Value + "]";
        }

        private static string PrintTuple(PyTuple tuple)
        {
            return "[PyTuple " + tuple.Items.Count + " items]";
        }

        private static string PrintInt(PyInt integer)
        {
            return "[PyInt " + integer.Value + "]";
        }

        private static string PrintFloat(PyFloat fl)
        {
            return "[PyFloat " + fl.Value + "]";
        }

        private static string PrintString(PyString str, string indention)
        {
// aggressive destiny update pretty printing
#if false
            UpdateReader reader;
            try
            {
                reader = new UpdateReader();
                reader.Read(new MemoryStream(str.Raw));
            }
            catch (Exception)
            {
                reader = null;
            }

            if (reader == null)
                return indention + "[PyString \"" + str.Value + "\"]";

            return eveDestiny.PrettyPrinter.Print(reader, indention);
#endif
            return indention + "[PyString \"" + str.Value + "\"]";
        }

        private static string PrintNone(PyNone none)
        {
            return "[PyNone]";
        }
    }

}