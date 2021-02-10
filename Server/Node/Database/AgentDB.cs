using Common.Database;
using PythonTypes.Types.Primitives;

namespace Node.Database
{
    public class AgentDB : DatabaseAccessor
    {
        public AgentDB(DatabaseConnection db) : base(db)
        {
        }

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
    }
}