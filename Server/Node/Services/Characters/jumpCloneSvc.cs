using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Database;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class jumpCloneSvc : BoundService
    {
        public jumpCloneSvc(ServiceManager manager) : base(manager)
        {
        }
        
        protected override Service CreateBoundInstance(PyDataType objectData)
        {
            PyTuple tupleData = objectData as PyTuple;
            
            /*
             * objectData [0] => entityID (station or solar system)
             * objectData [1] => groupID (station or solar system)
             */
            
            return new jumpCloneSvc(this.ServiceManager);
        }

        public PyDataType GetCloneState(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;

            return KeyVal.FromDictionary(new PyDictionary
                {
                    ["clones"] =
                        this.ServiceManager.Container.ItemFactory.ItemDB
                            .GetClonesForCharacter((int) client.CharacterID),
                    ["implants"] =
                        this.ServiceManager.Container.ItemFactory.ItemDB.GetImplantsForCharacterClones(
                            (int) client.CharacterID),
                    ["timeLastJump"] = character.TimeLastJump
                }
            );
        }

        public PyDataType DestroyInstalledClone(PyInteger jumpCloneID, PyDictionary namedPayload, Client client)
        {
            // if the clone is not loaded the clone cannot be removed, players can only remove clones from where they're at
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            if (this.ServiceManager.Container.ItemFactory.ItemManager.IsItemLoaded(jumpCloneID) == false)
                throw new CustomError("Cannot remotely destroy a clone");

            ItemEntity clone = this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem(jumpCloneID);
            
            if (clone.LocationID != client.LocationID)
                throw new UserError("JumpCantDestroyNonLocalClone");
            if (clone.OwnerID != (int) client.CharacterID)
                throw new UserError("MktNotOwner");

            // finally destroy the clone, this also destroys all the implants in it
            this.ServiceManager.Container.ItemFactory.ItemManager.DestroyItem(clone);
            
            // let the client know that the clones were updated
            client.NotifyCloneUpdate();
            
            return null;
        }

        public PyDataType GetShipCloneState(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            return this.ServiceManager.Container.ItemFactory.ItemDB.GetClonesInShipForCharacter(
                (int) client.CharacterID);
            
        }

        public PyDataType CloneJump(PyInteger locationID, PyBool unknown, PyDictionary namedPayload, Client client)
        {
            return null;
        }
    }
}