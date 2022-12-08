using EVESharp.Database;
using EVESharp.Database.Extensions;
using HarmonyLib;

namespace EVESharp.Node.Unit.MemoryDatabase.Patches;

public class Cluster
{
    [HarmonyPatch (typeof (ClusterDB), nameof(ClusterDB.CluCleanup))]
    public static bool CluCleanup (IDatabase Database)
    {
        return false;
    }

    [HarmonyPatch (typeof (ClusterDB), nameof(ClusterDB.CluRegisterSingleNode))]
    public static bool CluRegisterSingleNode (IDatabase Database, long nodeID)
    {
        return false;
    }
}