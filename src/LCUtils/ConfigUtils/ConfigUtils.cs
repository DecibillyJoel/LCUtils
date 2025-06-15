using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;

namespace LCUtils;

/// <summary>
/// Utility class for working with BepInEx configuration files.
/// Provides additional functionality for accessing internal configuration data.
/// </summary>
public static class ConfigUtils
{
    /// <summary>
    /// Cached reflection access to the private OrphanedEntries property of ConfigFile.
    /// This delegate allows us to invoke the property getter without repeated reflection calls.
    /// </summary>
    private static readonly Func<object, object[], object> AccessConfigOrphanedEntries = AccessTools.DeclaredPropertyGetter(typeof(ConfigFile), "OrphanedEntries").Invoke;

    /// <summary>
    /// Cache for storing orphaned entries per ConfigFile instance to avoid repeated reflection calls.
    /// Key: ConfigFile instance, Value: Dictionary of orphaned configuration entries
    /// </summary>
    private static readonly Dictionary<ConfigFile, Dictionary<ConfigDefinition, string>> OrphanedEntries = [];

    /// <summary>
    /// Retrieves orphaned configuration entries for the specified ConfigFile.
    /// Orphaned entries are configuration values that exist in the config file but
    /// don't have corresponding ConfigEntry objects bound to them in the current session.
    /// </summary>
    /// <param name="configFile">The ConfigFile to get orphaned entries from</param>
    /// <returns>
    /// A dictionary containing orphaned configuration entries where:
    /// - Key: ConfigDefinition containing the section and key information
    /// - Value: The string value of the orphaned entry
    /// </returns>
    /// <remarks>
    /// Results are cached to improve performance on subsequent calls with the same ConfigFile.
    /// This method uses reflection to access the internal OrphanedEntries property of ConfigFile.
    /// </remarks>
    public static Dictionary<ConfigDefinition, string> GetOrphanedEntries(this ConfigFile configFile)
    {
        // Check if we already have cached results for this ConfigFile
        if (OrphanedEntries.ContainsKey(configFile))
            return OrphanedEntries[configFile];
        
        // Use reflection to get the orphaned entries and cache the result
        return OrphanedEntries[configFile] = (Dictionary<ConfigDefinition, string>) AccessConfigOrphanedEntries(configFile, []);
    }
}