using Common.Services;
using Node.Database;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class agentMgr : IService
    {
        private AgentDB DB { get; }
        
        public agentMgr(AgentDB db)
        {
            this.DB = db;
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
    }
}