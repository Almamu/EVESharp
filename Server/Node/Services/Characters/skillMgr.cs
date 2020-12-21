using System.Collections.Generic;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class skillMgr : BoundService
    {
        public skillMgr(ServiceManager manager) : base(manager)
        {
        }

        protected override Service CreateBoundInstance(PyDataType objectData)
        {
            /*
             * objectData [0] => entityID (station or solar system)
             * objectData [1] => groupID (station or solar system)
             */
            PyTuple tupleData = objectData as PyTuple;
            
            return new skillMgr(this.ServiceManager);
        }

        public PyDataType GetSkillQueue(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;

            PyList skillQueueList = new PyList(character.SkillQueue.Count);

            int index = 0;
            
            foreach (Character.SkillQueueEntry entry in character.SkillQueue)
            {
                skillQueueList[index++] = entry;
            }

            return skillQueueList;
        }
    }
}