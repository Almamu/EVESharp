using System;
using EVESharp.Database.Types;
using EVESharp.EVE.Types;
using EVESharp.Types;
using EVESharp.Types.Collections;

namespace EVESharp.EVE.Data.Inventory.Items.Types;

public class Faction : ItemEntity
{
    public Database.Inventory.Types.Information.Faction FactionInformation { get; init; }

    public string Description          => this.FactionInformation.Description;
    public int    RaceIDs              => this.FactionInformation.RaceIDs;
    public int    SolarSystemId        => this.FactionInformation.SolarSystemID;
    public int    CorporationId        => this.FactionInformation.CorporationID;
    public double SizeFactor           => this.FactionInformation.SizeFactor;
    public int    StationCount         => this.FactionInformation.StationCount;
    public int    StationSystemCount   => this.FactionInformation.StationSystemCount;
    public int    MilitiaCorporationId => this.FactionInformation.MilitiaCorporationID;

    public Faction (Database.Inventory.Types.Information.Faction info) : base (info.Information)
    {
        this.FactionInformation = info;
    }

    public override void Destroy ()
    {
        throw new NotImplementedException ("Factions cannot be destroyed as they're regarded as static data!");
    }

    public PyDataType GetKeyVal ()
    {
        return KeyVal.FromDictionary (
            new PyDictionary
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