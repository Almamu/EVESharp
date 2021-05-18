/*
 * Replace CEOs on the corporations to known characters, we don't know info about the CEOs, just their names
 * so for now instead of creating the new ceo's information, just replace them in the database
 */
UPDATE corporation SET ceoID = ((SELECT IF(characterID IS NULL, 0, characterID) FROM chrInformation WHERE corporationID = corporation.corporationID LIMIT 1) UNION (SELECT 0) LIMIT 1);