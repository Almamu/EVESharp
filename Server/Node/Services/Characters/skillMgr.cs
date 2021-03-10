using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Node.Database;
using Node.Exceptions.character;
using Node.Exceptions.skillMgr;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using Node.Network;
using Node.Skills.Notifications;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class skillMgr : BoundService
    {
        private const int MAXIMUM_ATTRIBUTE_POINTS = 15;
        private const int MINIMUM_ATTRIBUTE_POINTS = 5;
        private const int MAXIMUM_TOTAL_ATTRIBUTE_POINTS = 39;
        private SkillDB DB { get; }
        private ItemManager ItemManager { get; }
        private TimerManager TimerManager { get; }
        private SystemManager SystemManager { get; }
        private Channel Log { get; }
        
        public skillMgr(SkillDB db, ItemManager itemManager, TimerManager timerManager, SystemManager systemManager,
            BoundServiceManager manager, Logger logger) : base(manager, null)
        {
            this.DB = db;
            this.ItemManager = itemManager;
            this.TimerManager = timerManager;
            this.SystemManager = systemManager;
            this.Log = logger.CreateLogChannel("SkillManager");
        }

        protected skillMgr(SkillDB db, ItemManager itemManager, TimerManager timerManager, SystemManager systemManager,
            BoundServiceManager manager, Logger logger, Client client) : base(manager, client)
        {
            this.DB = db;
            this.ItemManager = itemManager;
            this.TimerManager = timerManager;
            this.SystemManager = systemManager;
            this.Log = logger.CreateLogChannel("SkillManager");
        }

        public override PyInteger MachoResolveObject(PyTuple objectData, PyInteger zero, CallInformation call)
        {
            /*
             * objectData [0] => entityID (station or solar system)
             * objectData [1] => groupID (station or solar system)
             */

            // object data in this situation is not useful because the game looks for the SOL node that is processing
            // this information, but we don't have any SOL nodes in our model, just plain simple nodes
            // so to ensure the skillMgr operates properly the best way is to check where the character is loaded
            // and direct it towards that node
            int solarSystemID = call.Client.SolarSystemID2;

            if (this.SystemManager.SolarSystemBelongsToUs(solarSystemID) == true)
                return this.BoundServiceManager.Container.NodeID;

            return this.SystemManager.GetNodeSolarSystemBelongsTo(solarSystemID);
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData, CallInformation call)
        {
            if (this.MachoResolveObject(objectData as PyTuple, 0, call) != this.BoundServiceManager.Container.NodeID)
                throw new CustomError("Trying to bind an object that does not belong to us!");
            
            return new skillMgr(this.DB, this.ItemManager, this.TimerManager, this.SystemManager, this.BoundServiceManager, this.BoundServiceManager.Logger, call.Client);
        }

        public PyDataType GetSkillQueue(CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            PyList skillQueueList = new PyList(character.SkillQueue.Count);

            int index = 0;
            
            foreach (Character.SkillQueueEntry entry in character.SkillQueue)
                skillQueueList[index++] = entry;

            return skillQueueList;
        }

        public PyDataType GetSkillHistory(CallInformation call)
        {
            return this.DB.GetSkillHistory(call.Client.EnsureCharacterIsSelected());
        }

        public PyDataType InjectSkillIntoBrain(PyList itemIDs, CallInformation call)
        {
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            Character character = this.ItemManager.GetItem(callerCharacterID) as Character;
            
            foreach (PyDataType item in itemIDs)
            {
                if (item is PyInteger == false)
                    continue;

                int itemID = item as PyInteger;
                
                // TODO: SUPPORT MULTIPLE NOTIFICATIONS IN ONE GO TO NOT SPAM THE CLIENT WITH NOTIFICATION PACKETS
                try
                {
                    // get the item by it's ID and change the location of it
                    Skill skill = this.ItemManager.GetItem(itemID) as Skill;

                    // check if the character already has this skill injected
                    if (character.InjectedSkillsByTypeID.ContainsKey(skill.Type.ID) == true)
                        throw new CharacterAlreadyKnowsSkill(skill.Type);

                    // is this a stack of skills?
                    if (skill.Quantity > 1)
                    {
                        // add one of the skill into the character's brain
                        Skill newStack =
                            this.ItemManager.CreateSkill(skill.Type, character, 0, SkillHistoryReason.None);

                        // subtract one from the quantity
                        skill.Quantity -= 1;

                        // save to database
                        skill.Persist();
                        newStack.Persist();

                        // finally notify the client
                        call.Client.NotifyItemQuantityChange(skill, skill.Quantity + 1);
                        call.Client.NotifyNewItem(newStack);
                    }
                    else
                    {
                        // store old values for the notification
                        int oldLocationID = skill.LocationID;
                        ItemFlags oldFlag = skill.Flag;

                        // now set the new values
                        skill.LocationID = callerCharacterID;
                        skill.Flag = ItemFlags.Skill;

                        // ensure the changes are saved
                        skill.Persist();

                        // notify the character of the change in the item
                        call.Client.NotifyItemLocationChange(skill, oldFlag, oldLocationID);
                    }
                }
                catch (CharacterAlreadyKnowsSkill)
                {
                    throw;
                }
                catch (Exception)
                {
                    Log.Error($"Cannot inject itemID {itemID} into {callerCharacterID}'s brain...");
                    throw;
                }
            }
            
            // send the skill injected notification to refresh windows if needed
            call.Client.NotifyMultiEvent(new OnSkillInjected());

            return null;
        }
        
        public PyDataType SaveSkillQueue(PyList queue, CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            if (character.SkillQueue.Count > 0)
            {
                // calculate current skill in training points
                Skill currentSkill = character.SkillQueue[0].Skill;

                if (currentSkill.ExpiryTime > 0)
                {
                    // get the total amount of minutes the skill would have taken to train completely
                    long pointsLeft = (long) (currentSkill.GetSkillPointsForLevel(character.SkillQueue[0].TargetLevel) - currentSkill.Points);

                    TimeSpan timeLeft = TimeSpan.FromMinutes(pointsLeft / character.GetSkillPointsPerMinute(currentSkill));
                    DateTime endTime = DateTime.FromFileTimeUtc(currentSkill.ExpiryTime);
                    DateTime startTime = endTime.Subtract(timeLeft);
                    TimeSpan timePassed = DateTime.UtcNow - startTime;

                    // calculate the skill points to add
                    double skillPointsToAdd = timePassed.TotalMinutes * character.GetSkillPointsPerMinute(currentSkill);
                
                    currentSkill.Points += skillPointsToAdd;
                    currentSkill.ExpiryTime = 0;
                }
                
                // remove all the timers associated with the current skillQueue and the expiry times
                foreach (Character.SkillQueueEntry entry in character.SkillQueue)
                {
                    this.TimerManager.DequeueItemTimer(entry.Skill.ID, entry.Skill.ExpiryTime);
                    entry.Skill.Flag = ItemFlags.Skill;
                    
                    call.Client.NotifyItemLocationChange(entry.Skill, ItemFlags.SkillInTraining, (int) entry.Skill.LocationID);
            
                    // send notification of skill training stopped
                    call.Client.NotifyMultiEvent(new OnSkillTrainingStopped(entry.Skill));

                    // create history entry
                    this.DB.CreateSkillHistoryRecord(entry.Skill.Type, character, SkillHistoryReason.SkillTrainingCancelled,
                        entry.Skill.Points);
                    
                    entry.Skill.ExpiryTime = 0;
                    entry.Skill.Persist();
                }

                character.SkillQueue.Clear();
            }

            DateTime startDateTime = DateTime.UtcNow;
            bool first = true;
            
            foreach (PyTuple entry in queue)
            {
                // ignore wrong entries
                if (entry.Count != 2)
                    continue;

                int typeID = entry[0] as PyInteger;
                int level = entry[1] as PyInteger;
                
                // search for an item with the given typeID
                ItemEntity item = character.Items.First(x => x.Value.Type.ID == typeID && x.Value.Flag == ItemFlags.Skill).Value;

                // ignore items that are not skills
                if (item is Skill == false)
                    continue;

                Skill skill = item as Skill;

                double skillPointsLeft = skill.GetSkillPointsForLevel(level) - skill.Points;

                TimeSpan duration = TimeSpan.FromMinutes(skillPointsLeft / character.GetSkillPointsPerMinute(skill));

                DateTime expiryTime = startDateTime + duration;
                
                skill.ExpiryTime = expiryTime.ToFileTimeUtc();
                
                startDateTime = expiryTime;
                
                // skill added to the queue, persist the character to ensure all the changes are saved
                character.SkillQueue.Add(new Character.SkillQueueEntry() { Skill = skill, TargetLevel = level });
                
                // ensure the timer is present for this skill
                this.TimerManager.EnqueueItemTimer(skill.ExpiryTime, character.SkillTrainingCompleted, skill.ID);
                
                if (first == true)
                {
                    skill.Flag = ItemFlags.SkillInTraining;
                    
                    call.Client.NotifyItemLocationChange(skill, ItemFlags.Skill, (int) skill.LocationID);
            
                    // skill was trained, send the success message
                    call.Client.NotifyMultiEvent(new OnSkillStartTraining (skill));
                
                    // create history entry
                    this.DB.CreateSkillHistoryRecord(skill.Type, character, SkillHistoryReason.SkillTrainingStarted,
                        skill.Points);

                    first = false;
                }

                skill.Persist();
            }
            
            // finally persist the data to the database
            character.Persist();

            return null;
        }

        public PyDataType CharStartTrainingSkillByTypeID(PyInteger typeID, CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            // do not allow the user to do that if the skill queue is not empty
            if (character.SkillQueue.Count > 0)
                return null;

            Skill skill = character.InjectedSkills.First(x => x.Value.Type.ID == typeID).Value;

            // do not start the skill training if the level is 5 already
            if (skill == null || skill.Level == 5)
                return null;

            double skillPointsLeft = skill.GetSkillPointsForLevel(skill.Level + 1) - skill.Points;
                
            DateTime expiryTime = DateTime.UtcNow.AddMinutes(skillPointsLeft / character.GetSkillPointsPerMinute(skill));
                
            skill.ExpiryTime = expiryTime.ToFileTimeUtc();
            skill.Flag = ItemFlags.SkillInTraining;
                    
            call.Client.NotifyItemLocationChange(skill, ItemFlags.Skill, (int) skill.LocationID);
            
            // skill started training
            call.Client.NotifyMultiEvent(new OnSkillStartTraining (skill));
            
            // create history entry
            this.DB.CreateSkillHistoryRecord(skill.Type, character, SkillHistoryReason.SkillTrainingStarted,
                skill.Points);
                
            // ensure the timer is present for this skill
            this.TimerManager.EnqueueItemTimer(skill.ExpiryTime, character.SkillTrainingCompleted, skill.ID);

            skill.Persist();
            
            return null;
        }

        public PyDataType GetEndOfTraining(CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            // do not allow the user to do that if the skill queue is not empty
            if (character.SkillQueue.Count > 0)
                return 0;

            return character.SkillQueue[0].Skill.ExpiryTime;
        }

        public PyDataType CharStopTrainingSkill(CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;
            
            // iterate the whole skill queue, stop it and recalculate points for the skills
            if (character.SkillQueue.Count == 0)
                return 0;

            // only the skill on the front should have it's skillpoints recalculated
            Skill skill = character.SkillQueue[0].Skill;

            if (skill.ExpiryTime > 0)
            {
                // get the total amount of minutes the skill would have taken to train completely
                long pointsLeft = (long) (skill.GetSkillPointsForLevel(character.SkillQueue[0].TargetLevel) - skill.Points);

                TimeSpan timeLeft = TimeSpan.FromMinutes(pointsLeft / character.GetSkillPointsPerMinute(skill));
                DateTime endTime = DateTime.FromFileTimeUtc(skill.ExpiryTime);
                DateTime startTime = endTime.Subtract(timeLeft);
                TimeSpan timePassed = DateTime.UtcNow - startTime;

                // calculate the skill points to add
                double skillPointsToAdd = timePassed.TotalMinutes * character.GetSkillPointsPerMinute(skill);
                
                skill.Points += skillPointsToAdd;
            }
            
            foreach (Character.SkillQueueEntry entry in character.SkillQueue)
            {
                // dequeue the timer first
                this.TimerManager.DequeueItemTimer(entry.Skill.ID, entry.Skill.ExpiryTime);

                // mark the skill as stopped and store it in the database
                entry.Skill.ExpiryTime = 0;
                entry.Skill.Flag = ItemFlags.Skill;
                entry.Skill.Persist();
                    
                call.Client.NotifyItemLocationChange(entry.Skill, ItemFlags.SkillInTraining, (int) entry.Skill.LocationID);
                
                // notify the skill is not in training anymore
                call.Client.NotifyMultiEvent(new OnSkillTrainingStopped(entry.Skill));
                
                // create history entry
                this.DB.CreateSkillHistoryRecord(entry.Skill.Type, character, SkillHistoryReason.SkillTrainingCancelled,
                    entry.Skill.Points);
            }

            return null;
        }

        public PyDataType GetRespecInfo(CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            return new PyDictionary
            {
                ["nextRespecTime"] = character.NextReSpecTime,
                ["freeRespecs"] = character.FreeReSpecs
            };
        }

        public PyDataType GetCharacterAttributeModifiers(PyInteger attributeID, CallInformation call)
        {
            Character character = this.ItemManager.LoadItem(call.Client.EnsureCharacterIsSelected()) as Character;

            AttributeEnum attribute;

            switch ((int) attributeID)
            {
                case (int) AttributeEnum.willpower:
                    attribute = AttributeEnum.willpowerBonus;
                    break;
                case (int) AttributeEnum.charisma:
                    attribute = AttributeEnum.charismaBonus;
                    break;
                case (int) AttributeEnum.memory:
                    attribute = AttributeEnum.memoryBonus;
                    break;
                case (int) AttributeEnum.intelligence:
                    attribute = AttributeEnum.intelligenceBonus;
                    break;
                case (int) AttributeEnum.perception:
                    attribute = AttributeEnum.perceptionBonus;
                    break;
                default:
                    return new PyList();
            }
            
            PyList modifiers = new PyList();
            
            foreach (KeyValuePair<int, ItemEntity> modifier in character.Modifiers)
            {
                if (modifier.Value.Attributes.AttributeExists(attribute) == true)
                {
                    // for now add all the elements as dgmAssModAdd
                    // check ApplyModifiers on attributes.py
                    // TODO: THE THIRD PARAMETER HERE WAS DECIDED RANDOMLY BASED ON THE CODE ITSELF
                    // TODO: BUT THAT DOESN'T MEAN THAT IT'S ENTIRELY CORRECT
                    // TODO: SO MAYBE CHECK IF THIS IS CORRECT SOMETIME AFTER
                    modifiers.Add(new PyTuple(new PyDataType []
                        {
                            modifier.Value.ID, modifier.Value.Type.ID, 2,
                            modifier.Value.Attributes[attribute]                            
                        }
                    ));
                }
            }
            
            // search for skills that modify this attribute
            return modifiers;
        }

        public PyDataType RespecCharacter(PyInteger charisma, PyInteger intelligence, PyInteger memory,
            PyInteger perception, PyInteger willpower, CallInformation call)
        {
            if (charisma < MINIMUM_ATTRIBUTE_POINTS || intelligence < MINIMUM_ATTRIBUTE_POINTS ||
                memory < MINIMUM_ATTRIBUTE_POINTS || perception < MINIMUM_ATTRIBUTE_POINTS ||
                willpower < MINIMUM_ATTRIBUTE_POINTS)
                throw new RespecAttributesTooLow();
            if (charisma >= MAXIMUM_ATTRIBUTE_POINTS || intelligence >= MAXIMUM_ATTRIBUTE_POINTS ||
                memory >= MAXIMUM_ATTRIBUTE_POINTS || perception >= MAXIMUM_ATTRIBUTE_POINTS ||
                willpower >= MAXIMUM_ATTRIBUTE_POINTS)
                throw new RespecAttributesTooHigh();
            if (charisma + intelligence + memory + perception + willpower != MAXIMUM_TOTAL_ATTRIBUTE_POINTS)
                throw new RespecAttributesMisallocated();
            
            int callerCharacterID = call.Client.EnsureCharacterIsSelected();
            
            Character character = this.ItemManager.GetItem(callerCharacterID) as Character;

            if (character.FreeReSpecs == 0)
                throw new CustomError("You've already remapped your character too much times at once, wait some time");
            
            // check if the respec is the same as it was already
            if (charisma == character.Charisma && intelligence == character.Intelligence &&
                memory == character.Memory && perception == character.Perception && willpower == character.Willpower)
                throw new CustomError("No changes detected on the neural map");
            
            // take one respec out
            character.FreeReSpecs--;
            
            // if respec is zero now means we don't have any free respecs until a year later
            character.NextReSpecTime = DateTime.UtcNow.AddYears(1).ToFileTimeUtc();
            
            // finally set our attributes to the correct values
            character.Charisma = charisma;
            character.Intelligence = intelligence;
            character.Memory = memory;
            character.Perception = perception;
            character.Willpower = willpower;

            // save the character
            character.Persist();
            
            // notify the game of the change on the character
            call.Client.NotifyMultipleAttributeChange(
                new ItemAttribute[]
                {
                    character.Attributes[AttributeEnum.charisma],
                    character.Attributes[AttributeEnum.perception],
                    character.Attributes[AttributeEnum.intelligence],
                    character.Attributes[AttributeEnum.memory],
                    character.Attributes[AttributeEnum.willpower]
                },
                new ItemEntity[]
                {
                    character, character, character, character, character
                }
            );

            return null;
        }

        public PyDataType CharAddImplant(PyInteger itemID, CallInformation call)
        {
            Character character = this.ItemManager.GetItem(call.Client.EnsureCharacterIsSelected()) as Character;

            if (character.SkillQueue.Count > 0)
                throw new FailedPlugInImplant();
            
            // get the item and plug it into our brain now!
            ItemEntity item = this.ItemManager.LoadItem(itemID);
            
            // ensure the item is somewhere we can interact with it
            if (item.LocationID != call.Client.ShipID && item.LocationID != call.Client.StationID)
                throw new CustomError("You do not have direct access to this implant");

            // check if the slot is free or not
            character.EnsureFreeImplantSlot(item);
            
            // check ownership and skills required to plug in the implant
            item.EnsureOwnership(character);
            item.CheckPrerequisites(character);
            
            // separate the item if there's more than one
            if (item.Quantity > 1)
            {
                item.Quantity--;
                
                // notify the client of the stack change
                call.Client.NotifyItemQuantityChange(item, item.Quantity + 1);
                
                // save the item to the database
                item.Persist();
                
                // create the new item with a default location and flag
                // this way the item location change notification is only needed once
                item = this.ItemManager.CreateSimpleItem(item.Type, item.OwnerID, 0,
                    ItemFlags.None, 1, item.Contraband, item.Singleton);
            }

            int oldLocationID = item.LocationID;
            ItemFlags oldFlag = item.Flag;
            
            item.LocationID = character.ID;
            item.Flag = ItemFlags.Implant;

            call.Client.NotifyItemLocationChange(item, oldFlag, oldLocationID);

            // add the item to the inventory it belongs
            character.AddItem(item);

            // persist item changes to database
            item.Persist();
            
            return null;
        }

        public PyDataType RemoveImplantFromCharacter(PyInteger itemID, CallInformation call)
        {
            Character character = this.ItemManager.GetItem(call.Client.EnsureCharacterIsSelected()) as Character;

            if (character.Items.ContainsKey(itemID) == false)
                throw new CustomError("This implant is not in your brain!");

            // move the item to the recycler and then remove it
            ItemEntity item = character.Items[itemID];
            
            // now destroy the item
            this.ItemManager.DestroyItem(item);
            
            // notify the change
            call.Client.NotifyItemLocationChange(item, item.Flag, character.ID);
            
            return null;
        }
    }
}