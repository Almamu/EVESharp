DROP PROCEDURE IF EXISTS `CluCleanup`;

DELIMITER //

CREATE PROCEDURE `CluCleanup`()
SQL SECURITY INVOKER
COMMENT 'Registers a node in the cluster with default values as a placeholder for single-instance clusters'
BEGIN
	DELETE FROM cluster;
	DELETE FROM cluAddresses;
END//

DELIMITER ;