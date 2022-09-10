using System;
using System.Collections.Generic;
using EVESharp.Types.Collections;

namespace EVESharp.Types;

/// <summary>
/// Base class for all the python data types that EVE supports
/// </summary>
public abstract class PyDataType
{
    public override bool Equals (object obj)
    {
        // first check references to make things quick under some situations
        if (ReferenceEquals (this, obj)) return true;
        if (ReferenceEquals (null, obj)) return false;

        // last but not least, check types
        if (this.GetType () != obj.GetType ()) return false;

        return obj is PyDataType && this.GetHashCode () == obj.GetHashCode ();
    }

    public override int GetHashCode ()
    {
        throw new NotImplementedException ();
    }

    public static implicit operator PyDataType (string str)
    {
        if (str is null)
            return null;

        return new PyString (str);
    }

    public static implicit operator PyDataType (ulong value)
    {
        return new PyInteger ((long) value);
    }

    public static implicit operator PyDataType (long value)
    {
        return new PyInteger (value);
    }

    public static implicit operator PyDataType (uint value)
    {
        return new PyInteger (value);
    }

    public static implicit operator PyDataType (int value)
    {
        return new PyInteger (value);
    }

    public static implicit operator PyDataType (ushort value)
    {
        return new PyInteger (value);
    }

    public static implicit operator PyDataType (short value)
    {
        return new PyInteger (value);
    }

    public static implicit operator PyDataType (byte value)
    {
        return new PyInteger (value);
    }

    public static implicit operator PyDataType (sbyte value)
    {
        return new PyInteger (value);
    }

    public static implicit operator PyDataType (long? value)
    {
        if (value is null)
            return null;

        return new PyInteger ((long) value);
    }

    public static implicit operator PyDataType (int? value)
    {
        if (value is null)
            return null;

        return new PyInteger ((int) value);
    }

    public static implicit operator PyDataType (short? value)
    {
        if (value is null)
            return null;

        return new PyInteger ((short) value);
    }

    public static implicit operator PyDataType (byte? value)
    {
        if (value is null)
            return null;

        return new PyInteger ((byte) value);
    }

    public static implicit operator PyDataType (sbyte? value)
    {
        if (value is null)
            return null;

        return new PyInteger ((sbyte) value);
    }

    public static implicit operator PyDataType (byte [] value)
    {
        if (value is null)
            return null;

        return new PyBuffer (value);
    }

    public static implicit operator PyDataType (float value)
    {
        return new PyDecimal (value);
    }

    public static implicit operator PyDataType (double value)
    {
        return new PyDecimal (value);
    }

    public static implicit operator PyDataType (bool value)
    {
        return new PyBool (value);
    }

    public static implicit operator PyDataType (float? value)
    {
        if (value is null)
            return null;

        return new PyDecimal ((float) value);
    }

    public static implicit operator PyDataType (double? value)
    {
        if (value is null)
            return null;

        return new PyDecimal ((double) value);
    }

    public static implicit operator PyDataType (bool? value)
    {
        if (value is null)
            return null;

        return new PyBool ((bool) value);
    }

    public static implicit operator PyDataType (Dictionary <PyDataType, PyDataType> value)
    {
        return new PyDictionary (value);
    }

    public static implicit operator PyDataType (List <PyDataType> value)
    {
        return new PyList (value);
    }

    public static bool operator == (PyDataType left, PyDataType right)
    {
        // ensure the left side is not null so it can be used for comparison
        if (ReferenceEquals (left, right)) return true;

        // do extra checks for PyNones just in case
        if (left is PyNone && ReferenceEquals (null,  right)) return true;
        if (right is PyNone && ReferenceEquals (null, left)) return true;

        // normal checks for other types
        if (ReferenceEquals (null, left)) return false;

        // call the Equals method to perform the actual comparison
        return left.Equals (right);
    }

    public static bool operator != (PyDataType left, PyDataType right)
    {
        return !(left == right);
    }
}