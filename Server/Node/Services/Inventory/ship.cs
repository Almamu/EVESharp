using System;
using Common.Services;
using Node.Exceptions.jumpCloneSvc;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Services.Characters;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Inventory
{
    public class ship : BoundService
    {
        private ItemEntity Location { get; }
        private ItemManager ItemManager { get; }
        private TypeManager TypeManager { get; }
        public ship(ItemManager itemManager, TypeManager typeManager, BoundServiceManager manager) : base(manager)
        {
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
        }

        protected ship(ItemEntity location, ItemManager itemManager, TypeManager typeManager,
            BoundServiceManager manager) : this(itemManager, typeManager, manager)
        {
            this.Location = location;
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            /**
             * [0] => stationID/solarSystemID
             * [1] => groupID
             */

            if (objectData is PyTuple == false)
                throw new CustomError("Cannot bind ship service to unknown object");

            PyTuple tuple = objectData as PyTuple;

            if (tuple.Count != 2)
                throw new CustomError("Cannot bind ship service to unknown object");

            if (tuple[0] is PyInteger == false || tuple[1] is PyInteger == false)
                throw new CustomError("Cannot bind ship service to unknown object");

            PyInteger locationID = tuple[0] as PyInteger;
            PyInteger group = tuple[1] as PyInteger;

            if (group != (int) ItemGroups.Station && group != (int) ItemGroups.SolarSystem)
                throw new CustomError("Cannot bind ship service to non-solarsystem and non-station locations");
            if (this.ItemManager.IsItemLoaded(locationID) == false)
                throw new CustomError("This bind request does not belong here");

            ItemEntity location = this.ItemManager.GetItem(locationID);

            if (location.Type.Group.ID != group)
                throw new CustomError("Location and group do not match");

            return new ship(location, this.ItemManager, this.TypeManager, this.BoundServiceManager);
        }

        public PyDataType LeaveShip(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            Character character = this.ItemManager.GetItem(callerCharacterID) as Character;
            // get the item type
            ItemType capsuleType = this.TypeManager[ItemTypes.Capsule];
            // create a pod for this character
            ItemInventory capsule = this.ItemManager.CreateShip(capsuleType, this.Location, character);
            // update capsule's name
            capsule.Name = character.Name + "'s Capsule";
            // change character's location to the pod
            character.LocationID = capsule.ID;
            // notify the client about the item changes
            call.Client.NotifyItemLocationChange(capsule, ItemFlags.Capsule, capsule.LocationID);
            call.Client.NotifyItemLocationChange(character, ItemFlags.Pilot, (int) call.Client.ShipID);
            // update session
            call.Client.ShipID = capsule.ID;
            
            // persist changes!
            capsule.Persist();
            character.Persist();
            
            // TODO: CHECKS FOR IN-SPACE LEAVING!

            return capsule.ID;
        }

        public PyDataType Board(PyInteger itemID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            // ensure the item is loaded somewhere in this node
            // this will usually be taken care by the EVE Client
            if (this.ItemManager.IsItemLoaded(itemID) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Ship newShip = this.ItemManager.GetItem(itemID) as Ship;
            Character character = this.ItemManager.GetItem(callerCharacterID) as Character;
            Ship currentShip = this.ItemManager.GetItem((int) call.Client.ShipID) as Ship;

            if (newShip.Singleton == false)
                throw new UserError("TooFewSubSystemsToUndock");

            if (newShip.OwnerID != callerCharacterID)
                throw new MktNotOwner();

            // TODO: CHECKS FOR IN-SPACE BOARDING!
            
            // check skills required to board the given ship
            newShip.CheckShipPrerequisites(character);
            
            // move the character into this new ship
            character.LocationID = newShip.ID;
            // finally update the session
            call.Client.ShipID = newShip.ID;
            // notify the client about the change in location
            call.Client.NotifyItemLocationChange(character, ItemFlags.Pilot, currentShip.ID);

            character.Persist();

            // ensure the character is not removed when the capsule is removed
            currentShip.RemoveItem(character);

            if (currentShip.Type.ID == (int) ItemTypes.Capsule)
            {
                // destroy the pod from the database
                this.ItemManager.DestroyItem(currentShip);
                // notify the player of the item change
                call.Client.NotifyItemLocationChange(currentShip, currentShip.Flag, this.Location.ID);
            }
            
            return null;
        }

        public PyDataType AssembleShip(PyInteger itemID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            // ensure the item is loaded somewhere in this node
            // this will usually be taken care by the EVE Client
            if (this.ItemManager.IsItemLoaded(itemID) == false)
                throw new CustomError("Ships not loaded for player and hangar!");

            Ship ship = this.ItemManager.GetItem(itemID) as Ship;
            Character character = this.ItemManager.GetItem(callerCharacterID) as Character;

            if (ship.OwnerID != callerCharacterID)
                throw new MktNotOwner();

            // do not do anything if item is already assembled
            if (ship.Singleton == true)
                return null;

            // first split the stack
            if (ship.Quantity > 1)
            {
                // subtract one off the stack
                ship.Quantity -= 1;
                ship.Persist();
                // notify the quantity change
                call.Client.NotifyItemQuantityChange(ship, ship.Quantity + 1);
  
                // create the new item in the database
                Station station = this.ItemManager.GetStation((int) call.Client.StationID);
                ship = this.ItemManager.CreateShip(ship.Type, station, character);
                // notify the new item
                call.Client.NotifyNewItem(ship);
                // ensure the item is in the meta inventory
                try
                {
                    ItemInventory metaInventory =
                        this.ItemManager.MetaInventoryManager.GetOwnerInventoriesAtLocation(ship.LocationID, character.ID);

                    metaInventory.AddItem(ship);
                }
                catch (Exception)
                {
                    
                }
            }
            else
            {
                // stack of one, simple as changing the singleton flag
                ship.Singleton = true;
                call.Client.NotifySingletonChange(ship, false);                
            }

            // save the ship
            ship.Persist();

            return null;
        }

        public PyDataType AssembleShip(PyList itemIDs, CallInformation call)
        {
            foreach (PyDataType itemID in itemIDs)
            {
                // ignore item
                if (itemID is PyInteger == false)
                    continue;

                this.AssembleShip(itemID as PyInteger, call);
            }
            
            return null;
        }
    }
}