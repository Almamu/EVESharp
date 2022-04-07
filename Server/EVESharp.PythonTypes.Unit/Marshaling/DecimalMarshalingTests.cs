using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Marshaling;

public class DecimalMarshalingTests
{
    private static double sDecimal_RealZero = 0.0;
    private static double sDecimal_Value    = 15.0;

    private static byte[] sDecimal_RealZeroBuffer = new byte[] {0x0B};
    private static byte[] sDecimal_ValueBuffer    = new byte[] {0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2E, 0x40};

    [Test]
    public void DecimalMarshal_Zero()
    {
        PyDecimal value = new PyDecimal(sDecimal_RealZero);

        byte[] output = Marshal.Marshal.ToByteArray(value, false);

        Assert.AreEqual(sDecimal_RealZeroBuffer, output);
    }

    [Test]
    public void DecimalMarshal_Value()
    {
        PyDecimal value = new PyDecimal(sDecimal_Value);

        byte[] output = Marshal.Marshal.ToByteArray(value, false);

        Assert.AreEqual(sDecimal_ValueBuffer, output);
    }

    [Test]
    public void DecimalUnmarshal_Zero()
    {
        PyDataType value = Unmarshal.ReadFromByteArray(sDecimal_RealZeroBuffer, false);
            
        Assert.IsInstanceOf<PyDecimal>(value);

        PyDecimal @decimal = value as PyDecimal;
            
        Assert.AreEqual(sDecimal_RealZero, @decimal.Value);
    }

    [Test]
    public void DecimalUnmarshal_Value()
    {
        PyDataType value = Unmarshal.ReadFromByteArray(sDecimal_ValueBuffer, false);
            
        Assert.IsInstanceOf<PyDecimal>(value);

        PyDecimal @decimal = value as PyDecimal;
            
        Assert.AreEqual(sDecimal_Value, @decimal.Value);
    }
}