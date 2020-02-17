using System;
using System.Text;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace PythonTypes
{
    public static class PrettyPrinter
    {
        public const string Indention = "  ";

        public static string Print(PyDataType obj)
        {
            var ret = new StringBuilder();
            Print(ret, 0, obj);
            return ret.ToString();
        }

        private static void Print(StringBuilder builder, int indention, PyDataType obj)
        {
            var indent = "";
            for (int i = 0; i < indention; i++)
                indent += Indention;

            if (obj is PyString)
                builder.AppendLine(PrintString(obj as PyString, indent));
            else if (obj is PyNone)
                builder.AppendLine(indent + PrintNone(obj as PyNone));
            else if (obj is PyDecimal)
                builder.AppendLine(indent + PrintFloat(obj as PyDecimal));
            else if (obj is PyInteger)
                builder.AppendLine(indent + PrintInt(obj as PyInteger));
            else if (obj is PyTuple)
            {
                var tuple = obj as PyTuple;
                builder.AppendLine(indent + PrintTuple(tuple));
                foreach (var item in tuple)
                    Print(builder, indention + 1, item);
            }
            else if (obj is PyList)
            {
                var list = obj as PyList;
                builder.AppendLine(indent + PrintList(list));
                foreach (var item in list)
                    Print(builder, indention + 1, item);
            }
            else if (obj is PyBuffer)
                builder.AppendLine(indent + PrintBuffer(obj as PyBuffer));
            else if (obj is PyObjectData)
            {
                var objdata = obj as PyObjectData;
                builder.AppendLine(indent + PrintObjectData(objdata));
                Print(builder, indention + 1, objdata.Arguments);
            }
            else if (obj is PySubStream)
            {
                var sub = obj as PySubStream;
                builder.AppendLine(indent + PrintSubStream(sub));
                Print(builder, indention + 1, sub.Stream);
            }
            else if (obj is PyDictionary)
            {
                var dict = obj as PyDictionary;
                builder.AppendLine(indent + PrintDict(dict));
                foreach (var kvp in dict)
                {
                    Print(builder, indention + 1, kvp.Key);
                    Print(builder, indention + 1, kvp.Value);
                }
            }
            else if (obj is PyObject)
            {
                var objex = obj as PyObject;
                builder.AppendLine(indent + PrintObjectEx(objex));
                Print(builder, indention + 1, objex.Header);
                foreach (var item in objex.List)
                    Print(builder, indention + 1, item);

                if (objex.Dictionary != null)
                {
                    foreach (var kvp in objex.Dictionary)
                    {
                        Print(builder, indention + 1, kvp.Key);
                        Print(builder, indention + 1, kvp.Value);
                    }
                }
            }
            else if (obj is PyToken)
            {
                builder.AppendLine(indent + PrintToken(obj as PyToken));
            }
            else if (obj is PyPackedRow)
            {
                var packedRow = obj as PyPackedRow;
                // builder.AppendLine(indent + PrintPackedRow(packedRow));
                builder.AppendLine("[PyPackedRow]");
                foreach (var column in packedRow.Header.Columns)
                {
                    builder.AppendLine(indent + Indention + "[\"" + column.Name + "\" => " + packedRow[column.Name] +
                                       " [" + column.Type + "]]");
                }
            }
            else if (obj is PyBool)
            {
                builder.AppendLine(indent + PrintBool(obj as PyBool));
            }
            else if (obj is PySubStruct)
            {
                var subs = obj as PySubStruct;
                builder.AppendLine(indent + PrintSubStruct(subs));
                Print(builder, indention + 1, subs.Definition);
            }
            else if (obj is PyChecksumedStream)
            {
                var chk = obj as PyChecksumedStream;
                builder.AppendLine(indent + PrintChecksumedStream(chk));
                Print(builder, indention + 1, chk.Data);
            }
            else
                builder.AppendLine(indent + "[Warning: unable to print " + obj + "]");
        }

        private static string PrintChecksumedStream(PyChecksumedStream obj)
        {
            return "[PyChecksumedStream]";
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
            return "[PyPackedRow]";
        }

        private static string PrintToken(PyToken token)
        {
            return "[PyToken " + token.Token + "]";
        }

        private static string PrintObjectEx(PyObject obj)
        {
            return "[PyObjectEx " + (obj.Header.IsType2 ? "Type2" : "Normal") + "]";
        }

        private static string PrintDict(PyDictionary dict)
        {
            return "[PyDict " + dict.Length + " kvp]";
        }

        private static string PrintSubStream(PySubStream sub)
        {
            return "[PySubStream]";
        }

        private static string PrintList(PyList list)
        {
            return "[PyList " + list.Count + " items]";
        }

        private static string PrintObjectData(PyObjectData data)
        {
            return "[PyObjectData Name: " + data.Name + "]";
        }

        private static string PrintBuffer(PyBuffer buf)
        {
            return "[PyBuffer " + buf.Value.Length + " bytes]";
        }
        
        private static string PrintTuple(PyTuple tuple)
        {
            return "[PyTuple " + tuple.Count + " items]";
        }

        private static string PrintInt(PyInteger integer)
        {
            return "[PyInt " + integer.Value + "]";
        }

        private static string PrintFloat(PyDecimal fl)
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