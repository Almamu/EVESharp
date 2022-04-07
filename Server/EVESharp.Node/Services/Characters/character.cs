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
using EVESharp.EVE;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.character;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Market;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Chat;
using EVESharp.Node.Sessions;
using EVESharp.Node.StaticData;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Serilog;
using SessionManager = EVESharp.Node.Sessions.SessionManager;

namespace EVESharp.Node.Services.Characters;

public class character : Service
{
    public override AccessLevel AccessLevel => AccessLevel.LocationPreferred;
    enum NameValidationResults
    {
        Valid             = 1,
        TooShort          = -1,
        TooLong           = -2,
        IllegalCharacters = -5,
        MoreThanOneSpace  = -6,
        Taken             = -101,
        Banned            = -102
    }

    private          CharacterDB             DB                  { get; }
    private          CorporationDB           CorporationDB       { get; init; }
    private          ChatDB                  ChatDB              { get; }
    private          ItemFactory             ItemFactory         { get; }
    private          TypeManager             TypeManager         => this.ItemFactory.TypeManager;
    private          CacheStorage            CacheStorage        { get; }
    private          NodeContainer           Container           { get; }
    private          NotificationManager     NotificationManager { get; }
    private          WalletManager           WalletManager       { get; init; }
    private          AncestryManager         AncestryManager     { get; init; }
    private          SessionManager          SessionManager      { get; }
    private readonly Configuration.Character mConfiguration = null;
    private          ILogger                 Log { get; }

    public character(CacheStorage        cacheStorage,        CharacterDB   db,            ChatDB                  chatDB,        CorporationDB corporationDB,
                     ItemFactory         itemFactory,         ILogger       logger,        Configuration.Character configuration, NodeContainer container,
                     NotificationManager notificationManager, WalletManager walletManager, AncestryManager         ancestryManager,
                     SessionManager      sessionManager)
    {
        this.Log                 = logger;
        this.mConfiguration      = configuration;
        this.DB                  = db;
        this.ChatDB              = chatDB;
        this.CorporationDB       = corporationDB;
        this.ItemFactory         = itemFactory;
        this.CacheStorage        = cacheStorage;
        this.Container           = container;
        this.NotificationManager = notificationManager;
        this.WalletManager       = walletManager;
        this.AncestryManager     = ancestryManager;
        this.SessionManager      = sessionManager;
    }

    public PyDataType GetCharactersToSelect(CallInformation call)
    {
        return this.DB.GetCharacterList(call.Session.UserID);
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
            return (int) NameValidationResults.TooShort;

        // character name length is maximum 24 characters based on the error messages used for the user
        if (characterName.Length > 24)
            return (int) NameValidationResults.TooLong;

        // ensure only alphanumeric characters and/or spaces are used
        if (Regex.IsMatch(characterName, "^[a-zA-Z0-9 ]*$") == false)
            return (int) NameValidationResults.IllegalCharacters;

        // no more than one space allowed
        if (characterName.IndexOf(' ') != characterName.LastIndexOf(' '))
            return (int) NameValidationResults.MoreThanOneSpace;

        // ensure there is no character registered with this name already
        if (this.DB.IsCharacterNameTaken(characterName) == true)
            return (int) NameValidationResults.Taken;

        // TODO: IMPLEMENT BANLIST OF WORDS
        return (int) NameValidationResults.Valid;
    }

    private void GetRandomCareerForRace(int     raceID, out int careerID, out int schoolID, out int careerSpecialityID,
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

    private void GetLocationForCorporation(int     corporationID,   out int stationID, out int solarSystemID,
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

    private void ExtractExtraCharacterAppearance(PyDictionary  data,    out PyInteger accessoryID,
                                                 out PyInteger beardID, out PyInteger decoID,  out PyInteger lipstickID, out PyInteger makeupID,
                                                 out PyDecimal morph1e, out PyDecimal morph1n, out PyDecimal morph1s,    out PyDecimal morph1w,
                                                 out PyDecimal morph2e, out PyDecimal morph2n, out PyDecimal morph2s,    out PyDecimal morph2w,
                                                 out PyDecimal morph3e, out PyDecimal morph3n, out PyDecimal morph3s,    out PyDecimal morph3w,
                                                 out PyDecimal morph4e, out PyDecimal morph4n, out PyDecimal morph4s,    out PyDecimal morph4w)
    {
        data.TryGetValue("accessoryID", out accessoryID);
        data.TryGetValue("beardID",     out beardID);
        data.TryGetValue("decoID",      out decoID);
        data.TryGetValue("lipstickID",  out lipstickID);
        data.TryGetValue("makeupID",    out makeupID);
        data.TryGetValue("morph1e",     out morph1e);
        data.TryGetValue("morph1n",     out morph1n);
        data.TryGetValue("morph1s",     out morph1s);
        data.TryGetValue("morph1w",     out morph1w);
        data.TryGetValue("morph2e",     out morph2e);
        data.TryGetValue("morph2n",     out morph2n);
        data.TryGetValue("morph2s",     out morph2s);
        data.TryGetValue("morph2w",     out morph2w);
        data.TryGetValue("morph3e",     out morph3e);
        data.TryGetValue("morph3n",     out morph3n);
        data.TryGetValue("morph3s",     out morph3s);
        data.TryGetValue("morph3w",     out morph3w);
        data.TryGetValue("morph4e",     out morph4e);
        data.TryGetValue("morph4n",     out morph4n);
        data.TryGetValue("morph4s",     out morph4s);
        data.TryGetValue("morph4w",     out morph4w);
    }

    private void ExtractCharacterAppearance(PyDictionary  data,          out PyInteger costumeID,     out PyInteger eyebrowsID,
                                            out PyInteger eyesID,        out PyInteger hairID,        out PyInteger skinID, out PyInteger backgroundID,
                                            out PyInteger lightID,       out PyDecimal headRotation1, out PyDecimal headRotation2,
                                            out PyDecimal headRotation3, out PyDecimal eyeRotation1,  out PyDecimal eyeRotation2,
                                            out PyDecimal eyeRotation3,  out PyDecimal camPos1,       out PyDecimal camPos2, out PyDecimal camPos3)
    {
        data.SafeGetValue("costumeID",     out costumeID);
        data.SafeGetValue("eyebrowsID",    out eyebrowsID);
        data.SafeGetValue("eyesID",        out eyesID);
        data.SafeGetValue("hairID",        out hairID);
        data.SafeGetValue("skinID",        out skinID);
        data.SafeGetValue("backgroundID",  out backgroundID);
        data.SafeGetValue("lightID",       out lightID);
        data.SafeGetValue("headRotation1", out headRotation1);
        data.SafeGetValue("headRotation2", out headRotation2);
        data.SafeGetValue("headRotation3", out headRotation3);
        data.SafeGetValue("eyeRotation1",  out eyeRotation1);
        data.SafeGetValue("eyeRotation2",  out eyeRotation2);
        data.SafeGetValue("eyeRotation3",  out eyeRotation3);
        data.SafeGetValue("camPos1",       out camPos1);
        data.SafeGetValue("camPos2",       out camPos2);
        data.SafeGetValue("camPos3",       out camPos3);
    }
        
    private Character CreateCharacter(string characterName, Ancestry ancestry, int genderID, PyDictionary appearance, long currentTime, CallInformation call)
    {
        // load the item into memory
        ItemEntity owner = this.ItemFactory.LocationSystem;
            
        this.GetRandomCareerForRace(ancestry.Bloodline.RaceID, out int careerID, out int schoolID, out int careerSpecialityID, out int corporationID);
        this.GetLocationForCorporation(corporationID, out int stationID, out int solarSystemID, out int constellationID, out int regionID);
        this.ExtractCharacterAppearance(appearance, out PyInteger costumeID, out PyInteger eyebrowsID,
                                        out PyInteger eyesID, out PyInteger hairID, out PyInteger skinID, out PyInteger backgroundID,
                                        out PyInteger lightID, out PyDecimal headRotation1, out PyDecimal headRotation2,
                                        out PyDecimal headRotation3, out PyDecimal eyeRotation1, out PyDecimal eyeRotation2,
                                        out PyDecimal eyeRotation3, out PyDecimal camPos1, out PyDecimal camPos2, out PyDecimal camPos3
        );
        this.ExtractExtraCharacterAppearance(appearance, out PyInteger accessoryID, out PyInteger beardID,
                                             out PyInteger decoID, out PyInteger lipstickID, out PyInteger makeupID, out PyDecimal morph1e,
                                             out PyDecimal morph1n, out PyDecimal morph1s, out PyDecimal morph1w, out PyDecimal morph2e,
                                             out PyDecimal morph2n, out PyDecimal morph2s, out PyDecimal morph2w, out PyDecimal morph3e,
                                             out PyDecimal morph3n, out PyDecimal morph3s, out PyDecimal morph3w, out PyDecimal morph4e,
                                             out PyDecimal morph4n, out PyDecimal morph4s, out PyDecimal morph4w
        );
            
        int itemID = this.DB.CreateCharacter(
            ancestry.Bloodline.CharacterType, characterName, owner, call.Session.UserID,
            0.0, corporationID, 0, 0, 0, 0,
            currentTime, currentTime, currentTime, ancestry.ID,
            careerID, schoolID, careerSpecialityID, genderID,
            accessoryID, beardID, costumeID, decoID, eyebrowsID, eyesID, hairID, lipstickID,
            makeupID, skinID, backgroundID, lightID, headRotation1, headRotation2, headRotation3,
            eyeRotation1, eyeRotation2, eyeRotation3, camPos1, camPos2, camPos3,
            morph1e, morph1n, morph1s, morph1w, morph2e, morph2n, morph2s, morph2w,
            morph3e, morph3n, morph3s, morph3w, morph4e, morph4n, morph4s, morph4w,
            stationID, solarSystemID, constellationID, regionID
        );
            
        // create the wallet for the player
        this.WalletManager.CreateWallet(itemID, WalletKeys.MAIN_WALLET, this.mConfiguration.Balance);

        return this.ItemFactory.LoadItem(itemID) as Character;
    }

    public PyDataType CreateCharacter2(
        PyString     characterName, PyInteger       bloodlineID, PyInteger genderID, PyInteger ancestryID,
        PyDictionary appearance,    CallInformation call)
    {
        int validationError = this.ValidateNameEx(characterName, call);

        // ensure the name is valid
        switch (validationError)
        {
            case (int) NameValidationResults.TooLong:           throw new CharNameInvalidMaxLength ();
            case (int) NameValidationResults.Taken:             throw new CharNameInvalidTaken ();
            case (int) NameValidationResults.IllegalCharacters: throw new CharNameInvalidSomeChar ();
            case (int) NameValidationResults.TooShort:          throw new CharNameInvalidMinLength ();
            case (int) NameValidationResults.MoreThanOneSpace:  throw new CharNameInvalidMaxSpaces ();
            case (int) NameValidationResults.Banned:            throw new CharNameInvalidBannedWord ();
            case (int) NameValidationResults.Valid:             break;
            // unknown actual error, return generic error
            default: throw new CharNameInvalid();
        }
            
        // load bloodline and ancestry info for the requested character
        Ancestry  ancestry  = this.AncestryManager[ancestryID];
        Bloodline bloodline = this.AncestryManager.Bloodlines[bloodlineID];
            
        long currentTime = DateTime.UtcNow.ToFileTimeUtc();
            
        if (ancestry.Bloodline.ID != bloodlineID)
        {
            Log.Error($"The ancestry {ancestryID} doesn't belong to the given bloodline {bloodlineID}");

            throw new BannedBloodline(ancestry, bloodline);
        }

        Character character =
            this.CreateCharacter(characterName, ancestry, genderID, appearance, currentTime, call);
        Station station = this.ItemFactory.GetStaticStation(character.StationID);

        // TODO: CREATE DEFAULT STANDINGS FOR THE CHARACTER
        // change character attributes based on the picked ancestry
        character.Charisma     = bloodline.Charisma + ancestry.Charisma;
        character.Intelligence = bloodline.Intelligence + ancestry.Intelligence;
        character.Memory       = bloodline.Memory + ancestry.Memory;
        character.Willpower    = bloodline.Willpower + ancestry.Willpower;
        character.Perception   = bloodline.Perception + ancestry.Perception;
            
        // get skills by race and create them
        Dictionary<int, int> skills = this.DB.GetBasicSkillsByRace(bloodline.RaceID);

        foreach ((int skillTypeID, int level) in skills)
        {
            StaticData.Inventory.Type skillType = this.TypeManager[skillTypeID];
                    
            // create the skill at the required level
            this.ItemFactory.CreateSkill(skillType, character, level);
        }
            
        // create the ship for the character
        Ship ship = this.ItemFactory.CreateShip(bloodline.ShipType, station,
                                                character);
            
        // add one unit of Tritanium to the station's hangar for the player
        StaticData.Inventory.Type tritaniumType = this.TypeManager[Types.Tritanium];

        ItemEntity tritanium =
            this.ItemFactory.CreateSimpleItem(tritaniumType, character,
                                              station, Flags.Hangar);
            
        // add one unit of Damage Control I to the station's hangar for the player
        StaticData.Inventory.Type damageControlType = this.TypeManager[Types.DamageControlI];

        ItemEntity damageControl =
            this.ItemFactory.CreateSimpleItem(damageControlType, character,
                                              station, Flags.Hangar);
            
        // create an alpha clone
        StaticData.Inventory.Type cloneType = this.TypeManager[Types.CloneGradeAlpha];
            
        Clone clone = this.ItemFactory.CreateClone(cloneType, station, character);

        character.LocationID    = ship.ID;
        character.ActiveCloneID = clone.ID;

        // get the wallet for the player and give the money specified in the configuration
        using Wallet wallet = this.WalletManager.AcquireWallet(character.ID, WalletKeys.MAIN_WALLET);
        {
            wallet.CreateJournalRecord(MarketReference.Inheritance, null, null, this.mConfiguration.Balance);
        }
            
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
        this.ChatDB.JoinEntityChannel(character.SolarSystemID,   character.ID);
        this.ChatDB.JoinEntityChannel(character.ConstellationID, character.ID);
        this.ChatDB.JoinEntityChannel(character.RegionID,        character.ID);
        this.ChatDB.JoinEntityChannel(character.CorporationID,   character.ID);
        this.ChatDB.JoinEntityMailingList(character.CorporationID, character.ID);
            
        // unload items from list
        this.ItemFactory.UnloadItem(clone);
        this.ItemFactory.UnloadItem(damageControl);
        this.ItemFactory.UnloadItem(tritanium);
        this.ItemFactory.UnloadItem(ship);
        this.ItemFactory.UnloadItem(character);
            
        // finally return the new character's ID and wait for the subsequent calls from the EVE client :)
        return character.ID;
    }

    public PyDataType GetCharacterToSelect(PyInteger characterID, CallInformation call)
    {
        return this.DB.GetCharacterSelectionInfo(characterID, call.Session.UserID);
    }

    public PyDataType SelectCharacterID(PyInteger       characterID, PyBool loadDungeon, PyDataType secondChoiceID,
                                        CallInformation call)
    {
        return this.SelectCharacterID(characterID, loadDungeon == true ? 1 : 0, secondChoiceID, call);
    }

    public PyDataType SelectCharacterID(PyInteger characterID, CallInformation call)
    {
        return this.SelectCharacterID(characterID, 0, 0, call);
    }
        
    // TODO: THIS PyNone SHOULD REALLY BE AN INTEGER, ALTHOUGH THIS FUNCTIONALITY IS NOT USED
    // TODO: IT REVEALS AN IMPORTANT ISSUE, WE CAN'T HAVE A WILDCARD PARAMETER PyDataType
    public PyDataType SelectCharacterID(PyInteger       characterID, PyInteger loadDungeon, PyDataType secondChoiceID,
                                        CallInformation call)
    {
        // ensure the character belongs to the current account
        Character character = this.ItemFactory.LoadItem<Character>(characterID);

        if (character.AccountID != call.Session.UserID)
        {
            // unload character
            this.ItemFactory.UnloadItem(character);
                
            // throw proper error
            throw new CustomError("The selected character does not belong to this account, aborting...");                
        }

        Session updates = new Session();

        // update the session data for this client
        updates.CharacterID   = character.ID;
        updates.CorporationID = character.CorporationID;

        if (character.StationID == 0)
        {
            updates.SolarSystemID = character.SolarSystemID;
        }
        else
        {
            updates.StationID = character.StationID;
        }
            
        // get title roles and mask them with the current roles to ensure the user has proper roles
        this.CorporationDB.GetTitleInformation(character.CorporationID, character.TitleMask,
                                               out long roles, out long rolesAtHQ, out long rolesAtBase, out long rolesAtOther,
                                               out long grantableRoles, out long grantableRolesAtHQ, out long grantableRolesAtBase,
                                               out long grantableRolesAtOther, out _
        );
            
        updates.CorporationRole = character.Roles | roles;
        updates.RolesAtAll = character.Roles | character.RolesAtBase | character.RolesAtOther | character.RolesAtHq | roles | rolesAtHQ | rolesAtBase | rolesAtOther;
        updates.RolesAtBase = character.RolesAtBase | rolesAtBase;
        updates.RolesAtHQ = character.RolesAtHq | rolesAtHQ;
        updates.RolesAtOther = character.RolesAtOther | rolesAtOther;
        updates.AllianceID = this.CorporationDB.GetAllianceIDForCorporation(character.CorporationID);

        // set the rest of the important locations
        updates.SolarSystemID2  = character.SolarSystemID;
        updates.ConstellationID = character.ConstellationID;
        updates.RegionID        = character.RegionID;
        updates.HQID            = 0; // TODO: ADD SUPPORT FOR HQID
        updates.ShipID          = character.LocationID;
        updates.RaceID          = this.AncestryManager[character.AncestryID].Bloodline.RaceID;
            
        // check if the character has any accounting roles and set the correct accountKey based on the data
        if (this.WalletManager.IsAccessAllowed(updates, character.CorpAccountKey, updates.CorporationID) == true)
            updates.CorpAccountKey = character.CorpAccountKey;

        // set the war faction id if present
        if (character.WarFactionID is not null)
            updates.WarFactionID = character.WarFactionID;

        // update the logon status
        this.DB.UpdateCharacterLogonDateTime(character.ID);
        // unload the character, let the session change handler handle everything
        // TODO: CHECK IF THE PLAYER IS GOING TO SPAWN IN THIS NODE AND IF IT IS NOT, UNLOAD IT FROM THE ITEM MANAGER
        PyList<PyInteger> onlineFriends = this.DB.GetOnlineFriendList(character);

        this.NotificationManager.NotifyCharacters(
            onlineFriends, new OnContactLoggedOn(character.ID)
        );
            
        // unload the character
        this.ItemFactory.UnloadItem(characterID);
            
        // send the session change
        this.SessionManager.PerformSessionUpdate(Session.USERID, call.Session.UserID, updates);
            
        return null;
    }

    public PyDataType Ping(CallInformation call)
    {
        return call.Session.UserID;
    }

    public PyDataType GetOwnerNoteLabels(CallInformation call)
    {
        return this.DB.GetOwnerNoteLabels(call.Session.EnsureCharacterIsSelected());
    }

    public PyDataType GetCloneTypeID(CallInformation call)
    {
        Character character = this.ItemFactory.GetItem<Character>(call.Session.EnsureCharacterIsSelected());

        if (character.ActiveCloneID is null)
            throw new CustomError("You do not have any medical clone...");

        // TODO: FETCH THIS FROM THE DATABASE INSTEAD
        // return character.ActiveClone.Type.ID;
        return (int) Types.CloneGradeAlpha;
    }

    public PyDataType GetHomeStation(CallInformation call)
    {
        Character character = this.ItemFactory.GetItem<Character>(call.Session.EnsureCharacterIsSelected());

        if (character.ActiveCloneID is null)
            throw new CustomError("You do not have any medical clone...");

        // TODO: FETCH THIS FROM THE DATABASE INSTEAD
        // return character.ActiveClone.LocationID;
        return call.Session.StationID;
    }

    public PyDataType GetCharacterDescription(PyInteger characterID, CallInformation call)
    {
        Character character = this.ItemFactory.GetItem<Character>(call.Session.EnsureCharacterIsSelected());
            
        return character.Description;
    }

    public PyDataType SetCharacterDescription(PyString newBio, CallInformation call)
    {
        Character character = this.ItemFactory.GetItem<Character>(call.Session.EnsureCharacterIsSelected());

        character.Description = newBio;
        character.Persist();
            
        return null;
    }

    public PyDataType GetRecentShipKillsAndLosses(PyInteger count, PyInteger startIndex, CallInformation call)
    {
        // limit number of records to 100 at maximum
        if (count > 100)
            count = 100;
            
        return this.DB.GetRecentShipKillsAndLosses(call.Session.EnsureCharacterIsSelected(), count, startIndex);
    }

    public PyDataType GetCharacterAppearanceList(PyList ids, CallInformation call)
    {
        PyList result = new PyList(ids.Count);

        int index = 0;
            
        foreach (PyInteger id in ids.GetEnumerable<PyInteger>())
        {
            Rowset dbResult = this.DB.GetCharacterAppearanceInfo(id);

            if (dbResult.Rows.Count != 0)
                result[index] = dbResult;

            index++;
        }

        return result;
    }

    public PyDataType GetNote(PyInteger characterID, CallInformation call)
    {
        return this.DB.GetNote(characterID, call.Session.EnsureCharacterIsSelected());
    }

    public PyDataType SetNote(PyInteger characterID, PyString note, CallInformation call)
    {
        this.DB.SetNote(characterID, call.Session.EnsureCharacterIsSelected(), note);

        return null;
    }

    public PyDataType GetFactions(CallInformation call)
    {
        PyList result = new PyList();
            
        foreach ((int factionID, Faction faction) in this.ItemFactory.Factions)
            result.Add(faction.GetKeyVal());

        return result;
    }
}