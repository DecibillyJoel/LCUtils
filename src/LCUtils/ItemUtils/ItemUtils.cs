using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace LCUtils;

public static class ItemUtils
{
    public static List<Item> AllItems {get; private set;} = [];
    public static event UnityAction<Item>? NewItemFound;

    public static bool UpdateAllItems()
    {
        var foundAny = false;

        // StartOfRound.Instance.allItemsList.itemsList.DoIf(
        Resources.FindObjectsOfTypeAll<Item>().DoIf(
            newItem => newItem != null && !AllItems.Any(item => item.GetConfigName() == newItem.GetConfigName()), 
            newItem =>
            {
                foundAny = true;

                Plugin.Log(LogLevel.Debug, $"Registering item: {newItem.GetConfigName()} (Vanilla: {newItem.IsVanillaItem()}) (Assembly FullName: {newItem.spawnPrefab?.GetComponent<GrabbableObject>()?.GetType()?.Assembly?.FullName}) (Assembly Name: {newItem.spawnPrefab?.GetComponent<GrabbableObject>()?.GetType()?.Assembly?.GetName()?.Name}) (Assembly Version: {newItem.spawnPrefab?.GetComponent<GrabbableObject>()?.GetType()?.Assembly?.GetName()?.Version}) (Assembly Culture: {newItem.spawnPrefab?.GetComponent<GrabbableObject>()?.GetType()?.Assembly?.GetName()?.CultureInfo}) (Assembly PublicKeyToken: {newItem.spawnPrefab?.GetComponent<GrabbableObject>()?.GetType()?.Assembly?.GetName()?.GetPublicKeyToken()})");
                AllItems.Add(newItem);
                NewItemFound?.Invoke(newItem);
            }
        );

        return foundAny;
    }

    // Register event handler for SceneManager.activeSceneChanged
    static ItemUtils()
    {
        SceneManager.activeSceneChanged += (oldScene, newScene) =>
        {
            UpdateAllItems();
        };
    }

    public static string GetNodeText(this Item? item)
    {
        return item?.spawnPrefab?.GetComponentInChildren<ScanNodeProperties>()?.headerText ?? "";
    }

    public static string GetConfigName(this Item? item)
    {
        if (item == null) return "";
        
        return $"{item.name} ({item.GetNodeText()})".Replace("\r", " ").Replace("\n", " ").Replace("\\", "/").Replace("\"", "|").Replace("\'", "|").Replace("[", "{").Replace("]", "}");
    }

    // Item.LooselyEquals(OtherItem);
    public static bool LooselyEquals(this Item? item, Item? otherItem)
    {
        // Check if items are the same reference
        if (item == otherItem) return true;

        // If either is null at this point, they cannot be equal
        if (item == null || otherItem == null) return false;

        // Disqualify if names or item IDs are different
        if (item.name != otherItem.name) return false;
        if (item.itemId != otherItem.itemId) return false;

        // If they share the same spawnPrefab at this point, they are equal
        return item.spawnPrefab == otherItem.spawnPrefab;
    }

    public static bool IsVanillaItem(this Item item)
    {
        return item?.spawnPrefab?.GetComponent<GrabbableObject>()?.GetType()?.Assembly?.GetName()?.Name == "Assembly-CSharp";
    }

    public static void RegisterItemHandler(Action<Item> itemHandler)
    {
        AllItems.Do(itemHandler);
        NewItemFound += itemHandler.Invoke;
    }
}