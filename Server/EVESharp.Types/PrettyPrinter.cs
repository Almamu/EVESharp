using System.Text;
using EVESharp.Common;
using EVESharp.Types.Collections;

namespace EVESharp.Types;

/// <summary>
/// A helper class to dump any Python type to file or console in a readable format for the user
/// </summary>
public class PrettyPrinter
{
    /// <summary>
    /// The character used for an indention level
    /// </summary>
    private const string INDENTATION_CHARACTER = "  ";

    /// <summary>
    /// The output buffer used to write the values to
    /// </summary>
    protected readonly StringBuilder mStringBuilder = new StringBuilder ();
    /// <summary>
    /// The current level of indention
    /// </summary>
    protected int mIndentation;

    protected PrettyPrinter () { }

    /// <summary>
    /// Utility method, creates a new pretty printer instance and dumps the given <paramref name="obj" />
    /// </summary>
    /// <param name="obj">The Python type to dump</param>
    /// <returns></returns>
    public static string FromDataType (PyDataType obj)
    {
        PrettyPrinter printer = new PrettyPrinter ();

        printer.Process (obj);

        return printer.GetResult ();
    }

    /// <summary>
    /// Obtains the finalized dump
    /// </summary>
    /// <returns>The string representation of the dumped data with this PrettyPrinter's instance</returns>
    protected string GetResult ()
    {
        return this.mStringBuilder.ToString ();
    }

    protected virtual void AppendIndentation ()
    {
        // add indentation to the string
        for (int i = 0; i < this.mIndentation; i++)
            this.mStringBuilder.Append (INDENTATION_CHARACTER);
    }

    /// <summary>
    /// Main body of the pretty-printer class, takes care of type identification and passing it to the correct
    /// formatter for proper dump
    /// </summary>
    /// <param name="obj">The python type to dump</param>
    protected void Process (PyDataType obj)
    {
        this.AppendIndentation ();
        this.ProcessPythonType (obj);
    }

    protected virtual void ProcessPythonType (PyDataType obj)
    {
        switch (obj)
        {
            case PyNone:
            case null:
                this.ProcessNone ();
                break;

            case PyString pyString:
                this.ProcessString (pyString);
                break;

            case PyToken pyToken:
                this.ProcessToken (pyToken);
                break;

            case PyInteger pyInteger:
                this.ProcessInteger (pyInteger);
                break;

            case PyDecimal pyDecimal:
                this.ProcessDecimal (pyDecimal);
                break;

            case PyBuffer pyBuffer:
                this.ProcessBuffer (pyBuffer);
                break;

            case PyBool pyBool:
                this.ProcessBoolean (pyBool);
                break;

            case PyTuple pyTuple:
                this.ProcessTuple (pyTuple);
                break;

            case PyList pyList:
                this.ProcessList (pyList);
                break;

            case PyDictionary pyDictionary:
                this.ProcessDictionary (pyDictionary);
                break;

            case PyChecksumedStream pyChecksumedStream:
                this.ProcessChecksumedStream (pyChecksumedStream);
                break;

            case PyObject pyObject:
                this.ProcessObject (pyObject);
                break;

            case PyObjectData pyObjectData:
                this.ProcessObjectData (pyObjectData);
                break;

            case PySubStream pySubStream:
                this.ProcessSubStream (pySubStream);
                break;

            case PySubStruct pySubStruct:
                this.ProcessSubStruct (pySubStruct);
                break;

            case PyPackedRow pyPackedRow:
                this.ProcessPackedRow (pyPackedRow);
                break;

            default:
                this.mStringBuilder.AppendLine ("[--PyUnknown--]");
                break;
        }
    }

    protected virtual void ProcessString (PyString str)
    {
        this.mStringBuilder.AppendFormat ("[PyString {0} char(s): '{1}']", str.Length, str.Value);
        this.mStringBuilder.AppendLine ();
    }

    protected virtual void ProcessToken (PyToken token)
    {
        this.mStringBuilder.AppendFormat ("[PyToken {0} bytes: '{1}']", token.Token.Length, token.Token);
        this.mStringBuilder.AppendLine ();
    }

    protected virtual void ProcessBoolean (PyBool boolean)
    {
        this.mStringBuilder.AppendFormat ("[PyBool {0}]", boolean ? "true" : "false");
        this.mStringBuilder.AppendLine ();
    }

    protected virtual void ProcessInteger (PyInteger integer)
    {
        this.mStringBuilder.AppendFormat ("[PyInteger {0}]", integer.Value);
        this.mStringBuilder.AppendLine ();
    }

    protected virtual void ProcessDecimal (PyDecimal dec)
    {
        this.mStringBuilder.AppendFormat ("[PyDecimal {0}]", dec.Value);
        this.mStringBuilder.AppendLine ();
    }

    protected virtual void ProcessNone ()
    {
        this.mStringBuilder.Append ("[PyNone]");
        this.mStringBuilder.AppendLine ();
    }

    protected virtual void ProcessTuple (PyTuple tuple)
    {
        this.mStringBuilder.AppendFormat ("[PyTuple {0} items]", tuple.Count);
        this.mStringBuilder.AppendLine ();
        this.mIndentation++;

        // process all child elements
        foreach (PyDataType data in tuple)
            this.Process (data);

        this.mIndentation--;
    }

    protected virtual void ProcessList (PyList list)
    {
        this.mStringBuilder.AppendFormat ("[PyList {0} items]", list.Count);
        this.mStringBuilder.AppendLine ();
        this.mIndentation++;

        // process all child elements
        foreach (PyDataType data in list)
            this.Process (data);

        this.mIndentation--;
    }

    protected virtual void ProcessBuffer (PyBuffer buffer)
    {
        this.mStringBuilder.AppendFormat ("[PyBuffer {0} bytes: {1}]", buffer.Length, HexDump.ByteArrayToHexViaLookup32 (buffer.Value));
        this.mStringBuilder.AppendLine ();
    }

    protected virtual void ProcessDictionary (PyDictionary dictionary)
    {
        this.mStringBuilder.AppendFormat ("[PyDictionary {0} entries]", dictionary.Length);
        this.mStringBuilder.AppendLine ();
        this.mIndentation++;

        // process all the keys and values
        foreach (PyDictionaryKeyValuePair pair in dictionary)
        {
            this.Process (pair.Key);
            this.Process (pair.Value);
        }

        this.mIndentation--;
    }

    protected virtual void ProcessChecksumedStream (PyChecksumedStream stream)
    {
        this.mStringBuilder.Append ("[PyChecksumedStream]");
        this.mStringBuilder.AppendLine ();
        this.mIndentation++;

        this.Process (stream.Data);

        this.mIndentation--;
    }

    protected virtual void ProcessObject (PyObject obj)
    {
        this.mStringBuilder.AppendFormat ("[PyObject {0}]", obj.IsType2 ? "Type2" : "Type1");
        this.mStringBuilder.AppendLine ();
        this.mIndentation++;

        // process all object's parts
        this.Process (obj.Header);
        this.Process (obj.List);
        this.Process (obj.Dictionary);

        this.mIndentation--;
    }

    protected virtual void ProcessObjectData (PyObjectData data)
    {
        this.mStringBuilder.AppendFormat ("[PyObjectData {0}]", data.Name.Value);
        this.mStringBuilder.AppendLine ();
        this.mIndentation++;

        this.Process (data.Arguments);

        this.mIndentation--;
    }

    protected virtual void ProcessSubStream (PySubStream stream)
    {
        this.mStringBuilder.AppendFormat ("[PySubStream]");
        this.mStringBuilder.AppendLine ();
        this.mIndentation++;

        this.Process (stream.Stream);

        this.mIndentation--;
    }

    protected virtual void ProcessSubStruct (PySubStruct subStruct)
    {
        this.mStringBuilder.AppendFormat ("[PySubStruct]");
        this.mStringBuilder.AppendLine ();
        this.mIndentation++;

        this.Process (subStruct.Definition);

        this.mIndentation--;
    }

    protected virtual void ProcessPackedRow (PyPackedRow packedRow)
    {
        this.mStringBuilder.AppendFormat ("[PyPackedRow {0} columns]", packedRow.Header.Columns.Count);
        if (packedRow.Header.Columns.Count > 0)
            this.mStringBuilder.AppendLine ();
        this.mIndentation++;

        foreach (DBRowDescriptor.Column column in packedRow.Header.Columns)
        {
            this.AppendIndentation ();
            this.mStringBuilder.AppendFormat ("[PyPackedRowColumn '{0}' ({1})]", column.Name, column.Type.ToString ());
            this.mStringBuilder.AppendLine ();
            this.Process (packedRow [column.Name]);
        }

        this.mIndentation--;
    }
}