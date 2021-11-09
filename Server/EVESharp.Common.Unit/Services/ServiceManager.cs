namespace EVESharp.Common.Unit.Services
{
    public class ServiceManager : Common.Services.ServiceManager
    {
        public TestService1 TestService1 { get; private set; }
        public TestService2 TestService2 { get; private set; }
        
        public ServiceManager()
        {
            this.TestService1 = new TestService1();
            this.TestService2 = new TestService2();
        }
    }
}