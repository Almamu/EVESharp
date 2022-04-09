using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.War;

public class warRegistry : ClientBoundService
{
    private         int           mObjectID;
    public override AccessLevel   AccessLevel => AccessLevel.None;
    private         NodeContainer Container   { get; }

    public warRegistry (NodeContainer container, BoundServiceManager manager) : base (manager)
    {
        Container = container;
    }

    private warRegistry (NodeContainer container, BoundServiceManager manager, int objectID, Session session) : base (manager, session, objectID)
    {
        Container      = container;
        this.mObjectID = objectID;
    }

    public PyDataType GetWars (PyInteger ownerID, CallInformation call)
    {
        return new WarInfo ();
    }

    public PyDataType GetCostOfWarAgainst (PyInteger corporationID, CallInformation call)
    {
        return Container.Constants [Constants.warDeclarationCost].Value;
    }

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        // TODO: PROPERLY HANDLE THIS
        return BoundServiceManager.MachoNet.NodeID;
    }

    protected override BoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
        return new warRegistry (Container, BoundServiceManager, bindParams.ObjectID, call.Session);
    }
}