using EVESharp.Types.Collections;
using EVESharp.Types.Serialization;
using NUnit.Framework;

namespace EVESharp.Types.Unit.Marshaling;

public class TupleMarshalingTests
{
    private static int    sTupleMarshaling_FirstValue  = 100;
    private static string sTupleMarshaling_SecondValue = "Hello World!";
    private static double sTupleMarshaling_ThirdValue  = 1.0;

    private static byte[] sTupleMarshaling_Empty = new byte[] {0x24};
    private static byte[] sTupleMarshaling_One = new byte[] {0x25, 0x06, 0x64};
    private static byte[] sTupleMarshaling_Two = new byte[] {0x2C, 0x06, 0x64, 0x13, 0x0C, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x21};
    private static byte[] sTupleMarshaling_Three = new byte[] {0x14, 0x03, 0x06, 0x64, 0x13, 0x0C, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x21, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F};

    private static byte[] sTupleMarshaling_Big = new byte[]
    {
        0x14, 0xFF, 0x00, 0x01, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01
    };
        
    [Test]
    public void TupleMarshal_Big()
    {
        PyTuple result = new PyTuple(byte.MaxValue + 1);
            
        for(int i = 0; i < byte.MaxValue + 1; i++)
            result[i] = null;

        byte[] output = Marshal.ToByteArray(result, false);

        Assert.AreEqual(sTupleMarshaling_Big, output);
    }
        
    [Test]
    public void TupleMarshal_Empty()
    {
        PyTuple tuple = new PyTuple(0);

        byte[] output = Marshal.ToByteArray(tuple, false);
            
        Assert.AreEqual(sTupleMarshaling_Empty, output);
    }

    [Test]
    public void TupleMarshal_One()
    {
        PyTuple tuple = new PyTuple(1) {[0] = sTupleMarshaling_FirstValue};

        byte[] output = Marshal.ToByteArray(tuple, false);

        Assert.AreEqual(sTupleMarshaling_One, output);
    }

    [Test]
    public void TupleMarshal_Two()
    {
        PyTuple tuple = new PyTuple(2)
        {
            [0] = sTupleMarshaling_FirstValue,
            [1] = sTupleMarshaling_SecondValue
        };

        byte[] output = Marshal.ToByteArray(tuple, false);

        Assert.AreEqual(sTupleMarshaling_Two, output);
    }

    [Test]
    public void TupleMarshal_Three()
    {
        PyTuple tuple = new PyTuple(3)
        {
            [0] = sTupleMarshaling_FirstValue,
            [1] = sTupleMarshaling_SecondValue,
            [2] = sTupleMarshaling_ThirdValue
        };

        byte[] output = Marshal.ToByteArray(tuple, false);
            
        Assert.AreEqual(sTupleMarshaling_Three, output);
    }

    [Test]
    public void TupleUnmarshal_Empty()
    {
        PyDataType value = Unmarshal.ReadFromByteArray(sTupleMarshaling_Empty, false);
            
        Assert.IsInstanceOf<PyTuple>(value);

        PyTuple tuple = value as PyTuple;

        Assert.AreEqual(0, tuple.Count);
    }

    [Test]
    public void TupleUnmarshal_One()
    {
        PyDataType value = Unmarshal.ReadFromByteArray(sTupleMarshaling_One, false);
            
        Assert.IsInstanceOf<PyTuple>(value);

        PyTuple tuple = value as PyTuple;

        Assert.AreEqual(1, tuple.Count);
        Assert.IsInstanceOf<PyInteger>(tuple[0]);
        Assert.AreEqual(sTupleMarshaling_FirstValue, (tuple[0] as PyInteger).Value);
    }

    [Test]
    public void TupleUnmarshal_Two()
    {
        PyDataType value = Unmarshal.ReadFromByteArray(sTupleMarshaling_Two, false);
            
        Assert.IsInstanceOf<PyTuple>(value);

        PyTuple tuple = value as PyTuple;

        Assert.AreEqual(2, tuple.Count);
        Assert.IsInstanceOf<PyInteger>(tuple[0]);
        Assert.AreEqual(sTupleMarshaling_FirstValue, (tuple[0] as PyInteger).Value);
        Assert.IsInstanceOf<PyString>(tuple[1]);
        Assert.AreEqual(sTupleMarshaling_SecondValue, (tuple[1] as PyString).Value);
    }

    [Test]
    public void TupleUnmarshal_Three()
    {
        PyDataType value = Unmarshal.ReadFromByteArray(sTupleMarshaling_Three, false);
            
        Assert.IsInstanceOf<PyTuple>(value);

        PyTuple tuple = value as PyTuple;

        Assert.AreEqual(3, tuple.Count);
        Assert.IsInstanceOf<PyInteger>(tuple[0]);
        Assert.AreEqual(sTupleMarshaling_FirstValue, (tuple[0] as PyInteger).Value);
        Assert.IsInstanceOf<PyString>(tuple[1]);
        Assert.AreEqual(sTupleMarshaling_SecondValue, (tuple[1] as PyString).Value);
        Assert.IsInstanceOf<PyDecimal>(tuple[2]);
        Assert.AreEqual(sTupleMarshaling_ThirdValue, (tuple[2] as PyDecimal).Value);
    }

    [Test]
    public void TupleUnmarshal_Big()
    {
        PyDataType value = Unmarshal.ReadFromByteArray(sTupleMarshaling_Big, false);
            
        Assert.IsInstanceOf<PyTuple>(value);

        PyTuple tuple = value as PyTuple;
            
        Assert.AreEqual(256, tuple.Count);

        foreach (PyDataType entry in tuple)
            Assert.AreEqual(null, entry);
    }
}