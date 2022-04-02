using System.IO;
using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Marshaling;

public class SubStreamMarshalingTests
{
    public static byte[] sSubStreamMarshal_Bytes = {0x7E, 0x00, 0x00, 0x00, 0x00, 0x2B, 0x08, 0x7E, 0x00, 0x00, 0x00, 0x00, 0x05, 0xF4, 0x01};

    [Test]
    public void SubStreamMarshaling_Test()
    {
        PySubStream stream = new PySubStream(500);

        byte[] result = Marshal.Marshal.ToByteArray(stream);

        Assert.AreEqual(result, sSubStreamMarshal_Bytes);
    }

    [Test]
    public void SubStreamUnmarshaling_Test()
    {
        PyDataType data = Unmarshal.ReadFromByteArray(sSubStreamMarshal_Bytes);
        
        Assert.IsInstanceOf<PySubStream>(data);
        
        PySubStream stream = data as PySubStream;

        Assert.True(stream.Stream == 500);
    }
}