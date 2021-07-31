using Node.Exceptions.corpRegistry;

namespace Node.Inventory.Items.Types
{
    public class Alliance : ItemEntity
    {
        public Alliance(ItemEntity @from, string shortName, string description, string url, int executorCorpID, int creatorCorpID, int creatorCharID, int dictatorial) : base(@from)
        {
            this.mShortName = shortName;
            this.mDescription = description;
            this.mUrl = url;
            this.mExecutorCorpID = executorCorpID;
            this.mCreatorCorpID = creatorCorpID;
            this.mCreatorCharID = creatorCharID;
            this.mDictatorial = dictatorial;
        }

        private string mShortName;
        private string mDescription;
        private string mUrl;
        private int mExecutorCorpID;
        private int mCreatorCorpID;
        private int mCreatorCharID;
        private int mDictatorial;

        public string ShortName => mShortName;
        public string Description => mDescription;
        public string Url => mUrl;
        public int ExecutorCorpID => mExecutorCorpID;
        public int CreatorCorpID => mCreatorCorpID;
        public int CreatorCharID => mCreatorCharID;
        public int Dictatorial => mDictatorial;
    }
}