using EVESharp.Common.Unit.Services;
using EVESharp.EVE.Services.Exceptions;
using EVESharp.Node.Network;
using NUnit.Framework;
using EVESharp.PythonTypes.Types.Primitives;
using ServiceManager = EVESharp.Common.Unit.Services.ServiceManager;

namespace EVESharp.Common.Unit
{
    public class ServiceManagerTest
    {
        private ServiceManager mServiceManager;

        private static CallInformation BuildArguments(params PyDataType[] list)
        {
            return new CallInformation()
            {
                Payload = list,
            };
        }
        
        [SetUp]
        public void Setup()
        {
            this.mServiceManager = new ServiceManager();
        }

        [Test]
        public void TestInexistentService()
        {
            Assert.Throws<MissingServiceException<string>>(() => {this.mServiceManager.ServiceCall("Inexistent", "Call", null);});
        }

        [Test]
        public void TestInexistentCall()
        {
            Assert.Throws<MissingCallException>(() => {this.mServiceManager.ServiceCall("TestService1", "InexistentCall", null);});
            Assert.Throws<MissingCallException>(() => {this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments(1.0));});
            Assert.Throws<MissingCallException>(() => {this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments(5, 5, 5));});
        }

        [Test]
        public void TestCalls()
        {
            PyDataType result1 = this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments(TestService1.VALUE1));
            PyDataType result2 = this.mServiceManager.ServiceCall("TestService2", "AnotherTestCall", BuildArguments(TestService2.VALUE1));
            PyDataType result3 = this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments());
            PyDataType result4 = this.mServiceManager.ServiceCall("TestService1", "TestCall", BuildArguments(TestService1.VALUE1, TestService1.VALUE2));

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