using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Services;

namespace EVESharp.Node.Unit.Utils;

public static class Service
{
    public static ServiceCall GenerateServiceCall (Session session)
    {
        return new ServiceCall {Session = session};
    }
}