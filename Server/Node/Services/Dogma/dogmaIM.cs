using System;
using System.Collections.Generic;
using Common.Constants;
using EVE;
using EVE.Packets.Complex;
using EVE.Packets.Exceptions;
using Node.Dogma;
using Node.Dogma.Interpreter;
using Node.Dogma.Interpreter.Opcodes;
using Node.Exceptions;
using Node.Exceptions.dogma;
using Node.Exceptions.inventory;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Dogma;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.StaticData.Inventory;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;
using Environment = Node.Inventory.Items.Dogma.Environment;

namespace Node.Services.Dogma
{
    public class dogmaIM : BoundService
    {
        private ItemFactory ItemFactory { get; }
        private AttributeManager AttributeManager => this.ItemFactory.AttributeManager;
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        private ExpressionManager ExpressionManager => this.ItemFactory.ExpressionManager;
        
        public dogmaIM(ItemFactory itemFactory, BoundServiceManager manager) : base(manager, null)
        {
            this.ItemFactory = itemFactory;
        }

        protected dogmaIM(ItemFactory itemFactory, BoundServiceManager manager, Client client) : base(manager, client)
        {
            this.ItemFactory = itemFactory;
        }

        public override PyInteger MachoResolveObject(PyTuple objectData, PyInteger zero, CallInformation call)
        {
            /*
             * objectData [0] => entityID (station or solar system)
             * objectData [1] => groupID (station or solar system)
             */

            PyDataType first = objectData[0];
            PyDataType second = objectData[1];

            if (first is PyInteger == false || second is PyInteger == false)
                throw new CustomError("Cannot resolve object");

            PyInteger entityID = first as PyInteger;
            PyInteger groupID = second as PyInteger;

            int solarSystemID = 0;

            if (groupID == (int) Groups.SolarSystem)
                solarSystemID = this.ItemFactory.GetStaticSolarSystem(entityID).ID;
            else if (groupID == (int) Groups.Station)
                solarSystemID = this.ItemFactory.GetStaticStation(entityID).SolarSystemID;
            else
                throw new CustomError("Unknown item's groupID");

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            if (this.MachoResolveObject(objectData as PyTuple, 0, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            return new dogmaIM(this.ItemFactory, this.BoundServiceManager);
        }

        public PyDataType ShipGetInfo(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            int? shipID = call.Client.ShipID;
            
            if (shipID is null)
                throw new CustomError($"The character is not aboard any ship");
            
            // TODO: RE-EVALUATE WHERE THE SHIP LOADING IS PERFORMED, SHIPGETINFO DOESN'T LOOK LIKE A GOOD PLACE TO DO IT
            Ship ship = this.ItemFactory.LoadItem<Ship>((int) shipID);

            if (ship is null)
                throw new CustomError($"Cannot get information for ship {call.Client.ShipID}");
            if (ship.OwnerID != callerCharacterID)
                throw new CustomError("The ship you're trying to get info off does not belong to you");
            
            ItemInfo itemInfo = new ItemInfo();
            
            // TODO: find all the items inside this ship that are not characters
            itemInfo.AddRow(
	            ship.ID, ship.GetEntityRow(), ship.GetEffects (), ship.Attributes, DateTime.UtcNow.ToFileTime()
	        );

            foreach ((int _, ItemEntity item) in ship.Items)
            {
                if (item.IsInModuleSlot() == false && item.IsInRigSlot() == true)
                    continue;
        
                itemInfo.AddRow(
                    item.ID,
                    item.GetEntityRow(),
                    item.GetEffects (),
                    item.Attributes,
                    DateTime.UtcNow.ToFileTime()
                );
                break;
            }

            return itemInfo;
        }

        public PyDataType CharGetInfo(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);

            if (character is null)
                throw new CustomError($"Cannot get information for character {callerCharacterID}");

            ItemInfo itemInfo = new ItemInfo();

            itemInfo.AddRow(
                character.ID, character.GetEntityRow(), character.GetEffects(), character.Attributes, DateTime.UtcNow.ToFileTime()
            );

            foreach ((int _, ItemEntity item) in character.Items)
            {
                switch (item.Flag)
                {
                    case Flags.Booster:
                    case Flags.Implant:
                    case Flags.Skill:
                    case Flags.SkillInTraining:
                        itemInfo.AddRow(
                            item.ID,
                            item.GetEntityRow(),
                            item.GetEffects (),
                            item.Attributes,
                            DateTime.UtcNow.ToFileTime()
                        );
                        break;
                }
            }

            return itemInfo;
        }

        public PyDataType ItemGetInfo(PyInteger itemID, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            ItemEntity item = this.ItemFactory.LoadItem(itemID);

            if (item.OwnerID != callerCharacterID && item.OwnerID != call.Client.CorporationID)
                throw new TheItemIsNotYoursToTake(itemID);
            
            return new Row(
                new PyList<PyString>(5)
                {
                    [0] = "itemID",
                    [1] = "invItem",
                    [2] = "activeEffects",
                    [3] = "attributes",
                    [4] = "time"
                },
                new PyList(5)
                {
                    [0] = item.ID,
                    [1] = item.GetEntityRow(),
                    [2] = item.GetEffects(),
                    [3] = item.Attributes,
                    [4] = DateTime.UtcNow.ToFileTimeUtc()
                }
            );
        }

        public PyDataType GetWeaponBankInfoForShip(CallInformation call)
        {
            // this function seems to indicate the client when modules are grouped
            // so it can display them on the UI and I guess act on them too
            // for now there's no support for this functionality, so it can be stubbed out
            return new PyDictionary();
        }

        public PyDataType GetCharacterBaseAttributes(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            Character character = this.ItemFactory.GetItem<Character>(callerCharacterID);

            if (character is null)
                throw new CustomError($"Cannot get information for character {callerCharacterID}");

            return new PyDictionary
            {
                [(int) Attributes.willpower] = character.Willpower,
                [(int) Attributes.charisma] = character.Charisma,
                [(int) Attributes.intelligence] = character.Intelligence,
                [(int) Attributes.perception] = character.Perception,
                [(int) Attributes.memory] = character.Memory
            };
        }

        public PyDataType LogAttribute(PyInteger itemID, PyInteger attributeID, CallInformation call)
        {
            return this.LogAttribute(itemID, attributeID, "", call);
        }

        public PyList<PyString> LogAttribute(PyInteger itemID, PyInteger attributeID, PyString reason, CallInformation call)
        {
            int role = call.Client.Role;
            int roleMask = (int) (Roles.ROLE_GDH | Roles.ROLE_QA | Roles.ROLE_PROGRAMMER | Roles.ROLE_GMH);

            if ((role & roleMask) == 0)
                throw new CustomError("Not allowed!");

            ItemEntity item = this.ItemFactory.GetItem(itemID);

            if (item.Attributes.AttributeExists(attributeID) == false)
                throw new CustomError("The given attribute doesn't exists in the item");
            
            // we don't know the actual values of the returned function
            // but it should be enough to fill the required data by the client
            return new PyList<PyString>(5)
            {
                [0] = null,
                [1] = null,
                [2] = $"Server value: {item.Attributes[attributeID]}",
                [3] = $"Base value: {AttributeManager.DefaultAttributes[item.Type.ID][attributeID]}",
                [4] = $"Reason: {reason}"
            };
        }

        public PyDataType Activate(PyInteger itemID, PyString effectName, PyDataType target, PyDataType repeat, CallInformation call)
        {
            this.ItemFactory.GetItem<ShipModule>(itemID).ApplyEffect(effectName, call.Client);
            
            return null;
        }

        public PyDataType Deactivate(PyInteger itemID, PyString effectName, CallInformation call)
        {
            this.ItemFactory.GetItem<ShipModule>(itemID).StopApplyingEffect(effectName, call.Client);
            
            return null;
        }
    }
}