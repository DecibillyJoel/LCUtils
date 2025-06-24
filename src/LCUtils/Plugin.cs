﻿using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LCUtils;

/*
  Here are some basic resources on code style and naming 
  conventions to help you in your first CSharp plugin!

  https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
  https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names
  https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces
*/

[BepInPlugin(LCMProjectInfo.PROJECT_GUID, LCMProjectInfo.PROJECT_NAME, LCMProjectInfo.PROJECT_VERSION)]
[BepInDependency(LethalConfigNicerizer.LethalConfig_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
  #region Plugin Info
    /*
      Here, we make the plugin instance and info accessible anywhere.
      The GUID, name, etc. are inherited from a static class that is
      auto generated using properties set in Directory.Build.props
    */
    
    public static Plugin Instance { get; private set; } = null!;
    public const string PLUGIN_GUID = LCMProjectInfo.PROJECT_GUID;
    public const string PLUGIN_NAME = LCMProjectInfo.PROJECT_NAME;
    public const string PLUGIN_AUTHORS = LCMProjectInfo.PROJECT_AUTHORS;
    public const string PLUGIN_VERSION = LCMProjectInfo.PROJECT_VERSION;
  #endregion

  #region Log Methods
    /* 
      BepInEx makes you a ManualLogSource for free called "Logger"
      that is accessed via the BaseUnityPlugin instance. Your plugin's
      code can find it by using Plugin.Instance.Logger.

      For convenience, we define static logging functions here so that
      the logger's functions can be called via Plugin.LogInfo(...),
      Plugin.LogDebug(...), Plugin.Log(...), etc.
    */
  
    public static void Log(LogLevel level, object data) => Instance.Logger.Log(level, data);
    public static void LogFatal(object data) => Instance.Logger.LogFatal(data);
    public static void LogError(object data) => Instance.Logger.LogError(data);
    public static void LogWarning(object data) => Instance.Logger.LogWarning(data);
    public static void LogMessage(object data) => Instance.Logger.LogMessage(data);
    public static void LogInfo(object data) => Instance.Logger.LogInfo(data);
    public static void LogDebug(object data) => Instance.Logger.LogDebug(data);
  #endregion

  internal static readonly Harmony harmony = new($"{PLUGIN_AUTHORS}.{PLUGIN_NAME}");

  private void Awake()
  {
    Instance = this;

    LogInfo($"[v{PLUGIN_VERSION}] Loading...");

    try
    {
      LogDebug("Patching...");
      harmony.PatchAll();
    }
    catch (Exception e)
    {
      LogError($"Patching failed! Unpatching! Exception:\n{e}");

      harmony.UnpatchSelf();
    }
    
    LogInfo($"[v{PLUGIN_VERSION}] Finished loading!");
  }

}
