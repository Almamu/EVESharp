ALTER TABLE `chrBloodlines`
	ADD COLUMN `dataID` INT(10) UNSIGNED NOT NULL DEFAULT '0';

/*
 * Set proper values for bloodline's dataID
 */
UPDATE `chrBloodlines` SET `dataID` = 2888651 WHERE `bloodlineID` = 1;
UPDATE `chrBloodlines` SET `dataID` = 2888648 WHERE `bloodlineID` = 2;
UPDATE `chrBloodlines` SET `dataID` = 2888659 WHERE `bloodlineID` = 3;
UPDATE `chrBloodlines` SET `dataID` = 2888661 WHERE `bloodlineID` = 4;
UPDATE `chrBloodlines` SET `dataID` = 2888668 WHERE `bloodlineID` = 5;
UPDATE `chrBloodlines` SET `dataID` = 2823679 WHERE `bloodlineID` = 6;
UPDATE `chrBloodlines` SET `dataID` = 2888680 WHERE `bloodlineID` = 7;
UPDATE `chrBloodlines` SET `dataID` = 2888681 WHERE `bloodlineID` = 8;
UPDATE `chrBloodlines` SET `dataID` = 2397104 WHERE `bloodlineID` = 9;
UPDATE `chrBloodlines` SET `dataID` = 2397107 WHERE `bloodlineID` = 10;
UPDATE `chrBloodlines` SET `dataID` = 2888649 WHERE `bloodlineID` = 11;
UPDATE `chrBloodlines` SET `dataID` = 2888679 WHERE `bloodlineID` = 12;
UPDATE `chrBloodlines` SET `dataID` = 3250623 WHERE `bloodlineID` = 13;
UPDATE `chrBloodlines` SET `dataID` = 2888660 WHERE `bloodlineID` = 14;
