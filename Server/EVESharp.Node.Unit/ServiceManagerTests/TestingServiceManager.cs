using EVESharp.Node.Services;

namespace EVESharp.Node.Unit.ServiceManagerTests;

public class TestingServiceManager : ServiceManager
{
    public ExampleService         ExampleService         { get; } = new ExampleService ();
    public RestrictedService      RestrictedService      { get; } = new RestrictedService ();
    public ExtraRestrictedService ExtraRestrictedService { get; } = new ExtraRestrictedService ();
}