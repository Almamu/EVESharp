using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Agents;

public class AgentManager
{
    private DatabaseConnection DB { get; }

    public AgentManager (DatabaseConnection db)
    {
        DB = db;
    }

    public CRowset GetAgents ()
    {
        return DB.CRowset (AgentDB.GET_AGENTS);
    }

    public PyDataType GetInfo (int agentID)
    {
        PyDictionary <PyString, PyDataType> information = DB.Dictionary (
            AgentDB.GET_INFO,
            new Dictionary <string, object> {{"@agentID", agentID}}
        );

        information ["services"] = new PyList ();

        return KeyVal.FromDictionary (information);
    }
}