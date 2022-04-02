using EVESharp.PythonTypes.Types.Collections;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyDictionaryTests
{
    [Test]
    public void DictionaryComparison()
    {
        PyDictionary dict1 = new PyDictionary() {["hello"] = "world"};
        PyDictionary dict2 = new PyDictionary() {["world"] = "hello"};
        PyDictionary dict3 = new PyDictionary() {["hello"] = "world"};

        Assert.True(dict1 == dict3);
        Assert.False(dict2 == dict3);
        Assert.False(dict1 != dict3);
        Assert.True(dict1 != dict2);
    }
}