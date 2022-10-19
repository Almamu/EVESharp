DROP PROCEDURE IF EXISTS `CrpAlliancesUpdateSupportedExecutor`;

DELIMITER //

CREATE PROCEDURE `CrpAlliancesUpdateSupportedExecutor`(IN _corporationID INT(11), IN _chosenExecutorID INT(11), IN _allianceID INT(11))
SQL SECURITY INVOKER
COMMENT 'Updates the supported executor for a corporation and calculates new executor corp ID for the alliance'
BEGIN
	DECLARE totalVotes INT(11);
    DECLARE percentage DOUBLE;
    DECLARE votes INT(11);
    DECLARE newExecutorCorpID INT(11);
    
	UPDATE corporation SET chosenExecutorID = _chosenExecutorID WHERE corporationID = _corporationID;
    SELECT COUNT(*) INTO totalVotes FROM corporation WHERE allianceID = _allianceID;
	SELECT chosenExecutorID, COUNT(*) INTO newExecutorCorpID, votes FROM corporation WHERE allianceID = _allianceID GROUP BY chosenExecutorID ORDER BY votes DESC LIMIT 1;
    
    SET percentage = votes * 100 / totalVotes;
    
    IF percentage > 50 THEN
		SELECT newExecutorCorpID;
	ELSE
		SELECT NULL;
	END IF;
END//

DELIMITER ;