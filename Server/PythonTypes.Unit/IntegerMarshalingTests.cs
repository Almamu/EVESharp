using System;
using System.IO;
using NUnit.Framework;
using PythonTypes.Marshal;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Unit
{
    public class IntegerMarshalingTests
    {
        private static sbyte sIntegerMarshal_MinusOneValue = -1;
        private static sbyte sIntegerMarshal_ZeroValue = 0;
        private static sbyte sIntegerMarshal_OneValue = 1;
        private static byte sIntegerMarshal_ByteValue = 15;
        private static sbyte sIntegerMarshal_SByteValue = -15;
        private static short sIntegerMarshal_ShortValue = -30000;
        private static ushort sIntegerMarshal_UShortValue = 60000;
        private static int sIntegerMarshal_IntegerValue = Int32.MaxValue;
        private static int sIntegerMarshal_IntegerValue2 = Int32.MinValue;
        private static long sIntegerMarshal_LongValue = long.MaxValue;
        private static long sIntegerMarshal_LongValue2 = long.MinValue;

        private static byte[] sIntegerMarshal_MinusOneValueBuffer = new byte[] {0x07};
        private static byte[] sIntegerMarshal_ZeroValueBuffer = new byte[] {0x08};
        private static byte[] sIntegerMarshal_OneValueBuffer = new byte[] {0x09};
        private static byte[] sIntegerMarshal_ByteValueBuffer = new byte[] {0x06, 0x0F};
        private static byte[] sIntegerMarshal_SByteValueBuffer = new byte[] {0x06, 0xF1};
        private static byte[] sIntegerMarshal_ShortValueBuffer = new byte[] {0x05, 0xD0, 0x8A};
        private static byte[] sIntegerMarshal_UShortValueBuffer = new byte[] {0x04, 0x60, 0xEA, 0x00, 0x00};
        private static byte[] sIntegerMarshal_IntegerValueBuffer = new byte[] {0x04, 0xFF, 0xFF, 0xFF, 0x7F};
        private static byte[] sIntegerMarshal_IntegerValue2Buffer = new byte[] {0x04, 0x00, 0x00, 0x00, 0x80};
        private static byte[] sIntegerMarshal_LongValueBuffer = new byte[] {0x03, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F};
        private static byte[] sIntegerMarshal_LongValue2Buffer = new byte[] {0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80};

        [Test]
        public void IntegerMarshal_MinusOne()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_MinusOneValue), false);

            Assert.AreEqual(sIntegerMarshal_MinusOneValueBuffer, output);
        }
        
        [Test]
        public void IntegerMarshal_Zero()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_ZeroValue), false);

            Assert.AreEqual(sIntegerMarshal_ZeroValueBuffer, output);
        }
        
        [Test]
        public void IntegerMarshal_One()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_OneValue), false);

            Assert.AreEqual(sIntegerMarshal_OneValueBuffer, output);
        }
        
        [Test]
        public void IntegerMarshal_Byte()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_ByteValue), false);

            Assert.AreEqual(sIntegerMarshal_ByteValueBuffer, output);
        }

        [Test]
        public void IntegerMarshal_SByte()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_SByteValue), false);

            Assert.AreEqual(sIntegerMarshal_SByteValueBuffer, output);
        }

        [Test]
        public void IntegerMarshal_Short()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_ShortValue), false);

            Assert.AreEqual(sIntegerMarshal_ShortValueBuffer, output);
        }

        [Test]
        public void IntegerMarshal_UShort()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_UShortValue), false);

            Assert.AreEqual(sIntegerMarshal_UShortValueBuffer, output);
        }

        [Test]
        public void IntegerMarshal_Integer()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_IntegerValue), false);

            Assert.AreEqual(sIntegerMarshal_IntegerValueBuffer, output);
        }

        [Test]
        public void IntegerMarshal_Integer2()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_IntegerValue2), false);

            Assert.AreEqual(sIntegerMarshal_IntegerValue2Buffer, output);
        }

        [Test]
        public void IntegerMarshal_Long()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_LongValue), false);

            Assert.AreEqual(sIntegerMarshal_LongValueBuffer, output);
        }

        [Test]
        public void IntegerMarshal_Long2()
        {
            byte[] output = Marshal.Marshal.ToByteArray(new PyInteger(sIntegerMarshal_LongValue2), false);

            Assert.AreEqual(sIntegerMarshal_LongValue2Buffer, output);
        }

        [Test]
        public void IntegerUnmarshal_MinusOne()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_MinusOneValueBuffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_MinusOneValue, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_MinusOneValue == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Byte, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_Zero()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_ZeroValueBuffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_ZeroValue, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_ZeroValue == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Byte, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_One()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_OneValueBuffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_OneValue, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_OneValue == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Byte, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_Byte()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_ByteValueBuffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_ByteValue, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_ByteValue == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Byte, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_SByte()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_SByteValueBuffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_SByteValue, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_SByteValue == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Byte, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_Short()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_ShortValueBuffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_ShortValue, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_ShortValue == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Short, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_UShort()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_UShortValueBuffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_UShortValue, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_UShortValue == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Int, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_Integer()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_IntegerValueBuffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_IntegerValue, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_IntegerValue == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Int, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_Integer2()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_IntegerValue2Buffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_IntegerValue2, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_IntegerValue2 == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Int, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_Long()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_LongValueBuffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_LongValue, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_LongValue == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Long, pyInteger.IntegerType);
        }

        [Test]
        public void IntegerUnmarshal_Long2()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sIntegerMarshal_LongValue2Buffer, false);
            
            Assert.IsInstanceOf<PyInteger>(result);

            PyInteger pyInteger = result as PyInteger;
            
            Assert.AreEqual(sIntegerMarshal_LongValue2, pyInteger.Value);
            Assert.AreEqual(true, sIntegerMarshal_LongValue2 == pyInteger);
            Assert.AreEqual(PyInteger.IntegerTypeEnum.Long, pyInteger.IntegerType);
        }
    }
}