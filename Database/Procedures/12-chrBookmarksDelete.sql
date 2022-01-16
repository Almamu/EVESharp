DROP PROCEDURE IF EXISTS `ChrBookmarksDelete`;

DELIMITER //

CREATE PROCEDURE `ChrBookmarksDelete`(IN _ownerID INT(11), IN _bookmarkIDs TEXT)
SQL SECURITY INVOKER
COMMENT 'Deletes the specified bookmarks for the given ownerID'
BEGIN
	DELETE FROM chrBookmarks WHERE ownerID = _ownerID AND FIND_IN_SET(bookmarkID, _bookmarkIDs);
END//

DELIMITER ;