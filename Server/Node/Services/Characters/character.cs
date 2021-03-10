/*
    ------------------------------------------------------------------------------------
    LICENSE:
    ------------------------------------------------------------------------------------
    This file is part of EVE#: The EVE Online Server Emulator
    Copyright 2021 - EVE# Team
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
using System.Text.RegularExpressions;
using Common.Logging;
using Common.Services;
using Node.Data;
using Node.Database;
using Node.Exceptions.character;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Database;
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

        public PyDataType GetCharactersToSelect(CallInformation call)
        {
            return this.DB.GetCharacterList(call.Client.AccountID);
        }

        public PyDataType LogStartOfCharacterCreation(CallInformation call)
        {
            return null;
        }

        public PyDataType GetCharCreationInfo(CallInformation call)
        {
            return this.CacheStorage.GetHints(CacheStorage.CreateCharacterCacheTable);
        }

        public PyDataType GetAppearanceInfo(CallInformation call)
        {
            return this.CacheStorage.GetHints(CacheStorage.CharacterAppearanceCacheTable);
        }

        public PyDataType GetCharNewExtraCreationInfo(CallInformation call)
        {
            return new PyDictionary();
        }

        public PyInteger ValidateNameEx(PyString name, CallInformation call)
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

        private void GetRandomCareerForRace(int raceID, out int careerID, out int schoolID, out int careerSpecialityID,
            out int corporationID)
        {
            // TODO: DETERMINE SCHOOLID, CARREERID AND CAREERSPECIALITYID PROPERLY
            
            bool found = this.DB.GetRandomCareerForRace(raceID, out careerID, out schoolID,
                out careerSpecialityID, out corporationID);

            if (found == true)
                return;
            
            Log.Error($"Cannot find random career for race {raceID}");
                
            throw new CustomError($"Cannot find random career for race {raceID}");
        }

        private void GetLocationForCorporation(int corporationID, out int stationID, out int solarSystemID,
            out int constellationID, out int regionID)
        {
            // fetch information of starting location for the player
            bool found = this.DB.GetLocationForCorporation(corporationID, out stationID, out solarSystemID,
                out constellationID, out regionID);

            if (found == true)
                return;
            
            Log.Error($"Cannot find location for corporation {corporationID}");
                
            throw new CustomError($"Cannot find location for corporation {corporationID}");
        }

        private Character CreateCharacter(string characterName, Ancestry ancestry, int genderID, PyDictionary appearance, long currentTime, CallInformation call)
        {
            // load the item into memory
            ItemEntity owner = this.ItemManager.LocationSystem;
            
            int stationID, solarSystemID, constellationID, regionID, corporationID, careerID, schoolID, careerSpecialityID;
            
            this.GetRandomCareerForRace(ancestry.Bloodline.RaceID, out careerID, out schoolID, out careerSpecialityID, out corporationID);
            this.GetLocationForCorporation(corporationID, out stationID, out solarSystemID, out constellationID, out regionID);
            
            int itemID = this.DB.CreateCharacter(
                ancestry.Bloodline.ItemType, characterName, owner, call.Client.AccountID, this.mConfiguration.Balance,
                0.0, corporationID, 0, 0, 0, 0, 0,
                currentTime, currentTime, currentTime, ancestry.ID,
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

            return this.ItemManager.LoadItem(itemID) as Character;
        }

        public PyDataType CreateCharacter2(
            PyString characterName, PyInteger bloodlineID, PyInteger genderID, PyInteger ancestryID,
            PyDictionary appearance, CallInformation call)
        {
            int validationError = this.ValidateNameEx(characterName, call);

            // ensure the name is valid
            switch (validationError)
            {
                case (int) NameValidationResults.TooLong: throw new CharNameInvalidMaxLength ();
                case (int) NameValidationResults.Taken: throw new CharNameInvalidTaken ();
                case (int) NameValidationResults.IllegalCharacters: throw new CharNameInvalidSomeChar ();
                case (int) NameValidationResults.TooShort: throw new CharNameInvalidMinLength ();
                case (int) NameValidationResults.MoreThanOneSpace: throw new CharNameInvalidMaxSpaces ();
                case (int) NameValidationResults.Banned: throw new CharNameInvalidBannedWord ();
                case (int) NameValidationResults.Valid: break;
                // unknown actual error, return generic error
                default: throw new CharNameInvalid();
            }
            
            // load bloodline and ancestry info for the requested character
            Bloodline bloodline = this.mBloodlineCache[bloodlineID];
            Ancestry ancestry = this.mAncestriesCache[ancestryID];
            long currentTime = DateTime.UtcNow.ToFileTimeUtc();

            if (ancestry.Bloodline != bloodline)
            {
                Log.Error($"The ancestry {ancestryID} doesn't belong to the given bloodline {bloodlineID}");

                throw new BannedBloodline(ancestry, bloodline);
            }

            Character character =
                this.CreateCharacter(characterName, ancestry, genderID, appearance, currentTime, call);
            Station station = this.ItemManager.GetStation(character.StationID);

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
            
            Clone clone = this.ItemManager.CreateClone(cloneType, station, character);

            character.LocationID = ship.ID;
            character.ActiveClone = clone;
            
            // character is 100% created and the base items are too
            // persist objects to database and unload them as they do not really belong to us
            clone.Persist();
            damageControl.Persist();
            tritanium.Persist();
            ship.Persist();
            character.Persist();
            
            // join the character to all the general channels
            this.ChatDB.GrantAccessToStandardChannels(character.ID);
            // create required mailing list channel
            this.ChatDB.CreateChannel(character, character, characterName, true);
            // and subscribe the character to some channels
            this.ChatDB.JoinEntityMailingList(character.ID, character.ID);
            this.ChatDB.JoinEntityChannel(character.SolarSystemID, character.ID);
            this.ChatDB.JoinEntityChannel(character.ConstellationID, character.ID);
            this.ChatDB.JoinEntityChannel(character.RegionID, character.ID);
            this.ChatDB.JoinEntityChannel(character.CorporationID, character.ID);
            this.ChatDB.JoinEntityMailingList(character.CorporationID, character.ID);
            
            // unload items from list
            this.ItemManager.UnloadItem(clone);
            this.ItemManager.UnloadItem(damageControl);
            this.ItemManager.UnloadItem(tritanium);
            this.ItemManager.UnloadItem(ship);
            this.ItemManager.UnloadItem(character);
            
            // finally return the new character's ID and wait for the subsequent calls from the EVE client :)
            
            return character.ID;
        }

        public PyDataType GetCharacterToSelect(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetCharacterSelectionInfo(characterID, call.Client.AccountID);
        }

        public PyDataType SelectCharacterID(PyInteger characterID, PyBool loadDungeon, PyNone secondChoiceID,
            CallInformation call)
        {
            return this.SelectCharacterID(characterID, loadDungeon == true ? 1 : 0, secondChoiceID, call);
        }
        // TODO: THIS PyNone SHOULD REALLY BE AN INTEGER, ALTHOUGH THIS FUNCTIONALITY IS NOT USED
        // TODO: IT REVEALS AN IMPORTANT ISSUE, WE CAN'T HAVE A WILDCARD PARAMETER PyDataType
        public PyDataType SelectCharacterID(PyInteger characterID, PyInteger loadDungeon, PyNone secondChoiceID,
            CallInformation call)
        {
            // ensure the character belongs to the current account
            Character character = this.ItemManager.LoadItem(characterID) as Character;

            if (character.AccountID != call.Client.AccountID)
            {
                // unload character
                this.ItemManager.UnloadItem(character);
                
                // throw proper error
                throw new CustomError("The selected character does not belong to this account, aborting...");                
            }

            // update the session data for this client
            call.Client.CharacterID = character.ID;
            call.Client.CorporationID = character.CorporationID;

            if (character.StationID == 0)
            {
                call.Client.SolarSystemID = character.SolarSystemID;
            }
            else
            {
                call.Client.StationID = character.StationID;
            }

            call.Client.SolarSystemID2 = character.SolarSystemID;
            call.Client.ConstellationID = character.ConstellationID;
            call.Client.RegionID = character.RegionID;
            call.Client.HQID = 0;
            call.Client.CorporationRole = character.CorpRole;
            call.Client.RolesAtAll = character.RolesAtAll;
            call.Client.RolesAtBase = character.RolesAtBase;
            call.Client.RolesAtHQ = character.RolesAtHq;
            call.Client.RolesAtOther = character.RolesAtOther;
            call.Client.ShipID = character.LocationID;

            // update the character and set it's only flag to true
            character.Online = 1;
            // the online status must be persisted after update, so force the entity to be updated in the database
            character.Persist();
            
            // unload the character, let the session change handler handle everything
            // TODO: CHECK IF THE PLAYER IS GOING TO SPAWN IN THIS NODE AND IF IT IS NOT, UNLOAD IT FROM THE ITEM MANAGER
            List<int> onlineFriends = this.DB.GetOnlineFriendList(character);

            foreach (int friendID in onlineFriends)
                call.Client.ClusterConnection.SendNotification("OnContactLoggedOn", "charid", friendID, new PyTuple(1) { [0] = character.ID });

            // unload the character
            this.ItemManager.UnloadItem(characterID);
            
            // finally send the session change
            call.Client.SendSessionChange();
            
            return null;
        }

        public PyDataType Ping(CallInformation call)
        {
            return call.Client.AccountID;
        }

        public PyDataType GetOwnerNoteLabels(CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            return this.DB.GetOwnerNoteLabels(character);
        }

        public PyDataType GetCloneTypeID(CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            if (character.ActiveCloneID == null)
                throw new CustomError("You do not have any medical clone...");

            return character.ActiveClone.Type.ID;
        }

        public PyDataType GetHomeStation(CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            if (character.ActiveCloneID == null)
                throw new CustomError("You do not have any medical clone...");

            return character.ActiveClone.LocationID;
        }

        public PyDataType GetCharacterDescription(PyInteger characterID, CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;
            
            return character.Description;
        }

        public PyDataType SetCharacterDescription(PyString newBio, CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            character.Description = newBio;
            character.Persist();
            
            return null;
        }

        public PyDataType GetRecentShipKillsAndLosses(PyInteger count, PyInteger startIndex, CallInformation call)
        {
            // limit number of records to 100 at maximum
            if (count > 100)
                count = 100;
            
            return this.DB.GetRecentShipKillsAndLosses(call.Client.EnsureCharacterIsSelected(), count, startIndex);
        }

        public PyDataType GetCharacterAppearanceList(PyList ids, CallInformation call)
        {
            PyList result = new PyList(ids.Count);

            int index = 0;
            
            foreach (PyDataType id in ids)
            {
                // ignore non-integers
                if (id is PyInteger == false)
                    continue;

                Rowset dbResult = this.DB.GetCharacterAppearanceInfo(id as PyInteger);

                if (dbResult.Rows.Count != 0)
                    result[index] = dbResult;

                index++;
            }

            return result;
        }

        public PyDataType GetNote(PyInteger characterID, CallInformation call)
        {
            return this.DB.GetNote(characterID, call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType SetNote(PyInteger characterID, PyString note, CallInformation call)
        {
            this.DB.SetNote(characterID, call.Client.EnsureCharacterIsSelected(), note);

            return null;
        }
    }
}