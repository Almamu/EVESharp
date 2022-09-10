using EVESharp.Types.Collections;
using NUnit.Framework;

namespace EVESharp.Types.Unit.Types.Primitives;

public class PyObjectDataTests
{
    public static readonly PyList list = new PyList() { 5000000, 15000000, 150.0 };
    public static readonly PyTuple tuple1 = new PyTuple(3) { [0] = list, [1] = list, [2] = list };
    public static readonly PyTuple tuple2 = new PyTuple(1) {[0] = list};
    
    [Test]
    public void PyObjectDataComparison()
    {
        PyObjectData obj1 = new PyObjectData("HELLO", tuple1);
        PyObjectData obj2 = new PyObjectData("WORLD", tuple1);
        PyObjectData obj3 = new PyObjectData("HELLO", tuple1);
        PyObjectData obj4 = new PyObjectData("HELLO", tuple2);
        PyObjectData obj5 = new PyObjectData("WORLD", tuple2);
        PyObjectData obj6 = new PyObjectData("HELLO", tuple2);
        PyObjectData obj7 = null;

        Assert.True(obj1 == obj3);
        Assert.False(obj1 == obj2);
        Assert.False(obj1 != obj3);
        Assert.True(obj1 != obj2);

        Assert.True(obj4 == obj6);
        Assert.False(obj4 == obj5);
        Assert.False(obj4 != obj6);
        Assert.True(obj4 != obj5);

        Assert.False(obj1 == obj4);
        Assert.True(obj1 != obj4);
        
        Assert.False(obj1 == null);
        Assert.True(obj1 != null);
        Assert.False(obj1 is null);
        Assert.True(obj1 is not null);
        Assert.True(obj7 == null);
        Assert.False(obj7 != null);
        Assert.True(obj7 is null);
        Assert.False(obj7 is not null);
        Assert.False(obj1 == obj7);
        Assert.True(obj1 != obj7);
    }
}