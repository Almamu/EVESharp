using System.Collections.Generic;
using EVESharp.EVE.Data.Inventory;
using EVESharp.EVE.Data.Inventory.Items.Types;

namespace EVESharp.Node.Data.Inventory;

public class Factions : Dictionary <int, Faction>, IFactions
{
}