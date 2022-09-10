using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.Node.Database;
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

    public PyDecimal GetLPForCharacterCorp (CallInformation call, PyInteger corporationID)
    {
        return DB.GetLPForCharacterCorp (corporationID, call.Session.CharacterID);
    }
}