using EVESharp.Types.Serialization;
using EVESharp.Types.Unit.Marshaling;
using NUnit.Framework;

namespace EVESharp.Types.Unit.Types.Primitives;

public class PySubStreamTests
{
    [Test]
    public void PySubStreamComparison()
    {
        PySubStream obj1 = Unmarshal.ReadFromByteArray(SubStreamMarshalingTests.sSubStreamMarshal_Bytes) as PySubStream;
        PySubStream obj2 = new PySubStream(500);
        PySubStream obj3 = new PySubStream(600);
        PySubStream obj4 = null;

        Assert.True(obj1 == obj2);
        Assert.False(obj2 == obj3);
        Assert.False(obj1 == obj3);
        Assert.False(obj1 != obj2);
        Assert.True(obj2 != obj3);
        Assert.True(obj1 != obj3);
        
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
        
        // ensure value change updates the marshaling data
        obj2.Stream = 600;

        Assert.True(obj2 == obj3);
        Assert.False(obj1 == obj2);
        Assert.False(obj2 != obj3);
        Assert.True(obj1 != obj2);
    }
}