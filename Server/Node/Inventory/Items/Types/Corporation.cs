namespace Node.Inventory.Items.Types
{
    public class Corporation : ItemInventory
    {
        public Corporation(ItemEntity @from, string description, string tickerName, string url, double taxRate,
            double minimumJoinStanding, int corporationType, bool hasPlayerPersonnelManager,
            bool sendCharTerminationMessage, int creatorId, int stationId, int raceId, int allianceId, long shares,
            int memberCount, int memberLimit, int allowedMemberRaceIDs, int graphicId, int? shape1, int? shape2,
            int? shape3, int? color1, int? color2, int? color3, string typeface, string division1, string division2,
            string division3, string division4, string division5, string division6, string division7, double balance,
            bool deleted) : base(@from)
        {
            this.mDescription = description;
            this.mTickerName = tickerName;
            this.mUrl = url;
            this.mTaxRate = taxRate;
            this.mMinimumJoinStanding = minimumJoinStanding;
            this.mCorporationType = corporationType;
            this.mHasPlayerPersonnelManager = hasPlayerPersonnelManager;
            this.mSendCharTerminationMessage = sendCharTerminationMessage;
            this.mCreatorID = creatorId;
            this.mStationID = stationId;
            this.mRaceID = raceId;
            this.mAllianceID = allianceId;
            this.mShares = shares;
            this.mMemberCount = memberCount;
            this.mMemberLimit = memberLimit;
            this.mAllowedMemberRaceIDs = allowedMemberRaceIDs;
            this.mGraphicID = graphicId;
            this.mShape1 = shape1;
            this.mShape2 = shape2;
            this.mShape3 = shape3;
            this.mColor1 = color1;
            this.mColor2 = color2;
            this.mColor3 = color3;
            this.mTypeface = typeface;
            this.mDivision1 = division1;
            this.mDivision2 = division2;
            this.mDivision3 = division3;
            this.mDivision4 = division4;
            this.mDivision5 = division5;
            this.mDivision6 = division6;
            this.mDivision7 = division7;
            this.mBalance = balance;
            this.mDeleted = deleted;
        }

        string mDescription;
        string mTickerName;
        string mUrl;
        double mTaxRate;
        double mMinimumJoinStanding;
        int mCorporationType;
        bool mHasPlayerPersonnelManager;
        bool mSendCharTerminationMessage;
        int mCreatorID;
        int mStationID;
        int mRaceID;
        int mAllianceID;
        long mShares;
        int mMemberCount;
        int mMemberLimit;
        int mAllowedMemberRaceIDs;
        int mGraphicID;
        int? mShape1;
        int? mShape2;
        int? mShape3;
        int? mColor1;
        int? mColor2;
        int? mColor3;
        string mTypeface;
        string mDivision1;
        string mDivision2;
        string mDivision3;
        string mDivision4;
        string mDivision5;
        string mDivision6;
        string mDivision7;
        double mBalance;
        bool mDeleted;

        public string Description => mDescription;
        public string TickerName => mTickerName;
        public string Url => mUrl;
        public double TaxRate => mTaxRate;
        public double MinimumJoinStanding => mMinimumJoinStanding;
        public int CorporationType => mCorporationType;
        public bool HasPlayerPersonnelManager => mHasPlayerPersonnelManager;
        public bool SendCharTerminationMessage => mSendCharTerminationMessage;
        public int CreatorId => mCreatorID;
        public int StationId => mStationID;
        public int RaceId => mRaceID;
        public int AllianceID => mAllianceID;
        public long Shares => mShares;
        public int MemberCount => mMemberCount;
        public int MemberLimit => mMemberLimit;
        public int AllowedMemberRaceIDs => mAllowedMemberRaceIDs;
        public int GraphicId => mGraphicID;
        public int? Shape1 => mShape1;
        public int? Shape2 => mShape2;
        public int? Shape3 => mShape3;
        public int? Color1 => mColor1;
        public int? Color2 => mColor2;
        public int? Color3 => mColor3;
        public string Typeface => mTypeface;
        public string Division1 => mDivision1;
        public string Division2 => mDivision2;
        public string Division3 => mDivision3;
        public string Division4 => mDivision4;
        public string Division5 => mDivision5;
        public string Division6 => mDivision6;
        public string Division7 => mDivision7;
        public double Balance => mBalance;
        public bool Deleted => mDeleted;
    }
}