using System;
using System.Collections.Generic;
using Common.Logging;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Network;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Tls;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Dogma
{
    public class dogmaIM : BoundService
    {
        private int mObjectID;
        
        private ItemManager ItemManager { get; }
        public dogmaIM(ItemManager itemManager, BoundServiceManager manager) : base(manager)
        {
            this.ItemManager = itemManager;
        }

        private dogmaIM(ItemManager itemManager, BoundServiceManager manager, int objectID) : base(manager)
        {
            this.ItemManager = itemManager;
            this.mObjectID = objectID;
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            PyTuple tupleData = objectData as PyTuple;
            
            return new dogmaIM(this.ItemManager, this.BoundServiceManager, tupleData[0] as PyInteger);
        }

        public PyDataType ShipGetInfo(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (call.Client.ShipID == null)
                throw new CustomError($"The character is not aboard any ship");
            
            Ship ship = this.ItemManager.LoadItem((int) call.Client.ShipID) as Ship;

            if (ship == null)
                throw new CustomError($"Cannot get information for ship {call.Client.ShipID}");
            if (ship.OwnerID != callerCharacterID)
                throw new CustomError("The ship you're trying to get info off does not belong to you");
            
            PyItemInfo itemInfo = new PyItemInfo();
            
            // TODO: find all the items inside this ship that are not characters
            itemInfo.AddRow(
	            ship.ID, ship.GetEntityRow(), ship.GetEffects (), ship.Attributes, DateTime.UtcNow.ToFileTime()
	        );

            foreach (KeyValuePair<int, ItemEntity> pair in ship.Items)
            {
                switch (pair.Value.Flag)
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
                            pair.Value.ID,
                            pair.Value.GetEntityRow(),
                            pair.Value.GetEffects (),
                            pair.Value.Attributes,
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

            Character character = this.ItemManager.LoadItem(callerCharacterID) as Character;

            if (character == null)
                throw new CustomError($"Cannot get information for character {callerCharacterID}");

            PyItemInfo itemInfo = new PyItemInfo();

            itemInfo.AddRow(
                character.ID, character.GetEntityRow(), character.GetEffects(), character.Attributes, DateTime.UtcNow.ToFileTime()
            );

            foreach (KeyValuePair<int, ItemEntity> pair in character.Items)
            {
                switch (pair.Value.Flag)
                {
                    case ItemFlags.Booster:
                    case ItemFlags.Implant:
                    case ItemFlags.Skill:
                    case ItemFlags.SkillInTraining:
                        itemInfo.AddRow(
                            pair.Value.ID,
                            pair.Value.GetEntityRow(),
                            pair.Value.GetEffects (),
                            pair.Value.Attributes,
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
                throw new UserError("TheItemIsNotYoursToTake", new PyDictionary()
                {
                    {"item", itemID}
                });
            
            return new Row(
                (PyList) new PyDataType[]
                {
                    "itemID", "invItem", "activeEffects", "attributes", "time"
                },
                (PyList) new PyDataType[]
                {
                    item.ID, item.GetEntityRow(), item.GetEffects(), item.Attributes, DateTime.UtcNow.ToFileTimeUtc()
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
            
            Character character = this.ItemManager.LoadItem(callerCharacterID) as Character;

            if (character == null)
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
    }
}