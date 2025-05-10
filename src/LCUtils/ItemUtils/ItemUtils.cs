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

public static class ItemUtils
{
    public readonly static List<PersistentItemReference> AllItemRefs = [];
    public readonly static List<Item> AllItems = [];
    public static event UnityAction<PersistentItemReference>? NewItemFound;

    internal static void RegisterItem(Item? newItem)
    {
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

    internal static void RegisterAllItems()
    {
        Resources.FindObjectsOfTypeAll<Item>().Where(item => item != null).Do(RegisterItem);
    }

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

    public static Type? GetGrabbableObjectType(this Item? item)
    {
        return item.GetGrabbableObject()?.GetType() ?? null;
    }

    public static string GetGrabbableObjectName(this Item? item, string defaultIfEmpty = "")
    {
        string objName = item.GetGrabbableObject()?.name ?? "";
        return objName != "" ? objName : defaultIfEmpty;
    }

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

    public static string GetConfigName(this Item? item, string defaultIfEmpty = "")
    {
        if (item == null) return defaultIfEmpty;
        
        return $"{item.GetNodeText(defaultIfEmpty: item.itemName)} ({item.itemName} || {item.name} || {item.GetGrabbableObjectName(defaultIfEmpty: item.GetGrabbableObjectType()?.FullName ?? "null")})".Replace("\r", " ").Replace("\n", " ").Replace("\\", "/").Replace("\"", "|").Replace("\'", "|").Replace("[", "{").Replace("]", "}");
    }

    public static void RegisterItemHandler(Action<PersistentItemReference> itemHandler)
    {
        AllItemRefs.Do(itemHandler);
        NewItemFound += itemHandler.Invoke;
    }

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

[HarmonyPatch]
public static class ItemConstructorPatches
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> TargetMethods()
    {
        return AccessTools.GetDeclaredConstructors(typeof(Item), searchForStatic: false);
    }

    public static IEnumerator RegisterItemWithDelay(Item item)
    {
        yield return new WaitForSeconds(0.33f);

        if (item == null) yield break;

        ItemUtils.RegisterItem(item);
    }

    [HarmonyPriority(priority: int.MinValue)]
    [HarmonyPostfix]
    public static void ItemConstructor_Postfix(Item __instance)
    {
        Plugin.Instance.StartCoroutine(RegisterItemWithDelay(__instance));
    }
}