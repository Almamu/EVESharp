
DROP TABLE IF EXISTS `conBids`;

CREATE TABLE `conBids` (
  `bidID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `contractID` int(10) unsigned NOT NULL,
  `bidderID` int(10) unsigned NOT NULL,
  `amount` double unsigned NOT NULL,
  `issuerStationID` int(10) unsigned NOT NULL,
  PRIMARY KEY (`bidID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
