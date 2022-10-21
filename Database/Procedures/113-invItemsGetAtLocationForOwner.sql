DROP PROCEDURE IF EXISTS `InvItemsGetAtLocationForOwner`;

DELIMITER //

CREATE PROCEDURE `InvItemsGetAtLocationForOwner`(
	IN _locationID INT,
	IN _ownerID INT
)
SQL SECURITY INVOKER
COMMENT 'Gets all the items available at the given location, for the given owner'
BEGIN
	SELECT itemID FROM invItems WHERE locationID = _locationID AND ownerID = _ownerID;
END//

DELIMITER ;