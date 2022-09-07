using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.PythonTypes.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Database;

public static class AgentDB
{
    public static CRowset AgtGetAgents (this IDatabaseConnection Database)
    {
        return Database.CRowset ("AgtGetAgents");
    }

    public static PyDictionary <PyString, PyDataType> AgtGetInfo (this IDatabaseConnection Database, int agentID)
    {
        // TODO: SUPPORT MULTIPLE ANSWERS FROM THE PROCEDURE TO GET THE SERVICES INFORMATION REQUIRED FOR THIS QUERY
        return Database.Dictionary ("AgtGetInfo", new Dictionary <string, object> () {{"_agentID", agentID}});
    }
}