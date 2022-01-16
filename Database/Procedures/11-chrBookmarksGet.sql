DROP PROCEDURE IF EXISTS `ChrBookmarksGet`;

DELIMITER //

CREATE PROCEDURE `ChrBookmarksGet`(IN _ownerID INT(11))
SQL SECURITY INVOKER
COMMENT 'Returns the bookmarks for the specified ownerID'
BEGIN
	SELECT bookmarkID, ownerID, itemID, typeID, memo, comment, created, x, y, z, locationID FROM chrBookmarks WHERE ownerID = _ownerID;
END//

DELIMITER ;