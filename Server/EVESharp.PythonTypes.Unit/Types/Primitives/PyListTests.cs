using EVESharp.PythonTypes.Types.Collections;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Types.Primitives;

public class PyListTests
{
    [Test]
    public void ListComparison()
    {
        PyList obj1 = new PyList() { 500, 100, 300 };
        PyList obj2 = new PyList() { 100 };
        PyList obj3 = new PyList() { 500, 100, 300 };
        PyList obj4 = null;

        Assert.True(obj1 == obj3);
        Assert.False(obj1 == obj2);
        Assert.False(obj1 != obj3);
        Assert.True(obj1 != obj2);
        
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