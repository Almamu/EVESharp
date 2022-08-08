using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Configuration;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.War;

public class warRegistry : ClientBoundService
{
    private         int         mObjectID;
    public override AccessLevel AccessLevel => AccessLevel.None;
    private         IConstants   Constants   { get; }

    public warRegistry (IConstants constants, BoundServiceManager manager) : base (manager)
    {
        Constants = constants;
    }

    private warRegistry (IConstants constants, BoundServiceManager manager, int objectID, Session session) : base (manager, session, objectID)
    {
        Constants      = constants;
        this.mObjectID = objectID;
    }

    public PyDataType GetWars (CallInformation call, PyInteger ownerID)
    {
        return new WarInfo ();
    }

    public PyDataType GetCostOfWarAgainst (CallInformation call, PyInteger corporationID)
    {
        return Constants.WarDeclarationCost.Value;
    }

    protected override long MachoResolveObject (CallInformation call, ServiceBindParams parameters)
    {
        // TODO: PROPERLY HANDLE THIS
        return BoundServiceManager.MachoNet.NodeID;
    }

    protected override BoundService CreateBoundInstance (CallInformation call, ServiceBindParams bindParams)
    {
        return new warRegistry (Constants, BoundServiceManager, bindParams.ObjectID, call.Session);
    }
}