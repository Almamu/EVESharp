DROP TABLE IF EXISTS `crpAuditLog`;

CREATE TABLE `crpAuditLog` (
  `id` BIGINT NOT NULL AUTO_INCREMENT,
  `corporationID` INT NOT NULL,
  `eventDateTime` BIGINT NOT NULL,
  `eventTypeID` INT(4) NOT NULL,
  `charID` INT(11) NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `corporationID` (`corporationID` ASC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `crpAuditRole`;

CREATE TABLE `crpAuditRole` (
  `id` BIGINT NOT NULL AUTO_INCREMENT,
  `charID` INT NOT NULL,
  `issuerID` INT NOT NULL,
  `changeTime` BIGINT NOT NULL,
  `corporationID` INT NOT NULL,
  `grantable` TINYINT(1) NOT NULL,
  `oldRoles` BIGINT NOT NULL,
  `newRoles` BIGINT NOT NULL,
  PRIMARY KEY (`id`),
  INDEX `corporationID` (`corporationID` ASC),
  INDEX `characterID` (`charID` ASC),
  INDEX `changeTime` (`changeTime` ASC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;