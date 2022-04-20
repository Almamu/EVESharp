DROP PROCEDURE IF EXISTS `CluRegisterAddress`;

DELIMITER //

CREATE PROCEDURE `CluRegisterAddress`(
  IN `_type` char(30),
  IN `_objectID` int(11),
  IN `_nodeID` bigint(20)
)
SQL SECURITY INVOKER
COMMENT 'Registers a new macho address in the cluster'
BEGIN
	INSERT INTO cluAddresses(type, objectID, nodeID)VALUES(_type, _objectID, _nodeID);
END//

DELIMITER ;