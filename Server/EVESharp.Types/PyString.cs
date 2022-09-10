using EVESharp.Types.Collections;
using EVESharp.Types.Serialization;

namespace EVESharp.Types;

public class PyString : PyDataType
{
    public string                     Value                 { get; }
    public int                        Length                => this.Value.Length;
    public bool                       IsStringTableEntry    { get; }
    public StringTableUtils.EntryList StringTableEntryIndex { get; }
    public bool                       IsUTF8                { get; }

    public PyString (string value, bool isUTF8 = false)
    {
        this.IsStringTableEntry = false;

        // string found in the table, write a string entry and return
        if (StringTableUtils.LookupTable.TryGetValue (value, out StringTableUtils.EntryList index))
        {
            this.IsStringTableEntry    = true;
            this.StringTableEntryIndex = index;
        }

        this.Value  = value;
        this.IsUTF8 = isUTF8;
    }

    public PyString (StringTableUtils.EntryList entry)
    {
        this.Value                 = StringTableUtils.Entries [(int) entry];
        this.IsStringTableEntry    = true;
        this.StringTableEntryIndex = entry;
    }

    public override int GetHashCode ()
    {
        return this.Value is not null ? this.Value.GetHashCode () : PyNone.HASH_VALUE;
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
        return this.Value;
    }

    public static PyString Join <T> (char separator, PyList <T> collection) where T : PyDataType
    {
        return string.Join <T> (separator, collection);
    }
}