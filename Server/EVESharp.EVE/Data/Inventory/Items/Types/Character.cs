using System.Collections.Generic;
using System.Linq;
using EVESharp.EVE.Data.Inventory.Attributes;
using EVESharp.EVE.Exceptions;
using EVESharp.EVE.Exceptions.character;
using EVESharp.EVE.Exceptions.Internal;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class Character : ItemInventory
{
    public delegate List <SkillQueueEntry> CharacterSkillQueueLoadEventHandler (Character character, Dictionary <int, Skill> skillQueue);
    private         double                 mSkillPoints;

    private List <SkillQueueEntry> mSkillQueue;

    /// <summary>
    /// Event fired when the skill queue has to be loaded
    /// </summary>
    public CharacterSkillQueueLoadEventHandler OnSkillQueueLoad;

    public Information.Character CharacterInformation { get; }

    public int AccountID => this.CharacterInformation.AccountID;

    public int? ActiveCloneID
    {
        get => this.CharacterInformation.ActiveCloneID;
        set
        {
            this.Information.Dirty                  = true;
            this.CharacterInformation.ActiveCloneID = value;
        }
    }

    public string Title => this.CharacterInformation.Title;

    public string Description
    {
        get => this.CharacterInformation.Description;
        set
        {
            this.Information.Dirty                = true;
            this.CharacterInformation.Description = value;
        }
    }

    public int TitleMask
    {
        get => this.CharacterInformation.TitleMask;
        set
        {
            this.Information.Dirty              = true;
            this.CharacterInformation.TitleMask = value;
        }
    }

    public int CorporationID
    {
        get => this.CharacterInformation.CorporationID;
        set
        {
            this.Information.Dirty                  = true;
            this.CharacterInformation.CorporationID = value;
        }
    }

    public int? AllianceID
    {
        get => this.CharacterInformation.AllianceID;
        set => this.CharacterInformation.AllianceID = value;
    }

    public double SecurityRating  => this.CharacterInformation.SecurityRating;
    public string PetitionMessage => this.CharacterInformation.PetitionMessage;
    public int    LogonMinutes    => this.CharacterInformation.LogonMinutes;

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long Roles
    {
        get => this.CharacterInformation.Roles;
        set => this.CharacterInformation.Roles = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long RolesAtBase
    {
        get => this.CharacterInformation.RolesAtBase;
        set => this.CharacterInformation.RolesAtBase = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long RolesAtHq
    {
        get => this.CharacterInformation.RolesAtHq;
        set => this.CharacterInformation.RolesAtHq = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long RolesAtOther
    {
        get => this.CharacterInformation.RolesAtOther;
        set => this.CharacterInformation.RolesAtOther = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public int? BaseID
    {
        get => this.CharacterInformation.BaseID;
        set => this.CharacterInformation.BaseID = value;
    }

    public long CorporationDateTime
    {
        get => this.CharacterInformation.CorporationDateTime;
        set
        {
            this.Information.Dirty                        = true;
            this.CharacterInformation.CorporationDateTime = value;
        }
    }

    public int CorpAccountKey
    {
        get => this.CharacterInformation.CorpAccountKey;
        set
        {
            this.Information.Dirty                   = true;
            this.CharacterInformation.CorpAccountKey = value;
        }
    }

    public long    StartDateTime      => this.CharacterInformation.StartDateTime;
    public long    CreateDateTime     => this.CharacterInformation.CreateDateTime;
    public int     AncestryID         => this.CharacterInformation.AncestryID;
    public int     CareerID           => this.CharacterInformation.CareerID;
    public int     SchoolID           => this.CharacterInformation.SchoolID;
    public int     CareerSpecialityID => this.CharacterInformation.CareerSpecialityID;
    public int     Gender             => this.CharacterInformation.Gender;
    public int?    AccessoryID        => this.CharacterInformation.AncestryID;
    public int?    BeardID            => this.CharacterInformation.BeardID;
    public int     CostumeID          => this.CharacterInformation.CostumeID;
    public int?    DecoID             => this.CharacterInformation.DecoID;
    public int     EyebrowsID         => this.CharacterInformation.EyebrowsID;
    public int     EyesID             => this.CharacterInformation.EyesID;
    public int     HairID             => this.CharacterInformation.HairID;
    public int?    LipstickID         => this.CharacterInformation.LipstickID;
    public int?    MakeupID           => this.CharacterInformation.MakeupID;
    public int     SkinID             => this.CharacterInformation.SkinID;
    public int     BackgroundID       => this.CharacterInformation.BackgroundID;
    public int     LightID            => this.CharacterInformation.LightID;
    public double  HeadRotation1      => this.CharacterInformation.HeadRotation1;
    public double  HeadRotation2      => this.CharacterInformation.HeadRotation2;
    public double  HeadRotation3      => this.CharacterInformation.HeadRotation3;
    public double  EyeRotation1       => this.CharacterInformation.EyeRotation1;
    public double  EyeRotation2       => this.CharacterInformation.EyeRotation2;
    public double  EyeRotation3       => this.CharacterInformation.EyeRotation3;
    public double  CamPos1            => this.CharacterInformation.CamPos1;
    public double  CamPos2            => this.CharacterInformation.CamPos2;
    public double  CamPos3            => this.CharacterInformation.CamPos3;
    public double? Morph1E            => this.CharacterInformation.Morph1E;
    public double? Morph1N            => this.CharacterInformation.Morph1N;
    public double? Morph1S            => this.CharacterInformation.Morph1S;
    public double? Morph1W            => this.CharacterInformation.Morph1W;
    public double? Morph2E            => this.CharacterInformation.Morph2E;
    public double? Morph2N            => this.CharacterInformation.Morph2N;
    public double? Morph2S            => this.CharacterInformation.Morph2S;
    public double? Morph2W            => this.CharacterInformation.Morph2W;
    public double? Morph3E            => this.CharacterInformation.Morph3E;
    public double? Morph3N            => this.CharacterInformation.Morph3N;
    public double? Morph3S            => this.CharacterInformation.Morph3S;
    public double? Morph3W            => this.CharacterInformation.Morph3W;
    public double? Morph4E            => this.CharacterInformation.Morph4E;
    public double? Morph4N            => this.CharacterInformation.Morph4N;
    public double? Morph4S            => this.CharacterInformation.Morph4S;
    public double? Morph4W            => this.CharacterInformation.Morph4W;
    public int     StationID          => this.CharacterInformation.StationID;
    public int     SolarSystemID      => this.CharacterInformation.SolarSystemID;
    public int     ConstellationID    => this.CharacterInformation.ConstellationID;
    public int     RegionID           => this.CharacterInformation.RegionID;

    public int FreeReSpecs
    {
        get => this.CharacterInformation.FreeReSpecs;
        set
        {
            this.Information.Dirty                = true;
            this.CharacterInformation.FreeReSpecs = value;
        }
    }

    public long NextReSpecTime
    {
        get => this.CharacterInformation.NextReSpecTime;
        set
        {
            this.Information.Dirty                   = true;
            this.CharacterInformation.NextReSpecTime = value;
        }
    }

    public long TimeLastJump
    {
        get => this.CharacterInformation.TimeLastJump;
        set
        {
            this.Information.Dirty                 = true;
            this.CharacterInformation.TimeLastJump = value;
        }
    }

    public int? WarFactionID
    {
        get => this.CharacterInformation.WarFactionID;
        set
        {
            this.Information.Dirty                 = true;
            this.CharacterInformation.WarFactionID = value;
        }
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long GrantableRoles
    {
        get => this.CharacterInformation.GrantableRoles;
        set => this.CharacterInformation.GrantableRoles = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long GrantableRolesAtHQ
    {
        get => this.CharacterInformation.GrantableRolesAtHQ;
        set => this.CharacterInformation.GrantableRolesAtHQ = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long GrantableRolesAtBase
    {
        get => this.CharacterInformation.GrantableRolesAtBase;
        set => this.CharacterInformation.GrantableRolesAtBase = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long GrantableRolesAtOther
    {
        get => this.CharacterInformation.GrantableRolesAtOther;
        set => this.CharacterInformation.GrantableRolesAtOther = value;
    }

    public long Charisma
    {
        get => this.Attributes [AttributeTypes.charisma].Integer;
        set => this.Attributes [AttributeTypes.charisma].Integer = value;
    }

    public long Willpower
    {
        get => this.Attributes [AttributeTypes.willpower].Integer;
        set => this.Attributes [AttributeTypes.willpower].Integer = value;
    }

    public long Intelligence
    {
        get => this.Attributes [AttributeTypes.intelligence].Integer;
        set => this.Attributes [AttributeTypes.intelligence].Integer = value;
    }

    public long Perception
    {
        get => this.Attributes [AttributeTypes.perception].Integer;
        set => this.Attributes [AttributeTypes.perception].Integer = value;
    }

    public long Memory
    {
        get => this.Attributes [AttributeTypes.memory].Integer;
        set => this.Attributes [AttributeTypes.memory].Integer = value;
    }

    public List <SkillQueueEntry> SkillQueue
    {
        get
        {
            // if the contents are not loaded then that needs to happen first
            if (this.ContentsLoaded == false)
                this.LoadContents ();

            // accessing the skillQueue might be a modification attempt
            // so the character must be marked as dirty
            this.Information.Dirty = true;

            return this.mSkillQueue;
        }
    }

    public Dictionary <int, Skill> InjectedSkills =>
        this.Items
            .Where (x => (x.Value.Flag == Flags.SkillInTraining || x.Value.Flag == Flags.Skill) && x.Value is Skill)
            .ToDictionary (dict => dict.Key, dict => dict.Value as Skill);

    public Dictionary <int, ItemEntity> Modifiers =>
        this.Items
            .Where (x => x.Value.Flag == Flags.SkillInTraining || x.Value.Flag == Flags.Skill || x.Value.Flag == Flags.Implant)
            .ToDictionary (dict => dict.Key, dict => dict.Value);

    public Dictionary <int, Skill> InjectedSkillsByTypeID =>
        this.Items
            .Where (x => (x.Value.Flag == Flags.Skill || x.Value.Flag == Flags.SkillInTraining) && x.Value is Skill)
            .ToDictionary (dict => dict.Value.Type.ID, dict => dict.Value as Skill);

    public Dictionary <int, Implant> PluggedInImplants =>
        this.Items
            .Where (x => x.Value.Flag == Flags.Implant && x.Value is Implant)
            .ToDictionary (dict => dict.Key, dict => dict.Value as Implant);

    public Dictionary <int, Implant> PluggedInImplantsByTypeID =>
        this.Items
            .Where (x => x.Value.Flag == Flags.Implant && x.Value is Implant)
            .ToDictionary (dict => dict.Key, dict => dict.Value as Implant);

    public Character (Information.Character info) : base (info.Information)
    {
        this.CharacterInformation = info;
    }

    protected override void LoadContents (Flags ignoreFlags = Flags.None)
    {
        base.LoadContents (ignoreFlags);

        this.CalculateSkillPoints ();

        // put things where they belong
        Dictionary <int, Skill> skillQueue = this.Items
                                                 .Where (x => x.Value.Flag == Flags.SkillInTraining && x.Value is Skill)
                                                 .ToDictionary (dict => dict.Key, dict => dict.Value as Skill);

        this.mSkillQueue = this.OnSkillQueueLoad?.Invoke (this, skillQueue);
    }

    public void CalculateSkillPoints ()
    {
        foreach ((int itemID, Skill skill) in this.InjectedSkills)
            // increase our skillpoints count with all the trained skills
            this.mSkillPoints += skill.Points;
    }

    public double GetSkillPointsPerMinute (Skill skill)
    {
        Attribute primarySpPerMin   = this.Attributes [skill.PrimaryAttribute.Integer];
        Attribute secondarySpPerMin = this.Attributes [skill.SecondaryAttribute.Integer];

        long skillLearningLevel = 0;

        if (this.InjectedSkillsByTypeID.TryGetValue ((int) EVE.Data.Inventory.TypeID.Learning, out Skill learningSkill))
            skillLearningLevel = learningSkill.Level;

        double spPerMin = primarySpPerMin + secondarySpPerMin / 2.0f;
        spPerMin = spPerMin * (1.0f + 0.02f * skillLearningLevel);

        if (this.mSkillPoints < 1600000.0f)
            spPerMin = spPerMin * 2.0f;

        return spPerMin;
    }

    public long GetSkillLevel (EVE.Data.Inventory.TypeID skillTypeID)
    {
        return this.GetSkillLevel ((int) skillTypeID);
    }

    public long GetSkillLevel (int skillTypeID)
    {
        if (this.InjectedSkillsByTypeID.TryGetValue (skillTypeID, out Skill skill) == false)
            return 0;

        return skill.Level;
    }

    /// <summary>
    /// Checks if the character has the required skill at the specified level and throws an exception if not
    /// </summary>
    /// <param name="skillTypeID">The skill to look for</param>
    /// <param name="level">The minimum level</param>
    /// <exception cref="SkillMissingException">If the skill requirement is not met</exception>
    public void EnsureSkillLevel (EVE.Data.Inventory.TypeID skillTypeID, int level = 1)
    {
        if (this.GetSkillLevel (skillTypeID) < level)
            throw new SkillRequired (skillTypeID);
    }

    public void EnsureFreeImplantSlot (ItemEntity newImplant)
    {
        int implantSlot = (int) newImplant.Attributes [AttributeTypes.implantness].Integer;

        foreach ((int _, Implant implant) in this.PluggedInImplants)
        {
            // the implant does not use any slot, here for sanity checking
            if (implant.Attributes.AttributeExists (AttributeTypes.implantness) == false)
                continue;

            if (implant.Attributes [AttributeTypes.implantness].Integer == implantSlot)
                throw new OnlyOneImplantActive (newImplant.Type);
        }
    }

    public class SkillQueueEntry
    {
        public Skill Skill;
        public int   TargetLevel;

        public static implicit operator PyDataType (SkillQueueEntry from)
        {
            return new PyTuple (2)
            {
                [0] = from.Skill.Type.ID,
                [1] = from.TargetLevel
            };
        }
    }
}