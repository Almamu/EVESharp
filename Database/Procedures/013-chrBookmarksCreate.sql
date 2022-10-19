DROP PROCEDURE IF EXISTS `ChrBookmarksCreate`;

DELIMITER //

CREATE PROCEDURE `ChrBookmarksCreate`(
  IN `_ownerID` int(10),
  IN `_itemID` int(10),
  IN `_typeID` int(10),
  IN `_memo` varchar(85),
  IN `_comment` text,
  IN `_date` bigint(20),
  IN `_x` double,
  IN `_y` double,
  IN `_z` double,
  IN `_locationID` int(10)
)
SQL SECURITY INVOKER
COMMENT 'Creates a new bookmark'
BEGIN
	INSERT INTO chrBookmarks(
		ownerID, itemID, typeID, memo, `comment`, created, x, y, z, locationID
	)VALUES(
		_ownerID, _itemID, _typeID, _memo, _comment, _date, _x, _y, _z, _locationID
	);

	SELECT LAST_INSERT_ID();
END//

DELIMITER ;