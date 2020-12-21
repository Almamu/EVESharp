using System;
using Common.Database;
using Node.Database;
using Org.BouncyCastle.X509.Extension;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class agentMgr : Service
    {
        private AgentDB mDB = null;
        
        public agentMgr(DatabaseConnection db, ServiceManager manager) : base(manager)
        {
            this.mDB = new AgentDB(db);
        }

        public PyDataType GetAgents(PyDictionary namedPayload, Client client)
        {
            return this.mDB.GetAgents();
        }

        public PyDataType GetMyJournalDetails(PyDictionary namedPayload, Client client)
        {
            return new PyTuple(new PyDataType[]
            {
                new PyList(), // missions
                new PyList() // research
            });
        }
    }
}