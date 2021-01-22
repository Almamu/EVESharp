using System;
using Common.Constants;
using Common.Services;
using Microsoft.VisualBasic.FileIO;
using Node.Exceptions.slash;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Network;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Network
{
    public class slash : Service
    {
        private TypeManager TypeManager { get; }
        private ItemManager ItemManager { get; }
        
        public slash(TypeManager typeManager, ItemManager itemManager)
        {
            this.TypeManager = typeManager;
            this.ItemManager = itemManager;
        }
        
        public PyDataType SlashCmd(PyString line, CallInformation call)
        {
            if ((call.Client.Role & (int) Roles.ROLE_ADMIN) != (int) Roles.ROLE_ADMIN)
                throw new SlashError("Only admins can run slash commands!");

            try
            {
                string[] parts = line.Value.Split(' ');

                switch (parts[0])
                {
                    case "/spawn":
                        this.SpawnCmd(parts, call);
                        break;
                    default:
                        throw new SlashError("Unknown command: " + line.Value);
                }
            }
            catch (SlashError)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SlashError(e.Message);
            }
            
            return null;
        }

        private void SpawnCmd(string[] argv, CallInformation call)
        {
            int typeID = int.Parse(argv[1]);

            if (call.Client.StationID == null)
                throw new SlashError("Spawning items can only be done at station");
            // ensure the typeID exists
            if (this.TypeManager.Exists(typeID) == false)
                throw new SlashError("The specified typeID doesn't exist");
            
            // create a new item with the correct locationID
            Station location = this.ItemManager.GetStation((int) call.Client.StationID);
            Character character = this.ItemManager.GetItem(call.Client.EnsureCharacterIsSelected()) as Character;
            
            ItemType itemType = this.TypeManager[typeID];
            ItemEntity item = this.ItemManager.CreateSimpleItem(itemType, character, location, ItemFlags.Hangar);

            item.Persist();
            
            // send client a notification so they can display the item in the hangar
            call.Client.NotifyNewItem(item);
        }
    }
}