using BepInEx;
using BepInEx.Logging;

using LogLevel = BepInEx.Logging.LogLevel;

namespace LCUtils;

[BepInPlugin(BepPluginInfo.PLUGIN_GUID, $"{BepPluginInfo.PLUGIN_TS_TEAM}.{BepPluginInfo.PLUGIN_NAME}", BepPluginInfo.PLUGIN_VERSION)]

public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = BepPluginInfo.PLUGIN_GUID;
    public const string PLUGIN_NAME = BepPluginInfo.PLUGIN_NAME;
    public const string PLUGIN_VERSION = BepPluginInfo.PLUGIN_VERSION;
    public const string PLUGIN_TS_TEAM = BepPluginInfo.PLUGIN_TS_TEAM;

    public static ManualLogSource PluginLogger = null!;

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
        PluginLogger = Logger;
        Log($"[v{PLUGIN_VERSION}] Finished loading!");
    }
}