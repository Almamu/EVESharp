using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyStringTests
{
    [Test]
    public void StringComparison()
    {
        PyString obj1 = new PyString("hello");
        PyString obj2 = new PyString("world");
        PyString obj3 = new PyString("hello");

        Assert.True(obj1 == obj3);
        Assert.False(obj2 == obj3);
        Assert.False(obj1 != obj3);
        Assert.True(obj1 != obj2);
    }
}