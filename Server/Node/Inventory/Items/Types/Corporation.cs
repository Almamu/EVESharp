using PythonTypes.Types.Collections;
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
            this.mWalletDivision1 = walletDivision1;
            this.mWalletDivision2 = walletDivision2;
            this.mWalletDivision3 = walletDivision3;
            this.mWalletDivision4 = walletDivision4;
            this.mWalletDivision5 = walletDivision5;
            this.mWalletDivision6 = walletDivision6;
            this.mWalletDivision7 = walletDivision7;
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
                new PyList<PyString>(42)
                {
                    [0]  = "corporationID",
                    [1]  = "corporationName",
                    [2]  = "description",
                    [3]  = "tickerName",
                    [4]  = "url",
                    [5]  = "taxRate",
                    [6]  = "minimumJoinStanding",
                    [7]  = "corporationType",
                    [8]  = "hasPlayerPersonnelManager",
                    [9]  = "sendCharTerminationMessage",
                    [10] = "creatorID",
                    [11] = "ceoID",
                    [12] = "stationID",
                    [13] = "raceID",
                    [14] = "allianceID",
                    [15] = "shares",
                    [16] = "memberCount",
                    [17] = "memberLimit",
                    [18] = "allowedMemberRaceIDs",
                    [19] = "graphicID",
                    [20] = "shape1",
                    [21] = "shape2",
                    [22] = "shape3",
                    [23] = "color1",
                    [24] = "color2",
                    [25] = "color3",
                    [26] = "typeface",
                    [27] = "division1",
                    [28] = "division2",
                    [29] = "division3",
                    [30] = "division4",
                    [31] = "division5",
                    [32] = "division6",
                    [33] = "division7",
                    [34] = "walletDivision1",
                    [35] = "walletDivision2",
                    [36] = "walletDivision3",
                    [37] = "walletDivision4",
                    [38] = "walletDivision5",
                    [39] = "walletDivision6",
                    [40] = "walletDivision7",
                    [41] = "deleted"
                },
                new PyList(42)
                {
                    [0]  = this.ID,
                    [1]  = this.Name,
                    [2]  = this.Description,
                    [3]  = this.TickerName,
                    [4]  = this.Url,
                    [5]  = this.TaxRate,
                    [6]  = this.MinimumJoinStanding,
                    [7]  = this.CorporationType,
                    [8]  = this.HasPlayerPersonnelManager,
                    [9]  = this.SendCharTerminationMessage,
                    [10] = this.CreatorID,
                    [11] = this.CeoID,
                    [12] = this.StationID,
                    [13] = this.RaceID,
                    [14] = this.AllianceID,
                    [15] = this.Shares,
                    [16] = this.MemberCount,
                    [17] = this.MemberLimit,
                    [18] = this.AllowedMemberRaceIDs,
                    [19] = this.GraphicId,
                    [20] = this.Shape1,
                    [21] = this.Shape2,
                    [22] = this.Shape3,
                    [23] = this.Color1,
                    [24] = this.Color2,
                    [25] = this.Color3,
                    [26] = this.Typeface,
                    [27] = this.Division1,
                    [28] = this.Division2,
                    [29] = this.Division3,
                    [30] = this.Division4,
                    [31] = this.Division5,
                    [32] = this.Division6,
                    [33] = this.Division7,
                    [34] = this.WalletDivision1,
                    [35] = this.WalletDivision2,
                    [36] = this.WalletDivision3,
                    [37] = this.WalletDivision4,
                    [38] = this.WalletDivision5,
                    [39] = this.WalletDivision6,
                    [40] = this.WalletDivision7,
                    [41] = this.Deleted
                }
            );
        }
    }
}