CREATE TABLE `mktWallets` (
	`key` INT NOT NULL,
	`ownerID` INT NOT NULL,
	`balance` DOUBLE NOT NULL,
	PRIMARY KEY (`key`, `ownerID`)
) COLLATE='utf8_general_ci';