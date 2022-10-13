using System.Collections.Generic;
using EVESharp.Database.Inventory.Types;
using EVESharp.EVE.Data.Inventory.Items.Types;

namespace EVESharp.EVE.Data.Inventory;

public interface ISolarSystems : IDictionary <int, SolarSystem>
{
    SolarSystem this [TypeID id] { get; set; }
    
    long LoadSolarSystemOnCluster (int solarSystemID);
    bool StationBelongsToUs (int       stationID);
    bool SolarSystemBelongsToUs (int   solarSystemID);
    long GetNodeStationBelongsTo (int  stationID);
    long GetNodeSolarSystemBelongsTo (int solarSystemID);
}