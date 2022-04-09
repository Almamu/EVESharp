using System;
using EVESharp.EVE;
using EVESharp.EVE.Client.Exceptions.inventory;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.Node.Dogma;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Notifications;
using EVESharp.Node.Notifications.Client.Station;
using EVESharp.Node.Sessions;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Groups = EVESharp.EVE.StaticData.Inventory.Groups;

namespace EVESharp.Node.Services.Dogma;

public class dogmaIM : ClientBoundService
{
    public override AccessLevel AccessLevel => AccessLevel.None;

    private ItemFactory                 ItemFactory         { get; }
    private AttributeManager            AttributeManager    => ItemFactory.AttributeManager;
    private SystemManager               SystemManager       => ItemFactory.SystemManager;
    private Notifications.Notifications Notifications { get; }
    private EffectsManager              EffectsManager      { get; }

    public dogmaIM (
        EffectsManager effectsManager, ItemFactory itemFactory, Notifications.Notifications notifications, BoundServiceManager manager
    ) : base (manager)
    {
        EffectsManager      = effectsManager;
        ItemFactory         = itemFactory;
        Notifications = notifications;
    }

    protected dogmaIM (
        int     locationID, EffectsManager effectsManager, ItemFactory itemFactory, Notifications.Notifications notifications, BoundServiceManager manager,
        Session session
    ) : base (manager, session, locationID)
    {
        EffectsManager      = effectsManager;
        ItemFactory         = itemFactory;
        Notifications = notifications;
    }

    public PyDataType ShipGetInfo (CallInformation call)
    {
        int  callerCharacterID = call.Session.EnsureCharacterIsSelected ();
        int? shipID            = call.Session.ShipID;

        if (shipID is null)
            throw new CustomError ("The character is not aboard any ship");

        // TODO: RE-EVALUATE WHERE THE SHIP LOADING IS PERFORMED, SHIPGETINFO DOESN'T LOOK LIKE A GOOD PLACE TO DO IT
        Ship ship = ItemFactory.LoadItem <Ship> ((int) shipID);

        if (ship is null)
            throw new CustomError ($"Cannot get information for ship {call.Session.ShipID}");

        try
        {
            // ensure the player can use this ship
            ship.EnsureOwnership (callerCharacterID, call.Session.CorporationID, call.Session.CorporationRole, true);

            ItemInfo itemInfo = new ItemInfo ();

            // TODO: find all the items inside this ship that are not characters
            itemInfo.AddRow (ship.ID, ship.GetEntityRow (), ship.GetEffects (), ship.Attributes, DateTime.UtcNow.ToFileTime ());

            foreach ((int _, ItemEntity item) in ship.Items)
            {
                if (item.IsInModuleSlot () == false && item.IsInRigSlot ())
                    continue;

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
        catch (Exception)
        {
            // there was an exception, the ship has to be unloaded as it's not going to be used anymore
            ItemFactory.UnloadItem (ship);

            throw;
        }
    }

    public PyDataType CharGetInfo (CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

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

    public PyDataType ItemGetInfo (PyInteger itemID, CallInformation call)
    {
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        ItemEntity item = ItemFactory.LoadItem (itemID);

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
        int callerCharacterID = call.Session.EnsureCharacterIsSelected ();

        Character character = ItemFactory.GetItem <Character> (callerCharacterID);

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

    public PyDataType LogAttribute (PyInteger itemID, PyInteger attributeID, CallInformation call)
    {
        return this.LogAttribute (itemID, attributeID, "", call);
    }

    public PyList <PyString> LogAttribute (PyInteger itemID, PyInteger attributeID, PyString reason, CallInformation call)
    {
        ulong role     = call.Session.Role;
        ulong roleMask = (ulong) (Roles.ROLE_GDH | Roles.ROLE_QA | Roles.ROLE_PROGRAMMER | Roles.ROLE_GMH);

        if ((role & roleMask) == 0)
            throw new CustomError ("Not allowed!");

        ItemEntity item = ItemFactory.GetItem (itemID);

        if (item.Attributes.AttributeExists (attributeID) == false)
            throw new CustomError ("The given attribute doesn't exists in the item");

        // we don't know the actual values of the returned function
        // but it should be enough to fill the required data by the client
        return new PyList <PyString> (5)
        {
            [0] = null,
            [1] = null,
            [2] = $"Server value: {item.Attributes [attributeID]}",
            [3] = $"Base value: {AttributeManager.DefaultAttributes [item.Type.ID] [attributeID]}",
            [4] = $"Reason: {reason}"
        };
    }

    public PyDataType Activate (PyInteger itemID, PyString effectName, PyDataType target, PyDataType repeat, CallInformation call)
    {
        ShipModule module = ItemFactory.GetItem <ShipModule> (itemID);

        EffectsManager.GetForItem (module).ApplyEffect (effectName, call.Session);

        return null;
    }

    public PyDataType Deactivate (PyInteger itemID, PyString effectName, CallInformation call)
    {
        ShipModule module = ItemFactory.GetItem <ShipModule> (itemID);

        EffectsManager.GetForItem (module).StopApplyingEffect (effectName, call.Session);

        return null;
    }

    protected override long MachoResolveObject (ServiceBindParams parameters, CallInformation call)
    {
        int solarSystemID = 0;

        if (parameters.ExtraValue == (int) Groups.SolarSystem)
            solarSystemID = ItemFactory.GetStaticSolarSystem (parameters.ObjectID).ID;
        else if (parameters.ExtraValue == (int) Groups.Station)
            solarSystemID = ItemFactory.GetStaticStation (parameters.ObjectID).SolarSystemID;
        else
            throw new CustomError ("Unknown item's groupID");

        return SystemManager.LoadSolarSystemOnCluster (solarSystemID);
    }

    protected override BoundService CreateBoundInstance (ServiceBindParams bindParams, CallInformation call)
    {
        int characterID = call.Session.EnsureCharacterIsSelected ();

        if (this.MachoResolveObject (bindParams, call) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        // make sure the character is loaded
        Character character = ItemFactory.LoadItem <Character> (characterID);

        // depending on the type of binding we're doing it means the player might be entering a station
        if (bindParams.ExtraValue == (int) Groups.Station && call.Session.StationID == bindParams.ObjectID)
        {
            ItemFactory.GetStaticStation (bindParams.ObjectID).Guests [characterID] = character;

            // notify all station guests
            Notifications.NotifyStation (bindParams.ObjectID, new OnCharNowInStation (call.Session));
        }

        return new dogmaIM (bindParams.ObjectID, EffectsManager, ItemFactory, Notifications, BoundServiceManager, call.Session);
    }

    protected override void OnClientDisconnected ()
    {
        int characterID = Session.EnsureCharacterIsSelected ();

        // notify station about the player disconnecting from the object
        if (Session.StationID == ObjectID)
        {
            // remove the character from the list
            ItemFactory.GetStaticStation (ObjectID).Guests.Remove (characterID);

            // notify all station guests
            Notifications.NotifyStation (ObjectID, new OnCharNoLongerInStation (Session));

            // check if the character is loaded or their ship is loaded and unload it
            // TODO: THIS MIGHT REQUIRE CHANGES WHEN DESTINY WORK IS STARTED
            ItemFactory.UnloadItem (characterID);

            if (Session.ShipID is not null)
                ItemFactory.UnloadItem ((int) Session.ShipID);
        }
    }
}