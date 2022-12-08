using EVESharp.Database;
using EVESharp.Database.Extensions;
using HarmonyLib;

namespace EVESharp.Node.Unit.MemoryDatabase.Patches;

public class Item
{
    [HarmonyPatch (typeof (ItemDB), nameof(ItemDB.InvClearNodeAssociation))]
    public static bool InvClearNodeAssociation (IDatabase Database)
    {
        return false;
    }
}