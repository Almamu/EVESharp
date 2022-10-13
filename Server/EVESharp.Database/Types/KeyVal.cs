using System;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Types;

public class KeyVal
{
    public const string OBJECT_NAME = "util.KeyVal";

    /// <summary>
    /// Simple helper method that creates the correct KeyVal data off a dictionary and
    /// returns it's PyDataType representation, ready to be sent to the EVE Online client
    /// </summary>
    /// <param name="columns"></param>
    /// <returns></returns>
    public static PyDataType FromDictionary (PyDictionary columns)
    {
        return new PyObjectData (OBJECT_NAME, columns);
    }

    /// <summary>
    /// Converts a KeyVal to a normal PyDictionary
    /// </summary>
    /// <param name="from">The KeyVal to convert to a dictionary</param>
    /// <returns>The information in the keyval</returns>
    /// <exception cref="InvalidCastException">If the type is not a keyval</exception>
    public static PyDictionary ToDictionary (PyObjectData from)
    {
        if (from.Name != OBJECT_NAME)
            throw new InvalidCastException ($"Trying to cast a {from.Name} to a {OBJECT_NAME}");

        return from.Arguments as PyDictionary;
    }
}