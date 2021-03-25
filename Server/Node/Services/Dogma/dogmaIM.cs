using System;
using System.Collections.Generic;
using Common.Constants;
using Node.Dogma;
using Node.Dogma.Interpreter;
using Node.Dogma.Interpreter.Opcodes;
using Node.Exceptions;
using Node.Exceptions.dogma;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Dogma;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using Environment = Node.Inventory.Items.Dogma.Environment;

namespace Node.Services.Dogma
{
    public class dogmaIM : BoundService
    {
        private ItemManager ItemManager { get; }
        private AttributeManager AttributeManager { get; }
        private SystemManager SystemManager { get; }
        private DogmaExpressionManager ExpressionManager { get; }
        
        public dogmaIM(ItemManager itemManager, AttributeManager attributeManager, SystemManager systemManager, BoundServiceManager manager, DogmaExpressionManager expressionManager) : base(manager, null)
        {
            this.ItemManager = itemManager;
            this.AttributeManager = attributeManager;
            this.SystemManager = systemManager;
            this.ExpressionManager = expressionManager;
        }

        protected dogmaIM(ItemManager itemManager, AttributeManager attributeManager, SystemManager systemManager,
            BoundServiceManager manager, DogmaExpressionManager expressionManager, Client client) : base(manager, client)
        {
            this.ItemManager = itemManager;
            this.AttributeManager = attributeManager;
            this.SystemManager = systemManager;
            this.ExpressionManager = expressionManager;
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

            if (groupID == (int) ItemGroups.SolarSystem)
                solarSystemID = this.ItemManager.GetStaticSolarSystem(entityID).ID;
            else if (groupID == (int) ItemGroups.Station)
                solarSystemID = this.ItemManager.GetStaticStation(entityID).SolarSystemID;
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

            return new dogmaIM(this.ItemManager, this.AttributeManager, this.SystemManager, this.BoundServiceManager, this.ExpressionManager);
        }

        public PyDataType ShipGetInfo(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            int? shipID = call.Client.ShipID;
            
            if (shipID is null)
                throw new CustomError($"The character is not aboard any ship");
            
            // TODO: RE-EVALUATE WHERE THE SHIP LOADING IS PERFORMED, SHIPGETINFO DOESN'T LOOK LIKE A GOOD PLACE TO DO IT
            Ship ship = this.ItemManager.LoadItem<Ship>((int) shipID);

            if (ship is null)
                throw new CustomError($"Cannot get information for ship {call.Client.ShipID}");
            if (ship.OwnerID != callerCharacterID)
                throw new CustomError("The ship you're trying to get info off does not belong to you");
            
            PyItemInfo itemInfo = new PyItemInfo();
            
            // TODO: find all the items inside this ship that are not characters
            itemInfo.AddRow(
	            ship.ID, ship.GetEntityRow(), ship.GetEffects (), ship.Attributes, DateTime.UtcNow.ToFileTime()
	        );

            foreach ((int _, ItemEntity item) in ship.Items)
            {
                switch (item.Flag)
                {
                    case ItemFlags.HiSlot0:
                    case ItemFlags.HiSlot1:
                    case ItemFlags.HiSlot2:
                    case ItemFlags.HiSlot3:
                    case ItemFlags.HiSlot4:
                    case ItemFlags.HiSlot5:
                    case ItemFlags.HiSlot6:
                    case ItemFlags.HiSlot7:
                    case ItemFlags.MedSlot0:
                    case ItemFlags.MedSlot1:
                    case ItemFlags.MedSlot2:
                    case ItemFlags.MedSlot3:
                    case ItemFlags.MedSlot4:
                    case ItemFlags.MedSlot5:
                    case ItemFlags.MedSlot6:
                    case ItemFlags.MedSlot7:
                    case ItemFlags.LoSlot0:
                    case ItemFlags.LoSlot1:
                    case ItemFlags.LoSlot2:
                    case ItemFlags.LoSlot3:
                    case ItemFlags.LoSlot4:
                    case ItemFlags.LoSlot5:
                    case ItemFlags.LoSlot6:
                    case ItemFlags.LoSlot7:
                    case ItemFlags.FixedSlot:
                    case ItemFlags.RigSlot0:
                    case ItemFlags.RigSlot1:
                    case ItemFlags.RigSlot2:
                    case ItemFlags.RigSlot3:
                    case ItemFlags.RigSlot4:
                    case ItemFlags.RigSlot5:
                    case ItemFlags.RigSlot6:
                    case ItemFlags.RigSlot7:
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

        public PyDataType CharGetInfo(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();

            Character character = this.ItemManager.GetItem<Character>(callerCharacterID);

            if (character is null)
                throw new CustomError($"Cannot get information for character {callerCharacterID}");

            PyItemInfo itemInfo = new PyItemInfo();

            itemInfo.AddRow(
                character.ID, character.GetEntityRow(), character.GetEffects(), character.Attributes, DateTime.UtcNow.ToFileTime()
            );

            foreach ((int _, ItemEntity item) in character.Items)
            {
                switch (item.Flag)
                {
                    case ItemFlags.Booster:
                    case ItemFlags.Implant:
                    case ItemFlags.Skill:
                    case ItemFlags.SkillInTraining:
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

            ItemEntity item = this.ItemManager.LoadItem(itemID);

            if (item.OwnerID != callerCharacterID && item.OwnerID != call.Client.CorporationID)
                throw new TheItemIsNotYoursToTake(itemID);
            
            return new Row(
                new PyList(5)
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
            
            Character character = this.ItemManager.GetItem<Character>(callerCharacterID);

            if (character is null)
                throw new CustomError($"Cannot get information for character {callerCharacterID}");

            return new PyDictionary
            {
                [(int) AttributeEnum.willpower] = character.Willpower,
                [(int) AttributeEnum.charisma] = character.Charisma,
                [(int) AttributeEnum.intelligence] = character.Intelligence,
                [(int) AttributeEnum.perception] = character.Perception,
                [(int) AttributeEnum.memory] = character.Memory
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

            ItemEntity item = this.ItemManager.GetItem(itemID);

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
            this.ItemManager.GetItem<ShipModule>(itemID).ApplyEffect(effectName, call.Client);
            
            return null;
        }

        public PyDataType Deactivate(PyInteger itemID, PyString effectName, CallInformation call)
        {
            this.ItemManager.GetItem<ShipModule>(itemID).StopApplyingEffect(effectName, call.Client);
            
            return null;
        }
    }
}