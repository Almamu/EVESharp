using EVESharp.Database;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

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