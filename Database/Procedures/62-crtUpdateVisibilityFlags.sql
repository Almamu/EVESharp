DROP PROCEDURE IF EXISTS `CrtUpdateVisibilityFlags`;

DELIMITER //

CREATE PROCEDURE `CrtUpdateVisibilityFlags`(IN `_characterID` int(10) unsigned, IN `_certificateID` int(10) unsigned, IN `_flag` int(10))
SQL SECURITY INVOKER
COMMENT 'Updates the visibility flags for the given certificates and character'
BEGIN
	UPDATE chrCertificates SET visibilityFlags = _flags WHERE characterID = _characterID AND certificateID = _certificateID;
END//

DELIMITER ;