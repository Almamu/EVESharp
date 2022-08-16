using EVESharp.Database;
using EVESharp.PythonTypes.Types.Database;
using HarmonyLib;
using NUnit.Framework.Internal.Execution;

namespace EVESharp.Node.Unit.ClientBehaviourTest.Tests;

public class HarmonyPatches
{
    [HarmonyPatch (typeof (ItemDB), nameof(ItemDB.InvClearNodeAssociation))]
    public static bool InvClearNodeAssociation (IDatabaseConnection Database)
    {
        return false;
    }

    [HarmonyPatch (typeof (AccountDB), nameof(AccountDB.CluResetClientAddresses))]
    public static bool CluResetClientAddresses (IDatabaseConnection Database)
    {
        return false;
    }

    [HarmonyPatch (typeof (ClusterDB), nameof(ClusterDB.CluCleanup))]
    public static bool CluCleanup (IDatabaseConnection Database)
    {
        return false;
    }

    [HarmonyPatch (typeof (ClusterDB), nameof(ClusterDB.CluRegisterSingleNode))]
    public static bool CluRegisterSingleNode (IDatabaseConnection Database, long nodeID)
    {
        return false;
    }
}