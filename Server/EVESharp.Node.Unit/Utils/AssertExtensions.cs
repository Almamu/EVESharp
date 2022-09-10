using System.Collections.Generic;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;
using NUnit.Framework;

namespace EVESharp.Node.Unit.Utils;

public static class AssertExtensions
{
    public static void AssertKeyValContains (PyObjectData keyval, Dictionary<string, PyDataType> values)
    {
        Assert.DoesNotThrow (
            () =>
            {
                KeyVal.ToDictionary (keyval);
            });

        PyDictionary dictionary = KeyVal.ToDictionary (keyval);

        foreach (KeyValuePair <string, PyDataType> pair in values)
        {
            Assert.IsTrue (dictionary.TryGetValue (pair.Key, out PyDataType value));
            Assert.AreEqual (pair.Value, value);
        }
    }
}