using System;
using EVESharp.Database;
using EVESharp.EVE.Data.Account;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.inventory;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Station;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Services;
using EVESharp.EVE.Services.Validators;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Dogma;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Services.Dogma;

[MustBeCharacter]
public class dogmaIM : ClientBoundService
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    private IItems              Items             { get; }
    private IAttributes         Attributes        => this.Items.Attributes;
    private IDefaultAttributes  DefaultAttributes => this.Items.DefaultAttributes;
    private ISolarSystems       SolarSystems      { get; }
    private INotificationSender Notifications     { get; }
    private EffectsManager      EffectsManager    { get; }
    private IDatabaseConnection Database          { get; }

    public dogmaIM (EffectsManager effectsManager, IItems items, INotificationSender notificationSender, BoundServiceManager manager, IDatabaseConnection database,
                    ISolarSystems solarSystems) : base (manager)
    {
        EffectsManager = effectsManager;
        Items          = items;
        Notifications  = notificationSender;
        Database       = database;
        SolarSystems   = solarSystems;
    }

    protected dogmaIM (
        int     locationID, EffectsManager effectsManager, IItems items, INotificationSender notificationSender, BoundServiceManager manager,
        Session session, ISolarSystems solarSystems
    ) : base (manager, session, locationID)
    {
        EffectsManager = effectsManager;
        Items          = items;
        Notifications  = notificationSender;
        SolarSystems   = solarSystems;
    }

    public PyDataType ShipGetInfo (CallInformation call)
    {
        int  callerCharacterID = call.Session.CharacterID;
        int? shipID            = call.Session.ShipID;

        if (shipID is null)
            throw new CustomError ("The character is not aboard any ship");

        // TODO: RE-EVALUATE WHERE THE SHIP LOADING IS PERFORMED, SHIPGETINFO DOESN'T LOOK LIKE A GOOD PLACE TO DO IT
        Ship ship = this.Items.LoadItem <Ship> ((int) shipID);

        if (ship is null)
            throw new CustomError ($"Cannot get information for ship {call.Session.ShipID}");

        try
        {
            // ensure the player can use this ship
            ship.EnsureOwnership (callerCharacterID, call.Session.CorporationID, call.Session.CorporationRole, true);

            ItemInfo itemInfo = new ItemInfo ();

            itemInfo.AddRow (ship.ID, ship.GetEntityRow (), ship.GetEffects (), ship.Attributes, DateTime.UtcNow.ToFileTime ());

            foreach ((int _, ItemEntity item) in ship.Items)
            {
                if (item.IsInModuleSlot () == false && item.IsInRigSlot () == false)
                    continue;

                itemInfo.AddRow (
                    item.ID,
                    item.GetEntityRow (),
                    item.GetEffects (),
                    item.Attributes,
                    DateTime.UtcNow.ToFileTime ()
                );
            }

            return itemInfo;
        }
        catch (Exception)
        {
            // there was an exception, the ship has to be unloaded as it's not going to be used anymore
            this.Items.UnloadItem (ship);

            throw;
        }
    }

    public PyDataType CharGetInfo (CallInformation call)
    {
        int callerCharacterID = call.Session.CharacterID;

        Character character = this.Items.GetItem <Character> (callerCharacterID);

        if (character is null)
            throw new CustomError ($"Cannot get information for character {callerCharacterID}");

        ItemInfo itemInfo = new ItemInfo ();

        itemInfo.AddRow (character.ID, character.GetEntityRow (), character.GetEffects (), character.Attributes, DateTime.UtcNow.ToFileTime ());

        foreach ((int _, ItemEntity item) in character.Items)
            switch (item.Flag)
            {
                case Flags.Booster:
                case Flags.Implant:
                case Flags.Skill:
                case Flags.SkillInTraining:
                    itemInfo.AddRow (
                        item.ID,
                        item.GetEntityRow (),
                        item.GetEffects (),
                        item.Attributes,
                        DateTime.UtcNow.ToFileTime ()
                    );
                    break;

            }

        return itemInfo;
    }

    public PyDataType ItemGetInfo (CallInformation call, PyInteger itemID)
    {
        int callerCharacterID = call.Session.CharacterID;

        ItemEntity item = this.Items.LoadItem (itemID);

        if (item.ID != callerCharacterID && item.OwnerID != callerCharacterID && item.OwnerID != call.Session.CorporationID)
            throw new TheItemIsNotYoursToTake (itemID);

        return new Row (
            new PyList <PyString> (5)
            {
                [0] = "itemID",
                [1] = "invItem",
                [2] = "activeEffects",
                [3] = "attributes",
                [4] = "time"
            },
            new PyList (5)
            {
                [0] = item.ID,
                [1] = item.GetEntityRow (),
                [2] = item.GetEffects (),
                [3] = item.Attributes,
                [4] = DateTime.UtcNow.ToFileTimeUtc ()
            }
        );
    }

    public PyDataType GetWeaponBankInfoForShip (CallInformation call)
    {
        // this function seems to indicate the client when modules are grouped
        // so it can display them on the UI and I guess act on them too
        // for now there's no support for this functionality, so it can be stubbed out
        return new PyDictionary ();
    }

    public PyDataType GetCharacterBaseAttributes (CallInformation call)
    {
        int callerCharacterID = call.Session.CharacterID;

        Character character = this.Items.GetItem <Character> (callerCharacterID);

        if (character is null)
            throw new CustomError ($"Cannot get information for character {callerCharacterID}");

        return new PyDictionary
        {
            [(int) AttributeTypes.willpower]    = character.Willpower,
            [(int) AttributeTypes.charisma]     = character.Charisma,
            [(int) AttributeTypes.intelligence] = character.Intelligence,
            [(int) AttributeTypes.perception]   = character.Perception,
            [(int) AttributeTypes.memory]       = character.Memory
        };
    }

    public PyDataType LogAttribute (CallInformation call, PyInteger itemID, PyInteger attributeID)
    {
        return this.LogAttribute (call, itemID, attributeID, "");
    }

    public PyList <PyString> LogAttribute (CallInformation call, PyInteger itemID, PyInteger attributeID, PyString reason)
    {
        ulong role     = call.Session.Role;
        ulong roleMask = (ulong) (Roles.ROLE_GDH | Roles.ROLE_QA | Roles.ROLE_PROGRAMMER | Roles.ROLE_GMH);

        if ((role & roleMask) == 0)
            throw new CustomError ("Not allowed!");

        ItemEntity item = this.Items.GetItem (itemID);

        if (item.Attributes.AttributeExists (attributeID) == false)
            throw new CustomError ("The given attribute doesn't exists in the item");

        // we don't know the actual values of the returned function
        // but it should be enough to fill the required data by the client
        return new PyList <PyString> (5)
        {
            [0] = null,
            [1] = null,
            [2] = $"Server value: {item.Attributes [attributeID]}",
            [3] = $"Base value: {this.DefaultAttributes [item.Type.ID] [attributeID]}",
            [4] = $"Reason: {reason}"
        };
    }

    public PyDataType Activate (CallInformation call, PyInteger itemID, PyString effectName, PyDataType target, PyDataType repeat)
    {
        ShipModule module = this.Items.GetItem <ShipModule> (itemID);

        EffectsManager.GetForItem (module, call.Session).ApplyEffect (effectName, call.Session);

        return null;
    }

    public PyDataType Deactivate (CallInformation call, PyInteger itemID, PyString effectName)
    {
        ShipModule module = this.Items.GetItem <ShipModule> (itemID);

        EffectsManager.GetForItem (module, call.Session).StopApplyingEffect (effectName, call.Session);

        return null;
    }

    protected override long MachoResolveObject (CallInformation call, ServiceBindParams parameters)
    {
        return parameters.ExtraValue switch
        {
            (int) GroupID.SolarSystem => Database.CluResolveAddress ("solarsystem", parameters.ObjectID),
            (int) GroupID.Station     => Database.CluResolveAddress ("station",     parameters.ObjectID),
            _                        => throw new CustomError ("Unknown item's groupID")
        };
    }

    protected override BoundService CreateBoundInstance (CallInformation call, ServiceBindParams bindParams)
    {
        int characterID = call.Session.CharacterID;

        if (this.MachoResolveObject (call, bindParams) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        // make sure the character is loaded
        Character character = this.Items.LoadItem <Character> (characterID);

        // depending on the type of binding we're doing it means the player might be entering a station
        if (bindParams.ExtraValue == (int) GroupID.Station && call.Session.StationID == bindParams.ObjectID)
        {
            this.Items.GetStaticStation (bindParams.ObjectID).Guests [characterID] = character;

            // notify all station guests
            Notifications.NotifyStation (bindParams.ObjectID, new OnCharNowInStation (call.Session));
        }

        return new dogmaIM (bindParams.ObjectID, EffectsManager, Items, Notifications, BoundServiceManager, call.Session, SolarSystems);
    }

    protected override void OnClientDisconnected ()
    {
        int characterID = Session.CharacterID;

        // notify station about the player disconnecting from the object
        if (Session.StationID == ObjectID)
        {
            // remove the character from the list
            this.Items.GetStaticStation (ObjectID).Guests.Remove (characterID);

            // notify all station guests
            Notifications.NotifyStation (ObjectID, new OnCharNoLongerInStation (Session));

            // check if the character is loaded or their ship is loaded and unload it
            // TODO: THIS MIGHT REQUIRE CHANGES WHEN DESTINY WORK IS STARTED
            this.Items.UnloadItem (characterID);

            if (Session.ShipID is not null)
                this.Items.UnloadItem ((int) Session.ShipID);
        }
    }
}