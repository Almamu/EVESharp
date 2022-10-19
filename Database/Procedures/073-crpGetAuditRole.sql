DROP PROCEDURE IF EXISTS `CrpGetAuditRole`;

DELIMITER //

CREATE PROCEDURE `CrpGetAuditRole`(
  IN `_corporationID` int(11),
  IN `_charID` int(11),
  IN `_fromDate` bigint(20),
  IN `_toDate` bigint(20),
  IN `_limit` int(4)
)
SQL SECURITY INVOKER
COMMENT 'Returns the last audit log records for the given range'
BEGIN
	SELECT * FROM crpAuditRole WHERE changeTime >= _fromDate AND changeTime <= _toDate AND corporationID = _corporationID AND charID = _charID ORDER BY changeTime DESC LIMIT _limit;
END//

DELIMITER ;