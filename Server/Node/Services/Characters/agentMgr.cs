using Common.Services;
using EVE.Packets.Exceptions;
using Node.Agents;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class agentMgr : BoundService
    {
        private AgentDB DB { get; init; }
        private AgentManager AgentManager { get; init; }
        private NodeContainer Container { get; init; }
        private int AgentID { get; init; }
        
        public agentMgr(AgentDB db, NodeContainer container, AgentManager agentManager, BoundServiceManager manager) : base(manager, null)
        {
            this.DB = db;
            this.AgentManager = agentManager;
            this.Container = container;
        }

        protected agentMgr(int agentID, AgentDB db, NodeContainer container, AgentManager agentManager, BoundServiceManager manager, Client client) : base(manager, client)
        {
            this.DB = db;
            this.AgentManager = agentManager;
            this.Container = container;
            this.AgentID = agentID;
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

        /// <inheritdoc cref="MachoResolveObject"/>
        public override PyInteger MachoResolveObject(PyInteger objectID, PyInteger zero, CallInformation call)
        {
            return this.Container.NodeID;
        }

        /// <inheritdoc cref="BoundService.CreateBoundInstance"/>
        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            if (objectData is not PyInteger agentID)
                throw new CustomError("Unexpected!");
            
            return new agentMgr(agentID, this.DB, this.Container, this.AgentManager, this.BoundServiceManager, call.Client);
        }

        public PyDataType GetInfoServiceDetails(CallInformation call)
        {
            return this.DB.GetInfoServiceDetails(this.AgentID);
        }
    }
}