DROP PROCEDURE IF EXISTS `CluResolveAddress`;

DELIMITER //

CREATE PROCEDURE `CluResolveAddress`(IN _type CHAR(30), IN _objectID INT(11))
SQL SECURITY INVOKER
COMMENT 'Resolves the given address to a specific nodeID (or the less loaded one if no one is assigned to it)'
BEGIN
    # The name of the lock for resolving the address
    DECLARE lockName TEXT DEFAULT CONCAT("Address_", _type, "_", _objectID);
    DECLARE _nodeID BIGINT(20);
    DECLARE errno TINYINT;
    
    # obtain the lock
    SELECT GET_LOCK (lockName, 0xFFFFFFFF) INTO errno;
    
    SELECT nodeID INTO _nodeID FROM cluAddresses WHERE `type` = CONVERT(_type USING utf8mb4) COLLATE utf8mb4_unicode_ci AND objectID = _objectID;
    
    IF _nodeID IS NULL THEN
        # find the new node to take care of the address
        SELECT id INTO _nodeID FROM cluster ORDER BY `load` DESC LIMIT 1;
        # assign it
        CALL CluRegisterAddress(_type, _objectID, _nodeID);
    END IF;
    
    # release the lock
    SELECT RELEASE_LOCK (lockName) INTO errno;
    
    SELECT _nodeID;
END//

DELIMITER ;