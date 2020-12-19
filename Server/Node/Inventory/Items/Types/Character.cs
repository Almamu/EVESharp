using System.Collections.Generic;
using System.Linq;
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
                    from.Skill.ID, from.TargetLevel
                });
            }
        }
        
        public Character(ItemEntity from, int characterId, int accountId, string title, string description,
            double bounty, double balance, double securityRating, string petitionMessage, int logonMinutes,
            int corporationId, int corpRole, int rolesAtAll, int rolesAtBase, int rolesAtHq, int rolesAtOther,
            long corporationDateTime, long startDateTime, long createDateTime, int ancestryId, int careerId, int schoolId,
            int careerSpecialityId, int gender, int? accessoryId, int? beardId, int costumeId, int? decoId, int eyebrowsId,
            int eyesId, int hairId, int? lipstickId, int? makeupId, int skinId, int backgroundId, int lightId,
            double headRotation1, double headRotation2, double headRotation3, double eyeRotation1, double eyeRotation2,
            double eyeRotation3, double camPos1, double camPos2, double camPos3, double? morph1E, double? morph1N,
            double? morph1S, double? morph1W, double? morph2E, double? morph2N, double? morph2S, double? morph2W,
            double? morph3E, double? morph3N, double? morph3S, double? morph3W, double? morph4E, double? morph4N,
            double? morph4S, double? morph4W, int stationId, int solarSystemId, int constellationId, int regionId,
            int online) : base(from)
        {
            this.mCharacterID = characterId;
            this.mAccountID = accountId;
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
        }

        public int CharacterID => mCharacterID;
        public int AccountID => mAccountID;
        public string Title => mTitle;
        public string Description => mDescription;
        public double Bounty => mBounty;
        public double Balance => mBalance;
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

        public int Online
        {
            get => this.mOnline;
            set
            {
                this.mOnline = value;
                this.Dirty = true;
            }
        }
        
        private int mCharacterID;
        private int mAccountID;
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
        private List<SkillQueueEntry> mSkillQueue;
        private Corporation mCorporation = null;

        public ItemAttribute Charisma
        {
            get => this.Attributes[AttributeEnum.charisma];
            set => this.Attributes[AttributeEnum.charisma] = value;
        }

        public ItemAttribute Willpower
        {
            get => this.Attributes[AttributeEnum.willpower];
            set => this.Attributes[AttributeEnum.willpower] = value;
        }

        public ItemAttribute Intelligence
        {
            get => this.Attributes[AttributeEnum.intelligence];
            set => this.Attributes[AttributeEnum.intelligence] = value;
        }

        public ItemAttribute Perception
        {
            get => this.Attributes[AttributeEnum.perception];
            set => this.Attributes[AttributeEnum.perception] = value;
        }

        public ItemAttribute Memory
        {
            get => this.Attributes[AttributeEnum.memory];
            set => this.Attributes[AttributeEnum.memory] = value;
        }

        public List<SkillQueueEntry> SkillQueue => this.mSkillQueue;

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

        protected override void LoadContents()
        {
            base.LoadContents();
            
            // put things where they belong
            this.mSkillQueue = new List<SkillQueueEntry>();
            Dictionary<int, Skill> skillQueue = this.Items
                .Where(x => x.Value.Flag == ItemFlags.SkillInTraining && x.Value is Skill)
                .ToDictionary(dict => dict.Key, dict => dict.Value as Skill);

            foreach (KeyValuePair<int, Skill> pair in skillQueue)
            {
                this.mSkillQueue = base.mItemFactory.CharacterDB.LoadSkillQueue(this, skillQueue);
            }
        }
        
        protected override void SaveToDB()
        {
            base.SaveToDB();
            
            // ensure the online status is also persisted
            this.mItemFactory.CharacterDB.UpdateOnlineStatus(this);
        }
    }
}