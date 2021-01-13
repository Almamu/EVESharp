using System.Collections.Generic;
using Common.Logging;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Market;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Characters
{
    public class jumpCloneSvc : BoundService
    {
        private ItemDB ItemDB { get; }
        private MarketDB MarketDB { get; }
        private ItemManager ItemManager { get; }
        private TypeManager TypeManager { get; }
        
        public jumpCloneSvc(ItemDB itemDB, MarketDB marketDB, ItemManager itemManager, TypeManager typeManager, BoundServiceManager manager, Logger logger) : base(manager, logger)
        {
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
        }
        
        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            PyTuple tupleData = objectData as PyTuple;
            
            /*
             * objectData [0] => entityID (station or solar system)
             * objectData [1] => groupID (station or solar system)
             */
            
            return new jumpCloneSvc(this.ItemDB, this.MarketDB, this.ItemManager, this.TypeManager, this.BoundServiceManager, this.Log.Logger);
        }

        public PyDataType GetCloneState(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            return KeyVal.FromDictionary(new PyDictionary
                {
                    ["clones"] = this.ItemDB.GetClonesForCharacter((int) client.CharacterID, (int) character.ActiveCloneID),
                    ["implants"] = this.ItemDB.GetImplantsForCharacterClones((int) client.CharacterID),
                    ["timeLastJump"] = character.TimeLastJump
                }
            );
        }

        public PyDataType DestroyInstalledClone(PyInteger jumpCloneID, PyDictionary namedPayload, Client client)
        {
            // if the clone is not loaded the clone cannot be removed, players can only remove clones from where they're at
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            if (this.ItemManager.IsItemLoaded(jumpCloneID) == false)
                throw new CustomError("Cannot remotely destroy a clone");

            ItemEntity clone = this.ItemManager.LoadItem(jumpCloneID);
            
            if (clone.LocationID != client.LocationID)
                throw new UserError("JumpCantDestroyNonLocalClone");
            if (clone.OwnerID != (int) client.CharacterID)
                throw new UserError("MktNotOwner");

            // finally destroy the clone, this also destroys all the implants in it
            this.ItemManager.DestroyItem(clone);
            
            // let the client know that the clones were updated
            client.NotifyCloneUpdate();
            
            return null;
        }

        public PyDataType GetShipCloneState(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.ItemDB.GetClonesInShipForCharacter((int) client.CharacterID);
        }

        public PyDataType CloneJump(PyInteger locationID, PyBool unknown, PyDictionary namedPayload, Client client)
        {
            // TODO: IMPLEMENT THIS CALL PROPERLY, INVOLVES SESSION CHANGES
            // TODO: AND SEND PROPER NOTIFICATION AFTER A JUMP CLONE OnJumpCloneTransitionCompleted
            return null;
        }

        public PyInteger GetPriceForClone(PyDictionary namedPayload, Client client)
        {
            // TODO: CALCULATE THIS ON POS, AS THIS VALUE IS STATIC OTHERWISE
            
            // seems to be hardcoded for npc's stations
            return 100000;
        }

        public PyDataType InstallCloneInStation(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            if (client.StationID == null)
                throw new UserError("CanOnlyDoInStations");
            
            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;
            
            // check the maximum number of clones the character has assigned
            Dictionary<int, Skill> injectedSkills = character.InjectedSkillsByTypeID;
            
            // the skill is not trained
            if (injectedSkills.ContainsKey((int) ItemTypes.InfomorphPsychology) == false)
                throw new UserError("JumpCharStoringMaxClonesNone");

            long maximumClonesAvailable = 0;
            
            // the skill is needed to be able to have installed clones
            maximumClonesAvailable = injectedSkills[(int) ItemTypes.InfomorphPsychology].Level;

            // get list of clones (excluding the medical clone)
            Rowset clones = this.ItemDB.GetClonesForCharacter(character.ID, (int) character.ActiveCloneID);

            // ensure we don't have more than the allowed clones
            if (clones.Rows.Count >= maximumClonesAvailable)
                throw new UserError("JumpCharStoringMaxClones", new PyDictionary
                    {
                        ["have"] = clones.Rows.Count,
                        ["max"] = maximumClonesAvailable
                    }
                );
            
            // ensure that the character has enough money
            int cost = this.GetPriceForClone(namedPayload, client);

            if (character.Balance < cost)
                throw new UserError("NotEnoughMoney", new PyDictionary
                    {
                        ["balance"] = character.Balance,
                        ["amount"] = cost
                    }
                );
            
            // create an alpha clone
            ItemType cloneType = this.TypeManager[ItemTypes.CloneGradeAlpha];
            
            // get character's station
            Station station = this.ItemManager.GetItem((int) client.StationID) as Station;
            
            // create a new clone on the itemDB
            Clone clone = this.ItemManager.CreateClone(cloneType, station, character);

            // update the balance
            character.Balance -= cost;
            
            this.MarketDB.CreateJournalForCharacter(
                station.ID, MarketReference.JumpCloneInstallationFee, character.ID, null, station.ID,
                -cost, character.Balance, $"Installed clone at {station.Name}", 1000
            );

            // notify the client about the balance change
            client.NotifyBalanceUpdate(character.Balance);
            
            // finally create the jump clone and invalidate caches
            client.NotifyCloneUpdate();
            
            return null;
        }
    }
}