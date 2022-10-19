DROP PROCEDURE IF EXISTS `CrpGetAuditLog`;

DELIMITER //

CREATE PROCEDURE `CrpGetAuditLog`(
  IN `_corporationID` int(11),
  IN `_charID` int(11),
  IN `_fromDate` bigint(20),
  IN `_toDate` bigint(20),
  IN `_limit` int(4)
)
SQL SECURITY INVOKER
COMMENT 'Returns the last audit log records for the given range'
BEGIN
  SELECT * FROM crpAuditLog WHERE eventDateTime >= _fromDate AND eventDateTime <= _toDate AND corporationID = _corporationID AND charID = _charID ORDER BY eventDateTime DESC LIMIT _limit;
END//

DELIMITER ;