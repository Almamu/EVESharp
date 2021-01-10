using System;
using System.Collections.Generic;
using System.Linq;
using Node.Database;
using Node.Inventory.Items;
using Node.Inventory.Items.Types;
using PythonTypes.Marshal;
using PythonTypes.Types.Exceptions;
using PythonTypes.Types.Primitives;

namespace Node.Services.Characters
{
    public class skillMgr : BoundService
    {
        private SkillDB mDB = null;
        
        public skillMgr(ServiceManager manager) : base(manager)
        {
            this.mDB = new SkillDB(manager.Container.Database, manager.Container.ItemFactory);
        }

        protected override Service CreateBoundInstance(PyDataType objectData)
        {
            /*
             * objectData [0] => entityID (station or solar system)
             * objectData [1] => groupID (station or solar system)
             */
            PyTuple tupleData = objectData as PyTuple;
            
            return new skillMgr(this.ServiceManager);
        }

        public PyDataType GetSkillQueue(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;

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

            return this.mDB.GetSkillHistory((int) client.CharacterID);
        }

        public PyDataType SaveSkillQueue(PyList queue, PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");

            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;

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
                    this.ServiceManager.Container.TimerManager.DequeueTimer(entry.Skill.ID, entry.Skill.ExpiryTime);
                    entry.Skill.Flag = ItemFlags.Skill;
                    
                    client.NotifyItemChange(entry.Skill, ItemFlags.SkillInTraining, (int) entry.Skill.LocationID);
            
                    // send notification of skill training stopped
                    client.NotifySkillTrainingStopped(entry.Skill);

                    // create history entry
                    this.mDB.CreateSkillHistoryRecord(entry.Skill.Type, character, SkillHistoryReason.SkillTrainingCancelled,
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
                this.ServiceManager.Container.TimerManager.EnqueueTimer(skill.ExpiryTime, character.SkillTrainingCompleted, skill.ID);
                if (first == true)
                {
                    skill.Flag = ItemFlags.SkillInTraining;
                    
                    client.NotifyItemChange(skill, ItemFlags.Skill, (int) skill.LocationID);
            
                    // skill was trained, send the success message
                    client.NotifySkillStartTraining(skill);
                
                    // create history entry
                    this.mDB.CreateSkillHistoryRecord(skill.Type, character, SkillHistoryReason.SkillTrainingStarted,
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

            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;

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
            this.mDB.CreateSkillHistoryRecord(skill.Type, character, SkillHistoryReason.SkillTrainingStarted,
                skill.Points);
                
            // ensure the timer is present for this skill
            this.ServiceManager.Container.TimerManager.EnqueueTimer(skill.ExpiryTime, character.SkillTrainingCompleted, skill.ID);

            skill.Persist();
            
            return null;
        }

        public PyDataType GetEndOfTraining(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;

            // do not allow the user to do that if the skill queue is not empty
            if (character.SkillQueue.Count > 0)
                return 0;

            return character.SkillQueue[0].Skill.ExpiryTime;
        }

        public PyDataType CharStopTrainingSkill(PyDictionary namedPayload, Client client)
        {
            if (client.CharacterID == null)
                throw new UserError("NoCharacterSelected");
            
            Character character =
                this.ServiceManager.Container.ItemFactory.ItemManager.LoadItem((int) client.CharacterID) as Character;
            
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
                this.ServiceManager.Container.TimerManager.DequeueTimer(entry.Skill.ID, entry.Skill.ExpiryTime);

                // mark the skill as stopped and store it in the database
                entry.Skill.ExpiryTime = 0;
                entry.Skill.Flag = ItemFlags.Skill;
                entry.Skill.Persist();
                    
                client.NotifyItemChange(entry.Skill, ItemFlags.SkillInTraining, (int) entry.Skill.LocationID);
                
                // notify the skill is not in training anymore
                client.NotifySkillTrainingStopped(entry.Skill);
                
                // create history entry
                this.mDB.CreateSkillHistoryRecord(entry.Skill.Type, character, SkillHistoryReason.SkillTrainingCancelled,
                    entry.Skill.Points);
            }

            return null;
        }
    }
}