using EVESharp.Database;
using EVESharp.Database.Account;
using EVESharp.Database.Extensions;
using EVESharp.Types;
using EVESharp.Types.Collections;
using HarmonyLib;

namespace EVESharp.Node.Unit.ClientBehaviourTest.Tests;

public class HarmonyPatches
{
    [HarmonyPatch (typeof (ItemDB), nameof(ItemDB.InvClearNodeAssociation))]
    public static bool InvClearNodeAssociation (IDatabase Database)
    {
        return false;
    }

    [HarmonyPatch (typeof (AccountDB), nameof(AccountDB.CluResetClientAddresses))]
    public static bool CluResetClientAddresses (IDatabase Database)
    {
        return false;
    }

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

    [HarmonyPatch (typeof (AccountDB), nameof (AccountDB.ActExists))]
    public static bool ActExists (IDatabase Database, string username, ref bool __result)
    {
        __result = username switch
        {
            "Almamu" => true,
            "Kira"   => true,
            "Banned" => true
        };
        
        return false;
    }

    [HarmonyPatch (typeof (AccountDB), nameof (AccountDB.ActLogin))]
    public static bool ActLogin (IDatabase Database, string username, string password, out int? accountID, out ulong? role, out bool? banned, ref bool __result)
    {
        // provide some valid logins
        if (username == "Almamu" && password == "Password")
        {
            accountID = 1;
            role = (ulong) Roles.ROLE_PLAYER | (ulong) Roles.ROLE_LOGIN | (ulong) Roles.ROLE_ADMIN | (ulong) Roles.ROLE_QA | (ulong) Roles.ROLE_SPAWN |
                   (ulong) Roles.ROLE_GML | (ulong) Roles.ROLE_GDL | (ulong) Roles.ROLE_GDH | (ulong) Roles.ROLE_HOSTING | (ulong) Roles.ROLE_PROGRAMMER;
            banned   = false;
            __result = true;

            return false;
        }
        if (username == "Kira" && password == "Kira")
        {
            accountID = 2;
            role      = (ulong) Roles.ROLE_PLAYER;
            banned    = false;
            __result  = true;

            return false;
        }
        if (username == "Banned" && password == "Banned")
        {
            accountID = 3;
            role      = (ulong) Roles.ROLE_PLAYER;
            banned    = true;
            __result  = true;
            
            return false;
        }

        accountID = 0;
        role      = 0;
        banned    = false;
        __result  = false;
        
        return false;
    }

    [HarmonyPatch (typeof (SettingsDB), nameof(SettingsDB.EveFetchLiveUpdates))]
    public static bool EveFetchLiveUpdtes (IDatabase Database, ref PyList <PyObjectData> __result)
    {
        __result = new PyList <PyObjectData> ();
        return false;
    }
}