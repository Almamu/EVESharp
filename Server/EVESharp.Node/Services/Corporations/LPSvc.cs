using EVESharp.Database.Old;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.Types;

namespace EVESharp.Node.Services.Corporations;

[MustBeCharacter]
public class LPSvc : Service
{
    public override AccessLevel   AccessLevel => AccessLevel.None;
    private         CorporationDB DB          { get; }

    public LPSvc (CorporationDB db)
    {
        DB = db;
    }

    public PyDecimal GetLPForCharacterCorp (ServiceCall call, PyInteger corporationID)
    {
        return DB.GetLPForCharacterCorp (corporationID, call.Session.CharacterID);
    }
}