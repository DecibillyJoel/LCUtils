using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;

namespace LCUtils;

public static class ConfigUtils
{
    private static readonly Func<object, object[], object> AccessConfigOrphanedEntries = AccessTools.DeclaredPropertyGetter(typeof(ConfigFile), "OrphanedEntries").Invoke;
    private static readonly Dictionary<ConfigFile, Dictionary<ConfigDefinition, string>> OrphanedEntries = [];

    public static Dictionary<ConfigDefinition, string> GetOrphanedEntries(this ConfigFile configFile)
    {
        if (OrphanedEntries.ContainsKey(configFile)) return OrphanedEntries[configFile];
        
        return OrphanedEntries[configFile] = (Dictionary<ConfigDefinition, string>) AccessConfigOrphanedEntries(configFile, []);
    }
}