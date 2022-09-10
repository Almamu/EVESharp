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
using EVESharp.Database;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Market;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.character;
using EVESharp.EVE.Market;
using EVESharp.EVE.Network.Caching;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Chat;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.Types;
using EVESharp.Node.Database;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Serilog;
using Type = EVESharp.EVE.Data.Inventory.Type;

namespace EVESharp.Node.Services.Characters;

public class character : Service
{
    private readonly Configuration.Character mConfiguration;
    public override  AccessLevel             AccessLevel => AccessLevel.LocationPreferred;

    private OldCharacterDB      DB             { get; }
    private CorporationDB       CorporationDB  { get; }
    private ChatDB              ChatDB         { get; }
    private IItems              Items          { get; }
    private ITypes              Types          => this.Items.Types;
    private ICacheStorage       CacheStorage   { get; }
    private INotificationSender Notifications  { get; }
    private IWallets            Wallets        { get; }
    private IAncestries         Ancestries     { get; }
    private IBloodlines         Bloodlines     { get; }
    private ISessionManager     SessionManager { get; }
    private ILogger             Log            { get; }
    private IDatabaseConnection Database       { get; }

    public character
    (
        ICacheStorage       cacheStorage,       OldCharacterDB db,      ChatDB      chatDB, CorporationDB corporationDB,
        IItems              items,              ILogger     logger,  Configuration.Character   configuration,
        INotificationSender notificationSender, IWallets    wallets, IAncestries ancestries, IBloodlines bloodlines,
        ISessionManager     sessionManager, IDatabaseConnection database
    )
    {
        Log                 = logger;
        this.mConfiguration = configuration;
        DB                  = db;
        ChatDB              = chatDB;
        CorporationDB       = corporationDB;
        this.Items          = items;
        CacheStorage        = cacheStorage;
        Notifications       = notificationSender;
        this.Wallets        = wallets;
        Ancestries          = ancestries;
        Bloodlines          = bloodlines;
        SessionManager      = sessionManager;
        Database            = database;
    }

    [MustNotBeCharacter]
    public PyDataType GetCharactersToSelect (CallInformation call)
    {
        return DB.GetCharacterList (call.Session.UserID);
    }

    [MustNotBeCharacter]
    public PyDataType LogStartOfCharacterCreation (CallInformation call)
    {
        return null;
    }

    public PyDataType GetCharCreationInfo (CallInformation call)
    {
        return CacheStorage.GetHints (EVE.Data.Cache.CreateCharacterCacheTable);
    }

    public PyDataType GetAppearanceInfo (CallInformation call)
    {
        return CacheStorage.GetHints (EVE.Data.Cache.CharacterAppearanceCacheTable);
    }

    public PyDataType GetCharNewExtraCreationInfo (CallInformation call)
    {
        return new PyDictionary ();
    }

    [MustNotBeCharacter]
    public PyInteger ValidateNameEx (CallInformation call, PyString name)
    {
        string characterName = name;

        if (characterName.Length < 3)
            return (int) NameValidationResults.TooShort;

        // character name length is maximum 24 characters based on the error messages used for the user
        if (characterName.Length > 24)
            return (int) NameValidationResults.TooLong;

        // ensure only alphanumeric characters and/or spaces are used
        if (Regex.IsMatch (characterName, "^[a-zA-Z0-9 ]*$") == false)
            return (int) NameValidationResults.IllegalCharacters;

        // no more than one space allowed
        if (characterName.IndexOf (' ') != characterName.LastIndexOf (' '))
            return (int) NameValidationResults.MoreThanOneSpace;

        // ensure there is no character registered with this name already
        if (DB.IsCharacterNameTaken (characterName))
            return (int) NameValidationResults.Taken;

        // TODO: IMPLEMENT BANLIST OF WORDS
        return (int) NameValidationResults.Valid;
    }

    private void GetRandomCareerForRace
    (
        int     raceID, out int careerID, out int schoolID, out int careerSpecialityID,
        out int corporationID
    )
    {
        // TODO: DETERMINE SCHOOLID, CARREERID AND CAREERSPECIALITYID PROPERLY

        bool found = DB.GetRandomCareerForRace (
            raceID, out careerID, out schoolID,
            out careerSpecialityID, out corporationID
        );

        if (found)
            return;

        Log.Error ($"Cannot find random career for race {raceID}");

        throw new CustomError ($"Cannot find random career for race {raceID}");
    }

    private void GetLocationForCorporation
    (
        int     corporationID,   out int stationID, out int solarSystemID,
        out int constellationID, out int regionID
    )
    {
        // fetch information of starting location for the player
        bool found = DB.GetLocationForCorporation (
            corporationID, out stationID, out solarSystemID,
            out constellationID, out regionID
        );

        if (found)
            return;

        Log.Error ($"Cannot find location for corporation {corporationID}");

        throw new CustomError ($"Cannot find location for corporation {corporationID}");
    }

    private void ExtractExtraCharacterAppearance
    (
        PyDictionary  data,    out PyInteger accessoryID,
        out PyInteger beardID, out PyInteger decoID,  out PyInteger lipstickID, out PyInteger makeupID,
        out PyDecimal morph1e, out PyDecimal morph1n, out PyDecimal morph1s,    out PyDecimal morph1w,
        out PyDecimal morph2e, out PyDecimal morph2n, out PyDecimal morph2s,    out PyDecimal morph2w,
        out PyDecimal morph3e, out PyDecimal morph3n, out PyDecimal morph3s,    out PyDecimal morph3w,
        out PyDecimal morph4e, out PyDecimal morph4n, out PyDecimal morph4s,    out PyDecimal morph4w
    )
    {
        data.TryGetValue ("accessoryID", out accessoryID);
        data.TryGetValue ("beardID",     out beardID);
        data.TryGetValue ("decoID",      out decoID);
        data.TryGetValue ("lipstickID",  out lipstickID);
        data.TryGetValue ("makeupID",    out makeupID);
        data.TryGetValue ("morph1e",     out morph1e);
        data.TryGetValue ("morph1n",     out morph1n);
        data.TryGetValue ("morph1s",     out morph1s);
        data.TryGetValue ("morph1w",     out morph1w);
        data.TryGetValue ("morph2e",     out morph2e);
        data.TryGetValue ("morph2n",     out morph2n);
        data.TryGetValue ("morph2s",     out morph2s);
        data.TryGetValue ("morph2w",     out morph2w);
        data.TryGetValue ("morph3e",     out morph3e);
        data.TryGetValue ("morph3n",     out morph3n);
        data.TryGetValue ("morph3s",     out morph3s);
        data.TryGetValue ("morph3w",     out morph3w);
        data.TryGetValue ("morph4e",     out morph4e);
        data.TryGetValue ("morph4n",     out morph4n);
        data.TryGetValue ("morph4s",     out morph4s);
        data.TryGetValue ("morph4w",     out morph4w);
    }

    private void ExtractCharacterAppearance
    (
        PyDictionary  data,          out PyInteger costumeID,     out PyInteger eyebrowsID,
        out PyInteger eyesID,        out PyInteger hairID,        out PyInteger skinID, out PyInteger backgroundID,
        out PyInteger lightID,       out PyDecimal headRotation1, out PyDecimal headRotation2,
        out PyDecimal headRotation3, out PyDecimal eyeRotation1,  out PyDecimal eyeRotation2,
        out PyDecimal eyeRotation3,  out PyDecimal camPos1,       out PyDecimal camPos2, out PyDecimal camPos3
    )
    {
        data.SafeGetValue ("costumeID",     out costumeID);
        data.SafeGetValue ("eyebrowsID",    out eyebrowsID);
        data.SafeGetValue ("eyesID",        out eyesID);
        data.SafeGetValue ("hairID",        out hairID);
        data.SafeGetValue ("skinID",        out skinID);
        data.SafeGetValue ("backgroundID",  out backgroundID);
        data.SafeGetValue ("lightID",       out lightID);
        data.SafeGetValue ("headRotation1", out headRotation1);
        data.SafeGetValue ("headRotation2", out headRotation2);
        data.SafeGetValue ("headRotation3", out headRotation3);
        data.SafeGetValue ("eyeRotation1",  out eyeRotation1);
        data.SafeGetValue ("eyeRotation2",  out eyeRotation2);
        data.SafeGetValue ("eyeRotation3",  out eyeRotation3);
        data.SafeGetValue ("camPos1",       out camPos1);
        data.SafeGetValue ("camPos2",       out camPos2);
        data.SafeGetValue ("camPos3",       out camPos3);
    }

    private Character CreateCharacter
    (
        CallInformation call, string characterName, Ancestry ancestry, int genderID, PyDictionary appearance, long currentTime
    )
    {
        // load the item into memory
        ItemEntity owner = this.Items.LocationSystem;

        this.GetRandomCareerForRace (ancestry.Bloodline.RaceID, out int careerID, out int schoolID, out int careerSpecialityID, out int corporationID);
        this.GetLocationForCorporation (corporationID, out int stationID, out int solarSystemID, out int constellationID, out int regionID);

        this.ExtractCharacterAppearance (
            appearance, out PyInteger costumeID, out PyInteger eyebrowsID,
            out PyInteger eyesID, out PyInteger hairID, out PyInteger skinID, out PyInteger backgroundID,
            out PyInteger lightID, out PyDecimal headRotation1, out PyDecimal headRotation2,
            out PyDecimal headRotation3, out PyDecimal eyeRotation1, out PyDecimal eyeRotation2,
            out PyDecimal eyeRotation3, out PyDecimal camPos1, out PyDecimal camPos2, out PyDecimal camPos3
        );

        this.ExtractExtraCharacterAppearance (
            appearance, out PyInteger accessoryID, out PyInteger beardID,
            out PyInteger decoID, out PyInteger lipstickID, out PyInteger makeupID, out PyDecimal morph1e,
            out PyDecimal morph1n, out PyDecimal morph1s, out PyDecimal morph1w, out PyDecimal morph2e,
            out PyDecimal morph2n, out PyDecimal morph2s, out PyDecimal morph2w, out PyDecimal morph3e,
            out PyDecimal morph3n, out PyDecimal morph3s, out PyDecimal morph3w, out PyDecimal morph4e,
            out PyDecimal morph4n, out PyDecimal morph4s, out PyDecimal morph4w
        );

        int itemID = DB.CreateCharacter (
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
        this.Wallets.CreateWallet (itemID, WalletKeys.MAIN, this.mConfiguration.Balance);

        return this.Items.LoadItem (itemID) as Character;
    }

    [MustNotBeCharacter]
    public PyDataType CreateCharacter2
    (
        CallInformation call, PyString characterName, PyInteger bloodlineID, PyInteger genderID, PyInteger ancestryID,
        PyDictionary    appearance
    )
    {
        int validationError = this.ValidateNameEx (call, characterName);

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
            default: throw new CharNameInvalid ();
        }

        // load bloodline and ancestry info for the requested character
        Ancestry  ancestry  = Ancestries [ancestryID];
        Bloodline bloodline = Bloodlines [bloodlineID];

        long currentTime = DateTime.UtcNow.ToFileTimeUtc ();

        if (ancestry.Bloodline.ID != bloodlineID)
        {
            Log.Error ($"The ancestry {ancestryID} doesn't belong to the given bloodline {bloodlineID}");

            throw new BannedBloodline (ancestry, bloodline);
        }

        Character character =
            this.CreateCharacter (call, characterName, ancestry, genderID, appearance, currentTime);

        Station station = this.Items.GetStaticStation (character.StationID);

        // TODO: CREATE DEFAULT STANDINGS FOR THE CHARACTER
        // change character attributes based on the picked ancestry
        character.Charisma     = bloodline.Charisma + ancestry.Charisma;
        character.Intelligence = bloodline.Intelligence + ancestry.Intelligence;
        character.Memory       = bloodline.Memory + ancestry.Memory;
        character.Willpower    = bloodline.Willpower + ancestry.Willpower;
        character.Perception   = bloodline.Perception + ancestry.Perception;

        // get skills by race and create them
        Dictionary <int, int> skills = DB.GetBasicSkillsByRace (bloodline.RaceID);

        foreach ((int skillTypeID, int level) in skills)
        {
            Type skillType = this.Types [skillTypeID];

            // create the skill at the required level
            this.Items.CreateSkill (skillType, character, level);
        }

        // create the ship for the character
        Ship ship = this.Items.CreateShip (
            bloodline.ShipType, station,
            character
        );

        // add one unit of Tritanium to the station's hangar for the player
        Type tritaniumType = this.Types [TypeID.Tritanium];

        ItemEntity tritanium =
            this.Items.CreateSimpleItem (
                tritaniumType, character,
                station, Flags.Hangar
            );

        // add one unit of Damage Control I to the station's hangar for the player
        Type damageControlType = this.Types [TypeID.DamageControlI];

        ItemEntity damageControl =
            this.Items.CreateSimpleItem (
                damageControlType, character,
                station, Flags.Hangar
            );

        // create an alpha clone
        Type cloneType = this.Types [TypeID.CloneGradeAlpha];

        Clone clone = this.Items.CreateClone (cloneType, station, character);

        character.LocationID    = ship.ID;
        character.ActiveCloneID = clone.ID;

        // get the wallet for the player and give the money specified in the configuration
        using IWallet wallet = this.Wallets.AcquireWallet (character.ID, WalletKeys.MAIN);
        {
            wallet.CreateJournalRecord (MarketReference.Inheritance, null, null, this.mConfiguration.Balance);
        }

        // character is 100% created and the base items are too
        // persist objects to database and unload them as they do not really belong to us
        clone.Persist ();
        damageControl.Persist ();
        tritanium.Persist ();
        ship.Persist ();
        character.Persist ();

        // join the character to all the general channels
        ChatDB.GrantAccessToStandardChannels (character.ID);
        // create required mailing list channel
        ChatDB.CreateChannel (character, character, characterName, true);
        // and subscribe the character to some channels
        ChatDB.JoinEntityMailingList (character.ID, character.ID);
        ChatDB.JoinEntityChannel (character.SolarSystemID,   character.ID);
        ChatDB.JoinEntityChannel (character.ConstellationID, character.ID);
        ChatDB.JoinEntityChannel (character.RegionID,        character.ID);
        ChatDB.JoinEntityChannel (character.CorporationID,   character.ID);
        ChatDB.JoinEntityMailingList (character.CorporationID, character.ID);

        // unload items from list
        this.Items.UnloadItem (clone);
        this.Items.UnloadItem (damageControl);
        this.Items.UnloadItem (tritanium);
        this.Items.UnloadItem (ship);
        this.Items.UnloadItem (character);

        // finally return the new character's ID and wait for the subsequent calls from the EVE client :)
        return character.ID;
    }

    [MustNotBeCharacter]
    public PyDataType GetCharacterToSelect (CallInformation call, PyInteger characterID)
    {
        return DB.GetCharacterSelectionInfo (characterID, call.Session.UserID);
    }

    [MustNotBeCharacter]
    public PyDataType SelectCharacterID
    (
        CallInformation call, PyInteger characterID, PyBool loadDungeon, PyDataType secondChoiceID
    )
    {
        return this.SelectCharacterID (call, characterID, loadDungeon == true ? 1 : 0, secondChoiceID);
    }

    [MustNotBeCharacter]
    public PyDataType SelectCharacterID (CallInformation call, PyInteger characterID)
    {
        return this.SelectCharacterID (call, characterID, 0, 0);
    }

    // TODO: THIS PyNone SHOULD REALLY BE AN INTEGER, ALTHOUGH THIS FUNCTIONALITY IS NOT USED
    // TODO: IT REVEALS AN IMPORTANT ISSUE, WE CAN'T HAVE A WILDCARD PARAMETER PyDataType
    [MustNotBeCharacter]
    public PyDataType SelectCharacterID
    (
        CallInformation call, PyInteger characterID, PyInteger loadDungeon, PyDataType secondChoiceID
    )
    {
        // ensure the character belongs to the current account
        Character character = this.Items.LoadItem <Character> (characterID);

        if (character.AccountID != call.Session.UserID)
        {
            // unload character
            this.Items.UnloadItem (character);

            // throw proper error
            throw new CustomError ("The selected character does not belong to this account, aborting...");
        }

        Session updates = new Session ();

        // update the session data for this client
        updates.CharacterID   = character.ID;
        updates.CorporationID = character.CorporationID;

        if (character.StationID == 0)
            updates.SolarSystemID = character.SolarSystemID;
        else
            updates.StationID = character.StationID;

        // get title roles and mask them with the current roles to ensure the user has proper roles
        CorporationDB.GetTitleInformation (
            character.CorporationID, character.TitleMask,
            out long roles, out long rolesAtHQ, out long rolesAtBase, out long rolesAtOther,
            out long _, out long _, out long _,
            out long _, out _
        );

        updates.CorporationRole = character.Roles | roles;

        updates.RolesAtAll = character.Roles | character.RolesAtBase | character.RolesAtOther | character.RolesAtHq | roles | rolesAtHQ | rolesAtBase |
                             rolesAtOther;

        updates.RolesAtBase  = character.RolesAtBase | rolesAtBase;
        updates.RolesAtHQ    = character.RolesAtHq | rolesAtHQ;
        updates.RolesAtOther = character.RolesAtOther | rolesAtOther;
        updates.AllianceID   = CorporationDB.GetAllianceIDForCorporation (character.CorporationID);

        // set the rest of the important locations
        updates.SolarSystemID2  = character.SolarSystemID;
        updates.ConstellationID = character.ConstellationID;
        updates.RegionID        = character.RegionID;
        updates.HQID            = 0; // TODO: ADD SUPPORT FOR HQID
        updates.ShipID          = character.LocationID;
        updates.RaceID          = Ancestries [character.AncestryID].Bloodline.RaceID;

        // check if the character has any accounting roles and set the correct accountKey based on the data
        if (this.Wallets.IsAccessAllowed (updates, character.CorpAccountKey, updates.CorporationID))
            updates.CorpAccountKey = character.CorpAccountKey;

        // set the war faction id if present
        if (character.WarFactionID is not null)
            updates.WarFactionID = character.WarFactionID;

        // update the logon status
        Database.ChrUpdateLogonTime (character.ID);
        // unload the character, let the session change handler handle everything
        // TODO: CHECK IF THE PLAYER IS GOING TO SPAWN IN THIS NODE AND IF IT IS NOT, UNLOAD IT FROM THE ITEM MANAGER
        PyList <PyInteger> onlineFriends = DB.GetOnlineFriendList (character);

        Notifications.NotifyCharacters (onlineFriends, new OnContactLoggedOn (character.ID));

        // unload the character
        this.Items.UnloadItem (characterID);

        // send the session change
        SessionManager.PerformSessionUpdate (Session.USERID, call.Session.UserID, updates);

        return null;
    }

    public PyDataType Ping (CallInformation call)
    {
        return call.Session.UserID;
    }

    [MustBeCharacter]
    public PyDataType GetOwnerNoteLabels (CallInformation call)
    {
        return DB.GetOwnerNoteLabels (call.Session.CharacterID);
    }

    [MustBeCharacter]
    public PyDataType GetCloneTypeID (CallInformation call)
    {
        // TODO: FETCH THIS FROM THE DATABASE INSTEAD
        // return character.ActiveClone.Type.ID;
        return (int) TypeID.CloneGradeAlpha;
    }

    [MustBeCharacter]
    public PyDataType GetHomeStation (CallInformation call)
    {
        Character character = this.Items.GetItem <Character> (call.Session.CharacterID);

        if (character.ActiveCloneID is null)
            throw new CustomError ("You do not have any medical clone...");

        // TODO: FETCH THIS FROM THE DATABASE INSTEAD
        // return character.ActiveClone.LocationID;
        return call.Session.StationID;
    }

    [MustBeCharacter]
    public PyDataType GetCharacterDescription (CallInformation call, PyInteger characterID)
    {
        Character character = this.Items.GetItem <Character> (call.Session.CharacterID);

        return character.Description;
    }

    [MustBeCharacter]
    public PyDataType SetCharacterDescription (CallInformation call, PyString newBio)
    {
        Character character = this.Items.GetItem <Character> (call.Session.CharacterID);

        character.Description = newBio;
        character.Persist ();

        return null;
    }

    [MustBeCharacter]
    public PyDataType GetRecentShipKillsAndLosses (CallInformation call, PyInteger count, PyInteger startIndex)
    {
        // limit number of records to 100 at maximum
        if (count > 100)
            count = 100;

        return DB.GetRecentShipKillsAndLosses (call.Session.CharacterID, count, startIndex);
    }

    public PyDataType GetCharacterAppearanceList (CallInformation call, PyList ids)
    {
        PyList result = new PyList (ids.Count);

        int index = 0;

        foreach (PyInteger id in ids.GetEnumerable <PyInteger> ())
        {
            Rowset dbResult = DB.GetCharacterAppearanceInfo (id);

            if (dbResult.Rows.Count != 0)
                result [index] = dbResult;

            index++;
        }

        return result;
    }

    [MustBeCharacter]
    public PyDataType GetNote (CallInformation call, PyInteger characterID)
    {
        return Database.ChrGetNote (characterID, call.Session.CharacterID);
    }

    [MustBeCharacter]
    public PyDataType SetNote (CallInformation call, PyInteger characterID, PyString note)
    {
        Database.ChrSetNote (characterID, call.Session.CharacterID, note);

        return null;
    }

    public PyDataType GetFactions (CallInformation call)
    {
        PyList result = new PyList ();

        foreach ((int factionID, Faction faction) in this.Items.Factions)
            result.Add (faction.GetKeyVal ());

        return result;
    }

    private enum NameValidationResults
    {
        Valid             = 1,
        TooShort          = -1,
        TooLong           = -2,
        IllegalCharacters = -5,
        MoreThanOneSpace  = -6,
        Taken             = -101,
        Banned            = -102
    }
}