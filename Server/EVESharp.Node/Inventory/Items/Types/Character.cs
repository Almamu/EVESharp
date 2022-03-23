using System;
using System.Collections.Generic;
using System.Linq;
using EVESharp.Node.Exceptions;
using EVESharp.Node.Exceptions.character;
using EVESharp.Node.Exceptions.Internal;
using EVESharp.Node.Network;
using EVESharp.Node.StaticData.Inventory;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory.Items.Attributes;
using EVESharp.Node.StaticData.Corporation;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Primitives;
using Attribute = EVESharp.Node.Inventory.Items.Attributes.Attribute;

namespace EVESharp.Node.Inventory.Items.Types
{
    public class Character : ItemInventory
    {
        public class SkillQueueEntry
        {
            public Skill Skill;
            public int TargetLevel;
            
            public static implicit operator PyDataType(SkillQueueEntry from)
            {
                return new PyTuple(2)
                {
                    [0] = from.Skill.Type.ID,
                    [1] = from.TargetLevel
                };
            }
        }
        
        public Character(TimerManager timerManager, ItemEntity from, int characterId,
            int accountId, int? activeCloneID, string title, string description,
            double securityRating, string petitionMessage, int logonMinutes, int corporationId, long roles,
            long rolesAtBase, long rolesAtHq, long rolesAtOther, long corporationDateTime, long startDateTime,
            long createDateTime, int ancestryId, int careerId, int schoolId, int careerSpecialityId,
            int gender, int? accessoryId, int? beardId, int costumeId, int? decoId, int eyebrowsId, int eyesId,
            int hairId, int? lipstickId, int? makeupId, int skinId, int backgroundId, int lightId, double headRotation1,
            double headRotation2, double headRotation3, double eyeRotation1, double eyeRotation2, double eyeRotation3,
            double camPos1, double camPos2, double camPos3, double? morph1E, double? morph1N, double? morph1S,
            double? morph1W, double? morph2E, double? morph2N, double? morph2S, double? morph2W, double? morph3E,
            double? morph3N, double? morph3S, double? morph3W, double? morph4E, double? morph4N, double? morph4S,
            double? morph4W, int stationId, int solarSystemId, int constellationId, int regionId,
            int freeReSpecs, long nextReSpecTime, long timeLastJump, int titleMask, int? warFactionID, int corpAccountKey,
            long grantableRoles, long grantableRolesAtBase, long grantableRolesAtHq, long grantableRolesAtOther,
            int? baseID) : base(from)
        {
            this.TimerManager = timerManager;
            this.mCharacterID = characterId;
            this.mAccountID = accountId;
            this.mActiveCloneID = activeCloneID;
            this.mTitle = title;
            this.mDescription = description;
            this.mSecurityRating = securityRating;
            this.mPetitionMessage = petitionMessage;
            this.mLogonMinutes = logonMinutes;
            this.mCorporationID = corporationId;
            this.mRoles = roles;
            this.mRolesAtBase = rolesAtBase;
            this.mRolesAtHq = rolesAtHq;
            this.mRolesAtOther = rolesAtOther;
            this.mCorporationDateTime = corporationDateTime;
            this.mStartDateTime = startDateTime;
            this.mCreateDateTime = createDateTime;
            this.mAncestryID = ancestryId;
            this.mCareerID = careerId;
            this.mSchoolID = schoolId;
            this.mCareerSpecialityID = careerSpecialityId;
            this.mGender = gender;
            this.mAccessoryID = accessoryId;
            this.mBeardID = beardId;
            this.mCostumeID = costumeId;
            this.mDecoID = decoId;
            this.mEyebrowsID = eyebrowsId;
            this.mEyesID = eyesId;
            this.mHairID = hairId;
            this.mLipstickID = lipstickId;
            this.mMakeupID = makeupId;
            this.mSkinID = skinId;
            this.mBackgroundID = backgroundId;
            this.mLightID = lightId;
            this.mHeadRotation1 = headRotation1;
            this.mHeadRotation2 = headRotation2;
            this.mHeadRotation3 = headRotation3;
            this.mEyeRotation1 = eyeRotation1;
            this.mEyeRotation2 = eyeRotation2;
            this.mEyeRotation3 = eyeRotation3;
            this.mCamPos1 = camPos1;
            this.mCamPos2 = camPos2;
            this.mCamPos3 = camPos3;
            this.mMorph1E = morph1E;
            this.mMorph1N = morph1N;
            this.mMorph1S = morph1S;
            this.mMorph1W = morph1W;
            this.mMorph2E = morph2E;
            this.mMorph2N = morph2N;
            this.mMorph2S = morph2S;
            this.mMorph2W = morph2W;
            this.mMorph3E = morph3E;
            this.mMorph3N = morph3N;
            this.mMorph3S = morph3S;
            this.mMorph3W = morph3W;
            this.mMorph4E = morph4E;
            this.mMorph4N = morph4N;
            this.mMorph4S = morph4S;
            this.mMorph4W = morph4W;
            this.mStationID = stationId;
            this.mSolarSystemID = solarSystemId;
            this.mConstellationID = constellationId;
            this.mRegionID = regionId;
            this.mFreeReSpecs = freeReSpecs;
            this.mNextReSpecTime = nextReSpecTime;
            this.mTimeLastJump = timeLastJump;
            this.mTitleMask = titleMask;
            this.mWarFactionID = warFactionID;
            this.mCorpAccountKey = corpAccountKey;
            this.mGrantableRoles = grantableRoles;
            this.mGrantableRolesAtHQ = grantableRolesAtHq;
            this.mGrantableRolesAtBase = grantableRolesAtBase;
            this.mGrantableRolesAtOther = grantableRolesAtOther;
            this.mBaseID = baseID;
        }

        private TimerManager TimerManager { get; }
        public int CharacterID => mCharacterID;
        public int AccountID => mAccountID;

        public int? ActiveCloneID
        {
            get => this.mActiveCloneID;
            set
            {
                this.Dirty = true;
                this.mActiveCloneID = value;
            }
        }

        public string Title => mTitle;

        public string Description
        {
            get => this.mDescription;
            set
            {
                this.Dirty = true;
                this.mDescription = value;
            }
        }

        public int TitleMask
        {
            get => this.mTitleMask;
            set
            {
                this.Dirty = true;
                this.mTitleMask = value;
            }
        }

        public int CorporationID
        {
            get => this.mCorporationID;
            set
            {
                this.Dirty = true;
                this.mCorporationID = value;
            }
        }
        
        public double SecurityRating => mSecurityRating;
        public string PetitionMessage => mPetitionMessage;
        public int LogonMinutes => mLogonMinutes;

        /// <summary>
        /// WARNING: THIS FIELD IS NOT SAVED
        /// </summary>
        public long Roles
        {
            get => this.mRoles;
            set => this.mRoles = value;
        }

        /// <summary>
        /// WARNING: THIS FIELD IS NOT SAVED
        /// </summary>
        public long RolesAtBase
        {
            get => this.mRolesAtBase;
            set => this.mRolesAtBase = value;
        }

        /// <summary>
        /// WARNING: THIS FIELD IS NOT SAVED
        /// </summary>
        public long RolesAtHq
        {
            get => this.mRolesAtHq;
            set => this.mRolesAtHq = value;
        }

        /// <summary>
        /// WARNING: THIS FIELD IS NOT SAVED
        /// </summary>
        public long RolesAtOther
        {
            get => this.mRolesAtOther;
            set => this.mRolesAtOther = value;
        }

        /// <summary>
        /// WARNING: THIS FIELD IS NOT SAVED
        /// </summary>
        public int? BaseID
        {
            get => this.mBaseID;
            set => this.mBaseID = value;
        }

        public long CorporationDateTime
        {
            get => this.mCorporationDateTime;
            set
            {
                this.Dirty = true;
                this.mCorporationDateTime = value;
            }
        }

        public int CorpAccountKey
        {
            get => this.mCorpAccountKey;
            set => this.mCorpAccountKey = value;
        }
        
        public long StartDateTime => mStartDateTime;
        public long CreateDateTime => mCreateDateTime;
        public int AncestryID => mAncestryID;
        public int CareerID => mCareerID;
        public int SchoolID => mSchoolID;
        public int CareerSpecialityID => mCareerSpecialityID;
        public int Gender => mGender;
        public int? AccessoryID => mAccessoryID;
        public int? BeardID => mBeardID;
        public int CostumeID => mCostumeID;
        public int? DecoID => mDecoID;
        public int EyebrowsID => mEyebrowsID;
        public int EyesID => mEyesID;
        public int HairID => mHairID;
        public int? LipstickID => mLipstickID;
        public int? MakeupID => mMakeupID;
        public int SkinID => mSkinID;
        public int BackgroundID => mBackgroundID;
        public int LightID => mLightID;
        public double HeadRotation1 => mHeadRotation1;
        public double HeadRotation2 => mHeadRotation2;
        public double HeadRotation3 => mHeadRotation3;
        public double EyeRotation1 => mEyeRotation1;
        public double EyeRotation2 => mEyeRotation2;
        public double EyeRotation3 => mEyeRotation3;
        public double CamPos1 => mCamPos1;
        public double CamPos2 => mCamPos2;
        public double CamPos3 => mCamPos3;
        public double? Morph1E => mMorph1E;
        public double? Morph1N => mMorph1N;
        public double? Morph1S => mMorph1S;
        public double? Morph1W => mMorph1W;
        public double? Morph2E => mMorph2E;
        public double? Morph2N => mMorph2N;
        public double? Morph2S => mMorph2S;
        public double? Morph2W => mMorph2W;
        public double? Morph3E => mMorph3E;
        public double? Morph3N => mMorph3N;
        public double? Morph3S => mMorph3S;
        public double? Morph3W => mMorph3W;
        public double? Morph4E => mMorph4E;
        public double? Morph4N => mMorph4N;
        public double? Morph4S => mMorph4S;
        public double? Morph4W => mMorph4W;
        public int StationID => mStationID;
        public int SolarSystemID => mSolarSystemID;
        public int ConstellationID => mConstellationID;
        public int RegionID => mRegionID;

        public int FreeReSpecs
        {
            get => mFreeReSpecs;
            set
            {
                this.Dirty = true;
                this.mFreeReSpecs = value;
            }
        }

        public long NextReSpecTime
        {
            get => mNextReSpecTime;
            set
            {
                this.Dirty = true;
                this.mNextReSpecTime = value;
            }
        }

        public long TimeLastJump
        {
            get => this.mTimeLastJump;
            set
            {
                this.Dirty = true;
                this.mTimeLastJump = value;
            }
        }

        public Clone ActiveClone
        {
            get
            {
                if (this.mActiveClone == null)
                    this.mActiveClone = this.ItemFactory.LoadItem<Clone>((int) this.ActiveCloneID);

                return this.mActiveClone;
            }

            set
            {
                // free the current active clone (if loaded)
                if (this.mActiveClone != null)
                    this.ItemFactory.UnloadItem(this.mActiveClone);
                
                this.ActiveCloneID = value.ID;
                this.mActiveClone = value;
            }
        }

        public int? WarFactionID
        {
            get => this.mWarFactionID;
            set
            {
                this.Dirty = true;
                this.mWarFactionID = value;
            }
        }

        /// <summary>
        /// WARNING: THIS FIELD IS NOT SAVED
        /// </summary>
        public long GrantableRoles
        {
            get => this.mGrantableRoles;
            set => this.mGrantableRoles = value;
        }

        /// <summary>
        /// WARNING: THIS FIELD IS NOT SAVED
        /// </summary>
        public long GrantableRolesAtHQ
        {
            get => this.mGrantableRolesAtHQ;
            set => this.mGrantableRolesAtHQ = value;
        }

        /// <summary>
        /// WARNING: THIS FIELD IS NOT SAVED
        /// </summary>
        public long GrantableRolesAtBase
        {
            get => this.mGrantableRolesAtBase;
            set => this.mGrantableRolesAtBase = value;
        }

        /// <summary>
        /// WARNING: THIS FIELD IS NOT SAVED
        /// </summary>
        public long GrantableRolesAtOther
        {
            get => this.mGrantableRolesAtOther;
            set => this.mGrantableRolesAtOther = value;
        }
        
        private int mCharacterID;
        private int mAccountID;
        private int? mActiveCloneID;
        private string mTitle;
        private string mDescription;
        private double mSecurityRating;
        private string mPetitionMessage;
        private int mLogonMinutes;
        private int mCorporationID;
        private long mRoles;
        private long mRolesAtBase;
        private long mRolesAtHq;
        private long mRolesAtOther;
        private long mCorporationDateTime;
        private long mStartDateTime;
        private long mCreateDateTime;
        private int mAncestryID;
        private int mCareerID;
        private int mSchoolID;
        private int mCareerSpecialityID;
        private int mGender;
        private int? mAccessoryID;
        private int? mBeardID;
        private int mCostumeID;
        private int? mDecoID;
        private int mEyebrowsID;
        private int mEyesID;
        private int mHairID;
        private int? mLipstickID;
        private int? mMakeupID;
        private int mSkinID;
        private int mBackgroundID;
        private int mLightID;
        private double mHeadRotation1;
        private double mHeadRotation2;
        private double mHeadRotation3;
        private double mEyeRotation1;
        private double mEyeRotation2;
        private double mEyeRotation3;
        private double mCamPos1;
        private double mCamPos2;
        private double mCamPos3;
        private double? mMorph1E;
        private double? mMorph1N;
        private double? mMorph1S;
        private double? mMorph1W;
        private double? mMorph2E;
        private double? mMorph2N;
        private double? mMorph2S;
        private double? mMorph2W;
        private double? mMorph3E;
        private double? mMorph3N;
        private double? mMorph3S;
        private double? mMorph3W;
        private double? mMorph4E;
        private double? mMorph4N;
        private double? mMorph4S;
        private double? mMorph4W;
        private int mStationID;
        private int mSolarSystemID;
        private int mConstellationID;
        private int mRegionID;
        private int mFreeReSpecs;
        private long mNextReSpecTime;
        private long mTimeLastJump;
        private int mTitleMask;
        private int? mWarFactionID;
        private int mCorpAccountKey;
        private long mGrantableRoles;
        private long mGrantableRolesAtHQ;
        private long mGrantableRolesAtBase;
        private long mGrantableRolesAtOther;
        private int? mBaseID;
        
        private List<SkillQueueEntry> mSkillQueue;
        private Corporation mCorporation = null;
        private Clone mActiveClone = null;
        private double mSkillPoints = 0.0f;

        public long Charisma
        {
            get => this.Attributes[StaticData.Inventory.Attributes.charisma].Integer;
            set => this.Attributes[StaticData.Inventory.Attributes.charisma].Integer = value;
        }

        public long Willpower
        {
            get => this.Attributes[StaticData.Inventory.Attributes.willpower].Integer;
            set => this.Attributes[StaticData.Inventory.Attributes.willpower].Integer = value;
        }

        public long Intelligence
        {
            get => this.Attributes[StaticData.Inventory.Attributes.intelligence].Integer;
            set => this.Attributes[StaticData.Inventory.Attributes.intelligence].Integer = value;
        }

        public long Perception
        {
            get => this.Attributes[StaticData.Inventory.Attributes.perception].Integer;
            set => this.Attributes[StaticData.Inventory.Attributes.perception].Integer = value;
        }

        public long Memory
        {
            get => this.Attributes[StaticData.Inventory.Attributes.memory].Integer;
            set => this.Attributes[StaticData.Inventory.Attributes.memory].Integer = value;
        }

        public List<SkillQueueEntry> SkillQueue
        {
            get
            {
                // if the contents are not loaded then that needs to happen first
                if (this.ContentsLoaded == false)
                    this.LoadContents();
                
                // accessing the skillQueue might be a modification attempt
                // so the character must be marked as dirty
                this.Dirty = true;
                return this.mSkillQueue;
            }
        }

        public Corporation Corporation
        {
            get
            {
                if (this.mCorporation != null)
                    return this.mCorporation;

                this.mCorporation = this.ItemFactory.LoadItem(this.CorporationID) as Corporation;

                return this.mCorporation;
            }

            set
            {
                this.Dirty = true;
                this.mCorporation = value;
            }
        }
        
        public Dictionary<int, Skill> InjectedSkills =>
            this.Items
                .Where(x => (x.Value.Flag == Flags.SkillInTraining || x.Value.Flag == Flags.Skill) && x.Value is Skill)
                .ToDictionary(dict => dict.Key, dict => dict.Value as Skill);

        public Dictionary<int, ItemEntity> Modifiers => 
            this.Items
                .Where(x => (x.Value.Flag == Flags.SkillInTraining || x.Value.Flag == Flags.Skill || x.Value.Flag == Flags.Implant))
                .ToDictionary(dict => dict.Key, dict => dict.Value);
        
        public Dictionary<int, Skill> InjectedSkillsByTypeID =>
            this.Items
                .Where(x => (x.Value.Flag == Flags.Skill || x.Value.Flag == Flags.SkillInTraining) && x.Value is Skill)
                .ToDictionary(dict => dict.Value.Type.ID, dict => dict.Value as Skill);

        public Dictionary<int, Implant> PluggedInImplants =>
            this.Items
                .Where(x => x.Value.Flag == Flags.Implant && x.Value is Implant)
                .ToDictionary(dict => dict.Key, dict => dict.Value as Implant);

        public Dictionary<int, Implant> PluggedInImplantsByTypeID =>
            this.Items
                .Where(x => x.Value.Flag == Flags.Implant && x.Value is Implant)
                .ToDictionary(dict => dict.Key, dict => dict.Value as Implant);
        
        protected override void LoadContents(Flags ignoreFlags = Flags.None)
        {
            base.LoadContents(ignoreFlags);
            
            this.CalculateSkillPoints();

            // put things where they belong
            Dictionary<int, Skill> skillQueue = this.Items
                .Where(x => x.Value.Flag == Flags.SkillInTraining && x.Value is Skill)
                .ToDictionary(dict => dict.Key, dict => dict.Value as Skill);

            this.mSkillQueue = base.ItemFactory.CharacterDB.LoadSkillQueue(this, skillQueue);
        }

        public void CalculateSkillPoints()
        {
            foreach ((int itemID, Skill skill) in this.InjectedSkills)
                // increase our skillpoints count with all the trained skills
                this.mSkillPoints += skill.Points;
        }

        public double GetSkillPointsPerMinute(Skill skill)
        {
            Attributes.Attribute primarySpPerMin = this.Attributes[skill.PrimaryAttribute.Integer];
            Attributes.Attribute secondarySpPerMin = this.Attributes[skill.SecondaryAttribute.Integer];

            long skillLearningLevel = 0;
            
            if (this.InjectedSkillsByTypeID.TryGetValue((int) StaticData.Inventory.Types.Learning, out Skill learningSkill) == true)
                skillLearningLevel = learningSkill.Level;

            double spPerMin = primarySpPerMin + (secondarySpPerMin / 2.0f);
            spPerMin = spPerMin * (1.0f + (0.02f * skillLearningLevel));
            
            if (this.mSkillPoints < 1600000.0f)
                spPerMin = spPerMin * 2.0f;

            return spPerMin;
        }

        public long GetSkillLevel(StaticData.Inventory.Types skillTypeID)
        {
            return this.GetSkillLevel((int) skillTypeID);
        }

        public long GetSkillLevel(int skillTypeID)
        {
            if (this.InjectedSkillsByTypeID.TryGetValue(skillTypeID, out Skill skill) == false)
                return 0;

            return skill.Level;
        }

        /// <summary>
        /// Checks if the character has the required skill at the specified level and throws an exception if not
        /// </summary>
        /// <param name="skillTypeID">The skill to look for</param>
        /// <param name="level">The minimum level</param>
        /// <exception cref="SkillMissingException">If the skill requirement is not met</exception>
        public void EnsureSkillLevel(StaticData.Inventory.Types skillTypeID, int level = 1)
        {
            if (this.GetSkillLevel(skillTypeID) < level)
                throw new SkillRequired(skillTypeID);
        }
        
        public void EnsureFreeImplantSlot(ItemEntity newImplant)
        {
            int implantSlot = (int) newImplant.Attributes[StaticData.Inventory.Attributes.implantness].Integer;

            foreach ((int _, Implant implant) in this.PluggedInImplants)
            {
                // the implant does not use any slot, here for sanity checking
                if (implant.Attributes.AttributeExists(StaticData.Inventory.Attributes.implantness) == false)
                    continue;

                if (implant.Attributes[StaticData.Inventory.Attributes.implantness].Integer == implantSlot)
                    throw new OnlyOneImplantActive(newImplant);
            }
        }
        
        protected override void SaveToDB()
        {
            base.SaveToDB();

            // update the relevant character information
            this.ItemFactory.CharacterDB.UpdateCharacterInformation(this);
        }
    }
}