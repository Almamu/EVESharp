using System;
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
        private ChatDB ChatDB { get; }
        private CharacterDB CharacterDB { get; }
        private ItemManager ItemManager { get; }
        
        public onlineStatus(ChatDB chatDB, CharacterDB characterDB, ItemManager itemManager)
        {
            this.ChatDB = chatDB;
            this.CharacterDB = characterDB;
            this.ItemManager = itemManager;
        }

        public PyDataType GetInitialState(CallInformation call)
        {
            // TODO: CHECK IF THE OTHER CHARACTER HAS US IN THEIR ADDRESSBOOK
            return this.ChatDB.GetAddressBookMembers(call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType GetOnlineStatus(PyInteger characterID, CallInformation call)
        {
            // TODO: CHECK IF THE OTHER CHARACTER HAS US IN THEIR ADDRESSBOOK?
            try
            {
                Character character = this.ItemManager.GetItem(characterID) as Character;

                return character.Online;
            }
            catch (Exception)
            {
                return this.CharacterDB.IsOnline(characterID);
            }
        }
    }
}