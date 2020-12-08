/**
 * EVESharp custom table, indicates where solarSystems are loaded
 */

DROP TABLE IF EXISTS `solarsystemsloaded`;
CREATE TABLE IF NOT EXISTS `solarsystemsloaded` (
  `solarSystemID` int(10) NOT NULL,
  `nodeID` int(10) NOT NULL,
  PRIMARY KEY (`solarSystemID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

INSERT INTO `solarsystemsloaded` (`solarSystemID`, `nodeID`) SELECT itemID, 0 as nodeID FROM entity WHERE typeID = 5;