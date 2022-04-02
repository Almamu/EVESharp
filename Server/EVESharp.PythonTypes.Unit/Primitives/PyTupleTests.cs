using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyTupleTests
{
    [Test]
    public void TupleComparison()
    {
        PyTuple obj1 = new PyTuple(3) { [0] = 500, [1] = 100, [2] = 300 };
        PyTuple obj2 = new PyTuple(1) { [0] = 100 };
        PyTuple obj3 = new PyTuple(3) { [0] = 500, [1] = 100, [2] = 300 };

        Assert.True(obj1 == obj3);
        Assert.False(obj1 == obj2);
        Assert.False(obj1 != obj3);
        Assert.True(obj1 != obj2);
    }
}