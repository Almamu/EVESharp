using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.corpRegistry;

namespace EVESharp.Node.Inventory.Items.Types
{
    public class Alliance : ItemEntity
    {
        public Alliance(ItemEntity @from, string shortName, string description, string url, int? executorCorpID, int creatorCorpID, int creatorCharID, int dictatorial) : base(@from)
        {
            this.mShortName = shortName;
            this.mDescription = description;
            this.mUrl = url;
            this.mExecutorCorpID = executorCorpID;
            this.mCreatorCorpID = creatorCorpID;
            this.mCreatorCharID = creatorCharID;
            this.mDictatorial = dictatorial;
        }

        string mShortName;
        string mDescription;
        string mUrl;
        int? mExecutorCorpID;
        int mCreatorCorpID;
        int mCreatorCharID;
        int mDictatorial;

        public string ShortName => mShortName;
        public string Description
        {
            get => this.mDescription;
            set
            {
                this.Dirty = true;
                this.mDescription = value;
            }
        }

        public string Url
        {
            get => this.mUrl;
            set
            {
                this.Dirty = true;
                this.mUrl = value;
            }
        }

        public int? ExecutorCorpID
        {
            get => this.mExecutorCorpID;
            set
            {
                this.Dirty = true;
                this.mExecutorCorpID = value;
            }
        }
        
        public int CreatorCorpID => mCreatorCorpID;
        public int CreatorCharID => mCreatorCharID;
        public int Dictatorial => mDictatorial;
        
        protected override void SaveToDB()
        {
            base.SaveToDB();

            // update the alliance information
            Database.Procedure(
                AlliancesDB.UPDATE,
                new Dictionary<string, object>()
                {
                    {"_description", this.Description},
                    {"_url", this.Url},
                    {"_allianceID", this.ID},
                    {"_executorCorpID", this.ExecutorCorpID}
                }
            );
        }
    }
}