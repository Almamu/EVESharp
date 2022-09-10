using EVESharp.Types.Collections;
using NUnit.Framework;

namespace EVESharp.Types.Unit.Types.Primitives;

public class PyTupleTests
{
    [Test]
    public void TupleComparison()
    {
        PyTuple obj1 = new PyTuple(3) { [0] = 500, [1] = 100, [2] = 300 };
        PyTuple obj2 = new PyTuple(1) { [0] = 100 };
        PyTuple obj3 = new PyTuple(3) { [0] = 500, [1] = 100, [2] = 300 };
        PyTuple obj4 = null;

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