using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
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

        public PyDataType GetInitialState(CallInformation call)
        {
            if (call.Client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character = this.ItemManager.LoadItem((int) call.Client.CharacterID) as Character;
            
            return this.DB.GetFriendsList(character);
        }

        public PyDataType GetOnlineStatus(PyInteger characterID, CallInformation call)
        {
            // TODO: PROPERLY IMPLEMENT THIS
            return false;
        }
    }
}