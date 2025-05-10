using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LogLevel = BepInEx.Logging.LogLevel;

namespace LCUtils;

[BepInPlugin(BepPluginInfo.PLUGIN_GUID, $"{BepPluginInfo.PLUGIN_TS_TEAM}.{BepPluginInfo.PLUGIN_NAME}", BepPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalConfigNicerizer.LethalConfig_GUID, BepInDependency.DependencyFlags.SoftDependency)]

public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = BepPluginInfo.PLUGIN_GUID;
    public const string PLUGIN_NAME = BepPluginInfo.PLUGIN_NAME;
    public const string PLUGIN_VERSION = BepPluginInfo.PLUGIN_VERSION;
    public const string PLUGIN_TS_TEAM = BepPluginInfo.PLUGIN_TS_TEAM;

    public static Plugin Instance {get; private set;} = null!;
    public static ManualLogSource PluginLogger = null!;
    internal static readonly Harmony harmony = new($"{BepPluginInfo.PLUGIN_TS_TEAM}.{BepPluginInfo.PLUGIN_NAME}");

    public static void Log(LogLevel logLevel, string logMessage)
    {
        PluginLogger.Log(logLevel, $"{logMessage}");
    }

    public static void Log(string logMessage)
    {
        Log(LogLevel.Info, logMessage);
    }

    private void Awake()
    {
        Instance = this;
        PluginLogger = Logger;
        
        Log($"[v{PLUGIN_VERSION}] Finished loading!");

        Log(LogLevel.Debug, "Patching...");
        try{
            harmony.PatchAll();
        } catch (Exception e) {
            Log(LogLevel.Error, $"Patching failed! Unpatching! Exception:\n{e}");
            
            harmony.UnpatchSelf();
        }
    }
}