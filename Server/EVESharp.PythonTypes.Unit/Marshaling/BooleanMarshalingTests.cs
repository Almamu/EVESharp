using EVESharp.PythonTypes.Marshal;
using EVESharp.PythonTypes.Types.Primitives;
using NUnit.Framework;

namespace EVESharp.PythonTypes.Unit.Marshaling
{
    public class BooleanMarshalingTests
    {
        private static bool sBooleanMarshal_TrueValue = true;
        private static bool sBooleanMarshal_FalseValue = false;

        private static byte[] sBooleanMarshal_TrueValueBuffer = new byte[] {0x1F};
        private static byte[] sBooleanMarshal_FalseValueBuffer = new byte[] {0x20};

        [Test]
        public void BooleanMarshal_True()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyBool(sBooleanMarshal_TrueValue), false);

            Assert.AreEqual(sBooleanMarshal_TrueValueBuffer, output);
        }

        [Test]
        public void BooleanMarshal_False()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyBool(sBooleanMarshal_FalseValue), false);

            Assert.AreEqual(sBooleanMarshal_FalseValueBuffer, output);
        }
        
        [Test]
        public void BooleanUnmarshal_True()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sBooleanMarshal_TrueValueBuffer, false);

            Assert.IsInstanceOf<PyBool>(result);

            PyBool pyBool = result as PyBool;

            Assert.AreEqual(sBooleanMarshal_TrueValue, pyBool.Value);
        }
        
        [Test]
        public void BooleanUnmarshal_False()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sBooleanMarshal_FalseValueBuffer, false);

            Assert.IsInstanceOf<PyBool>(result);

            PyBool pyBool = result as PyBool;

            Assert.AreEqual(sBooleanMarshal_FalseValue, pyBool.Value);
        }
    }
}