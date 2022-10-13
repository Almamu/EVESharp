using System.Collections.Generic;
using EVESharp.Database.Inventory.Stations;

namespace EVESharp.EVE.Data.Inventory;

public interface IStations : IDictionary <int, Items.Types.Station>
{
    Dictionary <int, Operation> Operations { get; }
    Dictionary <int, Type>      StationTypes { get; }
    Dictionary <int, string> Services     { get; }
}