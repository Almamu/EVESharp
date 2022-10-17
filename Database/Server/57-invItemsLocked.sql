DROP TABLE IF EXISTS `invItemsLocked`;

CREATE TABLE `invItemsLocked` (
  `itemID` INT UNSIGNED NOT NULL,
  `stationID` INT UNSIGNED NOT NULL,
  `corporationID` INT UNSIGNED NOT NULL,
  PRIMARY KEY (`itemID`),
  INDEX `stationID` (`stationID` ASC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
