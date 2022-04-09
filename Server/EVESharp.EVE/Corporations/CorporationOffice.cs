namespace EVESharp.EVE.Corporations;

public class CorporationOffice
{
    public int  CorporationID { get; init; }
    public int  PeriodCost    { get; init; }
    public int  OfficeID      { get; init; }
    public int  StationID     { get; init; }
    public long DueDate       { get; init; }
}