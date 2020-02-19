using System.Collections.Generic;
using System.Text;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace PythonTypes
{
    public class PrettyPrinter
    {
        private const string Indention = "  ";

        private readonly StringBuilder mStringBuilder = new StringBuilder();
        private int mIndentation = 0;

        public static string FromDataType(PyDataType obj)
        {
            PrettyPrinter printer = new PrettyPrinter();

            return printer.Process(obj);
        }

        protected string Process(PyDataType obj)
        {
            // add indentation to the string
            for (int i = 0; i < this.mIndentation; i++)
                this.mStringBuilder.Append(Indention);

            if (obj == null || obj is PyNone)
                this.ProcessNone();
            else if (obj is PyString)
                this.ProcessString(obj as PyString);
            else if (obj is PyToken)
                this.ProcessToken(obj as PyToken);
            else if (obj is PyInteger)
                this.ProcessInteger(obj as PyInteger);
            else if (obj is PyDecimal)
                this.ProcessDecimal(obj as PyDecimal);
            else if (obj is PyBuffer)
                this.ProcessBuffer(obj as PyBuffer);
            else if (obj is PyBool)
                this.ProcessBoolean(obj as PyBool);
            else if (obj is PyTuple)
                this.ProcessTuple(obj as PyTuple);
            else if (obj is PyList)
                this.ProcessList(obj as PyList);
            else if (obj is PyDictionary)
                this.ProcessDictionary(obj as PyDictionary);
            else if (obj is PyChecksumedStream)
                this.ProcessChecksumedStream(obj as PyChecksumedStream);
            else if (obj is PyObject)
                this.ProcessObject(obj as PyObject);
            else if (obj is PyObjectData)
                this.ProcessObjectData(obj as PyObjectData);
            else if (obj is PySubStream)
                this.ProcessSubStream(obj as PySubStream);
            else if (obj is PySubStruct)
                this.ProcessSubStruct(obj as PySubStruct);
            else if (obj is PyPackedRow)
                this.ProcessPackedRow(obj as PyPackedRow);
            else
                this.mStringBuilder.AppendLine("[--PyUnknown--]");

            return this.mStringBuilder.ToString();
        }

        private void ProcessString(PyString str)
        {
            this.mStringBuilder.AppendFormat("[PyString {0} char(s): '{1}']", str.Length, str.Value);
            this.mStringBuilder.AppendLine();
        }

        private void ProcessToken(PyToken token)
        {
            this.mStringBuilder.AppendFormat("[PyToken {0} bytes: '{1}']", token.Token.Length, token.Token);
            this.mStringBuilder.AppendLine();
        }

        private void ProcessBoolean(PyBool boolean)
        {
            this.mStringBuilder.AppendFormat("[PyBool {0}]", (boolean) ? "true" : "false");
            this.mStringBuilder.AppendLine();
        }

        private void ProcessInteger(PyInteger integer)
        {
            this.mStringBuilder.AppendFormat("[PyInteger {0}]", integer.Value);
            this.mStringBuilder.AppendLine();
        }

        private void ProcessDecimal(PyDecimal dec)
        {
            this.mStringBuilder.AppendFormat("[PyDecimal {0}]", dec.Value);
            this.mStringBuilder.AppendLine();
        }

        private void ProcessNone()
        {
            this.mStringBuilder.Append("[PyNone]");
            this.mStringBuilder.AppendLine();
        }

        private void ProcessTuple(PyTuple tuple)
        {
            this.mStringBuilder.AppendFormat("[PyTuple {0} items]", tuple.Count);
            this.mStringBuilder.AppendLine();
            this.mIndentation++;

            // process all child elements
            foreach (PyDataType data in tuple)
                this.Process(data);

            this.mIndentation--;
        }

        private void ProcessList(PyList list)
        {
            this.mStringBuilder.AppendFormat("[PyList {0} items]", list.Count);
            this.mStringBuilder.AppendLine();
            this.mIndentation++;

            // process all child elements
            foreach (PyDataType data in list)
                this.Process(data);

            this.mIndentation--;
        }

        private void ProcessBuffer(PyBuffer buffer)
        {
            this.mStringBuilder.AppendFormat(
                "[PyBuffer {0} bytes: {1}]", buffer.Length, HexDump.ByteArrayToHexViaLookup32(buffer.Value)
            );
            this.mStringBuilder.AppendLine();
        }

        private void ProcessDictionary(PyDictionary dictionary)
        {
            this.mStringBuilder.AppendFormat("[PyDictionary {0} entries]", dictionary.Length);
            this.mStringBuilder.AppendLine();
            this.mIndentation++;

            // process all the keys and values
            foreach (KeyValuePair<string, PyDataType> pair in dictionary)
            {
                this.Process(pair.Key);
                this.Process(pair.Value);
            }

            this.mIndentation--;
        }

        private void ProcessChecksumedStream(PyChecksumedStream stream)
        {
            this.mStringBuilder.Append("[PyChecksumedStream]");
            this.mStringBuilder.AppendLine();
            this.mIndentation++;

            this.Process(stream.Data);

            this.mIndentation--;
        }

        private void ProcessObject(PyObject obj)
        {
            this.mStringBuilder.AppendFormat("[PyObject {0}]", (obj.Header.IsType2) ? "Type2" : "Type1");
            this.mStringBuilder.AppendLine();
            this.mIndentation++;

            // process all object's parts
            this.Process(obj.Header);
            this.Process(obj.List);
            this.Process(obj.Dictionary);

            this.mIndentation--;
        }

        private void ProcessObjectData(PyObjectData data)
        {
            this.mStringBuilder.AppendFormat("[PyObjectData {0}]", data.Name.Value);
            this.mStringBuilder.AppendLine();
            this.mIndentation++;

            this.Process(data.Arguments);

            this.mIndentation--;
        }

        private void ProcessSubStream(PySubStream stream)
        {
            this.mStringBuilder.AppendFormat("[PySubStream]");
            this.mStringBuilder.AppendLine();
            this.mIndentation++;

            this.Process(stream.Stream);

            this.mIndentation--;
        }

        private void ProcessSubStruct(PySubStruct subStruct)
        {
            this.mStringBuilder.AppendFormat("[PySubStruct]");
            this.mStringBuilder.AppendLine();
            this.mIndentation++;

            this.Process(subStruct.Definition);

            this.mIndentation--;
        }

        private void ProcessPackedRow(PyPackedRow packedRow)
        {
            this.mStringBuilder.AppendFormat("[PyPackedRow {0} columns]", packedRow.Header.Columns.Count);
            if (packedRow.Header.Columns.Count > 0)
                this.mStringBuilder.AppendLine();
            this.mIndentation++;

            foreach (DBRowDescriptor.Column column in packedRow.Header.Columns)
            {
                this.mStringBuilder.AppendFormat("[PyPackedRowColumn '{0}']", column.Name);
                this.mStringBuilder.AppendLine();
                this.Process(packedRow[column.Name]);
            }

            this.mIndentation--;
        }

        // TODO: MIGHT BE A GOOD IDEA TO IMPLEMENT A MARSHAL-STREAM DUMP TOO
    }
}