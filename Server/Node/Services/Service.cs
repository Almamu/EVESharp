namespace Node.Services
{
    public abstract class Service : Common.Services.Service
    {
        // cover the old ServiceManager declaration
        public ServiceManager ServiceManager { get; }
        protected Service(ServiceManager manager)
        {
            this.ServiceManager = manager;
        }
    }
}