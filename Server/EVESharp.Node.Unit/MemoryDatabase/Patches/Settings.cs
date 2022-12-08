using EVESharp.Database;
using EVESharp.Database.Extensions;
using EVESharp.Types;
using EVESharp.Types.Collections;
using HarmonyLib;

namespace EVESharp.Node.Unit.MemoryDatabase.Patches;

public class Settings
{
    [HarmonyPatch (typeof (SettingsDB), nameof(SettingsDB.EveFetchLiveUpdates))]
    public static bool EveFetchLiveUpdtes (IDatabase Database, ref PyList <PyObjectData> __result)
    {
        __result = new PyList <PyObjectData> ();
        return false;
    }
}