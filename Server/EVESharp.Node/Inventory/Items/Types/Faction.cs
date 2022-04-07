using System;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Inventory.Items.Types;

public class Faction : ItemEntity
{
    public Information.Faction FactionInformation { get; init; }
    public Faction(Information.Faction info) : base(info.Information)
    {
        this.FactionInformation = info;
    }
        
    public string Description          => this.FactionInformation.Description;
    public int    RaceIDs              => this.FactionInformation.RaceIDs;
    public int    SolarSystemId        => this.FactionInformation.SolarSystemID;
    public int    CorporationId        => this.FactionInformation.CorporationID;
    public double SizeFactor           => this.FactionInformation.SizeFactor;
    public int    StationCount         => this.FactionInformation.StationCount;
    public int    StationSystemCount   => this.FactionInformation.StationSystemCount;
    public int    MilitiaCorporationId => this.FactionInformation.MilitiaCorporationID;

    public override void Destroy()
    {
        throw new NotImplementedException("Factions cannot be destroyed as they're regarded as static data!");
    }

    public PyDataType GetKeyVal()
    {
        return KeyVal.FromDictionary(
            new PyDictionary()
            {
                ["factionID"]     = this.ID,
                ["factionName"]   = this.Name,
                ["description"]   = this.Description,
                ["solarSystemID"] = this.SolarSystemId,
                ["corporationID"] = this.CorporationId,
                ["militiaID"]     = this.MilitiaCorporationId
            }
        );
    }
}