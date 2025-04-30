using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using LethalConfig;

namespace LCUtils;

public static class LethalConfigNicerizer
{
    internal const string LethalConfig_GUID = "ainavt.lc.lethalconfig";

	public static bool CanHasNicerizationPlease => Chainloader.PluginInfos.ContainsKey(LethalConfig_GUID);

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	public static ConfigEntry<T> Nicerize<T>(ConfigEntry<T> entry, bool restartRequired = false, Assembly? callingAssembly = null)
	{
		if (entry is ConfigEntry<int>) {
			if (CanHasNicerizationPlease) AddConfigItem((entry as ConfigEntry<int>)!, restartRequired, callingAssembly ?? Assembly.GetCallingAssembly());
		} else if (entry is ConfigEntry<float>) {
			if (CanHasNicerizationPlease) AddConfigItem((entry as ConfigEntry<float>)!, restartRequired, callingAssembly ?? Assembly.GetCallingAssembly());
		} else if (entry is ConfigEntry<bool>) {
			if (CanHasNicerizationPlease) AddConfigItem((entry as ConfigEntry<bool>)!, restartRequired, callingAssembly ?? Assembly.GetCallingAssembly());
		} else if (entry is ConfigEntry<string>) {
			if (CanHasNicerizationPlease) AddConfigItem((entry as ConfigEntry<string>)!, restartRequired, callingAssembly ?? Assembly.GetCallingAssembly());
		} else if (entry is ConfigEntry<Enum>) {
			if (CanHasNicerizationPlease) AddConfigItem((entry as ConfigEntry<Enum>)!, restartRequired, callingAssembly ?? Assembly.GetCallingAssembly());
		} else {
			throw new ArgumentException($"[LCUtils.LethalConfigNicerizer.Nicerize] Cannot Nicerize ConfigEntry<{typeof(T)}>!");
		}

		return entry;
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<int> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
		if(entry.Description.AcceptableValues != null) {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.IntSliderConfigItem(entry, restartRequired), callingAssembly);
		} 
		else {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.IntInputFieldConfigItem(entry, restartRequired), callingAssembly);
		}
    }

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<float> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
		if(entry.Description.AcceptableValues != null) {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.FloatSliderConfigItem(entry, restartRequired), callingAssembly);
		} 
		else {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.FloatInputFieldConfigItem(entry, restartRequired), callingAssembly);
		}
    }

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<bool> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
		LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.BoolCheckBoxConfigItem(entry, restartRequired), callingAssembly);
    }

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<string> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
		if(entry.Description.AcceptableValues != null) {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.TextDropDownConfigItem(entry, restartRequired), callingAssembly);
		}
		else
		{
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.TextInputFieldConfigItem(entry, restartRequired), callingAssembly);
		}
    }

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<Enum> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
		LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.EnumDropDownConfigItem<Enum>(entry, restartRequired), callingAssembly);
    }
}