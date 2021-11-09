using System.Collections.Generic;
using EVESharp.Common.Database;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Database
{
    public class AgentDB : DatabaseAccessor
    {
	    /// <summary>
	    /// Returns a list of agents ready to be used by the EVE Client
	    /// </summary>
	    /// <returns></returns>
	    public PyDataType GetAgents()
        {
            return Database.PrepareCRowsetQuery(
	            "SELECT agentID, agentTypeID, divisionID, level, agtAgents.stationID, quality," +
	            " agtAgents.corporationID, bloodlineTypes.bloodlineID, chrInformation.gender" +
	            " FROM agtAgents" +
	            " LEFT JOIN chrInformation on chrInformation.characterID = agtAgents.agentID" +
	            " LEFT JOIN invItems ON invItems.itemID = chrInformation.characterID" +
	            " LEFT JOIN bloodlineTypes USING (typeID)"
	        );
        }

	    public PyDataType GetInfoServiceDetails(int agentID)
	    {
		    // TODO: calculate effective quality (i guess based on standings?)
		    PyDictionary<PyString, PyDataType> result = Database.PrepareDictionaryQuery(
			    "SELECT stationID, level, quality, quality AS effectiveQuality, 0 AS incompatible FROM agtAgents WHERE agentID = @agentID",
			    new Dictionary<string, object>()
			    {
				    {"@agentID", agentID}
			    }
		    );

		    // this is used to include relevant information like suggested skills, mission-granting requirements
		    // and other comments on the agent
		    // this information is not in the database and doesn't seem to be required
		    // but it would be nice to have
		    /*result["services"] = new PyList(1)
		    {
			    [0] = new PyTuple(2)
			    {
				    [0] = "asdf",
				    [1] = new PyTuple(2)
				    {
					    [0] = "fdsa",
					    [1] = "hjgj"
				    }
			    }
		    };*/
		    result["services"] = new PyList();

		    return KeyVal.FromDictionary(result);
	    }

        public AgentDB(DatabaseConnection db) : base(db)
        {
        }
    }
}