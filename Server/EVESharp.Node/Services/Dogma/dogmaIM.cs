using System;
using System.Collections.Generic;
using EVESharp.Common.Constants;
using EVESharp.EVE;
using EVESharp.EVE.Packets.Complex;
using EVESharp.EVE.Packets.Exceptions;
using EVESharp.Node.Exceptions.inventory;
using EVESharp.Node.Inventory;
using EVESharp.Node.Inventory.Items;
using EVESharp.Node.Inventory.Items.Types;
using EVESharp.Node.Network;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Dogma;
using EVESharp.Node.Dogma.Interpreter;
using EVESharp.Node.Dogma.Interpreter.Opcodes;
using EVESharp.Node.Exceptions;
using EVESharp.Node.Exceptions.dogma;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.Inventory.Items.Dogma;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;
using Environment = EVESharp.Node.Inventory.Items.Dogma.Environment;

namespace EVESharp.Node.Services.Dogma
{
    public class dogmaIM : ClientBoundService
    {
        private ItemFactory ItemFactory { get; }
        private AttributeManager AttributeManager => this.ItemFactory.AttributeManager;
        private SystemManager SystemManager => this.ItemFactory.SystemManager;
        
        public dogmaIM(ItemFactory itemFactory, BoundServiceManager manager) : base(manager)
        {
            this.ItemFactory = itemFactory;
        }

        protected dogmaIM(int locationID, ItemFactory itemFactory, BoundServiceManager manager, Client client) : base(manager, client, locationID)
        {
            this.ItemFactory = itemFactory;
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
            
            // ensure the player can use this ship
            ship.EnsureOwnership(callerCharacterID, call.Client.CorporationID, call.Client.CorporationRole, true);

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

        protected override long MachoResolveObject(ServiceBindParams parameters, CallInformation call)
        {
            int solarSystemID = 0;

            if (parameters.ExtraValue == (int) Groups.SolarSystem)
                solarSystemID = this.ItemFactory.GetStaticSolarSystem(parameters.ObjectID).ID;
            else if (parameters.ExtraValue == (int) Groups.Station)
                solarSystemID = this.ItemFactory.GetStaticStation(parameters.ObjectID).SolarSystemID;
            else
                throw new CustomError("Unknown item's groupID");

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(ServiceBindParams bindParams, CallInformation call)
        {
            if (this.MachoResolveObject(bindParams, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            return new dogmaIM(bindParams.ObjectID, this.ItemFactory, this.BoundServiceManager, call.Client);
        }
    }
}