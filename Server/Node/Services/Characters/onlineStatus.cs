using Node.Database;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class onlineStatus : Service
    {
        private CharacterDB mDB = null;
        
        public onlineStatus(ServiceManager manager) : base(manager)
        {
            this.mDB = manager.Container.ItemFactory.CharacterDB;
        }

        public PyDataType GetInitialState(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;
            
            return this.mDB.GetFriendsList(character);
        }

        public PyDataType GetOnlineStatus(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            // TODO: PROPERLY IMPLEMENT THIS
            return false;
        }
    }
}