using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Primitives;

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
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;
            
            return this.DB.GetFriendsList(character);
        }

        public PyDataType GetOnlineStatus(PyInteger characterID, CallInformation call)
        {
            // TODO: PROPERLY IMPLEMENT THIS
            return false;
        }
    }
}