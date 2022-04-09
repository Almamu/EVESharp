using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Agents;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Characters;

public class agentMgr : ClientBoundService
{
    public override AccessLevel   AccessLevel  => AccessLevel.None;
    private         AgentManager  AgentManager { get; }
    private         NodeContainer Container    { get; }

    public agentMgr (NodeContainer container, AgentManager agentManager, BoundServiceManager manager) : base (manager)
    {
        AgentManager = agentManager;
        Container    = container;
    }

    protected agentMgr (int agentID, NodeContainer container, AgentManager agentManager, BoundServiceManager manager, Session session) : base (
        manager, session, agentID
    )
    {
        AgentManager = agentManager;
        Container    = container;
    }

    public PyDataType GetAgents (CallInformation call)
    {
        return AgentManager.GetAgents ();
    }

    public PyDataType GetMyJournalDetails (CallInformation call)
    {
        return new PyTuple (2)
        {
            [0] = new PyList (), // missions
            [1] = new PyList () // research
        };
    }

    public PyDataType GetMyEpicJournalDetails (CallInformation call)
    {
        return new PyList ();
    }

    public PyDataType GetInfoServiceDetails (CallInformation call)
    {
        return AgentManager.GetInfo (ObjectID);
    }

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        return call.MachoNet.NodeID;
    }

    protected override BoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
        return new agentMgr (bindParams.ObjectID, Container, AgentManager, BoundServiceManager, call.Session);
    }
}