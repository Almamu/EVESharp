using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyBoolTests
{
    private const bool VALUE1 = true;
    private const bool VALUE2 = false;
    [Test]
    public void IntegerComparison()
    {
        PyBool obj1 = new PyBool(VALUE1);
        PyBool obj2 = new PyBool(VALUE2);
        PyBool obj3 = new PyBool(VALUE1);

        Assert.True(obj1 == obj3);
        Assert.False(obj2 == obj3);

        Assert.True(obj1 == VALUE1);
        Assert.False(obj1 == VALUE2);
        Assert.False(obj1 != VALUE1);
        Assert.True(obj1 != VALUE2);
    }

    [Test]
    public void IntegerAssignment()
    {
        PyBool obj1 = VALUE1;
        PyBool obj2 = VALUE2;
        
        Assert.True(obj1 == VALUE1);
        Assert.True(obj2 == VALUE2);
    }
}