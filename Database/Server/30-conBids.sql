
DROP TABLE IF EXISTS `conBids`;

CREATE TABLE `conBids` (
  `bidID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `contractID` int(10) unsigned NOT NULL,
  `bidderID` int(10) unsigned NOT NULL,
  `amount` double unsigned NOT NULL,
  `isCorp` tinyine unsigned NOT NULL DEFAULT 0,
  PRIMARY KEY (`bidID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
