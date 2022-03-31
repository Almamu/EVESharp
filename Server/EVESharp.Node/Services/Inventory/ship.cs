using System;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.EVE.Services;
using EVESharp.EVE.Sessions;
using EVESharp.Node.Dogma;
using EVESharp.Node.Exceptions.ship;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.Notifications.Client.Inventory;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Exceptions.jumpCloneSvc;
using EVESharp.Node.Sessions;
using EVESharp.Node.StaticData;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using SessionManager = EVESharp.Node.Sessions.SessionManager;
using Type = EVESharp.Node.StaticData.Inventory.Type;

namespace EVESharp.Node.Services.Inventory
{
    public class ship : ClientBoundService
    {
        public override AccessLevel AccessLevel => AccessLevel.None;
        private ItemEntity Location { get; }
        private ItemFactory ItemFactory { get; }
        private TypeManager TypeManager => this.ItemFactory.TypeManager;
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private SessionManager SessionManager { get; }
        private Node.Dogma.Dogma Dogma { get; }
        public ship(ItemFactory itemFactory, BoundServiceManager manager, SessionManager sessionManager, Node.Dogma.Dogma dogma) : base(manager)
        {
            this.ItemFactory = itemFactory;
            this.SessionManager = sessionManager;
            this.Dogma = dogma;
        }

        protected ship(ItemEntity location, ItemFactory itemFactory, BoundServiceManager manager, SessionManager sessionManager, Node.Dogma.Dogma dogma, Session session) : base(manager, session, location.ID)
        {
            this.Location = location;
            this.ItemFactory = itemFactory;
            this.SessionManager = sessionManager;
            this.Dogma = dogma;
        }

        public PyInteger LeaveShip(CallInformation call)
        {
            int callerCharacterID = call.Session.EnsureCharacterIsSelected();

            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            // get the item type
            StaticData.Inventory.Type capsuleType = this.TypeManager[Types.Capsule];
            // create a pod for this character
            ItemInventory capsule = this.ItemFactory.CreateShip(capsuleType, this.Location, character);
            // update capsule's name
            capsule.Name = character.Name + "'s Capsule";
            // change character's location to the pod
            character.LocationID = capsule.ID;
            // notify the client about the item changes
            this.Dogma.QueueMultiEvent(callerCharacterID, OnItemChange.BuildLocationChange(capsule, Flags.Capsule, this.ItemFactory.LocationRecycler.ID));
            this.Dogma.QueueMultiEvent(callerCharacterID, OnItemChange.BuildLocationChange(character, Flags.Pilot, call.Session.ShipID));
            // notify the client
            this.SessionManager.PerformSessionUpdate(Session.CHAR_ID, callerCharacterID, new Session() { ShipID = capsule.ID });
            
            // persist changes!
            capsule.Persist();
            character.Persist();
            
            // TODO: CHECKS FOR IN-SPACE LEAVING!

            return capsule.ID;
        }

        public PyDataType Board(PyInteger itemID, CallInformation call)
        {
            int callerCharacterID = call.Session.EnsureCharacterIsSelected();

            // ensure the item is loaded somewhere in this node
            // this will usually be taken care by the EVE Client
            if (this.ItemFactory.TryGetItem(itemID, out Ship newShip) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);
            Ship currentShip = this.ItemFactory.GetItem<Ship>((int) call.Session.ShipID);

            if (newShip.Singleton == false)
                throw new CustomError("TooFewSubSystemsToUndock");

            // TODO: CHECKS FOR IN-SPACE BOARDING!
            
            // check skills required to board the given ship
            newShip.EnsureOwnership(callerCharacterID, call.Session.CorporationID, call.Session.CorporationRole, true);
            newShip.CheckPrerequisites(character);
            
            // move the character into this new ship
            newShip.AddItem(character);
            character.LocationID = newShip.ID;
            // finally update the session
            this.SessionManager.PerformSessionUpdate(Session.CHAR_ID, callerCharacterID, new Session(){ShipID = newShip.ID});
            // notify the client about the change in location
            this.Dogma.QueueMultiEvent(callerCharacterID, OnItemChange.BuildLocationChange(character, Flags.Pilot, currentShip.ID));

            character.Persist();

            // ensure the character is not removed when the capsule is removed
            currentShip.RemoveItem(character);

            if (currentShip.Type.ID == (int) Types.Capsule)
            {
                // destroy the pod from the database
                this.ItemFactory.DestroyItem(currentShip);
                // notify the player of the item change
                this.Dogma.QueueMultiEvent(callerCharacterID, OnItemChange.BuildLocationChange(currentShip, this.Location.ID));
            }
            
            return null;
        }

        public PyDataType AssembleShip(PyInteger itemID, CallInformation call)
        {
            int callerCharacterID = call.Session.EnsureCharacterIsSelected();
            int stationID = call.Session.EnsureCharacterIsInStation();
            
            // ensure the item is loaded somewhere in this node
            // this will usually be taken care by the EVE Client
            if (this.ItemFactory.TryGetItem(itemID, out Ship ship) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);

            if (ship.OwnerID != callerCharacterID)
                throw new AssembleOwnShipsOnly(ship.OwnerID);

            // do not do anything if item is already assembled
            if (ship.Singleton == true)
                return new ShipAlreadyAssembled(ship.Type);

            // first split the stack
            if (ship.Quantity > 1)
            {
                // subtract one off the stack
                ship.Quantity -= 1;
                ship.Persist();
                // notify the quantity change
                this.Dogma.QueueMultiEvent(callerCharacterID, OnItemChange.BuildQuantityChange(ship, ship.Quantity + 1));
  
                // create the new item in the database
                Station station = this.ItemFactory.GetStaticStation(stationID);
                ship = this.ItemFactory.CreateShip(ship.Type, station, character);
                // notify the new item
                this.Dogma.QueueMultiEvent(callerCharacterID, OnItemChange.BuildNewItemChange(ship));
            }
            else
            {
                // stack of one, simple as changing the singleton flag
                ship.Singleton = true;
                this.Dogma.QueueMultiEvent(callerCharacterID, OnItemChange.BuildSingletonChange(ship, false));                
            }

            // save the ship
            ship.Persist();

            return null;
        }

        public PyDataType AssembleShip(PyList itemIDs, CallInformation call)
        {
            foreach (PyInteger itemID in itemIDs.GetEnumerable<PyInteger>())
                this.AssembleShip(itemID, call);

            return null;
        }

        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            int solarSystemID = 0;

            if (parameters.ExtraValue == (int) Groups.SolarSystem)
                solarSystemID = this.ItemFactory.GetStaticSolarSystem(parameters.ObjectID).ID;
            else if (parameters.ExtraValue == (int) Groups.Station)
                solarSystemID = this.ItemFactory.GetStaticStation(parameters.ObjectID).SolarSystemID;
            else
                throw new CustomError("Unknown item's groupID");

            return this.SystemManager.LoadSolarSystemOnCluster(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            if (this.MachoResolveObject(bindParams, call) != this.BoundServiceManager.MachoNet.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            if (bindParams.ExtraValue != (int) Groups.Station && bindParams.ExtraValue != (int) Groups.SolarSystem)
                throw new CustomError("Cannot bind ship service to non-solarsystem and non-station locations");
            if (this.ItemFactory.TryGetItem(bindParams.ObjectID, out ItemEntity location) == false)
                throw new CustomError("This bind request does not belong here");

            if (location.Type.Group.ID != bindParams.ExtraValue)
                throw new CustomError("Location and group do not match");

            return new ship(location, this.ItemFactory, this.BoundServiceManager, this.SessionManager, this.Dogma, call.Session);
        }
    }
}