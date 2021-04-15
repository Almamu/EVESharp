using System;
using Common.Services;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class onlineStatus : IService
    {
        private ChatDB ChatDB { get; }
        private CharacterDB CharacterDB { get; }
        private ItemFactory ItemFactory { get; }
        
        public onlineStatus(ChatDB chatDB, CharacterDB characterDB, ItemFactory itemFactory)
        {
            this.ChatDB = chatDB;
            this.CharacterDB = characterDB;
            this.ItemFactory = itemFactory;
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
                return this.ItemFactory.GetItem<Character>(characterID).Online;
            }
            catch (Exception)
            {
                return this.CharacterDB.IsOnline(characterID);
            }
        }
    }
}