using System.Collections.Generic;
using System.Linq;
using EVESharp.EVE.Client.Exceptions;
using EVESharp.EVE.Client.Exceptions.character;
using EVESharp.EVE.Client.Exceptions.Internal;
using EVESharp.EVE.StaticData.Inventory;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using Attribute = EVESharp.EVE.Inventory.Attributes.Attribute;

namespace EVESharp.Node.Inventory.Items.Types;

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

    public int AccountID => CharacterInformation.AccountID;

    public int? ActiveCloneID
    {
        get => CharacterInformation.ActiveCloneID;
        set
        {
            Information.Dirty                  = true;
            CharacterInformation.ActiveCloneID = value;
        }
    }

    public string Title => CharacterInformation.Title;

    public string Description
    {
        get => CharacterInformation.Description;
        set
        {
            Information.Dirty                = true;
            CharacterInformation.Description = value;
        }
    }

    public int TitleMask
    {
        get => CharacterInformation.TitleMask;
        set
        {
            Information.Dirty              = true;
            CharacterInformation.TitleMask = value;
        }
    }

    public int CorporationID
    {
        get => CharacterInformation.CorporationID;
        set
        {
            Information.Dirty                  = true;
            CharacterInformation.CorporationID = value;
        }
    }

    public int? AllianceID
    {
        get => CharacterInformation.AllianceID;
        set => CharacterInformation.AllianceID = value;
    }

    public double SecurityRating  => CharacterInformation.SecurityRating;
    public string PetitionMessage => CharacterInformation.PetitionMessage;
    public int    LogonMinutes    => CharacterInformation.LogonMinutes;

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long Roles
    {
        get => CharacterInformation.Roles;
        set => CharacterInformation.Roles = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long RolesAtBase
    {
        get => CharacterInformation.RolesAtBase;
        set => CharacterInformation.RolesAtBase = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long RolesAtHq
    {
        get => CharacterInformation.RolesAtHq;
        set => CharacterInformation.RolesAtHq = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long RolesAtOther
    {
        get => CharacterInformation.RolesAtOther;
        set => CharacterInformation.RolesAtOther = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public int? BaseID
    {
        get => CharacterInformation.BaseID;
        set => CharacterInformation.BaseID = value;
    }

    public long CorporationDateTime
    {
        get => CharacterInformation.CorporationDateTime;
        set
        {
            Information.Dirty                        = true;
            CharacterInformation.CorporationDateTime = value;
        }
    }

    public int CorpAccountKey
    {
        get => CharacterInformation.CorpAccountKey;
        set
        {
            Information.Dirty                   = true;
            CharacterInformation.CorpAccountKey = value;
        }
    }

    public long    StartDateTime      => CharacterInformation.StartDateTime;
    public long    CreateDateTime     => CharacterInformation.CreateDateTime;
    public int     AncestryID         => CharacterInformation.AncestryID;
    public int     CareerID           => CharacterInformation.CareerID;
    public int     SchoolID           => CharacterInformation.SchoolID;
    public int     CareerSpecialityID => CharacterInformation.CareerSpecialityID;
    public int     Gender             => CharacterInformation.Gender;
    public int?    AccessoryID        => CharacterInformation.AncestryID;
    public int?    BeardID            => CharacterInformation.BeardID;
    public int     CostumeID          => CharacterInformation.CostumeID;
    public int?    DecoID             => CharacterInformation.DecoID;
    public int     EyebrowsID         => CharacterInformation.EyebrowsID;
    public int     EyesID             => CharacterInformation.EyesID;
    public int     HairID             => CharacterInformation.HairID;
    public int?    LipstickID         => CharacterInformation.LipstickID;
    public int?    MakeupID           => CharacterInformation.MakeupID;
    public int     SkinID             => CharacterInformation.SkinID;
    public int     BackgroundID       => CharacterInformation.BackgroundID;
    public int     LightID            => CharacterInformation.LightID;
    public double  HeadRotation1      => CharacterInformation.HeadRotation1;
    public double  HeadRotation2      => CharacterInformation.HeadRotation2;
    public double  HeadRotation3      => CharacterInformation.HeadRotation3;
    public double  EyeRotation1       => CharacterInformation.EyeRotation1;
    public double  EyeRotation2       => CharacterInformation.EyeRotation2;
    public double  EyeRotation3       => CharacterInformation.EyeRotation3;
    public double  CamPos1            => CharacterInformation.CamPos1;
    public double  CamPos2            => CharacterInformation.CamPos2;
    public double  CamPos3            => CharacterInformation.CamPos3;
    public double? Morph1E            => CharacterInformation.Morph1E;
    public double? Morph1N            => CharacterInformation.Morph1N;
    public double? Morph1S            => CharacterInformation.Morph1S;
    public double? Morph1W            => CharacterInformation.Morph1W;
    public double? Morph2E            => CharacterInformation.Morph2E;
    public double? Morph2N            => CharacterInformation.Morph2N;
    public double? Morph2S            => CharacterInformation.Morph2S;
    public double? Morph2W            => CharacterInformation.Morph2W;
    public double? Morph3E            => CharacterInformation.Morph3E;
    public double? Morph3N            => CharacterInformation.Morph3N;
    public double? Morph3S            => CharacterInformation.Morph3S;
    public double? Morph3W            => CharacterInformation.Morph3W;
    public double? Morph4E            => CharacterInformation.Morph4E;
    public double? Morph4N            => CharacterInformation.Morph4N;
    public double? Morph4S            => CharacterInformation.Morph4S;
    public double? Morph4W            => CharacterInformation.Morph4W;
    public int     StationID          => CharacterInformation.StationID;
    public int     SolarSystemID      => CharacterInformation.SolarSystemID;
    public int     ConstellationID    => CharacterInformation.ConstellationID;
    public int     RegionID           => CharacterInformation.RegionID;

    public int FreeReSpecs
    {
        get => CharacterInformation.FreeReSpecs;
        set
        {
            Information.Dirty                = true;
            CharacterInformation.FreeReSpecs = value;
        }
    }

    public long NextReSpecTime
    {
        get => CharacterInformation.NextReSpecTime;
        set
        {
            Information.Dirty                   = true;
            CharacterInformation.NextReSpecTime = value;
        }
    }

    public long TimeLastJump
    {
        get => CharacterInformation.TimeLastJump;
        set
        {
            Information.Dirty                 = true;
            CharacterInformation.TimeLastJump = value;
        }
    }

    public int? WarFactionID
    {
        get => CharacterInformation.WarFactionID;
        set
        {
            Information.Dirty                 = true;
            CharacterInformation.WarFactionID = value;
        }
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long GrantableRoles
    {
        get => CharacterInformation.GrantableRoles;
        set => CharacterInformation.GrantableRoles = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long GrantableRolesAtHQ
    {
        get => CharacterInformation.GrantableRolesAtHQ;
        set => CharacterInformation.GrantableRolesAtHQ = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long GrantableRolesAtBase
    {
        get => CharacterInformation.GrantableRolesAtBase;
        set => CharacterInformation.GrantableRolesAtBase = value;
    }

    /// <summary>
    /// WARNING: THIS FIELD IS NOT SAVED
    /// </summary>
    public long GrantableRolesAtOther
    {
        get => CharacterInformation.GrantableRolesAtOther;
        set => CharacterInformation.GrantableRolesAtOther = value;
    }

    public long Charisma
    {
        get => Attributes [EVE.StaticData.Inventory.Attributes.charisma].Integer;
        set => Attributes [EVE.StaticData.Inventory.Attributes.charisma].Integer = value;
    }

    public long Willpower
    {
        get => Attributes [EVE.StaticData.Inventory.Attributes.willpower].Integer;
        set => Attributes [EVE.StaticData.Inventory.Attributes.willpower].Integer = value;
    }

    public long Intelligence
    {
        get => Attributes [EVE.StaticData.Inventory.Attributes.intelligence].Integer;
        set => Attributes [EVE.StaticData.Inventory.Attributes.intelligence].Integer = value;
    }

    public long Perception
    {
        get => Attributes [EVE.StaticData.Inventory.Attributes.perception].Integer;
        set => Attributes [EVE.StaticData.Inventory.Attributes.perception].Integer = value;
    }

    public long Memory
    {
        get => Attributes [EVE.StaticData.Inventory.Attributes.memory].Integer;
        set => Attributes [EVE.StaticData.Inventory.Attributes.memory].Integer = value;
    }

    public List <SkillQueueEntry> SkillQueue
    {
        get
        {
            // if the contents are not loaded then that needs to happen first
            if (ContentsLoaded == false)
                this.LoadContents ();

            // accessing the skillQueue might be a modification attempt
            // so the character must be marked as dirty
            Information.Dirty = true;

            return this.mSkillQueue;
        }
    }

    public Dictionary <int, Skill> InjectedSkills =>
        Items
            .Where (x => (x.Value.Flag == Flags.SkillInTraining || x.Value.Flag == Flags.Skill) && x.Value is Skill)
            .ToDictionary (dict => dict.Key, dict => dict.Value as Skill);

    public Dictionary <int, ItemEntity> Modifiers =>
        Items
            .Where (x => x.Value.Flag == Flags.SkillInTraining || x.Value.Flag == Flags.Skill || x.Value.Flag == Flags.Implant)
            .ToDictionary (dict => dict.Key, dict => dict.Value);

    public Dictionary <int, Skill> InjectedSkillsByTypeID =>
        Items
            .Where (x => (x.Value.Flag == Flags.Skill || x.Value.Flag == Flags.SkillInTraining) && x.Value is Skill)
            .ToDictionary (dict => dict.Value.Type.ID, dict => dict.Value as Skill);

    public Dictionary <int, Implant> PluggedInImplants =>
        Items
            .Where (x => x.Value.Flag == Flags.Implant && x.Value is Implant)
            .ToDictionary (dict => dict.Key, dict => dict.Value as Implant);

    public Dictionary <int, Implant> PluggedInImplantsByTypeID =>
        Items
            .Where (x => x.Value.Flag == Flags.Implant && x.Value is Implant)
            .ToDictionary (dict => dict.Key, dict => dict.Value as Implant);

    public Character (Information.Character info) : base (info.Information)
    {
        CharacterInformation = info;
    }

    protected override void LoadContents (Flags ignoreFlags = Flags.None)
    {
        base.LoadContents (ignoreFlags);

        this.CalculateSkillPoints ();

        // put things where they belong
        Dictionary <int, Skill> skillQueue = Items
                                             .Where (x => x.Value.Flag == Flags.SkillInTraining && x.Value is Skill)
                                             .ToDictionary (dict => dict.Key, dict => dict.Value as Skill);

        this.mSkillQueue = this.OnSkillQueueLoad?.Invoke (this, skillQueue);
    }

    public void CalculateSkillPoints ()
    {
        foreach ((int itemID, Skill skill) in InjectedSkills)
            // increase our skillpoints count with all the trained skills
            this.mSkillPoints += skill.Points;
    }

    public double GetSkillPointsPerMinute (Skill skill)
    {
        Attribute primarySpPerMin   = Attributes [skill.PrimaryAttribute.Integer];
        Attribute secondarySpPerMin = Attributes [skill.SecondaryAttribute.Integer];

        long skillLearningLevel = 0;

        if (InjectedSkillsByTypeID.TryGetValue ((int) EVE.StaticData.Inventory.Types.Learning, out Skill learningSkill))
            skillLearningLevel = learningSkill.Level;

        double spPerMin = primarySpPerMin + secondarySpPerMin / 2.0f;
        spPerMin = spPerMin * (1.0f + 0.02f * skillLearningLevel);

        if (this.mSkillPoints < 1600000.0f)
            spPerMin = spPerMin * 2.0f;

        return spPerMin;
    }

    public long GetSkillLevel (EVE.StaticData.Inventory.Types skillTypeID)
    {
        return this.GetSkillLevel ((int) skillTypeID);
    }

    public long GetSkillLevel (int skillTypeID)
    {
        if (InjectedSkillsByTypeID.TryGetValue (skillTypeID, out Skill skill) == false)
            return 0;

        return skill.Level;
    }

    /// <summary>
    /// Checks if the character has the required skill at the specified level and throws an exception if not
    /// </summary>
    /// <param name="skillTypeID">The skill to look for</param>
    /// <param name="level">The minimum level</param>
    /// <exception cref="SkillMissingException">If the skill requirement is not met</exception>
    public void EnsureSkillLevel (EVE.StaticData.Inventory.Types skillTypeID, int level = 1)
    {
        if (this.GetSkillLevel (skillTypeID) < level)
            throw new SkillRequired (skillTypeID);
    }

    public void EnsureFreeImplantSlot (ItemEntity newImplant)
    {
        int implantSlot = (int) newImplant.Attributes [EVE.StaticData.Inventory.Attributes.implantness].Integer;

        foreach ((int _, Implant implant) in PluggedInImplants)
        {
            // the implant does not use any slot, here for sanity checking
            if (implant.Attributes.AttributeExists (EVE.StaticData.Inventory.Attributes.implantness) == false)
                continue;

            if (implant.Attributes [EVE.StaticData.Inventory.Attributes.implantness].Integer == implantSlot)
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