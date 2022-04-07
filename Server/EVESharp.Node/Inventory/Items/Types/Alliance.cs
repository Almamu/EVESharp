using System.Collections.Generic;
using EVESharp.Database;
using EVESharp.Node.Database;
using EVESharp.Node.Exceptions.corpRegistry;

namespace EVESharp.Node.Inventory.Items.Types;

public class Alliance : ItemEntity
{
    private Information.Alliance AllianceInformation { get; }
        
    public Alliance(Information.Alliance allianceInformation) : base(allianceInformation.Information)
    {
        this.AllianceInformation = allianceInformation;
    }

    public string ShortName => this.AllianceInformation.ShortName;
    public string Description
    {
        get => this.AllianceInformation.Description;
        set
        {
            this.Information.Dirty               = true;
            this.AllianceInformation.Description = value;
        }
    }

    public string Url
    {
        get => this.AllianceInformation.URL;
        set
        {
            this.Information.Dirty       = true;
            this.AllianceInformation.URL = value;
        }
    }

    public int? ExecutorCorpID
    {
        get => this.AllianceInformation.ExecutorCorpID;
        set
        {
            this.Information.Dirty                  = true;
            this.AllianceInformation.ExecutorCorpID = value;
        }
    }
        
    public int  CreatorCorpID => this.AllianceInformation.CreatorCorpID;
    public int  CreatorCharID => this.AllianceInformation.CreatorCharID;
    public bool Dictatorial   => this.AllianceInformation.Dictatorial;
}