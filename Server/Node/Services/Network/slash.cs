using System;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Services;
using Node.Database;
using Node.Exceptions.slash;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using Node.Network;
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
                    case "/create":
                        this.CreateCmd(parts, call);
                        break;
                    case "/giveskill":
                        this.GiveSkillCmd(parts, call);
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
                throw new SlashError($"Runtime error: {e.Message}");
            }
            
            return null;
        }

        private void CreateCmd(string[] argv, CallInformation call)
        {
            int typeID = int.Parse(argv[1]);
            int quantity = 1;
            
            if (argv.Length > 2)
                quantity = int.Parse(argv[2]);

            if (call.Client.StationID == null)
                throw new SlashError("Creating items can only be done at station");
            // ensure the typeID exists
            if (this.TypeManager.ContainsKey(typeID) == false)
                throw new SlashError("The specified typeID doesn't exist");
            
            // create a new item with the correct locationID
            Station location = this.ItemManager.GetStation((int) call.Client.StationID);
            Character character = this.ItemManager.GetItem(call.Client.EnsureCharacterIsSelected()) as Character;
            
            ItemType itemType = this.TypeManager[typeID];
            ItemEntity item = this.ItemManager.CreateSimpleItem(itemType, character, location, ItemFlags.Hangar, quantity);

            item.Persist();
            
            // send client a notification so they can display the item in the hangar
            call.Client.NotifyNewItem(item);
        }

        private void GiveSkillCmd(string[] argv, CallInformation call)
        {
            if (argv.Length != 4)
                throw new SlashError("GiveSkill must have 4 arguments");

            string target = argv[1].Trim(new [] { '"', ' '});
            string skillType = argv[2];
            int level = int.Parse(argv[3]);

            if (target != "me")
                throw new SlashError("giveskill only supports me for now");

            Character character = this.ItemManager.GetItem(call.Client.EnsureCharacterIsSelected()) as Character;
            
            if (skillType == "all")
            {
                // player wants all the skills!
                IEnumerable<KeyValuePair<int, ItemType>> skillTypes =
                    this.TypeManager.Where(x => x.Value.Group.Category.ID == (int) ItemCategories.Skill);

                Dictionary<int, Skill> injectedSkills = character.InjectedSkillsByTypeID;

                foreach (KeyValuePair<int, ItemType> pair in skillTypes)
                {
                    // skill already injected, train it to the desired level
                    if (injectedSkills.ContainsKey(pair.Key) == true)
                    {
                        Skill skill = injectedSkills[pair.Key];

                        skill.Level = level;
                        skill.Persist();
                        call.Client.NotifyItemLocationChange(skill, skill.Flag, skill.LocationID);
                        call.Client.NotifySkillTrained(skill);
                    }
                    else
                    {
                        // skill not injected, create it, inject and done
                        Skill skill = this.ItemManager.CreateSkill(pair.Value, character, level,
                            SkillHistoryReason.GMGiveSkill);
                        
                        call.Client.NotifyNewItem(skill);
                        call.Client.NotifySkillInjected();
                        skill.Persist();
                    }
                }
            }
            else
            {
                int skillTypeID = int.Parse(skillType);
                Dictionary<int, Skill> injectedSkills = character.InjectedSkillsByTypeID;

                if (injectedSkills.ContainsKey(skillTypeID) == true)
                {
                    Skill skill = injectedSkills[skillTypeID];

                    skill.Level = level;
                    skill.Persist();
                    call.Client.NotifyItemLocationChange(skill, skill.Flag, skill.LocationID);
                    call.Client.NotifySkillTrained(skill);
                }
                else
                {
                    // skill not injected, create it, inject and done
                    Skill skill = this.ItemManager.CreateSkill(this.TypeManager[skillTypeID], character, level,
                        SkillHistoryReason.GMGiveSkill);

                    call.Client.NotifyNewItem(skill);
                    call.Client.NotifySkillInjected();
                    skill.Persist();
                }
            }
        }
    }
}