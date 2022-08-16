using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace EVESharp.Node.Unit.Utils;

public static class HarmonyExtensions
{
    public static void Setup (this Harmony harmony, object test)
    {
        harmony.Setup (test.GetType ());
    }
    
    public static void Setup (this Harmony harmony, Type test)
    {
        // look for the HarmonyPatch attribute and apply every patch
        MethodInfo [] methods = test.GetMethods (BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (MethodInfo method in methods)
        {
            foreach (HarmonyPatch patch in method.GetCustomAttributes <HarmonyPatch> ())
            {
                // build the parameter list (ignoring special parameters)
                ParameterInfo [] parameters = method.GetParameters ();
                List <Type>      types      = new List <Type> ();

                foreach (ParameterInfo parameter in parameters)
                {
                    switch (parameter.Name)
                    {
                        case "__instance":
                        case "__result":
                        case "__args":
                        case "__originalMethod":
                        case "__runOriginal":
                        case "__state":
                            break;
                        default: types.Add (parameter.ParameterType);
                            break;
                    }
                }

                MethodInfo tmp = AccessTools.Method (patch.info.declaringType, patch.info.methodName, types.ToArray ());
                
                harmony.Patch (tmp, new HarmonyMethod (method));
            }
        }
    }
}