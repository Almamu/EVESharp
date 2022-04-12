namespace EVESharp.Node.Inventory.Items.Types;

public class Alliance : ItemEntity
{
    private Information.Alliance AllianceInformation { get; }

    public string ShortName => AllianceInformation.ShortName;
    public string Description
    {
        get => AllianceInformation.Description;
        set
        {
            Information.Dirty               = true;
            AllianceInformation.Description = value;
        }
    }

    public string Url
    {
        get => AllianceInformation.URL;
        set
        {
            Information.Dirty       = true;
            AllianceInformation.URL = value;
        }
    }

    public int? ExecutorCorpID
    {
        get => AllianceInformation.ExecutorCorpID;
        set
        {
            Information.Dirty                  = true;
            AllianceInformation.ExecutorCorpID = value;
        }
    }

    public int  CreatorCorpID => AllianceInformation.CreatorCorpID;
    public int  CreatorCharID => AllianceInformation.CreatorCharID;
    public bool Dictatorial   => AllianceInformation.Dictatorial;

    public Alliance (Information.Alliance allianceInformation) : base (allianceInformation.Information)
    {
        AllianceInformation = allianceInformation;
    }
}