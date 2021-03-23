DROP TABLE IF EXISTS `conContracts`;

CREATE TABLE `conContracts` (
  `contractID` INT(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `issuerID` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `issuerCorpID` INT(10) UNSIGNED NOT NULL,
  `type` INT(10) NOT NULL,
  `availability` INT(10) NOT NULL,
  `assigneeID` INT(10) NULL DEFAULT NULL,
  `expiretime` INT(10) NOT NULL,
  `numDays` INT(10) NOT NULL,
  `startStationID` INT(10) UNSIGNED NOT NULL,
  `endStationID` INT(10) UNSIGNED NULL DEFAULT NULL,
  `price` DOUBLE(22,0) NOT NULL,
  `reward` DOUBLE(22,0) NOT NULL,
  `collateral` DOUBLE(22,0) NOT NULL,
  `title` VARCHAR(85) NULL DEFAULT NULL COLLATE 'utf8_general_ci',
  `description` TEXT(65535) NULL DEFAULT NULL COLLATE 'utf8_general_ci',
  `forCorp` TINYINT(1) NOT NULL,
  `status` INT(10) NOT NULL,
  `isAccepted` TINYINT(1) NOT NULL,
  `acceptorID` INT(10) NULL DEFAULT NULL,
  `dateIssued` BIGINT(20) NOT NULL,
  `dateExpired` BIGINT(20) NOT NULL,
  `dateAccepted` BIGINT(20) NULL DEFAULT NULL,
  `dateCompleted` BIGINT(20) NULL DEFAULT NULL,
  `volume` DOUBLE(22,0) NULL DEFAULT NULL,
  `requiresAttentionByOwner` TINYINT(1) NOT NULL DEFAULT '0',
  `requiresAttentionByAssignee` TINYINT(1) NOT NULL DEFAULT '0',
  `crateID` INT(10) UNSIGNED NULL DEFAULT '0',
  `issuerWalletKey` INT(10) UNSIGNED NOT NULL DEFAULT '0',
  `issuerAllianceID` INT(10) UNSIGNED NULL DEFAULT '0',
  `acceptorWalletKey` INT(10) UNSIGNED NULL DEFAULT '0',
  PRIMARY KEY (`contractID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;