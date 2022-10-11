DROP PROCEDURE IF EXISTS `CrpAuditLogCreate`;

DELIMITER //

CREATE PROCEDURE `CrpAuditLogCreate`(
  IN `_corporationID` int(11),
  IN `_eventDateTime` bigint(20),
  IN `_eventTypeID` int(4),
  IN `_charID` int(11)
)
SQL SECURITY INVOKER
COMMENT 'Creates a new audit log record'
BEGIN
	INSERT INTO crpAuditLog (
		corporationID,
        eventDateTime,
        eventTypeID,
        charID
	) VALUES (
		_corporationID,
        _eventDateTime,
        _eventTypeID,
        _charID
	);
END//

DELIMITER ;