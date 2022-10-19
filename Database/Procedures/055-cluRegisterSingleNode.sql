DROP PROCEDURE IF EXISTS `CluRegisterSingleNode`;

DELIMITER //

CREATE PROCEDURE `CluRegisterSingleNode`(IN `_nodeID` bigint(20))
SQL SECURITY INVOKER
COMMENT 'Registers a node in the cluster with default values as a placeholder for single-instance clusters'
BEGIN
	INSERT INTO cluster(id, ip, address, port, lastHeartBeat, role, `load`)VALUES(_nodeID, '', '', 26000, 0, 'server', 0.0);
END//

DELIMITER ;