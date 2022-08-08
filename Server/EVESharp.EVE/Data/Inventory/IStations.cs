using System.Collections;
using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Inventory.Station;
using Type = EVESharp.EVE.Data.Inventory.Station.Type;

namespace EVESharp.Node.Inventory;

public interface IStations : IDictionary <int, Station>
{
    Dictionary <int, Operation> Operations   { get; }
    Dictionary <int, Type>      StationTypes { get; }
    Dictionary <int, string> Services     { get; }
}