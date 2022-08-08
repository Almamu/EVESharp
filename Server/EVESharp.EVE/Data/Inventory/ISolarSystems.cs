using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;

namespace EVESharp.Node.Data.Inventory;

public interface ISolarSystems : IDictionary <int, SolarSystem>
{
    SolarSystem this [TypeID id] { get; set; }
    
    long LoadSolarSystemOnCluster (int solarSystemID);
    bool StationBelongsToUs (int       stationID);
    bool SolarSystemBelongsToUs (int   solarSystemID);
    long GetNodeStationBelongsTo (int  stationID);
    long GetNodeSolarSystemBelongsTo (int solarSystemID);
}