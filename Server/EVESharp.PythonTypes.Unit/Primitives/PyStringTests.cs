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
        PyString obj4 = null;

        Assert.True(obj1 == obj3);
        Assert.False(obj2 == obj3);
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

    [Test]
    public void StringTable()
    {
        PyString obj1 = "macho.MachoAddress";
        PyString obj2 = "SomeRandomString";

        Assert.True(obj1.IsStringTableEntry);
        Assert.False(obj2.IsStringTableEntry);
    }
}