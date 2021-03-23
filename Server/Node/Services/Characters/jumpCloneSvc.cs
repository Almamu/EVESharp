using System.Collections.Generic;
using Node.Database;
using Node.Exceptions;
using Node.Exceptions.jumpCloneSvc;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Inventory.SystemEntities;
using Node.Market;
using Node.Network;
using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class jumpCloneSvc : BoundService
    {
        private ItemDB ItemDB { get; }
        private MarketDB MarketDB { get; }
        private ItemManager ItemManager { get; }
        private TypeManager TypeManager { get; }
        private SystemManager SystemManager { get; }
        
        public jumpCloneSvc(ItemDB itemDB, MarketDB marketDB, ItemManager itemManager, TypeManager typeManager,
            SystemManager systemManager, BoundServiceManager manager) : base(manager, null)
        {
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
            this.SystemManager = systemManager;
        }
        
        protected jumpCloneSvc(ItemDB itemDB, MarketDB marketDB, ItemManager itemManager, TypeManager typeManager,
            SystemManager systemManager, BoundServiceManager manager, Client client) : base(manager, client)
        {
            this.ItemDB = itemDB;
            this.MarketDB = marketDB;
            this.ItemManager = itemManager;
            this.TypeManager = typeManager;
            this.SystemManager = systemManager;
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
                solarSystemID = this.ItemManager.GetSolarSystem(entityID).ID;
            else if (groupID == (int) ItemGroups.Station)
                solarSystemID = this.ItemManager.GetStation(entityID).SolarSystemID;
            else
                throw new CustomError("Unknown item's groupID");

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            PyTuple tupleData = objectData as PyTuple;
            
            /*
             * objectData [0] => entityID (station or solar system)
             * objectData [1] => groupID (station or solar system)
             */

            if (this.MachoResolveObject(tupleData, 0, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");

            return new jumpCloneSvc(this.ItemDB, this.MarketDB, this.ItemManager, this.TypeManager, this.SystemManager, this.BoundServiceManager, call.Client);
        }

        public PyDataType GetCloneState(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            Character character = this.ItemManager.GetItem<Character>(callerCharacterID);

            return KeyVal.FromDictionary(new PyDictionary
                {
                    ["clones"] = this.ItemDB.GetClonesForCharacter(callerCharacterID, (int) character.ActiveCloneID),
                    ["implants"] = this.ItemDB.GetImplantsForCharacterClones(callerCharacterID),
                    ["timeLastJump"] = character.TimeLastJump
                }
            );
        }

        public PyDataType DestroyInstalledClone(PyInteger jumpCloneID, CallInformation call)
        {
            // if the clone is not loaded the clone cannot be removed, players can only remove clones from where they're at
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            if (this.ItemManager.TryGetItem(jumpCloneID, out ItemEntity clone) == false)
                throw new JumpCantDestroyNonLocalClone();
            if (clone.LocationID != call.Client.LocationID)
                throw new JumpCantDestroyNonLocalClone();
            if (clone.OwnerID != callerCharacterID)
                throw new MktNotOwner();

            // finally destroy the clone, this also destroys all the implants in it
            this.ItemManager.DestroyItem(clone);
            
            // let the client know that the clones were updated
            call.Client.NotifyCloneUpdate();
            
            return null;
        }

        public PyDataType GetShipCloneState(CallInformation call)
        {
            return this.ItemDB.GetClonesInShipForCharacter(call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType CloneJump(PyInteger locationID, PyBool unknown, CallInformation call)
        {
            // TODO: IMPLEMENT THIS CALL PROPERLY, INVOLVES SESSION CHANGES
            // TODO: AND SEND PROPER NOTIFICATION AFTER A JUMP CLONE OnJumpCloneTransitionCompleted
            return null;
        }

        public PyInteger GetPriceForClone(CallInformation call)
        {
            // TODO: CALCULATE THIS ON POS, AS THIS VALUE IS STATIC OTHERWISE
            
            // seems to be hardcoded for npc's stations
            return 100000;
        }

        public PyDataType InstallCloneInStation(CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            int stationID = call.Client.EnsureCharacterIsInStation();
            
            Character character = this.ItemManager.GetItem<Character>(callerCharacterID);
            
            // check the maximum number of clones the character has assigned
            long maximumClonesAvailable = character.GetSkillLevel(ItemTypes.InfomorphPsychology);
            
            // the skill is not trained
            if (maximumClonesAvailable == 0)
                throw new JumpCharStoringMaxClonesNone();

            // get list of clones (excluding the medical clone)
            Rowset clones = this.ItemDB.GetClonesForCharacter(character.ID, (int) character.ActiveCloneID);

            // ensure we don't have more than the allowed clones
            if (clones.Rows.Count >= maximumClonesAvailable)
                throw new JumpCharStoringMaxClones(clones.Rows.Count, maximumClonesAvailable);
            
            // ensure that the character has enough money
            int cost = this.GetPriceForClone(call);

            character.EnsureEnoughBalance(cost);
            
            // create an alpha clone
            ItemType cloneType = this.TypeManager[ItemTypes.CloneGradeAlpha];
            
            // get character's station
            Station station = this.ItemManager.GetStation(stationID);
            
            // create a new clone on the itemDB
            Clone clone = this.ItemManager.CreateClone(cloneType, station, character);

            // update the balance
            character.Balance -= cost;
            
            this.MarketDB.CreateJournalForCharacter(
                MarketReference.JumpCloneInstallationFee, character.ID, character.ID, null, station.ID,
                -cost, character.Balance, $"Installed clone at {station.Name}", 1000
            );

            // notify the client about the balance change
            call.Client.NotifyBalanceUpdate(character.Balance);
            
            // finally create the jump clone and invalidate caches
            call.Client.NotifyCloneUpdate();
            
            // persist the character information
            character.Persist();
            
            return null;
        }
    }
}