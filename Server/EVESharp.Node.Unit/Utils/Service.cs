using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Services;

namespace EVESharp.Node.Unit.Utils;

public static class Service
{
    public static CallInformation GenerateServiceCall (Session session)
    {
        return new CallInformation {Session = session};
    }
}