using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Unit.ServiceManagerTests;

[MustBeCharacter]
public class ExtraRestrictedService : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    public PyDataType ExampleCall1 (ServiceCall extra)
    {
        return 0;
    }

    [MustBeInStation]
    public PyDataType ExampleCall2 (ServiceCall extra)
    {
        return 0;
    }

    [MustBeInStation]
    [MustHaveRole(Roles.ROLE_PLAYER, typeof (CrpCantQuitDefaultCorporation))]
    public PyDataType ExampleCall3 (ServiceCall extra)
    {
        return 0;
    }
}