using System;
using System.Collections.Generic;
using System.Linq;
using Common.Logging;
using Node.Database;
using Node.Inventory;
using Node.Inventory.Items;
using Node.Inventory.Items.Attributes;
using Node.Inventory.Items.Types;
using PythonTypes.Marshal;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;
using SimpleInjector;

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
        
        public skillMgr(SkillDB db, ItemManager itemManager, TimerManager timerManager, BoundServiceManager manager) : base(manager)
        {
            this.DB = db;
            this.ItemManager = itemManager;
            this.TimerManager = timerManager;
        }

        protected override BoundService CreateBoundInstance(PyDataType objectData)
        {
            /*
             * objectData [0] => entityID (station or solar system)
             * objectData [1] => groupID (station or solar system)
             */
            PyTuple tupleData = objectData as PyTuple;
            
            return new skillMgr(this.DB, this.ItemManager, this.TimerManager, this.BoundServiceManager);
        }

        public PyDataType GetSkillQueue(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            PyList skillQueueList = new PyList(character.SkillQueue.Count);

            int index = 0;
            
            foreach (Character.SkillQueueEntry entry in character.SkillQueue)
                skillQueueList[index++] = entry;

            return skillQueueList;
        }

        public PyDataType GetSkillHistory(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            return this.DB.GetSkillHistory((int) client.CharacterID);
        }

        public PyDataType SaveSkillQueue(PyList queue, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;

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
                    this.TimerManager.DequeueTimer(entry.Skill.ID, entry.Skill.ExpiryTime);
                    entry.Skill.Flag = ItemFlags.Skill;
                    
                    client.NotifyItemChange(entry.Skill, ItemFlags.SkillInTraining, (int) entry.Skill.LocationID);
            
                    // send notification of skill training stopped
                    client.NotifySkillTrainingStopped(entry.Skill);

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
                this.TimerManager.EnqueueTimer(skill.ExpiryTime, character.SkillTrainingCompleted, skill.ID);
                
                if (first == true)
                {
                    skill.Flag = ItemFlags.SkillInTraining;
                    
                    client.NotifyItemChange(skill, ItemFlags.Skill, (int) skill.LocationID);
            
                    // skill was trained, send the success message
                    client.NotifySkillStartTraining(skill);
                
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

        public PyDataType CharStartTrainingSkillByTypeID(PyInteger typeID, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;

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
                    
            client.NotifyItemChange(skill, ItemFlags.Skill, (int) skill.LocationID);
            
            // skill started training
            client.NotifySkillStartTraining(skill);
            
            // create history entry
            this.DB.CreateSkillHistoryRecord(skill.Type, character, SkillHistoryReason.SkillTrainingStarted,
                skill.Points);
                
            // ensure the timer is present for this skill
            this.TimerManager.EnqueueTimer(skill.ExpiryTime, character.SkillTrainingCompleted, skill.ID);

            skill.Persist();
            
            return null;
        }

        public PyDataType GetEndOfTraining(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character =
                this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            // do not allow the user to do that if the skill queue is not empty
            if (character.SkillQueue.Count > 0)
                return 0;

            return character.SkillQueue[0].Skill.ExpiryTime;
        }

        public PyDataType CharStopTrainingSkill(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;
            
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
                this.TimerManager.DequeueTimer(entry.Skill.ID, entry.Skill.ExpiryTime);

                // mark the skill as stopped and store it in the database
                entry.Skill.ExpiryTime = 0;
                entry.Skill.Flag = ItemFlags.Skill;
                entry.Skill.Persist();
                    
                client.NotifyItemChange(entry.Skill, ItemFlags.SkillInTraining, (int) entry.Skill.LocationID);
                
                // notify the skill is not in training anymore
                client.NotifySkillTrainingStopped(entry.Skill);
                
                // create history entry
                this.DB.CreateSkillHistoryRecord(entry.Skill.Type, character, SkillHistoryReason.SkillTrainingCancelled,
                    entry.Skill.Points);
            }

            return null;
        }

        public PyDataType GetRespecInfo(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;

            return new PyDictionary
            {
                ["nextRespecTime"] = character.NextReSpecTime,
                ["freeRespecs"] = character.FreeReSpecs
            };
        }

        public PyDataType GetCharacterAttributeModifiers(PyInteger attributeID, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character = this.ItemManager.LoadItem((int) client.CharacterID) as Character;

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
            PyInteger perception, PyInteger willpower, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            if (charisma < MINIMUM_ATTRIBUTE_POINTS || intelligence < MINIMUM_ATTRIBUTE_POINTS ||
                memory < MINIMUM_ATTRIBUTE_POINTS || perception < MINIMUM_ATTRIBUTE_POINTS ||
                willpower < MINIMUM_ATTRIBUTE_POINTS)
                throw new UserError("RespecAttributesTooLow");
            if (charisma >= MAXIMUM_ATTRIBUTE_POINTS || intelligence >= MAXIMUM_ATTRIBUTE_POINTS ||
                memory >= MAXIMUM_ATTRIBUTE_POINTS || perception >= MAXIMUM_ATTRIBUTE_POINTS ||
                willpower >= MAXIMUM_ATTRIBUTE_POINTS)
                throw new UserError("RespecAttributesTooHigh");
            if (charisma + intelligence + memory + perception + willpower != MAXIMUM_TOTAL_ATTRIBUTE_POINTS)
                throw new UserError("RespecAttributesMisallocated");
            
            Character character =
                this.ItemManager.LoadItem((int) client.CharacterID) as Character;

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
            client.NotifyMultipleAttributeChange(
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
    }
}