using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Types.Primitives;

public class PyObjectTests
{
    public static readonly PyList list = new PyList() { 5000000, 15000000, 150.0 };
    public static readonly PyTuple tuple1 = new PyTuple(3) { [0] = list, [1] = list, [2] = list };
    public static readonly PyTuple tuple2 = new PyTuple(1) {[0] = list};
    public static readonly PyDictionary dict = new PyDictionary() {["hello"] = "world"};
    
    [Test]
    public void PyObjectComparison()
    {
        PyObject obj1 = new PyObject(false, tuple1, list, dict);
        PyObject obj2 = new PyObject(false, tuple2, list, dict);
        PyObject obj3 = new PyObject(false, tuple1, list, dict);
        PyObject obj4 = new PyObject(true, tuple1, list, dict);
        PyObject obj5 = new PyObject(true, tuple2, list, dict);
        PyObject obj6 = new PyObject(true, tuple1, list, dict);
        PyObject obj7 = null;

        Assert.True(obj1 == obj3);
        Assert.True(obj4 == obj6);
        Assert.False(obj1 == obj2);
        Assert.False(obj4 == obj5);

        Assert.True(obj1 != obj2);
        Assert.False(obj1 != obj3);
        Assert.True(obj4 != obj5);
        Assert.False(obj4 != obj6);

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