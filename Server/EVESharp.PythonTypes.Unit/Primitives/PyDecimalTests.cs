using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyDecimalTests
{
    private const double VALUE1 = 100;
    private const double VALUE2 = 150;
    
    [Test]
    public void DecimalComparison()
    {
        PyDecimal obj1 = new PyDecimal(VALUE1);
        PyDecimal obj2 = new PyDecimal(VALUE2);
        PyDecimal obj3 = new PyDecimal(VALUE1);
        PyDecimal obj4 = null;

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
    public void DecimalAssignment()
    {
        PyDecimal obj1 = VALUE1;
        PyDecimal obj2 = VALUE2;
        
        Assert.True(obj1 == VALUE1);
        Assert.True(obj2 == VALUE2);
    }
}