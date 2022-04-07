using EVESharp.EVE.Services;
using EVESharp.Node.Database;
using EVESharp.Node.Network;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Corporations;

public class LPSvc : Service
{
    public override AccessLevel   AccessLevel => AccessLevel.None;
    private         CorporationDB DB          { get; }

    public LPSvc (CorporationDB db)
    {
        DB = db;
    }

    public PyDecimal GetLPForCharacterCorp (PyInteger corporationID, CallInformation call)
    {
        return DB.GetLPForCharacterCorp (corporationID, call.Session.EnsureCharacterIsSelected ());
    }
}