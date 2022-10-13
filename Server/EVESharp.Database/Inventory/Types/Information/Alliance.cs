namespace EVESharp.Database.Inventory.Types.Information;

public class Alliance
{
    public string ShortName      { get; init; }
    public string Description    { get; set; }
    public string URL            { get; set; }
    public int?   ExecutorCorpID { get; set; }
    public int    CreatorCorpID  { get; init; }
    public int    CreatorCharID  { get; init; }
    public bool   Dictatorial    { get; init; }
    public Item   Information    { get; init; }
}