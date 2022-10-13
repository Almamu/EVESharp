using System.Collections.Generic;
using EVESharp.Database.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Database.Extensions;

public static class AgentDB
{
    public static CRowset AgtGetAgents (this IDatabase Database)
    {
        return Database.CRowset ("AgtGetAgents");
    }

    public static PyDictionary <PyString, PyDataType> AgtGetInfo (this IDatabase Database, int agentID)
    {
        // TODO: SUPPORT MULTIPLE ANSWERS FROM THE PROCEDURE TO GET THE SERVICES INFORMATION REQUIRED FOR THIS QUERY
        return Database.Dictionary ("AgtGetInfo", new Dictionary <string, object> () {{"_agentID", agentID}});
    }
}