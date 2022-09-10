using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Data.Corporation;
using EVESharp.EVE.Data.Messages;
using EVESharp.EVE.Exceptions.corpRegistry;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.Types;

namespace EVESharp.Node.Unit.ServiceManagerTests;

public class RestrictedService : Service
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    [MustHaveCorporationRole(CorporationRole.Director)]
    [MustNotHaveCorporationRole(CorporationRole.Accountant)]
    public PyDataType CorporationRoleCall (ServiceCall extra)
    {
        return 0;
    }

    [MustHaveCorporationRole(CorporationRole.Director, CorporationRole.PersonnelManager)]
    [MustNotHaveCorporationRole(CorporationRole.Accountant, CorporationRole.JuniorAccountant)]
    public PyDataType ExtraCorporationRoleCall (ServiceCall extra)
    {
        return 0;
    }

    [MustHaveCorporationRole (MLS.UI_CORP_ACCESSDENIED1, CorporationRole.Director)]
    [MustNotHaveCorporationRole (MLS.UI_CORP_ACCESSDENIED2, CorporationRole.Accountant)]
    public PyDataType AnotherCorporationRoleCall (ServiceCall extra)
    {
        return 0;
    }

    [MustHaveCorporationRole (typeof (CrpOnlyDirectorsCanProposeVotes), CorporationRole.Director)]
    [MustNotHaveCorporationRole (typeof (CrpCantQuitDefaultCorporation), CorporationRole.Accountant)]
    public PyDataType VotesCorporationRoleCall (ServiceCall extra)
    {
        return 0;
    }

    [MustHaveRole (Roles.ROLE_LOGIN, typeof (CrpOnlyDirectorsCanProposeVotes))]
    public PyDataType AccountRoleEx (ServiceCall extra)
    {
        return 0;
    }

    [MustHaveRole(Roles.ROLE_LOGIN)]
    public PyDataType AccountRole (ServiceCall extra)
    {
        return 0;
    }

    [MustHaveSessionValue(Session.CHAR_ID)]
    public PyDataType SessionData (ServiceCall extra)
    {
        return 0;
    }

    [MustHaveSessionValue (Session.CHAR_ID, typeof(CrpOnlyDirectorsCanProposeVotes))]
    public PyDataType SessionDataEx (ServiceCall extra)
    {
        return 0;
    }

    [MustNotHaveSessionValue(Session.ALLIANCE_ID)]
    public PyDataType SessionDataMissing (ServiceCall extra)
    {
        return 0;
    }

    [MustNotHaveSessionValue (Session.ALLIANCE_ID, typeof (CrpOnlyDirectorsCanProposeVotes))]
    public PyDataType SessionDataMissingEx (ServiceCall extra)
    {
        return 0;
    }

    [MustBeCharacter]
    public PyDataType CharacterMissing (ServiceCall extra)
    {
        return 0;
    }

    [MustBeInStation]
    public PyDataType StationMissing (ServiceCall extra)
    {
        return 0;
    }

    [MustBeCharacter]
    [MustBeInStation]
    public PyDataType Chaining (ServiceCall extra)
    {
        return 0;
    }
}