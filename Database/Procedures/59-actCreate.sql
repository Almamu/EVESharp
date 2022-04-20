DROP PROCEDURE IF EXISTS `ActCreate`;

DELIMITER //

CREATE PROCEDURE `ActCreate`(IN _username TEXT, IN _password TEXT, IN _role BIGINT(20))
SQL SECURITY INVOKER
COMMENT 'Creates a new account in the system'
BEGIN
	INSERT INTO account(accountID, accountName, password, role, online, banned)VALUES(NULL, _username, SHA1(_password), _role, 0, 0);
END//

DELIMITER ;