DROP PROCEDURE IF EXISTS `CrpAuditRoleCreate`;

DELIMITER //

CREATE PROCEDURE `CrpAuditRoleCreate`(
  IN `_charID` int(11),
  IN `_issuerID` int(11),
  IN `_corporationID` int(11),
  IN `_changeTime` bigint(20),
  IN `_grantable` tinyint(1),
  IN `_oldRoles` bigint(20),
  IN `_newRoles` bigint(20)
)
SQL SECURITY INVOKER
COMMENT 'Creates a new audit log record'
BEGIN
	INSERT INTO crpAuditRole (
		charID,
        corporationID,
        issuerID,
        changeTime,
        grantable,
        oldRoles,
        newRoles
	) VALUES (
		_charID,
        _corporationID,
        _issuerID,
        _changeTime,
        _grantable,
        _oldRoles,
        _newRoles
	);
END//

DELIMITER ;