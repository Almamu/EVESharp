using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PyBufferTests
{
    private static readonly byte[] VALUE1 = new byte [] { 0x55, 0x30, 0x25 };
    private static readonly byte[] VALUE2 = new byte [] { 0x22, 0x35, 0x48 };
    
    [Test]
    public void BufferComparison()
    {
        PyBuffer obj1 = new PyBuffer(VALUE1);
        PyBuffer obj2 = new PyBuffer(VALUE2);
        PyBuffer obj3 = new PyBuffer(VALUE1);

        Assert.True(obj1 == obj3);
        Assert.False(obj2 == obj3);

        Assert.True(obj1 == VALUE1);
        Assert.False(obj1 == VALUE2);
        Assert.False(obj1 != VALUE1);
        Assert.True(obj1 != VALUE2);
    }

    [Test]
    public void BufferAssignment()
    {
        PyBuffer obj1 = VALUE1;
        PyBuffer obj2 = VALUE2;
        
        Assert.True(obj1 == VALUE1);
        Assert.True(obj2 == VALUE2);
    }
}