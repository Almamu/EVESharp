using System.Collections.Generic;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

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