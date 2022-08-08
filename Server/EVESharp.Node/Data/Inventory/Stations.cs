using System.Collections;
using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Inventory.Station;
using EVESharp.Node.Database;
using EVESharp.Node.Inventory;
using Type = EVESharp.EVE.Data.Inventory.Station.Type;

namespace EVESharp.Node.Data.Inventory;

public class Stations : Dictionary <int, Station>, IStations
{
    public           Dictionary <int, Operation> Operations   { get; }
    public           Dictionary <int, Type>      StationTypes { get; }
    public           Dictionary <int, string>    Services     { get; }

    public Stations (StationDB stationDB)
    {
        this.Operations   = stationDB.LoadOperations ();
        this.StationTypes = stationDB.LoadStationTypes ();
        this.Services     = stationDB.LoadServices ();
    }
}