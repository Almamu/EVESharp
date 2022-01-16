DROP PROCEDURE IF EXISTS `CrpAlliancesCreate`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesCreate`(
  IN `_allianceID` int(11),
  IN `_shortName` varchar(50),
  IN `_description` text,
  IN `_url` varchar(255),
  IN `_creatorID` int(11),
  IN `_creatorCharacterID` int(11),
  IN `_dictatorial` tinyint(4),
  IN `_startDate` bigint(20)
)
SQL SECURITY INVOKER
COMMENT 'Creates a new alliance'
BEGIN
	INSERT INTO crpAlliances(
		allianceID, shortName, description, url, executorCorpID, creatorCorpID, creatorCharID, dictatorial, startDate
	)VALUES(
		_allianceID, _shortName, _description, _url, _creatorID, _creatorID, _creatorCharacterID, _dictatorial, _startDate
	);
END//

DELIMITER ;