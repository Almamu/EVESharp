using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Inventory.Items.Types;

public class Corporation : ItemInventory
{
    public Information.Corporation CorporationInformation { get; }

    public string Description
    {
        get => CorporationInformation.Description;
        set
        {
            Information.Dirty                  = true;
            CorporationInformation.Description = value;
        }
    }
    public string TickerName => CorporationInformation.TickerName;
    public string Url
    {
        get => CorporationInformation.Url;
        set
        {
            Information.Dirty          = true;
            CorporationInformation.Url = value;
        }
    }
    public double TaxRate
    {
        get => CorporationInformation.TaxRate;
        set
        {
            Information.Dirty              = true;
            CorporationInformation.TaxRate = value;
        }
    }
    public double MinimumJoinStanding        => CorporationInformation.MinimumJoinStanding;
    public int    CorporationType            => CorporationInformation.CorporationType;
    public bool   HasPlayerPersonnelManager  => CorporationInformation.HasPlayerPersonnelManager;
    public bool   SendCharTerminationMessage => CorporationInformation.SendCharTerminationMessage;
    public int    CreatorID                  => CorporationInformation.CreatorID;
    public int    CeoID                      => CorporationInformation.CeoID;
    public int    StationID                  => CorporationInformation.StationID;
    public int    RaceID                     => CorporationInformation.RaceID;

    public int? AllianceID
    {
        get => CorporationInformation.AllianceID;
        set
        {
            Information.Dirty                 = true;
            CorporationInformation.AllianceID = value;
        }
    }
    public long Shares      => CorporationInformation.Shares;
    public int  MemberCount => CorporationInformation.MemberCount;
    public int MemberLimit
    {
        get => CorporationInformation.MemberLimit;
        set
        {
            Information.Dirty                  = true;
            CorporationInformation.MemberLimit = value;
        }
    }
    public int AllowedMemberRaceIDs
    {
        get => CorporationInformation.AllowedMemberRaceIDs;
        set
        {
            Information.Dirty                           = true;
            CorporationInformation.AllowedMemberRaceIDs = value;
        }
    }
    public int    GraphicId => CorporationInformation.GraphicId;
    public int?   Shape1    => CorporationInformation.Shape1;
    public int?   Shape2    => CorporationInformation.Shape2;
    public int?   Shape3    => CorporationInformation.Shape3;
    public int?   Color1    => CorporationInformation.Color1;
    public int?   Color2    => CorporationInformation.Color2;
    public int?   Color3    => CorporationInformation.Color3;
    public string Typeface  => CorporationInformation.Typeface;
    public string Division1
    {
        get => CorporationInformation.Division1;
        set
        {
            Information.Dirty                = true;
            CorporationInformation.Division1 = value;
        }
    }
    public string Division2
    {
        get => CorporationInformation.Division2;
        set
        {
            Information.Dirty                = true;
            CorporationInformation.Division2 = value;
        }
    }
    public string Division3
    {
        get => CorporationInformation.Division3;
        set
        {
            Information.Dirty                = true;
            CorporationInformation.Division3 = value;
        }
    }
    public string Division4
    {
        get => CorporationInformation.Division4;
        set
        {
            Information.Dirty                = true;
            CorporationInformation.Division4 = value;
        }
    }
    public string Division5
    {
        get => CorporationInformation.Division5;
        set
        {
            Information.Dirty                = true;
            CorporationInformation.Division5 = value;
        }
    }
    public string Division6
    {
        get => CorporationInformation.Division6;
        set
        {
            Information.Dirty                = true;
            CorporationInformation.Division6 = value;
        }
    }
    public string Division7
    {
        get => CorporationInformation.Division7;
        set
        {
            Information.Dirty                = true;
            CorporationInformation.Division7 = value;
        }
    }
    public string WalletDivision1
    {
        get => CorporationInformation.WalletDivision1;
        set
        {
            Information.Dirty                      = true;
            CorporationInformation.WalletDivision1 = value;
        }
    }
    public string WalletDivision2
    {
        get => CorporationInformation.WalletDivision2;
        set
        {
            Information.Dirty                      = true;
            CorporationInformation.WalletDivision2 = value;
        }
    }
    public string WalletDivision3
    {
        get => CorporationInformation.WalletDivision3;
        set
        {
            Information.Dirty                      = true;
            CorporationInformation.WalletDivision3 = value;
        }
    }
    public string WalletDivision4
    {
        get => CorporationInformation.WalletDivision4;
        set
        {
            Information.Dirty                      = true;
            CorporationInformation.WalletDivision4 = value;
        }
    }
    public string WalletDivision5
    {
        get => CorporationInformation.WalletDivision5;
        set
        {
            Information.Dirty                      = true;
            CorporationInformation.WalletDivision5 = value;
        }
    }
    public string WalletDivision6
    {
        get => CorporationInformation.WalletDivision6;
        set
        {
            Information.Dirty                      = true;
            CorporationInformation.WalletDivision6 = value;
        }
    }
    public string WalletDivision7
    {
        get => CorporationInformation.WalletDivision7;
        set
        {
            Information.Dirty                      = true;
            CorporationInformation.WalletDivision7 = value;
        }
    }
    public bool Deleted => CorporationInformation.Deleted;

    public long? StartDate
    {
        get => CorporationInformation.StartDate;
        set
        {
            Information.Dirty                = true;
            CorporationInformation.StartDate = value;
        }
    }

    public int? ExecutorCorpID
    {
        get => CorporationInformation.ExecutorCorpID;
        set
        {
            Information.Dirty                     = true;
            CorporationInformation.ExecutorCorpID = value;
        }
    }

    public Corporation (Information.Corporation info) : base (info.Information)
    {
        CorporationInformation = info;
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
                [0]  = ID,
                [1]  = Name,
                [2]  = Description,
                [3]  = TickerName,
                [4]  = Url,
                [5]  = TaxRate,
                [6]  = MinimumJoinStanding,
                [7]  = CorporationType,
                [8]  = HasPlayerPersonnelManager,
                [9]  = SendCharTerminationMessage,
                [10] = CreatorID,
                [11] = CeoID,
                [12] = StationID,
                [13] = RaceID,
                [14] = AllianceID,
                [15] = Shares,
                [16] = MemberCount,
                [17] = MemberLimit,
                [18] = AllowedMemberRaceIDs,
                [19] = GraphicId,
                [20] = Shape1,
                [21] = Shape2,
                [22] = Shape3,
                [23] = Color1,
                [24] = Color2,
                [25] = Color3,
                [26] = Typeface,
                [27] = Division1,
                [28] = Division2,
                [29] = Division3,
                [30] = Division4,
                [31] = Division5,
                [32] = Division6,
                [33] = Division7,
                [34] = WalletDivision1,
                [35] = WalletDivision2,
                [36] = WalletDivision3,
                [37] = WalletDivision4,
                [38] = WalletDivision5,
                [39] = WalletDivision6,
                [40] = WalletDivision7,
                [41] = Deleted
            }
        );
    }
}