using System;
using Common.Database;
using Common.Services;
using Node.Database;
using Node.Network;
using Org.BouncyCastle.X509.Extension;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Characters
{
    public class agentMgr : Service
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
            return new PyTuple(new PyDataType[]
            {
                new PyList(), // missions
                new PyList() // research
            });
        }

        public PyDataType GetMyEpicJournalDetails(CallInformation call)
        {
            return new PyList();
        }
    }
}