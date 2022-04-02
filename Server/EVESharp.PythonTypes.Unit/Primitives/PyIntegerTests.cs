using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyIntegerTests
{
    private const int VALUE1 = 100;
    private const int VALUE2 = 300;
    [Test]
    public void IntegerComparison()
    {
        PyInteger obj1 = new PyInteger(VALUE1);
        PyInteger obj2 = new PyInteger(VALUE2);
        PyInteger obj3 = new PyInteger(VALUE1);

        Assert.True(obj1 == obj3);
        Assert.False(obj2 == obj3);
        Assert.True(obj1 < obj2);
        Assert.True(obj1 <= obj3);
        Assert.True(obj1 >= obj3);
        Assert.True(obj2 > obj1);

        Assert.True(obj1 == VALUE1);
        Assert.False(obj1 == VALUE2);
        Assert.False(obj1 != VALUE1);
        Assert.True(obj1 != VALUE2);
    }

    [Test]
    public void IntegerAssignment()
    {
        PyInteger obj1 = VALUE1;
        PyInteger obj2 = VALUE2;
        
        Assert.True(obj1 == VALUE1);
        Assert.True(obj2 == VALUE2);
    }
}