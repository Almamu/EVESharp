using System.Linq;
using EVESharp.Database;
using EVESharp.Database.Extensions;
using HarmonyLib;

namespace EVESharp.Node.Unit.MemoryDatabase.Patches;

public class Accounts
{
    [HarmonyPatch (typeof (AccountDB), nameof(AccountDB.CluResetClientAddresses))]
    public static bool CluResetClientAddresses (IDatabase Database)
    {
        return false;
    }

    [HarmonyPatch (typeof (AccountDB), nameof (AccountDB.ActExists))]
    public static bool ActExists (IDatabase Database, string username, ref bool __result)
    {
        __result = MemoryDatabase.Accounts.Data.Any (x => x.username == username);
        
        return false;
    }

    [HarmonyPatch (typeof (AccountDB), nameof (AccountDB.ActLogin))]
    public static bool ActLogin (IDatabase Database, string username, string password, out int? accountID, out ulong? role, out bool? banned, ref bool __result)
    {
        MemoryDatabase.Accounts.Entity account = MemoryDatabase.Accounts.Data.FirstOrDefault(x => x.username == username && x.password == password);
        
        accountID = 0;
        role      = 0;
        banned    = false;
        __result  = false;

        if (account is null)
            return false;

        accountID = account.accountID;
        role      = account.role;
        banned    = account.banned;
        __result  = true;

        return false;
    }
}