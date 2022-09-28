using EVESharp.EVE.Network.Services;
using EVESharp.Types;

namespace EVESharp.Node.Unit.ServiceManagerTests;

public class SolarSystemService : Service
{
    public override AccessLevel AccessLevel => AccessLevel.SolarSystem;

    public PyDataType Call (ServiceCall extra)
    {
        return 0;
    }
}