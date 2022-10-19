DROP PROCEDURE IF EXISTS `CrpAdsHousekeeping`;

DELIMITER //

CREATE PROCEDURE `CrpAdsHousekeeping`(
	IN _currentTime BIGINT(20)
)
SQL SECURITY INVOKER
COMMENT 'Performs timed events related to corporations like changing vote status when they\'re done'
BEGIN
	DELETE FROM crpRecruitmentAds WHERE expiryDateTime < _currentTime;
END//

DELIMITER ;