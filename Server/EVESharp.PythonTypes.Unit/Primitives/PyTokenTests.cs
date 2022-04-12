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
        PyToken obj4 = null;
        
        Assert.True(obj1 == obj3);
        Assert.False(obj2 == obj3);
        
        Assert.False(obj1 == null);
        Assert.True(obj1 != null);
        Assert.False(obj1 is null);
        Assert.True(obj1 is not null);
        Assert.True(obj4 == null);
        Assert.False(obj4 != null);
        Assert.True(obj4 is null);
        Assert.False(obj4 is not null);
        Assert.False(obj1 == obj4);
        Assert.True(obj1 != obj4);
    }
}