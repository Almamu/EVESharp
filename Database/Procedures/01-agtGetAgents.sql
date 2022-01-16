DROP PROCEDURE IF EXISTS `AgtGetAgents`;

DELIMITER //

CREATE PROCEDURE `AgtGetAgents`()
SQL SECURITY INVOKER
COMMENT 'Lists the agents in the database'
BEGIN
	SELECT
		agentID, agentTypeID, divisionID, `level`, agtAgents.stationID,
		quality, agtAgents.corporationID, bloodlineTypes.bloodlineID, chrInformation.gender
	FROM agtAgents
	LEFT JOIN chrInformation on chrInformation.characterID = agtAgents.agentID
	LEFT JOIN invItems ON invItems.itemID = chrInformation.characterID
	LEFT JOIN bloodlineTypes USING (typeID);
END//

DELIMITER ;