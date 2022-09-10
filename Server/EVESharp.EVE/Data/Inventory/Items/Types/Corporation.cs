using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class Corporation : ItemInventory
{
    public Information.Corporation CorporationInformation { get; }

    public string Description
    {
        get => this.CorporationInformation.Description;
        set
        {
            this.Information.Dirty                  = true;
            this.CorporationInformation.Description = value;
        }
    }
    public string TickerName => this.CorporationInformation.TickerName;
    public string Url
    {
        get => this.CorporationInformation.Url;
        set
        {
            this.Information.Dirty          = true;
            this.CorporationInformation.Url = value;
        }
    }
    public double TaxRate
    {
        get => this.CorporationInformation.TaxRate;
        set
        {
            this.Information.Dirty              = true;
            this.CorporationInformation.TaxRate = value;
        }
    }
    public double MinimumJoinStanding        => this.CorporationInformation.MinimumJoinStanding;
    public int    CorporationType            => this.CorporationInformation.CorporationType;
    public bool   HasPlayerPersonnelManager  => this.CorporationInformation.HasPlayerPersonnelManager;
    public bool   SendCharTerminationMessage => this.CorporationInformation.SendCharTerminationMessage;
    public int    CreatorID                  => this.CorporationInformation.CreatorID;
    public int    CeoID                      => this.CorporationInformation.CeoID;
    public int    StationID                  => this.CorporationInformation.StationID;
    public int    RaceID                     => this.CorporationInformation.RaceID;

    public int? AllianceID
    {
        get => this.CorporationInformation.AllianceID;
        set
        {
            this.Information.Dirty                 = true;
            this.CorporationInformation.AllianceID = value;
        }
    }
    public long Shares      => this.CorporationInformation.Shares;
    public int  MemberCount => this.CorporationInformation.MemberCount;
    public int MemberLimit
    {
        get => this.CorporationInformation.MemberLimit;
        set
        {
            this.Information.Dirty                  = true;
            this.CorporationInformation.MemberLimit = value;
        }
    }
    public int AllowedMemberRaceIDs
    {
        get => this.CorporationInformation.AllowedMemberRaceIDs;
        set
        {
            this.Information.Dirty                           = true;
            this.CorporationInformation.AllowedMemberRaceIDs = value;
        }
    }
    public int    GraphicId => this.CorporationInformation.GraphicId;
    public int?   Shape1    => this.CorporationInformation.Shape1;
    public int?   Shape2    => this.CorporationInformation.Shape2;
    public int?   Shape3    => this.CorporationInformation.Shape3;
    public int?   Color1    => this.CorporationInformation.Color1;
    public int?   Color2    => this.CorporationInformation.Color2;
    public int?   Color3    => this.CorporationInformation.Color3;
    public string Typeface  => this.CorporationInformation.Typeface;
    public string Division1
    {
        get => this.CorporationInformation.Division1;
        set
        {
            this.Information.Dirty                = true;
            this.CorporationInformation.Division1 = value;
        }
    }
    public string Division2
    {
        get => this.CorporationInformation.Division2;
        set
        {
            this.Information.Dirty                = true;
            this.CorporationInformation.Division2 = value;
        }
    }
    public string Division3
    {
        get => this.CorporationInformation.Division3;
        set
        {
            this.Information.Dirty                = true;
            this.CorporationInformation.Division3 = value;
        }
    }
    public string Division4
    {
        get => this.CorporationInformation.Division4;
        set
        {
            this.Information.Dirty                = true;
            this.CorporationInformation.Division4 = value;
        }
    }
    public string Division5
    {
        get => this.CorporationInformation.Division5;
        set
        {
            this.Information.Dirty                = true;
            this.CorporationInformation.Division5 = value;
        }
    }
    public string Division6
    {
        get => this.CorporationInformation.Division6;
        set
        {
            this.Information.Dirty                = true;
            this.CorporationInformation.Division6 = value;
        }
    }
    public string Division7
    {
        get => this.CorporationInformation.Division7;
        set
        {
            this.Information.Dirty                = true;
            this.CorporationInformation.Division7 = value;
        }
    }
    public string WalletDivision1
    {
        get => this.CorporationInformation.WalletDivision1;
        set
        {
            this.Information.Dirty                      = true;
            this.CorporationInformation.WalletDivision1 = value;
        }
    }
    public string WalletDivision2
    {
        get => this.CorporationInformation.WalletDivision2;
        set
        {
            this.Information.Dirty                      = true;
            this.CorporationInformation.WalletDivision2 = value;
        }
    }
    public string WalletDivision3
    {
        get => this.CorporationInformation.WalletDivision3;
        set
        {
            this.Information.Dirty                      = true;
            this.CorporationInformation.WalletDivision3 = value;
        }
    }
    public string WalletDivision4
    {
        get => this.CorporationInformation.WalletDivision4;
        set
        {
            this.Information.Dirty                      = true;
            this.CorporationInformation.WalletDivision4 = value;
        }
    }
    public string WalletDivision5
    {
        get => this.CorporationInformation.WalletDivision5;
        set
        {
            this.Information.Dirty                      = true;
            this.CorporationInformation.WalletDivision5 = value;
        }
    }
    public string WalletDivision6
    {
        get => this.CorporationInformation.WalletDivision6;
        set
        {
            this.Information.Dirty                      = true;
            this.CorporationInformation.WalletDivision6 = value;
        }
    }
    public string WalletDivision7
    {
        get => this.CorporationInformation.WalletDivision7;
        set
        {
            this.Information.Dirty                      = true;
            this.CorporationInformation.WalletDivision7 = value;
        }
    }
    public bool Deleted => this.CorporationInformation.Deleted;

    public long? StartDate
    {
        get => this.CorporationInformation.StartDate;
        set
        {
            this.Information.Dirty                = true;
            this.CorporationInformation.StartDate = value;
        }
    }

    public int? ExecutorCorpID
    {
        get => this.CorporationInformation.ExecutorCorpID;
        set
        {
            this.Information.Dirty                     = true;
            this.CorporationInformation.ExecutorCorpID = value;
        }
    }

    public Corporation (Information.Corporation info) : base (info.Information)
    {
        this.CorporationInformation = info;
    }

    public Row GetCorporationInfoRow ()
    {
        return new Row (
            new PyList <PyString> (42)
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
            new PyList (42)
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