using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyListTests
{
    [Test]
    public void ListComparison()
    {
        PyList obj1 = new PyList() { 500, 100, 300 };
        PyList obj2 = new PyList() { 100 };
        PyList obj3 = new PyList() { 500, 100, 300 };

        Assert.True(obj1 == obj3);
        Assert.False(obj1 == obj2);
        Assert.False(obj1 != obj3);
        Assert.True(obj1 != obj2);
    }
}