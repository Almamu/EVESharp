using EVESharp.EVE.Data.Configuration;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Sessions;
using EVESharp.Types;

namespace EVESharp.Node.Services.War;

public class warRegistry : ClientBoundService
{
    private         int         mObjectID;
    public override AccessLevel AccessLevel => AccessLevel.None;
    private         IConstants  Constants   { get; }

    public warRegistry (IConstants constants, IBoundServiceManager manager) : base (manager)
    {
        Constants = constants;
    }

    private warRegistry (IConstants constants, IBoundServiceManager manager, int objectID, Session session) : base (manager, session, objectID)
    {
        Constants      = constants;
        this.mObjectID = objectID;
    }

    public PyDataType GetWars (ServiceCall call, PyInteger ownerID)
    {
        return new WarInfo ();
    }

    public PyDataType GetCostOfWarAgainst (ServiceCall call, PyInteger corporationID)
    {
        return Constants.WarDeclarationCost.Value;
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        // TODO: PROPERLY HANDLE THIS
        return BoundServiceManager.MachoNet.NodeID;
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        return new warRegistry (Constants, BoundServiceManager, bindParams.ObjectID, call.Session);
    }
}