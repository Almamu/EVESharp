DROP PROCEDURE IF EXISTS `CrtGetMyCertificates`;

DELIMITER //

CREATE PROCEDURE `CrtGetMyCertificates`(IN `_characterID` int(10) unsigned)
SQL SECURITY INVOKER
COMMENT 'Gets all the certificates for the given characterID'
BEGIN
	SELECT certificateID, grantDate, visibilityFlags FROM chrCertificates WHERE characterID = _characterID;
END//

DELIMITER ;