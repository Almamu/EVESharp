using System;
using EVESharp.PythonTypes.Types.Collections;
using EVESharp.PythonTypes.Types.Database;
using EVESharp.PythonTypes.Types.Primitives;

namespace EVESharp.Node.Inventory.Items.Types;

public class Faction : ItemEntity
{
    public Information.Faction FactionInformation { get; init; }

    public string Description          => FactionInformation.Description;
    public int    RaceIDs              => FactionInformation.RaceIDs;
    public int    SolarSystemId        => FactionInformation.SolarSystemID;
    public int    CorporationId        => FactionInformation.CorporationID;
    public double SizeFactor           => FactionInformation.SizeFactor;
    public int    StationCount         => FactionInformation.StationCount;
    public int    StationSystemCount   => FactionInformation.StationSystemCount;
    public int    MilitiaCorporationId => FactionInformation.MilitiaCorporationID;

    public Faction (Information.Faction info) : base (info.Information)
    {
        FactionInformation = info;
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
                ["factionID"]     = ID,
                ["factionName"]   = Name,
                ["description"]   = Description,
                ["solarSystemID"] = SolarSystemId,
                ["corporationID"] = CorporationId,
                ["militiaID"]     = MilitiaCorporationId
            }
        );
    }
}