using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Types;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Agents;

public class AgentManager
{
    private IDatabase DB { get; }

    public AgentManager (IDatabase db)
    {
        DB = db;
    }

    public CRowset GetAgents ()
    {
        return DB.AgtGetAgents ();
    }

    public PyDataType GetInfo (int agentID)
    {
        PyDictionary <PyString, PyDataType> information = DB.AgtGetInfo (agentID);

        information ["services"] = new PyList ();

        return KeyVal.FromDictionary (information);
    }
}