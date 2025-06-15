using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using LethalConfig;

namespace LCUtils;

/// <summary>
/// Provides utilities for easily integrating BepInEx configuration entries with LethalConfig UI.
/// LethalConfig creates nice in-game configuration menus for mod settings.
/// </summary>
public static class LethalConfigNicerizer
{
    /// <summary>
    /// The GUID identifier for the LethalConfig plugin.
    /// </summary>
    internal const string LethalConfig_GUID = "ainavt.lc.lethalconfig";

    /// <summary>
    /// Gets a value indicating whether LethalConfig is available and can be used to enhance configuration entries.
    /// </summary>
    /// <value>
    /// <c>true</c> if LethalConfig plugin is loaded; otherwise, <c>false</c>.
    /// </value>
	public static bool CanHasNicerizationPlease => Chainloader.PluginInfos.ContainsKey(LethalConfig_GUID);

    /// <summary>
    /// Enhances a configuration entry by adding it to LethalConfig's in-game UI if available.
    /// Automatically selects the appropriate UI control based on the entry type and acceptable values.
    /// </summary>
    /// <typeparam name="T">The type of the configuration entry value.</typeparam>
    /// <param name="entry">The configuration entry to enhance with LethalConfig UI.</param>
    /// <param name="restartRequired">Whether changing this setting requires a game restart to take effect.</param>
    /// <param name="callingAssembly">The assembly calling this method. If null, automatically detected.</param>
    /// <returns>The same configuration entry that was passed in (for method chaining).</returns>
    /// <exception cref="ArgumentException">Thrown when the configuration entry type is not supported.</exception>
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	public static ConfigEntry<T> Nicerize<T>(ConfigEntry<T> entry, bool restartRequired = false, Assembly? callingAssembly = null)
	{
        // Check each supported type and add appropriate UI control if LethalConfig is available
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

    /// <summary>
    /// Adds an integer configuration entry to LethalConfig.
    /// Creates a slider if the entry has acceptable value range, otherwise creates an input field.
    /// </summary>
    /// <param name="entry">The integer configuration entry to add.</param>
    /// <param name="restartRequired">Whether changing this setting requires a game restart.</param>
    /// <param name="callingAssembly">The assembly that owns this configuration entry.</param>
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<int> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
        // Use slider if acceptable values are defined (min/max range), otherwise use input field
		if(entry.Description.AcceptableValues != null) {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.IntSliderConfigItem(entry, restartRequired), callingAssembly);
		} 
		else {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.IntInputFieldConfigItem(entry, restartRequired), callingAssembly);
		}
    }

    /// <summary>
    /// Adds a floating-point configuration entry to LethalConfig.
    /// Creates a slider if the entry has acceptable value range, otherwise creates an input field.
    /// </summary>
    /// <param name="entry">The float configuration entry to add.</param>
    /// <param name="restartRequired">Whether changing this setting requires a game restart.</param>
    /// <param name="callingAssembly">The assembly that owns this configuration entry.</param>
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<float> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
        // Use slider if acceptable values are defined (min/max range), otherwise use input field
		if(entry.Description.AcceptableValues != null) {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.FloatSliderConfigItem(entry, restartRequired), callingAssembly);
		} 
		else {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.FloatInputFieldConfigItem(entry, restartRequired), callingAssembly);
		}
    }

    /// <summary>
    /// Adds a boolean configuration entry to LethalConfig as a checkbox control.
    /// </summary>
    /// <param name="entry">The boolean configuration entry to add.</param>
    /// <param name="restartRequired">Whether changing this setting requires a game restart.</param>
    /// <param name="callingAssembly">The assembly that owns this configuration entry.</param>
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<bool> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
        // Boolean entries always use checkbox controls
		LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.BoolCheckBoxConfigItem(entry, restartRequired), callingAssembly);
    }

    /// <summary>
    /// Adds a string configuration entry to LethalConfig.
    /// Creates a dropdown if the entry has acceptable values, otherwise creates a text input field.
    /// </summary>
    /// <param name="entry">The string configuration entry to add.</param>
    /// <param name="restartRequired">Whether changing this setting requires a game restart.</param>
    /// <param name="callingAssembly">The assembly that owns this configuration entry.</param>
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<string> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
        // Use dropdown if acceptable values are defined (predefined options), otherwise use text input
		if(entry.Description.AcceptableValues != null) {
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.TextDropDownConfigItem(entry, restartRequired), callingAssembly);
		}
		else
		{
			LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.TextInputFieldConfigItem(entry, restartRequired), callingAssembly);
		}
    }

    /// <summary>
    /// Adds an enumeration configuration entry to LethalConfig as a dropdown control.
    /// The dropdown will contain all possible enum values.
    /// </summary>
    /// <param name="entry">The enum configuration entry to add.</param>
    /// <param name="restartRequired">Whether changing this setting requires a game restart.</param>
    /// <param name="callingAssembly">The assembly that owns this configuration entry.</param>
	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddConfigItem(ConfigEntry<Enum> entry, bool restartRequired = false, Assembly? callingAssembly = null)
    {
        // Enum entries always use dropdown controls with all enum values
		LethalConfigManager.AddConfigItem(new LethalConfig.ConfigItems.EnumDropDownConfigItem<Enum>(entry, restartRequired), callingAssembly);
    }
}