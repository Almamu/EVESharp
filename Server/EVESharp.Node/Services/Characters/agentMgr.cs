using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Agents;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Characters;

public class agentMgr : ClientBoundService
{
    public override AccessLevel  AccessLevel  => AccessLevel.None;
    private         AgentManager AgentManager { get; }

    public agentMgr (AgentManager agentManager, IBoundServiceManager manager) : base (manager)
    {
        AgentManager = agentManager;
    }

    protected agentMgr (int agentID, AgentManager agentManager, IBoundServiceManager manager, Session session) : base (manager, session, agentID)
    {
        AgentManager = agentManager;
    }

    public PyDataType GetAgents (ServiceCall call)
    {
        return AgentManager.GetAgents ();
    }

    public PyDataType GetMyJournalDetails (ServiceCall call)
    {
        return new PyTuple (2)
        {
            [0] = new PyList (), // missions
            [1] = new PyList () // research
        };
    }

    public PyDataType GetMyEpicJournalDetails (ServiceCall call)
    {
        return new PyList ();
    }

    public PyDataType GetInfoServiceDetails (ServiceCall call)
    {
        return AgentManager.GetInfo (ObjectID);
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        // TODO: PROPERLY IMPLEMENT THIS ONE
        return call.MachoNet.NodeID;
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        return new agentMgr (bindParams.ObjectID, AgentManager, BoundServiceManager, call.Session);
    }
}