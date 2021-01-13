using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

namespace Node.Services.Characters
{
    public class onlineStatus : Service
    {
        private CharacterDB DB { get; }
        private ItemManager ItemManager { get; }
        
        public onlineStatus(CharacterDB db, ItemManager itemManager)
        {
            this.DB = db;
            this.ItemManager = itemManager;
        }

        public PyDataType GetInitialState(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;
            
            return this.DB.GetFriendsList(character);
        }

        public PyDataType GetOnlineStatus(PyInteger characterID, PyDictionary namedPayload, Client client)
        {
            // TODO: PROPERLY IMPLEMENT THIS
            return false;
        }
    }
}