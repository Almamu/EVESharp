using Node.Inventory.Items;

namespace Node.Data
{
    public class Bloodline
    {
        private int mBloodlineID;
        private ItemType mItemType;
        private string mName;
        private int mRaceID;
        private string mDescription;
        private string mMaleDescription;
        private string mFemaleDescription;
        private ItemType mShipType;
        private int mCorporationID;
        private int mPerception;
        private int mWillpower;
        private int mCharisma;
        private int mMemory;
        private int mIntelligence;
        private int mGraphicID;
        private string mShortDescription;
        private string mShortMaleDescription;
        private string mShortFemaleDescription;
        
        public Bloodline(int bloodlineID, ItemType itemType, string name, int raceID, string description,
            string maleDescription, string femaleDescription, ItemType shipType, int corporationID, int perception,
            int willpower, int charisma, int memory, int intelligence, int graphicID, string shortDescription,
            string shortMaleDescription, string shortFemaleDescription)
        {
            this.mBloodlineID = bloodlineID;
            this.mItemType = itemType;
            this.mName = name;
            this.mRaceID = raceID;
            this.mDescription = description;
            this.mMaleDescription = maleDescription;
            this.mFemaleDescription = femaleDescription;
            this.mShipType = shipType;
            this.mCorporationID = corporationID;
            this.mPerception = perception;
            this.mWillpower = willpower;
            this.mCharisma = charisma;
            this.mMemory = memory;
            this.mIntelligence = intelligence;
            this.mGraphicID = graphicID;
            this.mShortDescription = shortDescription;
            this.mShortMaleDescription = shortMaleDescription;
            this.mShortFemaleDescription = shortFemaleDescription;
        }

        public int ID => mBloodlineID;
        public ItemType ItemType => mItemType;
        public string Name => mName;
        public int RaceID => mRaceID;
        public string Description => mDescription;
        public string MaleDescription => mMaleDescription;
        public string FemaleDescription => mFemaleDescription;
        public ItemType ShipType => mShipType;
        public int CorporationID => mCorporationID;
        public int Perception => mPerception;
        public int Willpower => mWillpower;
        public int Charisma => mCharisma;
        public int Memory => mMemory;
        public int Intelligence => mIntelligence;
        public int GraphicID => mGraphicID;
        public string ShortDescription => mShortDescription;
        public string ShortMaleDescription => mShortMaleDescription;
        public string ShortFemaleDescription => mShortFemaleDescription;
    }
}