using EVESharp.Common.Services.Exceptions;
using EVESharp.Common.Unit.Services;
using NUnit.Framework;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Common.Unit
{
    public class ServiceManagerTest
    {
        private ServiceManager mServiceManager;

        private static PyTuple BuildArguments(params PyDataType[] list)
        {
            return list;
        }
        
        [SetUp]
        public void Setup()
        {
            this.mServiceManager = new ServiceManager();
        }

        [Test]
        public void TestInexistentService()
        {
            Assert.Throws<ServiceDoesNotExistsException>(() => {this.mServiceManager.ServiceCall("Inexistent", "Call", null, null);});
        }

        [Test]
        public void TestInexistentCall()
        {
            Assert.Throws<ServiceDoesNotContainCallException>(() => {this.mServiceManager.ServiceCall("TestService1", "InexistentCall", null, null);});
            Assert.Throws<ServiceDoesNotContainCallException>(() => {this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments(1.0), null);});
            Assert.Throws<ServiceDoesNotContainCallException>(() => {this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments(5, 5, 5), null);});
        }

        [Test]
        public void TestCalls()
        {
            PyDataType result1 = this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments(TestService1.VALUE1), new CallInfo());
            PyDataType result2 = this.mServiceManager.ServiceCall("TestService2", "AnotherTestCall", BuildArguments(TestService2.VALUE1), new CallInfo());
            PyDataType result3 = this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments(), new CallInfo());
            PyDataType result4 = this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments(TestService1.VALUE1, TestService1.VALUE2), new CallInfo());

            Assert.IsInstanceOf<PyInteger>(result1);
            Assert.AreEqual(TestService1.VALUE1, (result1 as PyInteger).Value);
            Assert.IsInstanceOf<PyInteger>(result2);
            Assert.AreEqual(TestService2.VALUE1, (result2 as PyInteger).Value);
            Assert.IsInstanceOf<PyInteger>(result3);
            Assert.AreEqual(TestService1.VALUE2, (result3 as PyInteger).Value);
            Assert.IsInstanceOf<PyInteger>(result4);
            Assert.AreEqual(TestService1.VALUE3, (result4 as PyInteger).Value);
        }
    }
}