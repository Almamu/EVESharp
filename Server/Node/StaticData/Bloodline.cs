using Node.Inventory.Items;
using Node.StaticData.Inventory;

namespace Node.StaticData
{
    public class Bloodline
    {
        private int mBloodlineID;
        private Type mCharacterType;
        private string mName;
        private int mRaceID;
        private string mDescription;
        private string mMaleDescription;
        private string mFemaleDescription;
        private Type mShipType;
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
        
        public Bloodline(int bloodlineID, Type characterType, string name, int raceID, string description,
            string maleDescription, string femaleDescription, Type shipType, int corporationID, int perception,
            int willpower, int charisma, int memory, int intelligence, int graphicID, string shortDescription,
            string shortMaleDescription, string shortFemaleDescription)
        {
            this.mBloodlineID = bloodlineID;
            this.mCharacterType = characterType;
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
        public Type CharacterType => mCharacterType;
        public string Name => mName;
        public int RaceID => mRaceID;
        public string Description => mDescription;
        public string MaleDescription => mMaleDescription;
        public string FemaleDescription => mFemaleDescription;
        public Type ShipType => mShipType;
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