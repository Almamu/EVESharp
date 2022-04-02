using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Primitives;
using EVESharp.PythonTypes.Unit.Marshaling;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Primitives;

public class PySubStreamTests
{
    [Test]
    public void PySubStreamComparison()
    {
        PySubStream obj1 = Unmarshal.ReadFromByteArray(SubStreamMarshalingTests.sSubStreamMarshal_Bytes) as PySubStream;
        PySubStream obj2 = new PySubStream(500);
        PySubStream obj3 = new PySubStream(600);

        Assert.True(obj1 == obj2);
        Assert.False(obj2 == obj3);
        Assert.False(obj1 == obj3);
        Assert.False(obj1 != obj2);
        Assert.True(obj2 != obj3);
        Assert.True(obj1 != obj3);
        
        // ensure value change updates the marshaling data
        obj2.Stream = 600;

        Assert.True(obj2 == obj3);
        Assert.False(obj1 == obj2);
        Assert.False(obj2 != obj3);
        Assert.True(obj1 != obj2);
    }
}