DROP PROCEDURE IF EXISTS `AgtGetInfo`;

DELIMITER //

CREATE PROCEDURE `AgtGetInfo`(IN agentID BIGINT(20))
SQL SECURITY INVOKER
COMMENT 'Lists the agents and their information in the database'
BEGIN
	SELECT stationID, level, quality, quality AS effectiveQuality, 0 AS incompatible FROM agtAgents WHERE agtAgents.agentID = agentID;
END//

DELIMITER ;