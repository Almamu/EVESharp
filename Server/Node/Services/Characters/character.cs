/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2012 - Glint Development Group
    ------------------------------------------------------------------------------------
    This program is free software; you can redistribute it and/or modify it under
    the terms of the GNU Lesser General Public License as published by the Free Software
    Foundation; either version 2 of the License, or (at your option) any later
    version.

    This program is distributed in the hope that it will be useful, but WITHOUT
    ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
    FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License along with
    this program; if not, write to the Free Software Foundation, Inc., 59 Temple
    Place - Suite 330, Boston, MA 02111-1307, USA, or go to
    http://www.gnu.org/copyleft/lesser.txt.
    ------------------------------------------------------------------------------------
    Creator: Almamu
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Common.Database;
using Common.Logging;
using Common.Services;
using Node.Data;
using Node.Database;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class character : Service
    {
        enum NameValidationResults
        {
            Valid = 1,
            TooShort = -1,
            TooLong = -2,
            IllegalCharacters = -5,
            MoreThanOneSpace = -6,
            Taken = -101,
            Banned = -102
        };

        private readonly CharacterDB mDB = null;
        private readonly SkillDB mSkillDB = null;
        private readonly Dictionary<int, Bloodline> mBloodlineCache = null;
        private readonly Dictionary<int, Ancestry> mAncestriesCache = null;
        private readonly Configuration.Character mConfiguration = null;
        private readonly Channel Log = null;

        public character(DatabaseConnection db, Configuration.Character configuration, ServiceManager manager) : base(manager)
        {
            this.Log = manager.Container.Logger.CreateLogChannel("character");
            this.mConfiguration = configuration;
            this.mDB = new CharacterDB(db, manager.Container.ItemFactory);
            this.mSkillDB = new SkillDB(db, manager.Container.ItemFactory);
            this.mBloodlineCache = this.mDB.GetBloodlineInformation();
            this.mAncestriesCache = this.mDB.GetAncestryInformation(this.mBloodlineCache);
        }

        public PyDataType GetCharactersToSelect(PyDictionary namedPayload, Client client)
        {
            return this.mDB.GetCharacterList(client.AccountID);
        }

        public PyDataType LogStartOfCharacterCreation(PyDictionary namedPayload, Client client)
        {
            return null;
        }

        public PyDataType GetCharCreationInfo(PyDictionary namedPayload, Client client)
        {
            return this.ServiceManager.CacheStorage.GetHints(CacheStorage.CreateCharacterCacheTable);
        }

        public PyDataType GetAppearanceInfo(PyDictionary namedPayload, Client client)
        {
            return this.ServiceManager.CacheStorage.GetHints(CacheStorage.CharacterAppearanceCacheTable);
        }

        public PyDataType GetCharNewExtraCreationInfo(PyDictionary namedPayload, Client client)
        {
            return new PyDictionary();
        }

        public PyInteger ValidateNameEx(PyString name, PyDictionary namedPayload, Client client)
        {
            string characterName = name;
            
            if (characterName.Length < 3)
                return new PyInteger((int) NameValidationResults.TooShort);

            // character name length is maximum 24 characters based on the error messages used for the user
            if (characterName.Length > 24)
                return new PyInteger((int) NameValidationResults.TooLong);

            // ensure only alphanumeric characters and/or spaces are used
            if (Regex.IsMatch(characterName, "^[a-zA-Z0-9 ]*$") == false)
                return new PyInteger((int) NameValidationResults.IllegalCharacters);

            // no more than one space allowed
            if (characterName.IndexOf(' ') != characterName.LastIndexOf(' '))
                return new PyInteger((int) NameValidationResults.MoreThanOneSpace);

            // ensure there is no character registered with this name already
            if (this.mDB.IsCharacterNameTaken(characterName) == true)
                return new PyInteger((int) NameValidationResults.Taken);

            // TODO: IMPLEMENT BANLIST OF WORDS
            return new PyInteger((int) NameValidationResults.Valid);
        }

        public PyDataType CreateCharacter2(
            PyString characterName, PyInteger bloodlineID, PyInteger genderID, PyInteger ancestryID,
            PyDictionary appearance, PyDictionary namedPayload, Client client)
        {
            int validationError = this.ValidateNameEx(characterName, null, client);

            // ensure the name is valid
            switch (validationError)
            {
                case (int) NameValidationResults.TooLong:
                    throw new UserError("CharNameInvalidMaxLength");
                case (int) NameValidationResults.Taken:
                    throw new UserError("CharNameInvalidTaken");
                case (int) NameValidationResults.IllegalCharacters:
                    throw new UserError("CharNameInvalidSomeChar");
                case (int) NameValidationResults.TooShort:
                    throw new UserError("CharNameInvalidMinLength");
                case (int) NameValidationResults.MoreThanOneSpace:
                    throw new UserError("CharNameInvalidMaxSpaces");
                case (int) NameValidationResults.Banned:
                    throw new UserError("CharNameInvalidBannedWord");
                case (int) NameValidationResults.Valid:
                    break;
                default:
                    // unknown actual error, return generic error
                    throw new UserError("CharNameInvalid");
            }
            
            // get the System item's id
            int systemItemID = this.ServiceManager.Container.Constants["locationSystem"];

            // load the item into memory
            ItemEntity owner = this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem(systemItemID);
            
            // load bloodline and ancestry info for the requested character
            Bloodline bloodline = this.mBloodlineCache[bloodlineID];
            Ancestry ancestry = this.mAncestriesCache[ancestryID];
            long currentTime = DateTime.UtcNow.ToFileTimeUtc();
            
            // TODO: DETERMINE SCHOOLID, CARREERID AND CAREERSPECIALITYID PROPERLY

            if (ancestry.Bloodline != bloodline)
            {
                Log.Error($"The ancestry {ancestryID} doesn't belong to the given bloodline {bloodlineID}");
                
                throw new UserError("BannedBloodline", 
                    new PyDictionary ()
                    {
                        {"name", ancestry.Name},
                        {"bloodlineName", bloodline.Name}
                    }
                );
            }

            int stationID, solarSystemID, constellationID, regionID;

            // fetch information of starting location for the player
            this.mDB.GetLocationForCorporation(bloodline.CorporationID, out stationID, out solarSystemID,
                out constellationID, out regionID);

            Station station = this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem(stationID) as Station;
            
            int itemID = this.mDB.CreateCharacter(
                bloodline.ItemType, characterName, owner, client.AccountID, this.mConfiguration.Balance, 0.0,
                bloodline.CorporationID, 0, 0, 0, 0, 0,
                currentTime, currentTime, currentTime, ancestryID, 0, 0,
                0, genderID,
                appearance.ContainsKey("accessoryID") ? appearance["accessoryID"] as PyInteger : null,
                appearance.ContainsKey("beardID") ? appearance["beardID"] as PyInteger : null,
                appearance["costumeID"] as PyInteger,
                appearance.ContainsKey("decoID") ? appearance["decoID"] as PyInteger : null,
                appearance["eyebrowsID"] as PyInteger,
                appearance["eyesID"] as PyInteger,
                appearance["hairID"] as PyInteger,
                appearance.ContainsKey("lipstickID") ? appearance["lipstickID"] as PyInteger : null,
                appearance.ContainsKey("makeupID") ? appearance["makeupID"] as PyInteger : null,
                appearance["skinID"] as PyInteger,
                appearance["backgroundID"] as PyInteger,
                appearance["lightID"] as PyInteger,
                appearance["headRotation1"] as PyDecimal,
                appearance["headRotation2"] as PyDecimal,
                appearance["headRotation3"] as PyDecimal,
                appearance["eyeRotation1"] as PyDecimal,
                appearance["eyeRotation2"] as PyDecimal,
                appearance["eyeRotation3"] as PyDecimal,
                appearance["camPos1"] as PyDecimal,
                appearance["camPos2"] as PyDecimal,
                appearance["camPos3"] as PyDecimal,
                appearance.ContainsKey("morph1e") ? appearance["morph1e"] as PyDecimal : null,
                appearance.ContainsKey("morph1n") ? appearance["morph1n"] as PyDecimal : null,
                appearance.ContainsKey("morph1s") ? appearance["morph1s"] as PyDecimal : null,
                appearance.ContainsKey("morph1w") ? appearance["morph1w"] as PyDecimal : null,
                appearance.ContainsKey("morph2e") ? appearance["morph2e"] as PyDecimal : null,
                appearance.ContainsKey("morph2n") ? appearance["morph2n"] as PyDecimal : null,
                appearance.ContainsKey("morph2s") ? appearance["morph2s"] as PyDecimal : null,
                appearance.ContainsKey("morph2w") ? appearance["morph2w"] as PyDecimal : null,
                appearance.ContainsKey("morph3e") ? appearance["morph3e"] as PyDecimal : null,
                appearance.ContainsKey("morph3n") ? appearance["morph3n"] as PyDecimal : null,
                appearance.ContainsKey("morph3s") ? appearance["morph3s"] as PyDecimal : null,
                appearance.ContainsKey("morph3w") ? appearance["morph3w"] as PyDecimal : null,
                appearance.ContainsKey("morph4e") ? appearance["morph4e"] as PyDecimal : null,
                appearance.ContainsKey("morph4n") ? appearance["morph4n"] as PyDecimal : null,
                appearance.ContainsKey("morph4s") ? appearance["morph4s"] as PyDecimal : null,
                appearance.ContainsKey("morph4w") ? appearance["morph4w"] as PyDecimal : null,
                stationID, solarSystemID, constellationID, regionID);

            Character character = this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem(itemID) as Character;

            // change character attributes based on the picked ancestry
            character.Charisma += ancestry.Charisma;
            character.Intelligence += ancestry.Intelligence;
            character.Memory += ancestry.Memory;
            character.Willpower += ancestry.Willpower;
            character.Perception += ancestry.Perception;
            
            // get skills by race and create them
            Dictionary<int, int> skills = this.mDB.GetBasicSkillsByRace(bloodline.RaceID);

            foreach (KeyValuePair<int, int> pair in skills)
            {
                ItemType skillType = this.ServiceManager.Container.ItemFactory.TypeManager[pair.Key];
                    
                // create the skill at the required level
                this.ServiceManager.Container.ItemFactory.ItemManager.CreateSkill(skillType, character, pair.Value);
            }
            
            // create the ship for the character
            Ship ship = this.ServiceManager.Container.ItemFactory.ItemManager.CreateShip(bloodline.ShipType, station,
                character);
            
            // add one unit of Tritanium to the station's hangar for the player
            ItemType tritaniumType = this.ServiceManager.Container.ItemFactory.TypeManager[ItemTypes.Tritanium];

            ItemEntity tritanium =
                this.ServiceManager.Container.ItemFactory.ItemManager.CreateSimpleItem(tritaniumType, character,
                    station, ItemFlags.Hangar);
            
            // add one unit of Damage Control I to the station's hangar for the player
            ItemType damageControlType = this.ServiceManager.Container.ItemFactory.TypeManager[ItemTypes.DamageControlI];

            ItemEntity damageControl =
                this.ServiceManager.Container.ItemFactory.ItemManager.CreateSimpleItem(damageControlType, character,
                    station, ItemFlags.Hangar);
            
            // create an alpha clone
            ItemType cloneType = this.ServiceManager.Container.ItemFactory.TypeManager[ItemTypes.CloneGradeAlpha];
            
            ItemEntity clone =
                this.ServiceManager.Container.ItemFactory.ItemManager.CreateSimpleItem(cloneType, character, station,
                    ItemFlags.None);

            // character is 100% created and the base items are too
            // finally return the new character's ID and wait for the subsequent calls from the EVE client :)
            
            return character.ID;
        }

        public PyDataType Ping(PyDictionary namedPayload, Client client)
        {
            return client.AccountID;
        }
    }
}