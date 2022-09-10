using EVESharp.Database;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Agents;

public class AgentManager
{
    private IDatabaseConnection DB { get; }

    public AgentManager (IDatabaseConnection db)
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