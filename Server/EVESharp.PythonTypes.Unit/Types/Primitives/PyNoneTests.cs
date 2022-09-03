using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Types.Primitives;

public class PyNoneTests
{
    [Test]
    public void NoneComparison()
    {
        PyNone obj1 = null;
        PyNone obj2 = new PyNone();
        PyInteger obj3 = 0;
        PyInteger obj4 = null;

        Assert.True(obj1 == obj2);
        Assert.False(obj1 != obj2);
        Assert.True(obj1 == obj4);
        Assert.False(obj1 != obj4);

        Assert.False(obj1 == obj3);
        Assert.True(obj1 != obj3);
    }
}