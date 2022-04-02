using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyChecksumedStreamTests
{
    public static readonly PyList list = new PyList() { 5000000, 15000000, 150.0 };
    public static readonly PyTuple tuple1 = new PyTuple(3) { [0] = list, [1] = list, [2] = list };
    public static readonly PyTuple tuple2 = new PyTuple(1) {[0] = list};
    
    [Test]
    public void ChecksumedStreamComparison()
    {
        PyChecksumedStream obj1 = new PyChecksumedStream(tuple1);
        PyChecksumedStream obj2 = new PyChecksumedStream(tuple2);
        PyChecksumedStream obj3 = new PyChecksumedStream(tuple1);

        Assert.True(obj1 == obj3);
        Assert.False(obj1 == obj2);
        Assert.False(obj1 != obj3);
        Assert.True(obj1 != obj2);
    }
}