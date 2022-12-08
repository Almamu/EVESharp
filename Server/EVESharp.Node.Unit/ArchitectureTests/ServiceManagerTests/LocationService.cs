using EVESharp.EVE.Network.Services;
using EVESharp.Types;

namespace EVESharp.Node.Unit.ArchitectureTests.ServiceManagerTests;

public class LocationService : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Location;

    public PyDataType Call (ServiceCall extra)
    {
        return 0;
    }
}