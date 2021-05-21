using Common.Services;
using EVE.Packets.Exceptions;
using Node.Agents;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class agentMgr : ClientBoundService
    {
        private AgentDB DB { get; init; }
        private AgentManager AgentManager { get; init; }
        private NodeContainer Container { get; init; }
        
        public agentMgr(AgentDB db, NodeContainer container, AgentManager agentManager, BoundServiceManager manager) : base(manager)
        {
            this.DB = db;
            this.AgentManager = agentManager;
            this.Container = container;
        }

        protected agentMgr(int agentID, AgentDB db, NodeContainer container, AgentManager agentManager, BoundServiceManager manager, Client client) : base(manager, client, agentID)
        {
            this.DB = db;
            this.AgentManager = agentManager;
            this.Container = container;
        }

        public PyDataType GetAgents(CallInformation call)
        {
            return this.DB.GetAgents();
        }

        public PyDataType GetMyJournalDetails(CallInformation call)
        {
            return new PyTuple(2)
            {
                [0] = new PyList(), // missions
                [1] = new PyList() // research
            };
        }

        public PyDataType GetMyEpicJournalDetails(CallInformation call)
        {
            return new PyList();
        }
        
        public PyDataType GetInfoServiceDetails(CallInformation call)
        {
            return this.DB.GetInfoServiceDetails(this.ObjectID);
        }

        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            return this.Container.NodeID;
        }

        protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            return new agentMgr(bindParams.ObjectID, this.DB, this.Container, this.AgentManager, this.BoundServiceManager, call.Client);
        }
    }
}