/**
 * Add some client constants that are not in the eveConstants table
 */
INSERT INTO `eveConstants`(constantID, constantValue)VALUES('corporationAdvertisementFlatFee', 500000),('corporationAdvertisementDailyRate', 250000);

/**
 * Add some custom constants used by the server that are nowhere to be seen
 */
INSERT INTO `eveConstants`(constantID, constantValue)VALUES('medalTaxCorporation', 1000017),('medalCost', 5000000);
INSERT INTO `eveConstants`(constantID, constantValue)VALUES('warDeclarationCost', 100000000);
INSERT INTO `eveConstants`(constantID, constantValue)VALUES('conBidMinimum', 1000000);