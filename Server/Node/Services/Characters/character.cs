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
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Network;
using PythonTypes.Types.Primitives;
using SimpleInjector;

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

        private CharacterDB DB { get; }
        private ChatDB ChatDB { get; }
        private ItemManager ItemManager { get; }
        private TypeManager TypeManager { get; }
        private CacheStorage CacheStorage { get; }
        private NodeContainer Container { get; }
        private readonly Dictionary<int, Bloodline> mBloodlineCache = null;
        private readonly Dictionary<int, Ancestry> mAncestriesCache = null;
        private readonly Configuration.Character mConfiguration = null;
        private readonly Channel Log = null;

        public character(CacheStorage cacheStorage, CharacterDB db, ChatDB chatDB, ItemManager itemManager, TypeManager typeManager, Logger logger, Configuration.Character configuration, NodeContainer container)
        {
            this.Log = logger.CreateLogChannel("character");
            this.mConfiguration = configuration;
            this.DB = db;
            this.ChatDB = chatDB;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
            this.CacheStorage = cacheStorage;
            this.Container = container;
            this.mBloodlineCache = this.DB.GetBloodlineInformation();
            this.mAncestriesCache = this.DB.GetAncestryInformation(this.mBloodlineCache);
        }

        public PyDataType GetCharactersToSelect(PyDictionary namedPayload, Client client)
        {
            return this.DB.GetCharacterList(client.AccountID);
        }

        public PyDataType LogStartOfCharacterCreation(PyDictionary namedPayload, Client client)
        {
            return null;
        }

        public PyDataType GetCharCreationInfo(PyDictionary namedPayload, Client client)
        {
            return this.CacheStorage.GetHints(CacheStorage.CreateCharacterCacheTable);
        }

        public PyDataType GetAppearanceInfo(PyDictionary namedPayload, Client client)
        {
            return this.CacheStorage.GetHints(CacheStorage.CharacterAppearanceCacheTable);
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
            if (this.DB.IsCharacterNameTaken(characterName) == true)
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
            int systemItemID = this.Container.Constants["locationSystem"];

            // load the item into memory
            ItemEntity owner = this.ItemManager.LoadItem(systemItemID);
            
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

            int stationID, solarSystemID, constellationID, regionID, corporationID, careerID, schoolID, careerSpecialityID;

            bool found = this.DB.GetRandomCareerForRace(bloodline.RaceID, out careerID, out schoolID,
                out careerSpecialityID, out corporationID);

            if (found == false)
            {
                this.Log.Error($"Cannot find random career for race {bloodline.RaceID}");
                
                throw new CustomError($"Cannot find random career for race {bloodline.RaceID}");
            }

            // fetch information of starting location for the player
            found = this.DB.GetLocationForCorporation(corporationID, out stationID, out solarSystemID,
                out constellationID, out regionID);

            if (found == false)
            {
                this.Log.Error($"Cannot find location for corporation {bloodline.CorporationID}");
                
                throw new CustomError($"Cannot find location for corporation {bloodline.CorporationID}");
            }

            Station station = this.ItemManager.LoadItem(stationID) as Station;
            
            int itemID = this.DB.CreateCharacter(
                bloodline.ItemType, characterName, owner, client.AccountID, this.mConfiguration.Balance, 0.0,
                corporationID, 0, 0, 0, 0, 0,
                currentTime, currentTime, currentTime, ancestryID,
                careerID, schoolID, careerSpecialityID, genderID,
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

            Character character = this.ItemManager.LoadItem(itemID) as Character;

            // change character attributes based on the picked ancestry
            character.Charisma = bloodline.Charisma + ancestry.Charisma;
            character.Intelligence = bloodline.Intelligence + ancestry.Intelligence;
            character.Memory = bloodline.Memory + ancestry.Memory;
            character.Willpower = bloodline.Willpower + ancestry.Willpower;
            character.Perception = bloodline.Perception + ancestry.Perception;
            
            // get skills by race and create them
            Dictionary<int, int> skills = this.DB.GetBasicSkillsByRace(bloodline.RaceID);

            foreach (KeyValuePair<int, int> pair in skills)
            {
                ItemType skillType = this.TypeManager[pair.Key];
                    
                // create the skill at the required level
                this.ItemManager.CreateSkill(skillType, character, pair.Value);
            }
            
            // create the ship for the character
            Ship ship = this.ItemManager.CreateShip(bloodline.ShipType, station,
                character);
            
            // add one unit of Tritanium to the station's hangar for the player
            ItemType tritaniumType = this.TypeManager[ItemTypes.Tritanium];

            ItemEntity tritanium =
                this.ItemManager.CreateSimpleItem(tritaniumType, character,
                    station, ItemFlags.Hangar);
            
            // add one unit of Damage Control I to the station's hangar for the player
            ItemType damageControlType = this.TypeManager[ItemTypes.DamageControlI];

            ItemEntity damageControl =
                this.ItemManager.CreateSimpleItem(damageControlType, character,
                    station, ItemFlags.Hangar);
            
            // create an alpha clone
            ItemType cloneType = this.TypeManager[ItemTypes.CloneGradeAlpha];
            
            Clone clone =
                this.ItemManager.CreateClone(cloneType, station, character);

            character.LocationID = ship.ID;
            character.ActiveClone = clone;
            
            // character is 100% created and the base items are too
            // persist objects to database and unload them as they do not really belong to us
            clone.Persist();
            damageControl.Persist();
            tritanium.Persist();
            ship.Persist();
            character.Persist();
            
            // create required mailing list channel
            int channelID = (int) this.ChatDB.CreateChannel(character, character, characterName, true);
            // and subscribe the character to some channels
            this.ChatDB.JoinChannel(ChatDB.CHANNEL_ROOKIECHANNELID, character.ID, ChatDB.CHATROLE_CONVERSATIONALIST);
            this.ChatDB.JoinChannel(channelID, character.ID, ChatDB.CHATROLE_CREATOR);
            this.ChatDB.JoinChannel(solarSystemID, character.ID, ChatDB.CHATROLE_CONVERSATIONALIST);
            this.ChatDB.JoinChannel(constellationID, character.ID, ChatDB.CHATROLE_CONVERSATIONALIST);
            this.ChatDB.JoinChannel(regionID, character.ID, ChatDB.CHATROLE_CONVERSATIONALIST);
            this.ChatDB.JoinChannel(character.CorporationID, character.ID, ChatDB.CHATROLE_CONVERSATIONALIST);
            
            // unload items from list
            this.ItemManager.UnloadItem(clone);
            this.ItemManager.UnloadItem(damageControl);
            this.ItemManager.UnloadItem(tritanium);
            this.ItemManager.UnloadItem(ship);
            this.ItemManager.UnloadItem(character);
            
            // finally return the new character's ID and wait for the subsequent calls from the EVE client :)
            
            return character.ID;
        }

        public PyDataType GetCharacterToSelect(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            return this.DB.GetCharacterSelectionInfo(characterID, client.AccountID);
        }

        public PyDataType SelectCharacterID(PyInteger characterID, PyBool loadDungeon, PyNone secondChoiceID,
            PyDictionary namedPayload, Client client)
        {
            return this.SelectCharacterID(characterID, loadDungeon == true ? 1 : 0, secondChoiceID, namedPayload, client);
        }
        // TODO: THIS PyNone SHOULD REALLY BE AN INTEGER, ALTHOUGH THIS FUNCTIONALITY IS NOT USED
        // TODO: IT REVEALS AN IMPORTANT ISSUE, WE CAN'T HAVE A WILDCARD PARAMETER PyDataType
        public PyDataType SelectCharacterID(PyInteger characterID, PyInteger loadDungeon, PyNone secondChoiceID,
            PyDictionary namedPayload, Client client)
        {
            // ensure the character belongs to the current account
            Character character =
                this.ItemManager.LoadItem(characterID) as Character;

            if (character.AccountID != client.AccountID)
                throw new CustomError("The selected character does not belong to this account, aborting...");
            
            // update the session data for this client
            client.CharacterID = character.ID;
            client.CorporationID = character.CorporationID;

            if (character.StationID == 0)
            {
                client.SolarSystemID = character.SolarSystemID;
            }
            else
            {
                client.StationID = character.StationID;
            }

            client.SolarSystemID2 = character.SolarSystemID;
            client.ConstellationID = character.ConstellationID;
            client.RegionID = character.RegionID;
            client.HQID = 0;
            client.CorporationRole = character.CorpRole;
            client.RolesAtAll = character.RolesAtAll;
            client.RolesAtBase = character.RolesAtBase;
            client.RolesAtHQ = character.RolesAtHq;
            client.RolesAtOther = character.RolesAtOther;
            client.ShipID = character.LocationID;
            
            // TODO: CHECK WHAT NODE HAS THE SOLAR SYSTEM LOADED AND PROPERLY LET THE CLIENT KNOW
            client.SendSessionChange();
            
            // update the character and set it's only flag to true
            character.Online = 1;
            // the online status must be persisted after update, so force the entity to be updated in the database
            character.Persist();
            
            // TODO: SEND CHARACTER CONNECTION NOTIFICATION
            // TODO: send "OnContactLoggedOn" to all the friends in the list as long as they're online

            return null;
        }

        public PyDataType Ping(PyDictionary namedPayload, Client client)
        {
            return client.AccountID;
        }

        public PyDataType GetOwnerNoteLabels(PyDictionary namedPayload, Client client)
        {
            Character character =
                this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            return this.DB.GetOwnerNoteLabels(character);
        }

        public PyDataType GetCloneTypeID(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character =
                this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            if (character.ActiveCloneID == null)
                throw new CustomError("You do not have any medical clone...");

            return character.ActiveClone.Type.ID;
        }

        public PyDataType GetHomeStation(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character =
                this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            if (character.ActiveCloneID == null)
                throw new CustomError("You do not have any medical clone...");

            return character.ActiveClone.LocationID;
        }

        public PyDataType GetCharacterDescription(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character =
                this.ItemManager.LoadItem((int) client.CharacterID) as Character;
            
            return character.Description;
        }

        public PyDataType SetCharacterDescription(PyString newBio, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character =
                this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            character.Description = newBio;
            character.Persist();
            
            return null;
        }

        public PyDataType GetRecentShipKillsAndLosses(PyInteger count, PyInteger startIndex, PyDictionary namedPayload,
            Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            // limit number of records to 100 at maximum
            if (count > 100)
                count = 100;
            
            return this.DB.GetRecentShipKillsAndLosses((int) client.CharacterID, count, startIndex);
        }
    }
}