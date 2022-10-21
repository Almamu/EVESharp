using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Groups;
using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.ship;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Sessions;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.Node.Services.Inventory;

[MustBeCharacter]
public class ship : ClientBoundService
{
    public override AccessLevel         AccessLevel        => AccessLevel.None;
    private         ItemEntity          Location           { get; }
    private         IItems              Items              { get; }
    private         ITypes              Types              => this.Items.Types;
    private         ISolarSystems       SolarSystems       { get; }
    private         ISessionManager     SessionManager     { get; }
    private         IDogmaNotifications DogmaNotifications { get; }
    private         IDatabase           Database           { get; }
    private         IDogmaItems         DogmaItems         { get; }

    public ship
    (
        IItems        items, IBoundServiceManager manager, ISessionManager sessionManager, IDogmaNotifications dogmaNotifications, IDatabase database,
        ISolarSystems solarSystems, IDogmaItems dogmaItems
    ) : base (manager)
    {
        Items              = items;
        SessionManager     = sessionManager;
        DogmaNotifications = dogmaNotifications;
        Database           = database;
        SolarSystems       = solarSystems;
        DogmaItems         = dogmaItems;
    }

    protected ship
    (
        ItemEntity location, IItems items, IBoundServiceManager manager, ISessionManager sessionManager, IDogmaNotifications dogmaNotifications, Session session,
        ISolarSystems solarSystems, IDogmaItems dogmaItems
    ) : base (manager, session, location.ID)
    {
        Location           = location;
        Items              = items;
        SessionManager     = sessionManager;
        DogmaNotifications = dogmaNotifications;
        SolarSystems       = solarSystems;
        DogmaItems         = dogmaItems;
    }

    public PyInteger LeaveShip (ServiceCall call)
    {
        int callerCharacterID = call.Session.CharacterID;

        Character character = this.Items.GetItem <Character> (callerCharacterID);
        // get the item type
        Type capsuleType = this.Types [TypeID.Capsule];
        // create a pod for this character
        ItemInventory capsule = this.Items.CreateShip (capsuleType, Location, character);
        // update capsule's name
        capsule.Name = character.Name + "'s Capsule";
        // change character's location to the pod
        character.LocationID = capsule.ID;
        // notify the client about the item changes
        this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildLocationChange (capsule, Flags.Capsule, this.Items.LocationRecycler.ID));
        this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildLocationChange (character, Flags.Pilot, call.Session.ShipID));
        // notify the client
        SessionManager.PerformSessionUpdate (Session.CHAR_ID, callerCharacterID, new Session {ShipID = capsule.ID});

        // persist changes!
        capsule.Persist ();
        character.Persist ();

        // TODO: CHECKS FOR IN-SPACE LEAVING!

        return capsule.ID;
    }

    public PyDataType Board (ServiceCall call, PyInteger itemID)
    {
        int callerCharacterID = call.Session.CharacterID;

        // ensure the item is loaded somewhere in this node
        // this will usually be taken care by the EVE Client
        if (this.Items.TryGetItem (itemID, out Ship newShip) == false)
            throw new CustomError ("Ships not loaded for player and hangar!");

        Character character   = this.Items.GetItem <Character> (callerCharacterID);
        Ship      currentShip = this.Items.GetItem <Ship> ((int) call.Session.ShipID);

        if (newShip.Singleton == false)
            throw new CustomError ("TooFewSubSystemsToUndock");

        // TODO: CHECKS FOR IN-SPACE BOARDING!

        // check skills required to board the given ship
        newShip.EnsureOwnership (callerCharacterID, call.Session.CorporationID, call.Session.CorporationRole, true);
        newShip.CheckPrerequisites (character);

        // move the character into this new ship
        newShip.AddItem (character);
        character.LocationID = newShip.ID;
        // finally update the session
        SessionManager.PerformSessionUpdate (Session.CHAR_ID, callerCharacterID, new Session {ShipID = newShip.ID});
        // notify the client about the change in location
        this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildLocationChange (character, Flags.Pilot, currentShip.ID));

        character.Persist ();

        // ensure the character is not removed when the capsule is removed
        currentShip.RemoveItem (character);

        if (currentShip.Type.ID == (int) TypeID.Capsule)
            DogmaItems.DestroyItem (currentShip);
        
        return null;
    }

    [MustBeInStation]
    public PyDataType AssembleShip (ServiceCall call, PyInteger itemID)
    {
        int callerCharacterID = call.Session.CharacterID;
        int stationID         = call.Session.StationID;

        // ensure the item is loaded somewhere in this node
        // this will usually be taken care by the EVE Client
        if (this.Items.TryGetItem (itemID, out Ship ship) == false)
            throw new CustomError ("Ships not loaded for player and hangar!");

        Character character = this.Items.GetItem <Character> (callerCharacterID);

        if (ship.OwnerID != callerCharacterID)
            throw new AssembleOwnShipsOnly (ship.OwnerID);

        // do not do anything if item is already assembled
        if (ship.Singleton)
            return new ShipAlreadyAssembled (ship.Type);

        // first split the stack
        if (ship.Quantity > 1)
        {
            // subtract one off the stack
            ship.Quantity -= 1;
            ship.Persist ();
            // notify the quantity change
            this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildQuantityChange (ship, ship.Quantity + 1));

            // create the new item in the database
            Station station = this.Items.GetStaticStation (stationID);
            ship = this.Items.CreateShip (ship.Type, station, character);
            // notify the new item
            this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildNewItemChange (ship));
        }
        else
        {
            // stack of one, simple as changing the singleton flag
            ship.Singleton = true;
            this.DogmaNotifications.QueueMultiEvent (callerCharacterID, OnItemChange.BuildSingletonChange (ship, false));
        }

        // save the ship
        ship.Persist ();

        return null;
    }

    public PyDataType AssembleShip (ServiceCall call, PyList itemIDs)
    {
        foreach (PyInteger itemID in itemIDs.GetEnumerable <PyInteger> ())
            this.AssembleShip (call, itemID);

        return null;
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        return parameters.ExtraValue switch
        {
            (int) GroupID.SolarSystem => Database.CluResolveAddress ("solarsystem", parameters.ObjectID),
            (int) GroupID.Station     => Database.CluResolveAddress ("station",     parameters.ObjectID),
            _                         => throw new CustomError ("Unknown item's groupID")
        };
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        if (bindParams.ExtraValue != (int) GroupID.Station && bindParams.ExtraValue != (int) GroupID.SolarSystem)
            throw new CustomError ("Cannot bind ship service to non-solarsystem and non-station locations");

        if (this.Items.TryGetItem (bindParams.ObjectID, out ItemEntity location) == false)
            throw new CustomError ("This bind request does not belong here");

        if (location.Type.Group.ID != bindParams.ExtraValue)
            throw new CustomError ("Location and group do not match");

        return new ship (location, this.Items, BoundServiceManager, SessionManager, this.DogmaNotifications, call.Session, this.SolarSystems, DogmaItems);
    }
}