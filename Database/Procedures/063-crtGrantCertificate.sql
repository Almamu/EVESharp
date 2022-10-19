DROP PROCEDURE IF EXISTS `CrtGrantCertificate`;

DELIMITER //

CREATE PROCEDURE `CrtGrantCertificate`(IN `_characterID` int(10) unsigned, IN `_certificateID` int(10), IN `_grantDate` bigint(20))
SQL SECURITY INVOKER
COMMENT 'Grants a new certificate for the given character'
BEGIN
	REPLACE INTO chrCertificates (characterID, certificateID, grantDate, visibilityFlags) VALUES (_characterID, _certificateID, _grantDate, 0);
END//

DELIMITER ;