using System.Collections;
using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;
using EVESharp.EVE.Data.Inventory.Station;
using EVESharp.Node.Database;
using Type = EVESharp.EVE.Data.Inventory.Station.Type;

namespace EVESharp.Node.Data.Inventory;

public class Factions : Dictionary <int, Faction>, IFactions
{
}