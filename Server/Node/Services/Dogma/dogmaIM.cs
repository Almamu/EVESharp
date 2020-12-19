using System;
using System.Collections.Generic;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Tls;
using PythonTypes.Types.Complex;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Dogma
{
    public class dogmaIM : BoundService
    {
        private int mObjectID;
        
        public dogmaIM(ServiceManager manager) : base(manager)
        {
        }

        private dogmaIM(ServiceManager manager, int objectID) : base(manager)
        {
            this.mObjectID = objectID;
        }

        protected override Service CreateBoundInstance(PyDataType objectData)
        {
            PyTuple tupleData = objectData as PyTuple;
            
            return new dogmaIM(this.ServiceManager, tupleData[0] as PyInteger);
        }

        public PyDataType ShipGetInfo(PyDictionary namedPayload, Client client)
        {
            if (client.ShipID == null)
                throw new CustomError($"The character is not aboard any ship");
            
            Ship ship = this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.ShipID) as Ship;

            if (ship == null)
                throw new CustomError($"Cannot get information for ship {client.ShipID}");
            if (ship.OwnerID != client.CharacterID)
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

        public PyDataType CharGetInfo(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new CustomError("This client has not selected a character yet");

            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;

            if (character == null)
                throw new CustomError($"Cannot get information for character {client.CharacterID}");

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

        public PyDataType GetWeaponBankInfoForShip(PyDictionary namedPayload, Client client)
        {
            // this function seems to indicate the client when modules are grouped
            // so it can display them on the UI and I guess act on them too
            // for now there's no support for this functionality, so it can be stubbed out
            return new PyDictionary();
        }
    }
}