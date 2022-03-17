using EVESharp.EVE.Services;
using NUnit.Framework;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Common.Unit.Services
{
    public class TestService1 : Service
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        
        public const int VALUE1 = 10;
        public const int VALUE2 = 20;
        public const int VALUE3 = 30;
        
        public PyDataType TestCall(PyInteger value1, CallInfo extra)
        {
            Assert.AreEqual(VALUE1, value1.Value);
            Assert.NotNull(extra);
            Assert.IsInstanceOf<CallInfo>(extra);

            return VALUE1;
        }

        public PyDataType TestCall(CallInfo extra)
        {
            return VALUE2;
        }

        public PyDataType TestCall(PyInteger value1, PyInteger value2, CallInfo extra)
        {
            Assert.AreEqual(VALUE1, value1.Value);
            Assert.AreEqual(VALUE2, value2.Value);
            Assert.NotNull(extra);
            Assert.IsInstanceOf<CallInfo>(extra);

            return VALUE3;
        }
    }
}