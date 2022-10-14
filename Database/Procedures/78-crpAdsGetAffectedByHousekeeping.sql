DROP PROCEDURE IF EXISTS `CrpAdsGetAffectedByHousekeeping`;

DELIMITER //

CREATE PROCEDURE `CrpAdsGetAffectedByHousekeeping`(
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Returns a list of corporationIDs that will be affected by the next housekeeping (with the same date/time)'
BEGIN
	SELECT corporationID, adID FROM crpRecruitmentAds WHERE expiryDateTime < _currentTime;
END//

DELIMITER ;