DROP PROCEDURE IF EXISTS `CrpListApplications`;

DELIMITER //

CREATE PROCEDURE `CrpListApplications`(IN _corporationID INT(11))
SQL SECURITY INVOKER
COMMENT 'List the applications to the given corporation'
BEGIN
  SELECT corporationID, characterID, applicationText, 0 AS status, applicationDateTime FROM chrApplications WHERE corporationID = _corporationID;
END//

DELIMITER ;