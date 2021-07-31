using PythonTypes.Types.Collections;
using PythonTypes.Types.Database;
using PythonTypes.Types.Primitives;

namespace Node.Inventory.Items.Types
{
    public class Corporation : ItemInventory
    {
        public Corporation(ItemEntity @from, string description, string tickerName, string url, double taxRate,
            double minimumJoinStanding, int corporationType, bool hasPlayerPersonnelManager,
            bool sendCharTerminationMessage, int creatorID, int ceoID, int stationID, int raceID, int? allianceID, long shares,
            int memberCount, int memberLimit, int allowedMemberRaceIDs, int graphicId, int? shape1, int? shape2,
            int? shape3, int? color1, int? color2, int? color3, string typeface, string division1, string division2,
            string division3, string division4, string division5, string division6, string division7,
            string walletDivision1, string walletDivision2, string walletDivision3, string walletDivision4,
            string walletDivision5, string walletDivision6, string walletDivision7, bool deleted, long? startDate) : base(@from)
        {
            this.Description = description;
            this.mTickerName = tickerName;
            this.Url = url;
            this.TaxRate = taxRate;
            this.mMinimumJoinStanding = minimumJoinStanding;
            this.mCorporationType = corporationType;
            this.mHasPlayerPersonnelManager = hasPlayerPersonnelManager;
            this.mSendCharTerminationMessage = sendCharTerminationMessage;
            this.mCreatorID = creatorID;
            this.mCeoID = ceoID;
            this.mStationID = stationID;
            this.mRaceID = raceID;
            this.mAllianceID = allianceID;
            this.mShares = shares;
            this.mMemberCount = memberCount;
            this.MemberLimit = memberLimit;
            this.AllowedMemberRaceIDs = allowedMemberRaceIDs;
            this.mGraphicID = graphicId;
            this.mShape1 = shape1;
            this.mShape2 = shape2;
            this.mShape3 = shape3;
            this.mColor1 = color1;
            this.mColor2 = color2;
            this.mColor3 = color3;
            this.mTypeface = typeface;
            this.Division1 = division1;
            this.Division2 = division2;
            this.Division3 = division3;
            this.Division4 = division4;
            this.Division5 = division5;
            this.Division6 = division6;
            this.Division7 = division7;
            this.WalletDivision1 = walletDivision1;
            this.WalletDivision2 = walletDivision2;
            this.WalletDivision3 = walletDivision3;
            this.WalletDivision4 = walletDivision4;
            this.WalletDivision5 = walletDivision5;
            this.WalletDivision6 = walletDivision6;
            this.WalletDivision7 = walletDivision7;
            this.mDeleted = deleted;
            this.mStartDate = startDate;
        }

        string mTickerName;
        double mMinimumJoinStanding;
        int mCorporationType;
        bool mHasPlayerPersonnelManager;
        bool mSendCharTerminationMessage;
        int mCreatorID;
        int mCeoID;
        int mStationID;
        int mRaceID;
        int? mAllianceID;
        long mShares;
        int mMemberCount;
        int mGraphicID;
        int? mShape1;
        int? mShape2;
        int? mShape3;
        int? mColor1;
        int? mColor2;
        int? mColor3;
        string mTypeface;
        bool mDeleted;
        long? mStartDate;

        public string Description { get; set; }
        public string TickerName => mTickerName;
        public string Url { get; set; }
        public double TaxRate { get; set; }
        public double MinimumJoinStanding => mMinimumJoinStanding;
        public int CorporationType => mCorporationType;
        public bool HasPlayerPersonnelManager => mHasPlayerPersonnelManager;
        public bool SendCharTerminationMessage => mSendCharTerminationMessage;
        public int CreatorID => mCreatorID;
        public int CeoID => this.mCeoID;
        public int StationID => mStationID;
        public int RaceID => mRaceID;

        public int? AllianceID
        {
            get => this.mAllianceID;
            set
            {
                this.Dirty = true;
                this.mAllianceID = value;
            }
        }
        public long Shares => mShares;
        public int MemberCount => mMemberCount;
        public int MemberLimit { get; set; }
        public int AllowedMemberRaceIDs { get; set; }
        public int GraphicId => mGraphicID;
        public int? Shape1 => mShape1;
        public int? Shape2 => mShape2;
        public int? Shape3 => mShape3;
        public int? Color1 => mColor1;
        public int? Color2 => mColor2;
        public int? Color3 => mColor3;
        public string Typeface => mTypeface;
        public string Division1 { get; set; }
        public string Division2 { get; set; }
        public string Division3 { get; set; }
        public string Division4 { get; set; }
        public string Division5 { get; set; }
        public string Division6 { get; set; }
        public string Division7 { get; set; }
        public string WalletDivision1 { get; set; }
        public string WalletDivision2 { get; set; }
        public string WalletDivision3 { get; set; }
        public string WalletDivision4 { get; set; }
        public string WalletDivision5 { get; set; }
        public string WalletDivision6 { get; set; }
        public string WalletDivision7 { get; set; }
        public bool Deleted => mDeleted;

        public long? StartDate
        {
            get => this.mStartDate;
            set
            {
                this.Dirty = true;
                this.mStartDate = value;
            }
        }

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
        
        protected override void SaveToDB()
        {
            base.SaveToDB();

            // update the relevant character information
            this.ItemFactory.CorporationDB.UpdateCorporationInformation(this);
        }
    }
}