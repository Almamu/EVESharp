DROP PROCEDURE IF EXISTS `ActLogin`;

DELIMITER //

CREATE PROCEDURE `ActLogin`(IN _username TEXT, IN _password TEXT)
SQL SECURITY INVOKER
COMMENT 'Performs login on the specified account'
BEGIN
	DECLARE _accountID INT;
	DECLARE _banned TINYINT(1);
	DECLARE _role BIGINT(20) UNSIGNED;

	SELECT accountID, banned, role INTO _accountID, _banned, _role FROM account WHERE accountName LIKE _username AND password LIKE SHA1(_password);

	IF _accountID IS NOT NULL THEN
		# update the online flag for the player
		UPDATE account SET `online` = 1 WHERE accountID = _accountID;
		# return the right data back
		SELECT _accountID, _role, _banned;
	END IF;
END//

DELIMITER ;