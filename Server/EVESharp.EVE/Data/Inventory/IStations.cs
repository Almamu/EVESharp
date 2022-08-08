using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory.Station;

namespace EVESharp.EVE.Data.Inventory;

public interface IStations : IDictionary <int, Items.Types.Station>
{
    Dictionary <int, Operation> Operations { get; }
    Dictionary <int, Station.Type>      StationTypes { get; }
    Dictionary <int, string> Services     { get; }
}