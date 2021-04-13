namespace Node.StaticData
{
    public class Ancestry
    {
        private int mAncestryID;
        private string mName;
        private Bloodline mBloodline;
        private string mDescription;
        private int mPerception;
        private int mWillpower;
        private int mCharisma;
        private int mMemory;
        private int mIntelligence;
        private int mGraphicID;
        private string mShortDescription;

        public Ancestry(int ancestryId, string name, Bloodline bloodline, string description, int perception,
            int willpower, int charisma, int memory, int intelligence, int graphicId, string shortDescription)
        {
            mAncestryID = ancestryId;
            mName = name;
            mBloodline = bloodline;
            mDescription = description;
            mPerception = perception;
            mWillpower = willpower;
            mCharisma = charisma;
            mMemory = memory;
            mIntelligence = intelligence;
            mGraphicID = graphicId;
            mShortDescription = shortDescription;
        }

        public int ID => mAncestryID;
        public string Name => mName;
        public Bloodline Bloodline => mBloodline;
        public string Description => mDescription;
        public int Perception => mPerception;
        public int Willpower => mWillpower;
        public int Charisma => mCharisma;
        public int Memory => mMemory;
        public int Intelligence => mIntelligence;
        public int GraphicID => mGraphicID;
        public string ShortDescription => mShortDescription;
    }
}