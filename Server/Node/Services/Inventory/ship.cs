using Common.Services;
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
            ItemEntity capsule =
                this.ItemManager.CreateSimpleItem(character.Name + "'s Capsule", capsuleType, character, this.Location, ItemFlags.Hangar, singleton: true);
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
            
            return capsule.ID;
        }
    }
}