using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Node.Database;
using Node.Exceptions;
using Node.Inventory.Items.Attributes;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items.Types
{
    public class Character : ItemInventory
    {
        public class SkillQueueEntry
        {
            public Skill Skill;
            public int TargetLevel;
            
            public static implicit operator PyDataType(SkillQueueEntry from)
            {
                return new PyTuple(new PyDataType[]
                {
                    from.Skill.Type.ID, from.TargetLevel
                });
            }
        }
        
        public Character(ClientManager clientManager, TimerManager timerManager, ItemEntity from, int characterId,
            int accountId, int? activeCloneID, string title, string description, double bounty, double balance,
            double securityRating, string petitionMessage, int logonMinutes, int corporationId, int corpRole,
            int rolesAtAll, int rolesAtBase, int rolesAtHq, int rolesAtOther, long corporationDateTime,
            long startDateTime, long createDateTime, int ancestryId, int careerId, int schoolId, int careerSpecialityId,
            int gender, int? accessoryId, int? beardId, int costumeId, int? decoId, int eyebrowsId, int eyesId,
            int hairId, int? lipstickId, int? makeupId, int skinId, int backgroundId, int lightId, double headRotation1,
            double headRotation2, double headRotation3, double eyeRotation1, double eyeRotation2, double eyeRotation3,
            double camPos1, double camPos2, double camPos3, double? morph1E, double? morph1N, double? morph1S,
            double? morph1W, double? morph2E, double? morph2N, double? morph2S, double? morph2W, double? morph3E,
            double? morph3N, double? morph3S, double? morph3W, double? morph4E, double? morph4N, double? morph4S,
            double? morph4W, int stationId, int solarSystemId, int constellationId, int regionId, int online,
            int freeReSpecs, long nextReSpecTime, long timeLastJump, int titleMask) : base(from)
        {
            this.ClientManager = clientManager;
            this.TimerManager = timerManager;
            this.mCharacterID = characterId;
            this.mAccountID = accountId;
            this.mActiveCloneID = activeCloneID;
            this.mTitle = title;
            this.mDescription = description;
            this.mBounty = bounty;
            this.mBalance = balance;
            this.mSecurityRating = securityRating;
            this.mPetitionMessage = petitionMessage;
            this.mLogonMinutes = logonMinutes;
            this.mCorporationID = corporationId;
            this.mCorpRole = corpRole;
            this.mRolesAtAll = rolesAtAll;
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
            this.mOnline = online;
            this.mFreeReSpecs = freeReSpecs;
            this.mNextReSpecTime = nextReSpecTime;
            this.mTimeLastJump = timeLastJump;
            this.mTitleMask = titleMask;
        }

        private ClientManager ClientManager { get; }
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

        public double Bounty
        {
            get => this.mBounty;
            set
            {
                this.Dirty = true;
                this.mBounty = value;
            }
        }

        public double Balance
        {
            get => this.mBalance;
            set
            {
                this.Dirty = true;
                this.mBalance = value;
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
        public double SecurityRating => mSecurityRating;
        public string PetitionMessage => mPetitionMessage;
        public int LogonMinutes => mLogonMinutes;
        public int CorporationID => mCorporationID;
        public int CorpRole => mCorpRole;
        public int RolesAtAll => mRolesAtAll;
        public int RolesAtBase => mRolesAtBase;
        public int RolesAtHq => mRolesAtHq;
        public int RolesAtOther => mRolesAtOther;
        public long CorporationDateTime => mCorporationDateTime;
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

        public int Online
        {
            get => this.mOnline;
            set
            {
                this.mOnline = value;
                this.Dirty = true;
            }
        }

        public Clone ActiveClone
        {
            get
            {
                if (this.mActiveClone == null)
                    this.mActiveClone = this.mItemFactory.ItemManager.LoadItem((int) this.ActiveCloneID) as Clone;

                return this.mActiveClone;
            }

            set
            {
                // free the current active clone (if loaded)
                if (this.mActiveClone != null)
                    this.mItemFactory.ItemManager.UnloadItem(this.mActiveClone);
                
                this.ActiveCloneID = value.ID;
                this.mActiveClone = value;
            }
        }
        
        private int mCharacterID;
        private int mAccountID;
        private int? mActiveCloneID;
        private string mTitle;
        private string mDescription;
        private double mBounty;
        private double mBalance;
        private double mSecurityRating;
        private string mPetitionMessage;
        private int mLogonMinutes;
        private int mCorporationID;
        private int mCorpRole;
        private int mRolesAtAll;
        private int mRolesAtBase;
        private int mRolesAtHq;
        private int mRolesAtOther;
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
        private int mOnline;
        private int mFreeReSpecs;
        private long mNextReSpecTime;
        private long mTimeLastJump;
        private int mTitleMask;
        
        private List<SkillQueueEntry> mSkillQueue;
        private Corporation mCorporation = null;
        private Clone mActiveClone = null;
        private double mSkillPoints = 0.0f;

        public long Charisma
        {
            get => this.Attributes[AttributeEnum.charisma].Integer;
            set => this.Attributes[AttributeEnum.charisma].Integer = value;
        }

        public long Willpower
        {
            get => this.Attributes[AttributeEnum.willpower].Integer;
            set => this.Attributes[AttributeEnum.willpower].Integer = value;
        }

        public long Intelligence
        {
            get => this.Attributes[AttributeEnum.intelligence].Integer;
            set => this.Attributes[AttributeEnum.intelligence].Integer = value;
        }

        public long Perception
        {
            get => this.Attributes[AttributeEnum.perception].Integer;
            set => this.Attributes[AttributeEnum.perception].Integer = value;
        }

        public long Memory
        {
            get => this.Attributes[AttributeEnum.memory].Integer;
            set => this.Attributes[AttributeEnum.memory].Integer = value;
        }

        public List<SkillQueueEntry> SkillQueue
        {
            get
            {
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

                this.mCorporation = this.mItemFactory.ItemManager.LoadItem(this.CorporationID) as Corporation;

                return this.mCorporation;
            }
        }
        
        public Dictionary<int, Skill> InjectedSkills =>
            this.Items
                .Where(x => (x.Value.Flag == ItemFlags.SkillInTraining || x.Value.Flag == ItemFlags.Skill) && x.Value is Skill)
                .ToDictionary(dict => dict.Key, dict => dict.Value as Skill);

        public Dictionary<int, ItemEntity> Modifiers => 
            this.Items
                .Where(x => (x.Value.Flag == ItemFlags.SkillInTraining || x.Value.Flag == ItemFlags.Skill || x.Value.Flag == ItemFlags.Implant))
                .ToDictionary(dict => dict.Key, dict => dict.Value);
        
        public Dictionary<int, Skill> InjectedSkillsByTypeID =>
            this.Items
                .Where(x => (x.Value.Flag == ItemFlags.Skill || x.Value.Flag == ItemFlags.SkillInTraining) && x.Value is Skill)
                .ToDictionary(dict => dict.Value.Type.ID, dict => dict.Value as Skill);
        
        protected override void LoadContents()
        {
            base.LoadContents();
            
            this.CalculateSkillPoints();

            // put things where they belong
            Dictionary<int, Skill> skillQueue = this.Items
                .Where(x => x.Value.Flag == ItemFlags.SkillInTraining && x.Value is Skill)
                .ToDictionary(dict => dict.Key, dict => dict.Value as Skill);

            this.mSkillQueue = base.mItemFactory.CharacterDB.LoadSkillQueue(this, skillQueue);
            
            // iterate the skill queue and generate timers for the skills
            foreach (SkillQueueEntry entry in this.mSkillQueue)
                if (entry.Skill.ExpiryTime != 0)
                    this.TimerManager.EnqueueItemTimer(entry.Skill.ExpiryTime, SkillTrainingCompleted, entry.Skill.ID);
            
            // send notification of the first skill being in the queue
            if (this.mSkillQueue.Count > 0)
            {
                if (this.ClientManager.Contains(this.AccountID) == true)
                {
                    // skill was trained, send the success message
                    this.ClientManager.Get(this.AccountID).NotifySkillStartTraining(this.mSkillQueue[0].Skill);                
                }
            }
        }

        private void CalculateSkillPoints()
        {
            foreach (KeyValuePair<int, Skill> skills in this.InjectedSkills)
                // increase our skillpoints count with all the trained skills
                this.mSkillPoints += skills.Value.Points;
        }
        
        public void SkillTrainingCompleted(int itemID)
        {
            Skill skill = this.Items[itemID] as Skill;
            
            // set the skill to the proper flag and set the correct attributes
            skill.Flag = ItemFlags.Skill;
            skill.Level = skill.Level + 1;
            
            // notify the client of a change in the item's flag
            if (this.ClientManager.Contains(this.AccountID) == true)
            {
                this.ClientManager.Get(this.AccountID).NotifyItemLocationChange(skill, ItemFlags.SkillInTraining, this.ID);
            
                // skill was trained, send the success message
                this.ClientManager.Get(this.AccountID).NotifySkillTrained(skill);                
            }

            skill.Persist();
            
            // create history entry
            this.mItemFactory.SkillDB.CreateSkillHistoryRecord(skill.Type, this, SkillHistoryReason.SkillTrainingComplete,
                skill.Points);

            // finally remove it off the skill queue
            this.SkillQueue.RemoveAll(x => x.Skill.ID == skill.ID);

            this.CalculateSkillPoints();
            
            // get the next skill from the queue (if any) and send the client proper notifications
            if (this.SkillQueue.Count == 0)
            {
                // persists the skill queue
                this.Dirty = true;
                this.Persist();
                return;
            }

            skill = this.SkillQueue[0].Skill;
            
            skill.Flag = ItemFlags.SkillInTraining;
            
            if (this.ClientManager.Contains(this.AccountID) == true)
            {
                this.ClientManager.Get(this.AccountID).NotifyItemLocationChange(skill, ItemFlags.Skill, this.ID);
            
                // skill was trained, send the success message
                this.ClientManager.Get(this.AccountID).NotifySkillStartTraining(skill);                
            }

            // create history entry
            this.mItemFactory.SkillDB.CreateSkillHistoryRecord(skill.Type, this, SkillHistoryReason.SkillTrainingStarted,
                skill.Points);
            
            // persists the skill queue
            this.Dirty = true;
            this.Persist();
        }

        public double GetSkillPointsPerMinute(Skill skill)
        {
            ItemAttribute primarySpPerMin = this.Attributes[skill.PrimaryAttribute.Integer];
            ItemAttribute secondarySpPerMin = this.Attributes[skill.SecondaryAttribute.Integer];
            
            long skillLearningLevel = 0;

            Dictionary<int,Skill> injectedSkillsByTypeID = this.InjectedSkillsByTypeID;

            if (injectedSkillsByTypeID.ContainsKey((int) ItemTypes.Learning) == true)
                skillLearningLevel = injectedSkillsByTypeID[(int) ItemTypes.Learning].Level;

            double spPerMin = primarySpPerMin + secondarySpPerMin / 2.0f;
            spPerMin = spPerMin * (1.0f + 0.02f * skillLearningLevel);
            
            if (this.mSkillPoints < 1600000.0f)
                spPerMin = spPerMin * 2.0f;

            return spPerMin;
        }

        public void EnsureEnoughBalance(double needed)
        {
            if (this.Balance < needed)
                throw new NotEnoughMoney(this.Balance, needed);
        }
        
        protected override void SaveToDB()
        {
            base.SaveToDB();

            // update the relevant character information
            this.mItemFactory.CharacterDB.UpdateCharacterInformation(this);
        }

        public override void Destroy()
        {
            // remove all timers this user might have
            foreach (SkillQueueEntry entry in this.mSkillQueue)
                this.TimerManager.DequeueItemTimer(entry.Skill.ID, entry.Skill.ExpiryTime);

            base.Destroy();
        }

        public override void Dispose()
        {
            base.Dispose();
            
            // remove timers if loaded
            if (this.ContentsLoaded)
            {
                foreach (SkillQueueEntry entry in this.mSkillQueue)
                    this.TimerManager.DequeueItemTimer(entry.Skill.ID, entry.Skill.ExpiryTime);
            }
        }
    }
}