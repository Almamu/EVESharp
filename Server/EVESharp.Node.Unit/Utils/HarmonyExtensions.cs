using System;
using System.Reflection;
using HarmonyLib;

namespace EVESharp.Node.Unit.Utils;

public static class HarmonyExtensions
{
    public static void Setup (this Harmony harmony, Type test)
    {
        // look for the HarmonyPatch attribute and apply every patch
        MethodInfo [] methods = test.GetMethods (BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (MethodInfo method in methods)
        {
            foreach (HarmonyPatch patch in method.GetCustomAttributes <HarmonyPatch> ())
            {
                harmony.Patch (patch.info.declaringType.GetMethod (patch.info.methodName), new HarmonyMethod (method));
            }
        }
    }
}