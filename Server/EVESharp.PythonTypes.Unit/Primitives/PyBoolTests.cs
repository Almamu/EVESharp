using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyBoolTests
{
    private const bool VALUE1 = true;
    private const bool VALUE2 = false;
    [Test]
    public void BooleanComparison()
    {
        PyBool obj1 = new PyBool(VALUE1);
        PyBool obj2 = new PyBool(VALUE2);
        PyBool obj3 = new PyBool(VALUE1);
        PyBool obj4 = null;

        Assert.True(obj1 == obj3);
        Assert.False(obj2 == obj3);

        Assert.True(obj1 == VALUE1);
        Assert.False(obj1 == VALUE2);
        Assert.False(obj1 != VALUE1);
        Assert.True(obj1 != VALUE2);
        
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
    public void BooleanAssignment()
    {
        PyBool obj1 = VALUE1;
        PyBool obj2 = VALUE2;
        
        Assert.True(obj1 == VALUE1);
        Assert.True(obj2 == VALUE2);
    }
}