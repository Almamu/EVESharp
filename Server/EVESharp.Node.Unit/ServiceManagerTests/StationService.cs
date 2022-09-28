using EVESharp.EVE.Network.Services;
using EVESharp.Types;

namespace EVESharp.Node.Unit.ServiceManagerTests;

public class StationService : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Station;

    public PyDataType Call (ServiceCall extra)
    {
        return 0;
    }
}