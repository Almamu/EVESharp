using System.IO;
using NUnit.Framework;
using PythonTypes.Marshal;
using PythonTypes.Types.Primitives;

namespace PythonTypes.Unit
{
    public class StringMarshalingTests
    {
        private static string sStringMarshal_EmptyString = "";
        private static string sStringMarshal_HelloWorld = "Hello World!";
        private static string sStringMarshal_LongString = "hZxwIeSNOFsWVMNS5lB9sKi93YXqiCTxW8I0InLU2eOWWv8KnTKX58vBKBVYZl7l9X3qmuGYxLTk6TbZZCQHE09OJhgyAz1FtXieF96KPVNOK1DUD0Wn2V6yw61zLLrW7qL5BNHs5pNpJIcrSmii9uQ8zDlK4eiDfODG1bQkL6tKS8V8q1aH23gBWeo0fpyFqyxmAGAglBCsdUNWSWQnISJI6R9NqhIXkijyKjImD7YBXw9ua2Ay3RXojbswpt7Dmb4lYWuKlHfQsnCZldgws3e90esvhG6Do2hoIZO2vw9m";
        private static string sStringMarshal_StringTable = "contraband";
        private static byte[] sStringMarshal_StringTableBuffer = new byte[] {0x11, 0x13};
        private static byte[] sStringMarshal_EmptyByteStringBuffer = new byte[] {0x0E};
        private static byte[] sStringMarshal_EmptyMultiByteStringBuffer = new byte[] {0x28};
        private static byte[] sStringMarshal_HelloWorldByteStringBuffer = new byte[] {0x13, 0x0C, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x21};
        private static byte[] sStringMarshal_HelloWorldMultiByteStringBuffer = new byte[] {0x2E, 0x0C, 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64, 0x21};
        private static byte[] sStringMarshal_LongByteStringBuffer = new byte[]
        {
            0x13, 0xFF, 0x2C, 0x01, 0x00, 0x00, 0x68, 0x5A, 0x78, 0x77, 0x49, 0x65, 0x53, 0x4E, 0x4F, 0x46, 0x73, 0x57,
            0x56, 0x4D, 0x4E, 0x53, 0x35, 0x6C, 0x42, 0x39, 0x73, 0x4B, 0x69, 0x39, 0x33, 0x59, 0x58, 0x71, 0x69, 0x43,
            0x54, 0x78, 0x57, 0x38, 0x49, 0x30, 0x49, 0x6E, 0x4C, 0x55, 0x32, 0x65, 0x4F, 0x57, 0x57, 0x76, 0x38, 0x4B,
            0x6E, 0x54, 0x4B, 0x58, 0x35, 0x38, 0x76, 0x42, 0x4B, 0x42, 0x56, 0x59, 0x5A, 0x6C, 0x37, 0x6C, 0x39, 0x58,
            0x33, 0x71, 0x6D, 0x75, 0x47, 0x59, 0x78, 0x4C, 0x54, 0x6B, 0x36, 0x54, 0x62, 0x5A, 0x5A, 0x43, 0x51, 0x48,
            0x45, 0x30, 0x39, 0x4F, 0x4A, 0x68, 0x67, 0x79, 0x41, 0x7A, 0x31, 0x46, 0x74, 0x58, 0x69, 0x65, 0x46, 0x39,
            0x36, 0x4B, 0x50, 0x56, 0x4E, 0x4F, 0x4B, 0x31, 0x44, 0x55, 0x44, 0x30, 0x57, 0x6E, 0x32, 0x56, 0x36, 0x79,
            0x77, 0x36, 0x31, 0x7A, 0x4C, 0x4C, 0x72, 0x57, 0x37, 0x71, 0x4C, 0x35, 0x42, 0x4E, 0x48, 0x73, 0x35, 0x70,
            0x4E, 0x70, 0x4A, 0x49, 0x63, 0x72, 0x53, 0x6D, 0x69, 0x69, 0x39, 0x75, 0x51, 0x38, 0x7A, 0x44, 0x6C, 0x4B,
            0x34, 0x65, 0x69, 0x44, 0x66, 0x4F, 0x44, 0x47, 0x31, 0x62, 0x51, 0x6B, 0x4C, 0x36, 0x74, 0x4B, 0x53, 0x38,
            0x56, 0x38, 0x71, 0x31, 0x61, 0x48, 0x32, 0x33, 0x67, 0x42, 0x57, 0x65, 0x6F, 0x30, 0x66, 0x70, 0x79, 0x46,
            0x71, 0x79, 0x78, 0x6D, 0x41, 0x47, 0x41, 0x67, 0x6C, 0x42, 0x43, 0x73, 0x64, 0x55, 0x4E, 0x57, 0x53, 0x57,
            0x51, 0x6E, 0x49, 0x53, 0x4A, 0x49, 0x36, 0x52, 0x39, 0x4E, 0x71, 0x68, 0x49, 0x58, 0x6B, 0x69, 0x6A, 0x79,
            0x4B, 0x6A, 0x49, 0x6D, 0x44, 0x37, 0x59, 0x42, 0x58, 0x77, 0x39, 0x75, 0x61, 0x32, 0x41, 0x79, 0x33, 0x52,
            0x58, 0x6F, 0x6A, 0x62, 0x73, 0x77, 0x70, 0x74, 0x37, 0x44, 0x6D, 0x62, 0x34, 0x6C, 0x59, 0x57, 0x75, 0x4B,
            0x6C, 0x48, 0x66, 0x51, 0x73, 0x6E, 0x43, 0x5A, 0x6C, 0x64, 0x67, 0x77, 0x73, 0x33, 0x65, 0x39, 0x30, 0x65,
            0x73, 0x76, 0x68, 0x47, 0x36, 0x44, 0x6F, 0x32, 0x68, 0x6F, 0x49, 0x5A, 0x4F, 0x32, 0x76, 0x77, 0x39, 0x6D
        };
        private static byte[] sStringMarshal_LongMultiByteStringBuffer = new byte[]
        {
            0x2E, 0xFF, 0x2C, 0x01, 0x00, 0x00, 0x68, 0x5A, 0x78, 0x77, 0x49, 0x65, 0x53, 0x4E, 0x4F, 0x46, 0x73, 0x57,
            0x56, 0x4D, 0x4E, 0x53, 0x35, 0x6C, 0x42, 0x39, 0x73, 0x4B, 0x69, 0x39, 0x33, 0x59, 0x58, 0x71, 0x69, 0x43,
            0x54, 0x78, 0x57, 0x38, 0x49, 0x30, 0x49, 0x6E, 0x4C, 0x55, 0x32, 0x65, 0x4F, 0x57, 0x57, 0x76, 0x38, 0x4B,
            0x6E, 0x54, 0x4B, 0x58, 0x35, 0x38, 0x76, 0x42, 0x4B, 0x42, 0x56, 0x59, 0x5A, 0x6C, 0x37, 0x6C, 0x39, 0x58,
            0x33, 0x71, 0x6D, 0x75, 0x47, 0x59, 0x78, 0x4C, 0x54, 0x6B, 0x36, 0x54, 0x62, 0x5A, 0x5A, 0x43, 0x51, 0x48,
            0x45, 0x30, 0x39, 0x4F, 0x4A, 0x68, 0x67, 0x79, 0x41, 0x7A, 0x31, 0x46, 0x74, 0x58, 0x69, 0x65, 0x46, 0x39,
            0x36, 0x4B, 0x50, 0x56, 0x4E, 0x4F, 0x4B, 0x31, 0x44, 0x55, 0x44, 0x30, 0x57, 0x6E, 0x32, 0x56, 0x36, 0x79,
            0x77, 0x36, 0x31, 0x7A, 0x4C, 0x4C, 0x72, 0x57, 0x37, 0x71, 0x4C, 0x35, 0x42, 0x4E, 0x48, 0x73, 0x35, 0x70,
            0x4E, 0x70, 0x4A, 0x49, 0x63, 0x72, 0x53, 0x6D, 0x69, 0x69, 0x39, 0x75, 0x51, 0x38, 0x7A, 0x44, 0x6C, 0x4B,
            0x34, 0x65, 0x69, 0x44, 0x66, 0x4F, 0x44, 0x47, 0x31, 0x62, 0x51, 0x6B, 0x4C, 0x36, 0x74, 0x4B, 0x53, 0x38,
            0x56, 0x38, 0x71, 0x31, 0x61, 0x48, 0x32, 0x33, 0x67, 0x42, 0x57, 0x65, 0x6F, 0x30, 0x66, 0x70, 0x79, 0x46,
            0x71, 0x79, 0x78, 0x6D, 0x41, 0x47, 0x41, 0x67, 0x6C, 0x42, 0x43, 0x73, 0x64, 0x55, 0x4E, 0x57, 0x53, 0x57,
            0x51, 0x6E, 0x49, 0x53, 0x4A, 0x49, 0x36, 0x52, 0x39, 0x4E, 0x71, 0x68, 0x49, 0x58, 0x6B, 0x69, 0x6A, 0x79,
            0x4B, 0x6A, 0x49, 0x6D, 0x44, 0x37, 0x59, 0x42, 0x58, 0x77, 0x39, 0x75, 0x61, 0x32, 0x41, 0x79, 0x33, 0x52,
            0x58, 0x6F, 0x6A, 0x62, 0x73, 0x77, 0x70, 0x74, 0x37, 0x44, 0x6D, 0x62, 0x34, 0x6C, 0x59, 0x57, 0x75, 0x4B,
            0x6C, 0x48, 0x66, 0x51, 0x73, 0x6E, 0x43, 0x5A, 0x6C, 0x64, 0x67, 0x77, 0x73, 0x33, 0x65, 0x39, 0x30, 0x65,
            0x73, 0x76, 0x68, 0x47, 0x36, 0x44, 0x6F, 0x32, 0x68, 0x6F, 0x49, 0x5A, 0x4F, 0x32, 0x76, 0x77, 0x39, 0x6D
        };

        [Test]
        public void StringMarshal_StringTable()
        {
            byte[] byteOutput = Marshal.Marshal.ToByteArray(new PyString(sStringMarshal_StringTable), false);

            Assert.AreEqual(sStringMarshal_StringTableBuffer, byteOutput);
        }
        
        [Test]
        public void StringMarshal_EmptyByteString()
        {
            byte[] byteOutput = Marshal.Marshal.ToByteArray(new PyString(sStringMarshal_EmptyString), false);

            Assert.AreEqual(sStringMarshal_EmptyByteStringBuffer, byteOutput);
        }

        [Test]
        public void StringMarshal_EmptyMultiByteString()
        {
            byte[] byteOutput = Marshal.Marshal.ToByteArray(new PyString(sStringMarshal_EmptyString, true), false);

            Assert.AreEqual(sStringMarshal_EmptyMultiByteStringBuffer, byteOutput);
        }

        [Test]
        public void StringMarshal_HelloWorldByteString()
        {
            byte[] byteOutput = Marshal.Marshal.ToByteArray(new PyString(sStringMarshal_HelloWorld), false);
            
            Assert.AreEqual(sStringMarshal_HelloWorldByteStringBuffer, byteOutput);
        }

        [Test]
        public void StringMarshal_HelloWorldMultiByteString()
        {
            byte[] byteOutput = Marshal.Marshal.ToByteArray(new PyString(sStringMarshal_HelloWorld, true), false);
            
            Assert.AreEqual(sStringMarshal_HelloWorldMultiByteStringBuffer, byteOutput);
        }

        [Test]
        public void StringMarshal_LongByteString()
        {
            byte[] byteOutput = Marshal.Marshal.ToByteArray(new PyString(sStringMarshal_LongString), false);

            Assert.AreEqual(sStringMarshal_LongByteStringBuffer, byteOutput);
        }

        [Test]
        public void StringMarshal_LongMultiByteString()
        {
            byte[] byteOutput = Marshal.Marshal.ToByteArray(new PyString(sStringMarshal_LongString, true), false);

            Assert.AreEqual(sStringMarshal_LongMultiByteStringBuffer, byteOutput);
        }

        [Test]
        public void StringUnmarshal_StringTableString()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sStringMarshal_StringTableBuffer, false);
            
            Assert.IsInstanceOf<PyString>(result);

            PyString pyString = result as PyString;

            Assert.AreEqual(sStringMarshal_StringTable.Length, pyString.Length);
            Assert.AreEqual(sStringMarshal_StringTable, pyString.Value);
            Assert.AreEqual(StringTableUtils.EntryList.contraband, pyString.StringTableEntryIndex);
            Assert.AreEqual(false, pyString.IsUTF8);
            Assert.AreEqual(true, pyString.IsStringTableEntry);
        }

        [Test]
        public void StringUnmarshal_EmptyByteString()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sStringMarshal_EmptyByteStringBuffer, false);
            
            Assert.IsInstanceOf<PyString>(result);

            PyString pyString = result as PyString;

            Assert.AreEqual(sStringMarshal_EmptyString.Length, pyString.Length);
            Assert.AreEqual(sStringMarshal_EmptyString, pyString.Value);
            Assert.AreEqual(false, pyString.IsUTF8);
            Assert.AreEqual(false, pyString.IsStringTableEntry);
        }

        [Test]
        public void StringUnmarshal_EmptyMultiByteString()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sStringMarshal_EmptyMultiByteStringBuffer, false);
            
            Assert.IsInstanceOf<PyString>(result);

            PyString pyString = result as PyString;

            Assert.AreEqual(sStringMarshal_EmptyString.Length, pyString.Length);
            Assert.AreEqual(sStringMarshal_EmptyString, pyString.Value);
            Assert.AreEqual(true, pyString.IsUTF8);
            Assert.AreEqual(false, pyString.IsStringTableEntry);
        }

        [Test]
        public void StringUnmarshal_HelloWorldByteString()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sStringMarshal_HelloWorldByteStringBuffer, false);
            
            Assert.IsInstanceOf<PyString>(result);

            PyString pyString = result as PyString;

            Assert.AreEqual(sStringMarshal_HelloWorld.Length, pyString.Length);
            Assert.AreEqual(sStringMarshal_HelloWorld, pyString.Value);
            Assert.AreEqual(false, pyString.IsUTF8);
            Assert.AreEqual(false, pyString.IsStringTableEntry);
        }

        [Test]
        public void StringUnmarshal_HelloWorldMultiByteString()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sStringMarshal_HelloWorldMultiByteStringBuffer, false);
            
            Assert.IsInstanceOf<PyString>(result);

            PyString pyString = result as PyString;

            Assert.AreEqual(sStringMarshal_HelloWorld.Length, pyString.Length);
            Assert.AreEqual(sStringMarshal_HelloWorld, pyString.Value);
            Assert.AreEqual(true, pyString.IsUTF8);
            Assert.AreEqual(false, pyString.IsStringTableEntry);
        }

        [Test]
        public void StringUnmarshal_LongByteString()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sStringMarshal_LongByteStringBuffer, false);
            
            Assert.IsInstanceOf<PyString>(result);

            PyString pyString = result as PyString;

            Assert.AreEqual(sStringMarshal_LongString.Length, pyString.Length);
            Assert.AreEqual(sStringMarshal_LongString, pyString.Value);
            Assert.AreEqual(false, pyString.IsUTF8);
            Assert.AreEqual(false, pyString.IsStringTableEntry);
        }

        [Test]
        public void StringUnmarshal_LongMultiByteString()
        {
            PyDataType result = Unmarshal.ReadFromByteArray(sStringMarshal_LongMultiByteStringBuffer, false);
            
            Assert.IsInstanceOf<PyString>(result);

            PyString pyString = result as PyString;

            Assert.AreEqual(sStringMarshal_LongString.Length, pyString.Length);
            Assert.AreEqual(sStringMarshal_LongString, pyString.Value);
            Assert.AreEqual(true, pyString.IsUTF8);
            Assert.AreEqual(false, pyString.IsStringTableEntry);
        }
    }
}