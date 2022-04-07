using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Collections;

namespace EVESharp.PythonTypes.Types.Primitives;

public class PyString : PyDataType
{
    public string                     Value                 { get; }
    public int                        Length                => Value.Length;
    public bool                       IsStringTableEntry    { get; }
    public StringTableUtils.EntryList StringTableEntryIndex { get; }
    public bool                       IsUTF8                { get; }

    public PyString (string value, bool isUTF8 = false)
    {
        IsStringTableEntry = false;

        // string found in the table, write a string entry and return
        if (StringTableUtils.LookupTable.TryGetValue (value, out StringTableUtils.EntryList index))
        {
            IsStringTableEntry    = true;
            StringTableEntryIndex = index;
        }

        Value  = value;
        IsUTF8 = isUTF8;
    }

    public PyString (StringTableUtils.EntryList entry)
    {
        Value                 = StringTableUtils.Entries [(int) entry];
        IsStringTableEntry    = true;
        StringTableEntryIndex = entry;
    }

    public override int GetHashCode ()
    {
        return Value is not null ? Value.GetHashCode () : 0;
    }

    public static bool operator == (PyString obj, string value)
    {
        if (ReferenceEquals (null, obj))
        {
            if (value == null)
                return true;

            return false;
        }

        return obj.Value == value;
    }

    public static bool operator != (PyString obj, string value)
    {
        return !(obj == value);
    }

    public static implicit operator string (PyString obj)
    {
        if (obj == null)
            return null;

        return obj.Value;
    }

    public static implicit operator PyString (string value)
    {
        return new PyString (value);
    }

    public static implicit operator PyString (char value)
    {
        return new PyString (new string (new [] {value}));
    }

    public override string ToString ()
    {
        return Value;
    }

    public static PyString Join <T> (char separator, PyList <T> collection) where T : PyDataType
    {
        return string.Join <T> (separator, collection);
    }
}