using EVESharp.EVE.Services;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Unit.ServiceManagerTests;

public class LocationService : Service
{
    public override AccessLevel AccessLevel => AccessLevel.Location;

    public PyDataType Call (ServiceCall extra)
    {
        return 0;
    }
}