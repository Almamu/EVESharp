using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items.Types
{
    public class Corporation : ItemInventory
    {
        public Corporation(ItemEntity @from, string description, string tickerName, string url, double taxRate,
            double minimumJoinStanding, int corporationType, bool hasPlayerPersonnelManager,
            bool sendCharTerminationMessage, int creatorID, int ceoID, int stationID, int raceID, int allianceId, long shares,
            int memberCount, int memberLimit, int allowedMemberRaceIDs, int graphicId, int? shape1, int? shape2,
            int? shape3, int? color1, int? color2, int? color3, string typeface, string division1, string division2,
            string division3, string division4, string division5, string division6, string division7,
            string walletDivision1, string walletDivision2, string walletDivision3, string walletDivision4,
            string walletDivision5, string walletDivision6, string walletDivision7, double balance,
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
            this.mCreatorID = creatorID;
            this.mCeoID = ceoID;
            this.mStationID = stationID;
            this.mRaceID = raceID;
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
        int mCeoID;
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
        string mWalletDivision1;
        string mWalletDivision2;
        string mWalletDivision3;
        string mWalletDivision4;
        string mWalletDivision5;
        string mWalletDivision6;
        string mWalletDivision7;
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
        public int CreatorID => mCreatorID;
        public int CeoID => this.mCeoID;
        public int StationID => mStationID;
        public int RaceID => mRaceID;
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
        public string WalletDivision1 => mWalletDivision1;
        public string WalletDivision2 => mWalletDivision2;
        public string WalletDivision3 => mWalletDivision3;
        public string WalletDivision4 => mWalletDivision4;
        public string WalletDivision5 => mWalletDivision5;
        public string WalletDivision6 => mWalletDivision6;
        public string WalletDivision7 => mWalletDivision7;
        public double Balance => mBalance;
        public bool Deleted => mDeleted;

        public Row GetCorporationInfoRow()
        {
            return new Row(
                (PyList) new PyDataType[]
                {
                    "corporationID", "corporationName", "description", "tickerName", "url", "taxRate",
                    "minimumJoinStanding", "corporationType", "hasPlayerPersonnelManager", "sendCharTerminationMessage",
                    "creatorID", "ceoID", "stationID", "raceID", "allianceID", "shares", "memberCount", "memberLimit",
                    "allowedMemberRaceIDs", "graphicID", "shape1", "shape2", "shape3", "color1", "color2", "color3",
                    "typeface", "division1", "division2", "division3", "division4", "division5", "division6",
                    "division7", "walletDivision1", "walletDivision2", "walletDivision3", "walletDivision4",
                    "walletDivision5", "walletDivision6", "walletDivision7", "deleted"
                },
                (PyList) new PyDataType []
                {
                    this.ID, this.Name, this.Description, this.TickerName, this.Url, this.TaxRate,
                    this.MinimumJoinStanding, this.CorporationType, this.HasPlayerPersonnelManager,
                    this.SendCharTerminationMessage, this.CreatorID, this.CeoID, this.StationID, this.RaceID,
                    this.AllianceID, this.Shares, this.MemberCount, this.MemberLimit, this.AllowedMemberRaceIDs,
                    this.GraphicId, this.Shape1, this.Shape2, this.Shape3, this.Color1, this.Color2, this.Color3,
                    this.Typeface, this.Division1, this.Division2, this.Division3, this.Division4, this.Division5,
                    this.Division6, this.Division7, this.WalletDivision1, this.WalletDivision2, this.WalletDivision3,
                    this.WalletDivision4, this.WalletDivision5, this.WalletDivision6, this.WalletDivision7, this.Deleted
                }
            );
        }
    }
}