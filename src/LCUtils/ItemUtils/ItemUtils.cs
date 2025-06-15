using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;

namespace LCUtils;

/// <summary>
/// Utility class for managing and working with Item objects in the game.
/// Provides centralized registration, tracking, and utility methods for Item instances.
/// </summary>
public static class ItemUtils
{
    /// <summary>
    /// Collection of all registered item references.
    /// </summary>
    public readonly static List<PersistentItemReference> AllItemRefs = [];

    /// <summary>
    /// Collection of all registered Item instances.
    /// </summary>
    public readonly static List<Item> AllItems = [];

    /// <summary>
    /// Event triggered when a new item reference is discovered and registered.
    /// </summary>
    public static event UnityAction<PersistentItemReference>? NewItemFound;

    /// <summary>
    /// Registers a new Item instance and its persistent reference if valid.
    /// </summary>
    /// <param name="newItem">The Item instance to register.</param>
    internal static void RegisterItem(Item? newItem)
    {
        // Skip if item is null / destroyed
        if (newItem == null) return;

        // Add item to AllItems
        AllItems.Add(newItem);

        // Skip if item reference cannot be made
        PersistentItemReference? newItemReference = newItem.GetPersistentReference();
        if (newItemReference == null) return;
        
        // Skip if item reference already exists
        if (AllItemRefs.Contains(newItemReference)) return;
        
        // Register item reference \\

        Plugin.Log(LogLevel.Debug, $"Registering item: {newItemReference.configName}");

        AllItemRefs.Add(newItemReference);
        NewItemFound?.Invoke(newItemReference);
    }

    /// <summary>
    /// Finds and registers all Item objects currently loaded in the game.
    /// </summary>
    internal static void RegisterAllItems()
    {
        Resources.FindObjectsOfTypeAll<Item>().Where(item => item != null).Do(RegisterItem);
    }

    /// <summary>
    /// Gets the GrabbableObject component from the item's spawn prefab.
    /// </summary>
    /// <param name="item">The Item to get the GrabbableObject from.</param>
    /// <returns>The GrabbableObject component, or null if not found.</returns>
    public static GrabbableObject? GetGrabbableObject(this Item? item)
    {
        // Early return if item or spawn prefab are null
        if (item == null) return null;
        if (item.spawnPrefab == null) return null;

        // Early return if cannot find grabbable object
        GrabbableObject? grabbableObject = item.spawnPrefab.GetComponent<GrabbableObject>();
        if (grabbableObject == null) return null;

        return grabbableObject;
    }

    /// <summary>
    /// Gets the Type of the GrabbableObject component from the item's spawn prefab.
    /// </summary>
    /// <param name="item">The Item to get the GrabbableObject type from.</param>
    /// <returns>The Type of the GrabbableObject, or null if not found.</returns>
    public static Type? GetGrabbableObjectType(this Item? item)
    {
        return item.GetGrabbableObject()?.GetType() ?? null;
    }

    /// <summary>
    /// Gets the name of the GrabbableObject from the item's spawn prefab.
    /// </summary>
    /// <param name="item">The Item to get the GrabbableObject name from.</param>
    /// <param name="defaultIfEmpty">Default value to return if the name is empty or null.</param>
    /// <returns>The name of the GrabbableObject, or the default value if not found.</returns>
    public static string GetGrabbableObjectName(this Item? item, string defaultIfEmpty = "")
    {
        string objName = item.GetGrabbableObject()?.name ?? "";
        return objName != "" ? objName : defaultIfEmpty;
    }

    /// <summary>
    /// Gets the header text from the item's scan node properties.
    /// </summary>
    /// <param name="item">The Item to get the node text from.</param>
    /// <param name="defaultIfEmpty">Default value to return if the text is empty or null.</param>
    /// <returns>The scan node header text, or the default value if not found.</returns>
    public static string GetNodeText(this Item? item, string defaultIfEmpty = "")
    {
        // Early return if item or spawn prefab are null
        if (item == null) return defaultIfEmpty;
        if (item.spawnPrefab == null) return defaultIfEmpty;

        // Early return if cannot find scan node properties
        ScanNodeProperties? nodeProps = item.spawnPrefab.GetComponentInChildren<ScanNodeProperties>();
        if (nodeProps == null) return defaultIfEmpty;

        // Early return if header text is empty
        string headerText = nodeProps.headerText;
        if (headerText.Length == 0) return defaultIfEmpty;

        return headerText;
    }

    /// <summary>
    /// Generates a configuration-safe name for the item using various fallbacks.
    /// The name is sanitized to be safe for use in configuration files.
    /// </summary>
    /// <param name="item">The Item to generate a config name for.</param>
    /// <param name="defaultIfEmpty">Default value to return if no name can be determined.</param>
    /// <returns>A sanitized configuration name for the item.</returns>
    public static string GetConfigName(this Item? item, string defaultIfEmpty = "")
    {
        if (item == null) return defaultIfEmpty;
        
        string nodeText = item.GetNodeText(defaultIfEmpty: item.itemName);
        string grabbableObjectName = item.GetGrabbableObjectName(defaultIfEmpty: item.GetGrabbableObjectType()?.FullName ?? "null");

        return $"{nodeText} ({item.itemName} || {item.name} || {grabbableObjectName})"
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace("\\", "/")
            .Replace("\"", "|")
            .Replace("\'", "|")
            .Replace("[", "{")
            .Replace("]", "}");
    }

    /// <summary>
    /// Registers a handler to be called for all existing items and any newly discovered items.
    /// </summary>
    /// <param name="itemHandler">The action to execute for each item reference.</param>
    public static void RegisterItemHandler(Action<PersistentItemReference> itemHandler)
    {
        AllItemRefs.Do(itemHandler);
        NewItemFound += itemHandler.Invoke;
    }

    /// <summary>
    /// Compares two Item instances for loose equality based on ID, config name, and grabbable object type.
    /// This is more flexible than reference equality and accounts for items that may be different instances
    /// but represent the same logical item.
    /// </summary>
    /// <param name="item">The first Item to compare.</param>
    /// <param name="otherItem">The second Item to compare.</param>
    /// <returns>True if the items are loosely equal, false otherwise.</returns>
    public static bool LooselyEquals(this Item? item, Item? otherItem)
    {
        // Check if the references point to the same value
        if (ReferenceEquals(item, otherItem)) return true;

        // If one item is null, they are equal if the other is also null
        if (item == null) return otherItem == null;
        if (otherItem == null) return false;

        // Disqualify if properties are different
        if (item.itemId != otherItem.itemId) return false;
        if (item.GetConfigName() != otherItem.GetConfigName()) return false;
        if (item.GetGrabbableObjectType() != otherItem.GetGrabbableObjectType()) return false;

        return true;
    }
}

/// <summary>
/// Harmony patches for intercepting Item constructor calls to automatically register new items.
/// Uses a delayed registration approach to ensure items are fully initialized before processing.
/// </summary>
[HarmonyPatch]
public static class ItemConstructorPatches
{
    /// <summary>
    /// Identifies all constructor methods of the Item class for patching.
    /// </summary>
    /// <returns>An enumerable of MethodBase objects representing Item constructors.</returns>
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> TargetMethods()
    {
        return AccessTools.GetDeclaredConstructors(typeof(Item), searchForStatic: false);
    }

    /// <summary>
    /// Coroutine that waits before registering an item to ensure it's fully initialized.
    /// </summary>
    /// <param name="item">The Item instance to register after the delay.</param>
    /// <returns>IEnumerator for coroutine execution.</returns>
    public static IEnumerator RegisterItemWithDelay(Item item)
    {
        yield return new WaitForSeconds(0.33f);

        if (item == null) yield break;

        ItemUtils.RegisterItem(item);
    }

    /// <summary>
    /// Postfix patch that runs after Item constructors to start delayed registration.
    /// Uses minimum priority to ensure it runs after all other patches.
    /// </summary>
    /// <param name="__instance">The newly constructed Item instance.</param>
    [HarmonyPriority(priority: int.MinValue)]
    [HarmonyPostfix]
    public static void ItemConstructor_Postfix(Item __instance)
    {
        Plugin.Instance.StartCoroutine(RegisterItemWithDelay(__instance));
    }

    /// <summary>
    /// Called when all patching operations are complete to ensure no items were missed.
    /// Performs a final sweep to register any items that may have been skipped.
    /// </summary>
    /// <param name="orig">The original method being patched. Null indicates patching is complete.</param>
    [HarmonyCleanup]
    public static void PatchingDone(MethodBase? orig)
    {
        // If orig is not null, it's not actually done
        if (orig != null) return;

        // Ensure no items skip registration
        ItemUtils.RegisterAllItems();
    }
}