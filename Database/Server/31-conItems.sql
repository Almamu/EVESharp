DROP TABLE IF EXISTS `conItems`;

CREATE TABLE IF NOT EXISTS `conItems` (
  `id` int(10) NOT NULL AUTO_INCREMENT,
  `contractID` int(10) NOT NULL,
  `itemTypeID` int(10) NOT NULL,
  `quantity` int(10) NOT NULL,
  `inCrate` tinyint(1) NOT NULL,
  `itemID` int(11) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
