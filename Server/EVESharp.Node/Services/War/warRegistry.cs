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
    private         Constants   Constants   { get; }

    public warRegistry (Constants constants, BoundServiceManager manager) : base (manager)
    {
        Constants = constants;
    }

    private warRegistry (Constants constants, BoundServiceManager manager, int objectID, Session session) : base (manager, session, objectID)
    {
        Constants      = constants;
        this.mObjectID = objectID;
    }

    public PyDataType GetWars (PyInteger ownerID, CallInformation call)
    {
        return new WarInfo ();
    }

    public PyDataType GetCostOfWarAgainst (PyInteger corporationID, CallInformation call)
    {
        return Constants.WarDeclarationCost.Value;
    }

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        // TODO: PROPERLY HANDLE THIS
        return BoundServiceManager.MachoNet.NodeID;
    }

    protected override BoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
        return new warRegistry (Constants, BoundServiceManager, bindParams.ObjectID, call.Session);
    }
}