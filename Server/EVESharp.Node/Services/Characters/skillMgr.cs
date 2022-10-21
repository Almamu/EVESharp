using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.Database;
using EVESharp.Database.Characters;
using EVESharp.Database.Extensions;
using EVESharp.Database.Inventory;
using EVESharp.Database.Inventory.Attributes;
using EVESharp.Database.Old;
using EVESharp.EVE;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Dogma;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.character;
using EVESharp.EVE.Exceptions.skillMgr;
using EVESharp.EVE.Network.Services;
using EVESharp.EVE.Network.Services.Validators;
using EVESharp.EVE.Notifications;
using EVESharp.EVE.Notifications.Inventory;
using EVESharp.EVE.Notifications.Skills;
using EVESharp.EVE.Sessions;
using EVESharp.Types;
using EVESharp.Types.Collections;
using Serilog;

namespace EVESharp.Node.Services.Characters;

[MustBeCharacter]
public class skillMgr : ClientBoundService
{
    private const   int                 MAXIMUM_ATTRIBUTE_POINTS       = 15;
    private const   int                 MINIMUM_ATTRIBUTE_POINTS       = 5;
    private const   int                 MAXIMUM_TOTAL_ATTRIBUTE_POINTS = 39;
    public override AccessLevel         AccessLevel        => AccessLevel.None;
    private         SkillDB             DB                 { get; }
    private         IItems              Items              { get; }
    private         ITimers             Timers             { get; }
    private         ISolarSystems       SolarSystems       { get; }
    private         ILogger             Log                { get; }
    private         Character           Character          { get; }
    private         IDogmaNotifications DogmaNotifications { get; }
    private         Timer <int>         NextSkillTimer     { get; set; }
    private         Timer <int>         ReSpecTimer        { get; set; }
    private         IDatabase           Database           { get; }
    private         IDogmaItems         DogmaItems         { get; }

    public skillMgr
    (
        SkillDB              db,      IItems  items,  ITimers             timers,   IDogmaNotifications dogmaNotifications,
        IBoundServiceManager manager, ILogger logger, IDatabase database, ISolarSystems       solarSystems, IDogmaItems dogmaItems
    ) : base (manager)
    {
        DB                 = db;
        Items              = items;
        Timers             = timers;
        DogmaNotifications = dogmaNotifications;
        Log                = logger;
        Database           = database;
        SolarSystems       = solarSystems;
        DogmaItems         = dogmaItems;
    }

    protected skillMgr
    (
        SkillDB              db,      IItems  items,  ITimers timers,  IDogmaNotifications dogmaNotifications,
        IBoundServiceManager manager, ILogger logger, Session session, ISolarSystems       solarSystems,
        IDogmaItems dogmaItems
    ) : base (manager, session, session.CharacterID)
    {
        DB                 = db;
        Items              = items;
        Timers             = timers;
        DogmaNotifications = dogmaNotifications;
        Character          = this.Items.GetItem <Character> (ObjectID);
        Log                = logger;
        SolarSystems       = solarSystems;
        DogmaItems         = dogmaItems;

        this.InitializeCharacter ();
    }

    private void SetupTimerForNextSkillInQueue ()
    {
        if (Character.SkillQueue.Count == 0)
            return;

        Character.SkillQueueEntry entry = Character.SkillQueue [0];

        if (entry.Skill.ExpiryTime == 0)
            return;

        // send notification of skill training started
        this.DogmaNotifications.QueueMultiEvent (Session.CharacterID, new OnSkillStartTraining (entry.Skill));

        NextSkillTimer = Timers.EnqueueTimer (DateTime.FromFileTimeUtc (entry.Skill.ExpiryTime), this.OnSkillTrainingCompleted, entry.Skill.ID);
    }

    private void SetupReSpecTimers ()
    {
        if (Character.FreeReSpecs == 0 && Character.NextReSpecTime > 0)
            ReSpecTimer = Timers.EnqueueTimer (DateTime.FromFileTimeUtc (Character.NextReSpecTime), this.OnNextReSpecAvailable, Character.ID);
    }

    private void InitializeCharacter ()
    {
        // perform basic checks on the skill queue

        // iterate the skill queue and generate a timer for the first skill that must be trained
        // this also prepares the correct notification for multiple skill training done
        PyList <PyInteger>               skillTypeIDs = new PyList <PyInteger> ();
        List <Character.SkillQueueEntry> toRemove     = new List <Character.SkillQueueEntry> ();

        foreach (Character.SkillQueueEntry entry in Character.SkillQueue)
            if (entry.Skill.ExpiryTime < DateTime.Now.ToFileTimeUtc ())
            {
                // ensure the skill is marked as trained and that they have the correct values stored
                entry.Skill.Level      = entry.TargetLevel;
                entry.Skill.ExpiryTime = 0;
                
                DogmaItems.MoveItem (entry.Skill, Flags.Skill);
                
                // add the skill to the list of trained skills for the big notification
                skillTypeIDs.Add (entry.Skill.Type.ID);
                toRemove.Add (entry);

                // also notify attribute changes
                this.DogmaNotifications.NotifyAttributeChange (Character.ID, new [] {AttributeTypes.skillPoints, AttributeTypes.skillLevel}, entry.Skill);
            }

        // remove skills that already expired
        Character.SkillQueue.RemoveAll (x => toRemove.Contains (x));

        // send notification of multiple skills being finished training (if any)
        if (skillTypeIDs.Count > 0)
            this.DogmaNotifications.QueueMultiEvent (Session.CharacterID, new OnGodmaMultipleSkillsTrained (skillTypeIDs));

        // persists the skill queue
        Character.Persist ();

        // setup the process for training next skill in the queue
        this.SetupTimerForNextSkillInQueue ();
    }

    private void FreeSkillQueueTimers ()
    {
        if (Character.SkillQueue.Count == 0)
            return;

        Character.SkillQueueEntry entry = Character.SkillQueue [0];

        if (entry.Skill.ExpiryTime == 0)
            return;

        NextSkillTimer?.Dispose ();
        NextSkillTimer = null;
    }

    private void FreeReSpecTimers ()
    {
        if (Character.NextReSpecTime == 0)
            return;

        ReSpecTimer?.Dispose ();
        ReSpecTimer = null;
    }

    protected override void OnClientDisconnected ()
    {
        this.FreeSkillQueueTimers ();
        this.FreeReSpecTimers ();
    }

    private void OnNextReSpecAvailable (int itemID)
    {
        // update respec values
        Character.NextReSpecTime = 0;
        Character.FreeReSpecs    = 1;
        Character.Persist ();
    }

    private void OnSkillTrainingCompleted (int itemID)
    {
        Skill skill = Character.Items [itemID] as Skill;

        // setup the skill data
        skill.Level      += 1;
        skill.ExpiryTime =  0;
        
        // notify the attribute changes
        this.DogmaNotifications.NotifyAttributeChange (Character.ID, new [] {AttributeTypes.skillPoints, AttributeTypes.skillLevel}, skill);
        
        // finally move the item
        DogmaItems.MoveItem (skill, Flags.Skill);
        
        // send OnSkillTrained so the player receives a nice popup
        this.DogmaNotifications.QueueMultiEvent (Character.ID, new OnSkillTrained (skill));

        // create history entry
        DB.CreateSkillHistoryRecord (skill.Type, Character, SkillHistoryReason.SkillTrainingComplete, skill.Points);

        // finally remove it off the skill queue
        Character.SkillQueue.RemoveAll (x => x.Skill.ID == skill.ID && x.TargetLevel == skill.Level);

        Character.CalculateSkillPoints ();

        // get the next skill from the queue (if any) and send the client proper notifications
        if (Character.SkillQueue.Count == 0)
        {
            Character.Persist ();

            return;
        }

        skill = Character.SkillQueue [0].Skill;

        // setup the process for training next skill in the queue
        this.SetupTimerForNextSkillInQueue ();

        // create history entry
        DB.CreateSkillHistoryRecord (skill.Type, Character, SkillHistoryReason.SkillTrainingStarted, skill.Points);

        // persist the character changes
        Character.Persist ();
    }

    public PyDataType GetSkillQueue (ServiceCall call)
    {
        Character character = this.Items.GetItem <Character> (call.Session.CharacterID);

        PyList skillQueueList = new PyList (character.SkillQueue.Count);

        int index = 0;

        foreach (Character.SkillQueueEntry entry in character.SkillQueue)
            skillQueueList [index++] = entry;

        return skillQueueList;
    }

    public PyDataType GetSkillHistory (ServiceCall call)
    {
        return DB.GetSkillHistory (call.Session.CharacterID);
    }

    public PyDataType InjectSkillIntoBrain (ServiceCall call, PyList itemIDs)
    {
        foreach (PyInteger item in itemIDs.GetEnumerable <PyInteger> ())
            try
            {
                // get the item by it's ID and change the location of it
                Skill skill = this.Items.GetItem <Skill> (item);

                // check if the character already has this skill injected
                if (Character.InjectedSkillsByTypeID.ContainsKey (skill.Type.ID))
                    throw new CharacterAlreadyKnowsSkill (skill.Type);
                
                // split the item and plug it into the character's brain
                Skill injectedSkill = DogmaItems.SplitStack (
                    skill, 1, Character.ID, Character.ID, Flags.Skill
                ) as Skill;
                // ensure it's singleton
                DogmaItems.SetSingleton (injectedSkill, true);
                // ensure the skill level is saved
                injectedSkill.Level = 0;
                // save changes
                injectedSkill.Persist ();
            }
            catch (CharacterAlreadyKnowsSkill)
            {
                throw;
            }
            catch (Exception)
            {
                Log.Error ($"Cannot inject itemID {item} into {Character.ID}'s brain...");

                throw;
            }

        // send the skill injected notification to refresh windows if needed
        this.DogmaNotifications.QueueMultiEvent (call.Session.CharacterID, new OnSkillInjected ());

        return null;
    }

    public PyDataType SaveSkillQueue (ServiceCall call, PyList queue)
    {
        if (Character.SkillQueue.Count > 0)
        {
            // calculate current skill in training points
            Skill currentSkill = Character.SkillQueue [0].Skill;

            if (currentSkill.ExpiryTime > 0)
            {
                // get the total amount of minutes the skill would have taken to train completely
                long pointsLeft = (long) (currentSkill.GetSkillPointsForLevel (Character.SkillQueue [0].TargetLevel) - currentSkill.Points);

                TimeSpan timeLeft   = TimeSpan.FromMinutes (pointsLeft / Character.GetSkillPointsPerMinute (currentSkill));
                DateTime endTime    = DateTime.FromFileTimeUtc (currentSkill.ExpiryTime);
                DateTime startTime  = endTime.Subtract (timeLeft);
                TimeSpan timePassed = DateTime.UtcNow - startTime;

                // calculate the skill points to add
                double skillPointsToAdd = timePassed.TotalMinutes * Character.GetSkillPointsPerMinute (currentSkill);

                currentSkill.Points += skillPointsToAdd;
            }

            // remove the timer associated with the queue
            this.FreeSkillQueueTimers ();

            foreach (Character.SkillQueueEntry entry in Character.SkillQueue)
            {
                entry.Skill.ExpiryTime = 0;
                
                DogmaItems.MoveItem (entry.Skill, Flags.Skill);
                
                // send notification of skill training stopped
                this.DogmaNotifications.QueueMultiEvent (call.Session.CharacterID, new OnSkillTrainingStopped (entry.Skill));

                // create history entry
                DB.CreateSkillHistoryRecord (
                    entry.Skill.Type, Character, SkillHistoryReason.SkillTrainingCancelled,
                    entry.Skill.Points
                );
            }

            Character.SkillQueue.Clear ();
        }

        DateTime startDateTime = DateTime.UtcNow;
        bool     first         = true;

        foreach (PyTuple entry in queue.GetEnumerable <PyTuple> ())
        {
            // ignore wrong entries
            if (entry.Count != 2)
                continue;

            int typeID = entry [0] as PyInteger;
            int level  = entry [1] as PyInteger;

            // search for an item with the given typeID
            ItemEntity item = Character.Items.First (x => x.Value.Type.ID == typeID && (x.Value.Flag == Flags.Skill || x.Value.Flag == Flags.SkillInTraining))
                                       .Value;

            // ignore items that are not skills
            if (item is Skill == false)
                continue;

            Skill skill = item as Skill;

            double skillPointsLeft = skill.GetSkillPointsForLevel (level) - skill.Points;

            TimeSpan duration = TimeSpan.FromMinutes (skillPointsLeft / Character.GetSkillPointsPerMinute (skill));

            DateTime expiryTime = startDateTime + duration;
            
            skill.ExpiryTime = expiryTime.ToFileTimeUtc ();
            DogmaItems.MoveItem (skill, Flags.SkillInTraining);
            
            startDateTime = expiryTime;

            // skill added to the queue, persist the character to ensure all the changes are saved
            Character.SkillQueue.Add (
                new Character.SkillQueueEntry
                {
                    Skill       = skill,
                    TargetLevel = level
                }
            );

            if (first)
            {
                // skill was trained, send the success message
                this.DogmaNotifications.QueueMultiEvent (call.Session.CharacterID, new OnSkillStartTraining (skill));

                // create history entry
                DB.CreateSkillHistoryRecord (skill.Type, Character, SkillHistoryReason.SkillTrainingStarted, skill.Points);

                first = false;
            }

            skill.Persist ();
        }

        // ensure the timer is present for the first skill in the queue
        this.SetupTimerForNextSkillInQueue ();

        // finally persist the data to the database
        Character.Persist ();

        return null;
    }

    public PyDataType CharStartTrainingSkillByTypeID (ServiceCall call, PyInteger typeID)
    {
        // get the skill the player wants to train
        Skill skill = Character.InjectedSkills.First (x => x.Value.Type.ID == typeID).Value;

        // do not start the skill training if the level is 5 already
        if (skill is null || skill.Level == 5)
            return null;

        PyList <PyTuple> queue = new PyList <PyTuple> (1)
        {
            [0] = new PyTuple (2)
            {
                [0] = typeID,
                [1] = skill.Level + 1
            }
        };

        // build a list of skills to train based off the original queue
        // but with the new skill on top
        foreach (Character.SkillQueueEntry entry in Character.SkillQueue)
        {
            // ignore the skill in the queue if it was the one requested
            if (entry.Skill.Type.ID == typeID)
                continue;

            queue.Add (
                new PyTuple (2)
                {
                    [0] = entry.Skill.Type.ID,
                    [1] = entry.TargetLevel
                }
            );
        }

        // save the new skill queue
        return this.SaveSkillQueue (call, queue);
    }

    public PyDataType GetEndOfTraining (ServiceCall call)
    {
        // do not allow the user to do that if the skill queue is empty
        if (Character.SkillQueue.Count == 0)
            return 0;

        return Character.SkillQueue [0].Skill.ExpiryTime;
    }

    public PyDataType CharStopTrainingSkill (ServiceCall call)
    {
        // iterate the whole skill queue, stop it and recalculate points for the skills
        if (Character.SkillQueue.Count == 0)
            return null;

        // only the skill on the front should have it's skillpoints recalculated
        Skill skill = Character.SkillQueue [0].Skill;

        if (skill.ExpiryTime > 0)
        {
            // get the total amount of minutes the skill would have taken to train completely
            long pointsLeft = (long) (skill.GetSkillPointsForLevel (Character.SkillQueue [0].TargetLevel) - skill.Points);

            TimeSpan timeLeft   = TimeSpan.FromMinutes (pointsLeft / Character.GetSkillPointsPerMinute (skill));
            DateTime endTime    = DateTime.FromFileTimeUtc (skill.ExpiryTime);
            DateTime startTime  = endTime.Subtract (timeLeft);
            TimeSpan timePassed = DateTime.UtcNow - startTime;

            // calculate the skill points to add
            double skillPointsToAdd = timePassed.TotalMinutes * Character.GetSkillPointsPerMinute (skill);

            skill.Points += skillPointsToAdd;
        }

        this.FreeSkillQueueTimers ();

        foreach (Character.SkillQueueEntry entry in Character.SkillQueue)
        {
            // mark the skill as stopped and store it in the database
            entry.Skill.ExpiryTime = 0;
            entry.Skill.Persist ();

            // notify the skill is not in training anymore
            this.DogmaNotifications.QueueMultiEvent (call.Session.CharacterID, new OnSkillTrainingStopped (entry.Skill));

            // create history entry
            DB.CreateSkillHistoryRecord (entry.Skill.Type, Character, SkillHistoryReason.SkillTrainingCancelled, entry.Skill.Points);
        }

        return null;
    }

    public PyDataType AddToEndOfSkillQueue (ServiceCall call, PyInteger typeID, PyInteger level)
    {
        // the skill queue must start only if it's empty OR there's something already in there
        bool shouldStart = true;

        if (Character.SkillQueue.Count > 0)
            shouldStart = Character.SkillQueue [0].Skill.ExpiryTime != 0;

        // get the skill the player wants to train
        Skill skill = Character.InjectedSkills.First (x => x.Value.Type.ID == typeID).Value;

        // do not start the skill training if the level is 5 already
        if (skill is null || skill.Level == 5)
            return null;

        PyList <PyTuple> queue = new PyList <PyTuple> ();

        bool alreadyAdded = false;

        // build a list of skills to train based off the original queue
        // but with the new skill on top
        foreach (Character.SkillQueueEntry entry in Character.SkillQueue)
        {
            // ignore the skill in the queue if it was the one requested
            if (entry.Skill.Type.ID == typeID && entry.TargetLevel == level)
                alreadyAdded = true;

            queue.Add (
                new PyTuple (2)
                {
                    [0] = entry.Skill.Type.ID,
                    [1] = entry.TargetLevel
                }
            );
        }

        if (alreadyAdded == false)
            queue.Add (
                new PyTuple (2)
                {
                    [0] = typeID,
                    [1] = level
                }
            );

        // save the new skill queue
        this.SaveSkillQueue (call, queue);

        if (shouldStart == false)
            // stop the queue, there's nothing we should be training as the queue is currently paused
            this.CharStopTrainingSkill (call);

        return null;
    }

    public PyDictionary <PyString, PyInteger> GetRespecInfo (ServiceCall call)
    {
        return new PyDictionary <PyString, PyInteger>
        {
            ["nextRespecTime"] = Character.NextReSpecTime,
            ["freeRespecs"]    = Character.FreeReSpecs
        };
    }

    public PyDataType GetCharacterAttributeModifiers (ServiceCall call, PyInteger attributeID)
    {
        AttributeTypes attribute;

        switch ((int) attributeID)
        {
            case (int) AttributeTypes.willpower:
                attribute = AttributeTypes.willpowerBonus;
                break;

            case (int) AttributeTypes.charisma:
                attribute = AttributeTypes.charismaBonus;
                break;

            case (int) AttributeTypes.memory:
                attribute = AttributeTypes.memoryBonus;
                break;

            case (int) AttributeTypes.intelligence:
                attribute = AttributeTypes.intelligenceBonus;
                break;

            case (int) AttributeTypes.perception:
                attribute = AttributeTypes.perceptionBonus;
                break;

            default: return new PyList <PyTuple> ();
        }

        PyList <PyTuple> modifiers = new PyList <PyTuple> ();

        foreach ((int _, ItemEntity modifier) in Character.Modifiers)
        {
            if (modifier.Attributes.AttributeExists (attribute))
                // for now add all the elements as dgmAssModAdd
                // check ApplyModifiers on attributes.py
                // TODO: THE THIRD PARAMETER HERE WAS DECIDED RANDOMLY BASED ON THE CODE ITSELF
                // TODO: BUT THAT DOESN'T MEAN THAT IT'S ENTIRELY CORRECT
                // TODO: SO MAYBE CHECK IF THIS IS CORRECT SOMETIME AFTER
                modifiers.Add (
                    new PyTuple (4)
                    {
                        [0] = modifier.ID,
                        [1] = modifier.Type.ID,
                        [2] = 2,
                        [3] = modifier.Attributes [attribute]
                    }
                );
        }

        // search for skills that modify this attribute
        return modifiers;
    }

    public PyDataType RespecCharacter
    (
        ServiceCall call,       PyInteger charisma, PyInteger intelligence, PyInteger memory,
        PyInteger       perception, PyInteger willpower
    )
    {
        if (charisma < MINIMUM_ATTRIBUTE_POINTS || intelligence < MINIMUM_ATTRIBUTE_POINTS ||
            memory < MINIMUM_ATTRIBUTE_POINTS || perception < MINIMUM_ATTRIBUTE_POINTS ||
            willpower < MINIMUM_ATTRIBUTE_POINTS)
            throw new RespecAttributesTooLow ();

        if (charisma >= MAXIMUM_ATTRIBUTE_POINTS || intelligence >= MAXIMUM_ATTRIBUTE_POINTS ||
            memory >= MAXIMUM_ATTRIBUTE_POINTS || perception >= MAXIMUM_ATTRIBUTE_POINTS ||
            willpower >= MAXIMUM_ATTRIBUTE_POINTS)
            throw new RespecAttributesTooHigh ();

        if (charisma + intelligence + memory + perception + willpower != MAXIMUM_TOTAL_ATTRIBUTE_POINTS)
            throw new RespecAttributesMisallocated ();

        if (Character.FreeReSpecs == 0)
            throw new CustomError ("You've already remapped your character too much times at once, wait some time");

        // check if the respec is the same as it was already
        if (charisma == Character.Charisma && intelligence == Character.Intelligence &&
            memory == Character.Memory && perception == Character.Perception && willpower == Character.Willpower)
            throw new CustomError ("No changes detected on the neural map");

        // take one respec out
        Character.FreeReSpecs--;

        // if respec is zero now means we don't have any free respecs until a year later
        if (Character.FreeReSpecs == 0)
            Character.NextReSpecTime = DateTime.UtcNow.AddYears (1).ToFileTimeUtc ();

        // ensure the respec timer is there
        this.SetupReSpecTimers ();

        // finally set our attributes to the correct values
        Character.Charisma     = charisma;
        Character.Intelligence = intelligence;
        Character.Memory       = memory;
        Character.Perception   = perception;
        Character.Willpower    = willpower;

        // save the character
        Character.Persist ();

        // notify the game of the change on the character
        this.DogmaNotifications.NotifyAttributeChange (
            Character.ID,
            new []
            {
                Character.Attributes [AttributeTypes.charisma],
                Character.Attributes [AttributeTypes.perception],
                Character.Attributes [AttributeTypes.intelligence],
                Character.Attributes [AttributeTypes.memory],
                Character.Attributes [AttributeTypes.willpower]
            },
            Character
        );

        return null;
    }

    public PyDataType CharAddImplant (ServiceCall call, PyInteger itemID)
    {
        if (Character.SkillQueue.Count > 0)
            throw new FailedPlugInImplant ();

        int characterID = call.Session.CharacterID;

        // get the item and plug it into our brain now!
        ItemEntity item = this.Items.LoadItem (itemID);

        // ensure the item is somewhere we can interact with it
        item.EnsureOwnership (characterID, call.Session.CorporationID, call.Session.CorporationRole, true);

        // check if the slot is free or not
        Character.EnsureFreeImplantSlot (item);

        // check ownership and skills required to plug in the implant
        item.CheckPrerequisites (Character);
        
        // move the item into the character's implant slots and set the singleton value
        DogmaItems.SplitStack (item, 1, Flags.Implant);
        DogmaItems.SetSingleton (item, true);

        return null;
    }

    public PyDataType RemoveImplantFromCharacter (ServiceCall call, PyInteger itemID)
    {
        if (Character.Items.TryGetValue (itemID, out ItemEntity item) == false)
            throw new CustomError ("This implant is not in your brain!");

        DogmaItems.DestroyItem (item);
        
        return null;
    }

    protected override long MachoResolveObject (ServiceCall call, ServiceBindParams parameters)
    {
        return Database.CluResolveAddress ("solarsystem", parameters.ObjectID);
    }

    protected override BoundService CreateBoundInstance (ServiceCall call, ServiceBindParams bindParams)
    {
        if (this.MachoResolveObject (call, bindParams) != BoundServiceManager.MachoNet.NodeID)
            throw new CustomError ("Trying to bind an object that does not belong to us!");

        return new skillMgr (DB, this.Items, Timers, this.DogmaNotifications, BoundServiceManager, Log, call.Session, this.SolarSystems, DogmaItems);
    }
}