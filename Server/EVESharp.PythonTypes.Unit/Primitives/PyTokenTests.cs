using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyTokenTests
{
    [Test]
    public void TokenComparison()
    {
        PyToken obj1 = new PyToken("hello");
        PyToken obj2 = new PyToken("world");
        PyToken obj3 = new PyToken("hello");

        Assert.True(obj1 == obj3);
        Assert.False(obj2 == obj3);
    }
}