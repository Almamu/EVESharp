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
using Node.Notifications.Inventory;
using Node.Notifications.Skills;
using PythonTypes.Types.Collections;
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
        private Character Character { get; }
        
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
            this.Character = this.ItemManager.GetItem<Character>(client.EnsureCharacterIsSelected());
            this.Log = logger.CreateLogChannel("SkillManager");

            this.InitializeCharacter();
        }
        
        private void SetupTimerForNextSkillInQueue()
        {
            if (this.Character.SkillQueue.Count == 0)
                return;
            
            Character.SkillQueueEntry entry = this.Character.SkillQueue[0];
            
            if (entry.Skill.ExpiryTime == 0)
                return;

            // send notification of skill training started
            this.Client.NotifyMultiEvent(new OnSkillStartTraining(entry.Skill));
            
            this.TimerManager.EnqueueItemTimer(entry.Skill.ExpiryTime, OnSkillTrainingCompleted, entry.Skill.ID);
        }

        private void SetupReSpecTimers()
        {
            if (this.Character.FreeReSpecs == 0 && this.Character.NextReSpecTime > 0)
                this.TimerManager.EnqueueItemTimer(this.Character.NextReSpecTime, OnNextReSpecAvailable, this.Character.ID);
        }

        private void InitializeCharacter()
        {
            // perform basic checks on the skill queue
                
            // iterate the skill queue and generate a timer for the first skill that must be trained
            // this also prepares the correct notification for multiple skill training done
            PyList<PyInteger> skillTypeIDs = new PyList<PyInteger>();
            List<Character.SkillQueueEntry> toRemove = new List<Character.SkillQueueEntry>();

            foreach (Character.SkillQueueEntry entry in this.Character.SkillQueue)
            {
                if (entry.Skill.ExpiryTime < DateTime.Now.ToFileTimeUtc())
                {
                    // ensure the skill is marked as trained and that they have the correct values stored
                    entry.Skill.Level = entry.TargetLevel;
                    entry.Skill.Flag = ItemFlags.Skill;
                    entry.Skill.ExpiryTime = 0;

                    // add the skill to the list of trained skills for the big notification
                    skillTypeIDs.Add(entry.Skill.Type.ID);
                    toRemove.Add(entry);
                    
                    // update it's location in the client if needed
                    this.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(entry.Skill, ItemFlags.SkillInTraining));
                    // also notify attribute changes
                    this.Client.NotifyAttributeChange(new AttributeEnum[] { AttributeEnum.skillPoints, AttributeEnum.skillLevel}, entry.Skill);
                }
            }

            // remove skills that already expired
            this.Character.SkillQueue.RemoveAll(x => toRemove.Contains(x));

            // send notification of multiple skills being finished training (if any)
            if (skillTypeIDs.Count > 0)
                this.Client.NotifyMultiEvent(new OnGodmaMultipleSkillsTrained(skillTypeIDs));
            
            // persists the skill queue
            this.Character.Persist();
            
            // setup the process for training next skill in the queue
            this.SetupTimerForNextSkillInQueue();
        }

        private void FreeSkillQueueTimers()
        {
            if (this.Character.SkillQueue.Count == 0)
                return;

            Character.SkillQueueEntry entry = this.Character.SkillQueue[0];

            if (entry.Skill.ExpiryTime == 0)
                return;

            this.TimerManager.DequeueItemTimer(entry.Skill.ID, entry.Skill.ExpiryTime);
        }

        private void FreeReSpecTimers()
        {
            if (this.Character.NextReSpecTime == 0)
                return;
            
            this.TimerManager.DequeueItemTimer(this.Character.ID, this.Character.NextReSpecTime);
        }
        
        public override void OnServiceFree()
        {
            this.FreeSkillQueueTimers();
            this.FreeReSpecTimers();
        }

        private void OnNextReSpecAvailable(int itemID)
        {
            // update respec values
            this.Character.NextReSpecTime = 0;
            this.Character.FreeReSpecs = 1;
            this.Character.Persist();
        }

        private void OnSkillTrainingCompleted(int itemID)
        {
            Skill skill = this.Character.Items[itemID] as Skill;
            
            // set the skill to the proper flag and set the correct attributes
            skill.Flag = ItemFlags.Skill;
            skill.Level = skill.Level + 1;
            skill.ExpiryTime = 0;
            
            // make sure the client is aware of the new item's status
            this.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(skill, ItemFlags.SkillInTraining));
            // also notify attribute changes
            this.Client.NotifyAttributeChange(new AttributeEnum[] { AttributeEnum.skillPoints, AttributeEnum.skillLevel}, skill);
            this.Client.NotifyMultiEvent(new OnSkillTrained(skill));
            this.Client.SendPendingNotifications();

            skill.Persist();
            
            // create history entry
            this.DB.CreateSkillHistoryRecord(skill.Type, this.Character, SkillHistoryReason.SkillTrainingComplete, skill.Points);

            // finally remove it off the skill queue
            this.Character.SkillQueue.RemoveAll(x => x.Skill.ID == skill.ID && x.TargetLevel == skill.Level);

            this.Character.CalculateSkillPoints();
            
            // get the next skill from the queue (if any) and send the client proper notifications
            if (this.Character.SkillQueue.Count == 0)
            {
                this.Character.Persist();
                return;
            }

            skill = this.Character.SkillQueue[0].Skill;
            
            // setup the process for training next skill in the queue
            this.SetupTimerForNextSkillInQueue();
            this.Client.SendPendingNotifications();

            // create history entry
            this.DB.CreateSkillHistoryRecord(skill.Type, this.Character, SkillHistoryReason.SkillTrainingStarted, skill.Points);
            
            // persist the character changes
            this.Character.Persist();
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
            Character character = this.ItemManager.GetItem<Character>(call.Client.EnsureCharacterIsSelected());

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
            foreach (PyInteger item in itemIDs.GetEnumerable<PyInteger>())
            {
                try
                {
                    // get the item by it's ID and change the location of it
                    Skill skill = this.ItemManager.GetItem<Skill>(item);

                    // check if the character already has this skill injected
                    if (this.Character.InjectedSkillsByTypeID.ContainsKey(skill.Type.ID) == true)
                        throw new CharacterAlreadyKnowsSkill(skill.Type);

                    // is this a stack of skills?
                    if (skill.Quantity > 1)
                    {
                        // add one of the skill into the character's brain
                        Skill newStack = this.ItemManager.CreateSkill(skill.Type, this.Character, 0, SkillHistoryReason.None);

                        // subtract one from the quantity
                        skill.Quantity -= 1;

                        // save to database
                        skill.Persist();

                        // finally notify the client
                        call.Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(skill, skill.Quantity + 1));
                        call.Client.NotifyMultiEvent(OnItemChange.BuildNewItemChange(newStack));
                    }
                    else
                    {
                        // store old values for the notification
                        int oldLocationID = skill.LocationID;
                        ItemFlags oldFlag = skill.Flag;

                        // now set the new values
                        skill.LocationID = this.Character.ID;
                        skill.Flag = ItemFlags.Skill;
                        skill.Level = 0;
                        skill.Singleton = true;
                        
                        // ensure the character has the skill in his/her brain
                        this.Character.AddItem(skill);

                        // ensure the changes are saved
                        skill.Persist();

                        // notify the character of the change in the item
                        call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(skill, oldFlag, oldLocationID));
                        call.Client.NotifyMultiEvent(OnItemChange.BuildSingletonChange(skill, false));
                    }
                }
                catch (CharacterAlreadyKnowsSkill)
                {
                    throw;
                }
                catch (Exception)
                {
                    Log.Error($"Cannot inject itemID {item} into {this.Character.ID}'s brain...");
                    throw;
                }
            }
            
            // send the skill injected notification to refresh windows if needed
            call.Client.NotifyMultiEvent(new OnSkillInjected());

            return null;
        }
        
        public PyDataType SaveSkillQueue(PyList queue, CallInformation call)
        {
            if (this.Character.SkillQueue.Count > 0)
            {
                // calculate current skill in training points
                Skill currentSkill = this.Character.SkillQueue[0].Skill;

                if (currentSkill.ExpiryTime > 0)
                {
                    // get the total amount of minutes the skill would have taken to train completely
                    long pointsLeft = (long) (currentSkill.GetSkillPointsForLevel(this.Character.SkillQueue[0].TargetLevel) - currentSkill.Points);

                    TimeSpan timeLeft = TimeSpan.FromMinutes(pointsLeft / this.Character.GetSkillPointsPerMinute(currentSkill));
                    DateTime endTime = DateTime.FromFileTimeUtc(currentSkill.ExpiryTime);
                    DateTime startTime = endTime.Subtract(timeLeft);
                    TimeSpan timePassed = DateTime.UtcNow - startTime;

                    // calculate the skill points to add
                    double skillPointsToAdd = timePassed.TotalMinutes * this.Character.GetSkillPointsPerMinute(currentSkill);
                
                    currentSkill.Points += skillPointsToAdd;
                }
                
                // remove the timer associated with the queue
                this.FreeSkillQueueTimers();
                
                foreach (Character.SkillQueueEntry entry in this.Character.SkillQueue)
                {
                    entry.Skill.Flag = ItemFlags.Skill;
                    
                    call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(entry.Skill, ItemFlags.SkillInTraining));
            
                    // send notification of skill training stopped
                    call.Client.NotifyMultiEvent(new OnSkillTrainingStopped(entry.Skill));

                    // create history entry
                    this.DB.CreateSkillHistoryRecord(entry.Skill.Type, this.Character, SkillHistoryReason.SkillTrainingCancelled,
                        entry.Skill.Points);
                    
                    entry.Skill.ExpiryTime = 0;
                    entry.Skill.Persist();
                }

                this.Character.SkillQueue.Clear();
            }

            DateTime startDateTime = DateTime.UtcNow;
            bool first = true;
            
            foreach (PyTuple entry in queue.GetEnumerable<PyTuple>())
            {
                // ignore wrong entries
                if (entry.Count != 2)
                    continue;

                int typeID = entry[0] as PyInteger;
                int level = entry[1] as PyInteger;
                
                // search for an item with the given typeID
                ItemEntity item = this.Character.Items.First(x => x.Value.Type.ID == typeID && (x.Value.Flag == ItemFlags.Skill || x.Value.Flag == ItemFlags.SkillInTraining)).Value;

                // ignore items that are not skills
                if (item is Skill == false)
                    continue;

                Skill skill = item as Skill;

                double skillPointsLeft = skill.GetSkillPointsForLevel(level) - skill.Points;

                TimeSpan duration = TimeSpan.FromMinutes(skillPointsLeft / this.Character.GetSkillPointsPerMinute(skill));

                DateTime expiryTime = startDateTime + duration;
                
                skill.ExpiryTime = expiryTime.ToFileTimeUtc();
                skill.Flag = ItemFlags.SkillInTraining;

                call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(skill, ItemFlags.Skill));
                
                startDateTime = expiryTime;
                
                // skill added to the queue, persist the character to ensure all the changes are saved
                this.Character.SkillQueue.Add(new Character.SkillQueueEntry() { Skill = skill, TargetLevel = level });
                
                if (first == true)
                {
                    // skill was trained, send the success message
                    call.Client.NotifyMultiEvent(new OnSkillStartTraining (skill));
                
                    // create history entry
                    this.DB.CreateSkillHistoryRecord(skill.Type, this.Character, SkillHistoryReason.SkillTrainingStarted, skill.Points);

                    first = false;
                }

                skill.Persist();
            }
            
            // ensure the timer is present for the first skill in the queue
            this.SetupTimerForNextSkillInQueue();
            
            // finally persist the data to the database
            this.Character.Persist();

            return null;
        }

        public PyDataType CharStartTrainingSkillByTypeID(PyInteger typeID, CallInformation call)
        {
            // get the skill the player wants to train
            Skill skill = this.Character.InjectedSkills.First(x => x.Value.Type.ID == typeID).Value;
            
            // do not start the skill training if the level is 5 already
            if (skill is null || skill.Level == 5)
                return null;

            PyList<PyTuple> queue = new PyList<PyTuple>(1)
            {
                [0] = new PyTuple(2)
                {
                    [0] = typeID,
                    [1] = skill.Level + 1
                }
            };


            // build a list of skills to train based off the original queue
            // but with the new skill on top
            foreach (Character.SkillQueueEntry entry in this.Character.SkillQueue)
            {
                // ignore the skill in the queue if it was the one requested
                if (entry.Skill.Type.ID == typeID)
                    continue;

                queue.Add(
                    new PyTuple(2)
                    {
                        [0] = entry.Skill.Type.ID,
                        [1] = entry.TargetLevel
                    }
                );
            }
            
            // save the new skill queue
            return this.SaveSkillQueue(queue, call);
        }

        public PyDataType GetEndOfTraining(CallInformation call)
        {
            // do not allow the user to do that if the skill queue is empty
            if (this.Character.SkillQueue.Count == 0)
                return 0;

            return this.Character.SkillQueue[0].Skill.ExpiryTime;
        }

        public PyDataType CharStopTrainingSkill(CallInformation call)
        {
            // iterate the whole skill queue, stop it and recalculate points for the skills
            if (this.Character.SkillQueue.Count == 0)
                return null;

            // only the skill on the front should have it's skillpoints recalculated
            Skill skill = this.Character.SkillQueue[0].Skill;

            if (skill.ExpiryTime > 0)
            {
                // get the total amount of minutes the skill would have taken to train completely
                long pointsLeft = (long) (skill.GetSkillPointsForLevel(this.Character.SkillQueue[0].TargetLevel) - skill.Points);

                TimeSpan timeLeft = TimeSpan.FromMinutes(pointsLeft / this.Character.GetSkillPointsPerMinute(skill));
                DateTime endTime = DateTime.FromFileTimeUtc(skill.ExpiryTime);
                DateTime startTime = endTime.Subtract(timeLeft);
                TimeSpan timePassed = DateTime.UtcNow - startTime;

                // calculate the skill points to add
                double skillPointsToAdd = timePassed.TotalMinutes * this.Character.GetSkillPointsPerMinute(skill);
                
                skill.Points += skillPointsToAdd;
            }
            
            this.FreeSkillQueueTimers();
            
            foreach (Character.SkillQueueEntry entry in this.Character.SkillQueue)
            {
                // mark the skill as stopped and store it in the database
                entry.Skill.ExpiryTime = 0;
                entry.Skill.Persist();
                
                // notify the skill is not in training anymore
                call.Client.NotifyMultiEvent(new OnSkillTrainingStopped(entry.Skill));
                
                // create history entry
                this.DB.CreateSkillHistoryRecord(entry.Skill.Type, this.Character, SkillHistoryReason.SkillTrainingCancelled, entry.Skill.Points);
            }

            return null;
        }

        public PyDataType AddToEndOfSkillQueue(PyInteger typeID, PyInteger level, CallInformation call)
        {
            // the skill queue must start only if it's empty OR there's something already in there
            bool shouldStart = true;

            if (this.Character.SkillQueue.Count > 0)
                shouldStart = this.Character.SkillQueue[0].Skill.ExpiryTime != 0;

            // get the skill the player wants to train
            Skill skill = this.Character.InjectedSkills.First(x => x.Value.Type.ID == typeID).Value;
            
            // do not start the skill training if the level is 5 already
            if (skill is null || skill.Level == 5)
                return null;

            PyList<PyTuple> queue = new PyList<PyTuple>();

            bool alreadyAdded = false;
            
            // build a list of skills to train based off the original queue
            // but with the new skill on top
            foreach (Character.SkillQueueEntry entry in this.Character.SkillQueue)
            {
                // ignore the skill in the queue if it was the one requested
                if (entry.Skill.Type.ID == typeID && entry.TargetLevel == level)
                    alreadyAdded = true;

                queue.Add(
                    new PyTuple(2)
                    {
                        [0] = entry.Skill.Type.ID,
                        [1] = entry.TargetLevel
                    }
                );
            }

            if (alreadyAdded == false)
            {
                queue.Add(
                    new PyTuple(2)
                    {
                        [0] = typeID,
                        [1] = level
                    }
                );
            }
            
            // save the new skill queue
            this.SaveSkillQueue(queue, call);

            if (shouldStart == false)
            {
                // stop the queue, there's nothing we should be training as the queue is currently paused
                this.CharStopTrainingSkill(call);
            }
            
            return null;
        }

        public PyDictionary<PyString, PyInteger> GetRespecInfo(CallInformation call)
        {
            return new PyDictionary<PyString, PyInteger>
            {
                ["nextRespecTime"] = this.Character.NextReSpecTime,
                ["freeRespecs"] = this.Character.FreeReSpecs
            };
        }

        public PyDataType GetCharacterAttributeModifiers(PyInteger attributeID, CallInformation call)
        {
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
                    return new PyList<PyTuple>();
            }
            
            PyList<PyTuple> modifiers = new PyList<PyTuple>();
            
            foreach (KeyValuePair<int, ItemEntity> modifier in this.Character.Modifiers)
            {
                if (modifier.Value.Attributes.AttributeExists(attribute) == true)
                {
                    // for now add all the elements as dgmAssModAdd
                    // check ApplyModifiers on attributes.py
                    // TODO: THE THIRD PARAMETER HERE WAS DECIDED RANDOMLY BASED ON THE CODE ITSELF
                    // TODO: BUT THAT DOESN'T MEAN THAT IT'S ENTIRELY CORRECT
                    // TODO: SO MAYBE CHECK IF THIS IS CORRECT SOMETIME AFTER
                    modifiers.Add(
                        new PyTuple(4)
                        {
                            [0] = modifier.Value.ID,
                            [1] = modifier.Value.Type.ID,
                            [2] = 2,
                            [3] = modifier.Value.Attributes[attribute]                            
                        }
                    );
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
            
            if (this.Character.FreeReSpecs == 0)
                throw new CustomError("You've already remapped your character too much times at once, wait some time");
            
            // check if the respec is the same as it was already
            if (charisma == this.Character.Charisma && intelligence == this.Character.Intelligence &&
                memory == this.Character.Memory && perception == this.Character.Perception && willpower == this.Character.Willpower)
                throw new CustomError("No changes detected on the neural map");
            
            // take one respec out
            this.Character.FreeReSpecs--;
            
            // if respec is zero now means we don't have any free respecs until a year later
            if (this.Character.FreeReSpecs == 0)
                this.Character.NextReSpecTime = DateTime.UtcNow.AddYears(1).ToFileTimeUtc();
            
            // ensure the respec timer is there
            this.SetupReSpecTimers();
            
            // finally set our attributes to the correct values
            this.Character.Charisma = charisma;
            this.Character.Intelligence = intelligence;
            this.Character.Memory = memory;
            this.Character.Perception = perception;
            this.Character.Willpower = willpower;

            // save the character
            this.Character.Persist();
            
            // notify the game of the change on the character
            call.Client.NotifyAttributeChange(
                new ItemAttribute[]
                {
                    this.Character.Attributes[AttributeEnum.charisma],
                    this.Character.Attributes[AttributeEnum.perception],
                    this.Character.Attributes[AttributeEnum.intelligence],
                    this.Character.Attributes[AttributeEnum.memory],
                    this.Character.Attributes[AttributeEnum.willpower]
                },
                this.Character
            );

            return null;
        }

        public PyDataType CharAddImplant(PyInteger itemID, CallInformation call)
        {
            if (this.Character.SkillQueue.Count > 0)
                throw new FailedPlugInImplant();
            
            // get the item and plug it into our brain now!
            ItemEntity item = this.ItemManager.LoadItem(itemID);
            
            // ensure the item is somewhere we can interact with it
            if (item.LocationID != call.Client.ShipID && item.LocationID != call.Client.StationID)
                throw new CustomError("You do not have direct access to this implant");

            // check if the slot is free or not
            this.Character.EnsureFreeImplantSlot(item);
            
            // check ownership and skills required to plug in the implant
            item.EnsureOwnership(this.Character);
            item.CheckPrerequisites(this.Character);
            
            // separate the item if there's more than one
            if (item.Quantity > 1)
            {
                item.Quantity--;
                
                // notify the client of the stack change
                call.Client.NotifyMultiEvent(OnItemChange.BuildQuantityChange(item, item.Quantity + 1));
                
                // save the item to the database
                item.Persist();
                
                // create the new item with a default location and flag
                // this way the item location change notification is only needed once
                item = this.ItemManager.CreateSimpleItem(item.Type, item.OwnerID, 0,
                    ItemFlags.None, 1, item.Contraband, item.Singleton);
            }

            int oldLocationID = item.LocationID;
            ItemFlags oldFlag = item.Flag;
            
            item.LocationID = this.Character.ID;
            item.Flag = ItemFlags.Implant;

            call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, oldFlag, oldLocationID));

            // add the item to the inventory it belongs
            this.Character.AddItem(item);

            // persist item changes to database
            item.Persist();
            
            return null;
        }

        public PyDataType RemoveImplantFromCharacter(PyInteger itemID, CallInformation call)
        {
            if (this.Character.Items.TryGetValue(itemID, out ItemEntity item) == false)
                throw new CustomError("This implant is not in your brain!");

            // now destroy the item
            this.ItemManager.DestroyItem(item);
            
            // notify the change
            call.Client.NotifyMultiEvent(OnItemChange.BuildLocationChange(item, this.Character.ID));
            
            return null;
        }
    }
}