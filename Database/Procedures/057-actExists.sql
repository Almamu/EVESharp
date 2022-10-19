DROP PROCEDURE IF EXISTS `ActExists`;

DELIMITER //

CREATE PROCEDURE `ActExists`(IN _username TEXT)
SQL SECURITY INVOKER
COMMENT 'Checks if the given username is already registered or not'
BEGIN
	DECLARE numberOfAccounts INT;

	SELECT COUNT(accountID) INTO numberOfAccounts FROM account WHERE accountName = _username;

	IF numberOfAccounts > 0 THEN
		SELECT true;
	ELSE
		SELECT false;
	END IF;
END//

DELIMITER ;