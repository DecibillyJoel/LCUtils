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

    public static void UpdateAllItems()
    {
        Resources.FindObjectsOfTypeAll<Item>().DoIf(
            newItem => newItem != null && !AllItems.Any(item => item.GetConfigName() == newItem.GetConfigName()), 
            newItem =>
            {
                Plugin.Log(LogLevel.Debug, $"Registering item: {newItem.GetConfigName()} (Vanilla: {newItem.IsVanillaItem()}) (Assembly FullName: {newItem.spawnPrefab?.GetType()?.Assembly?.FullName}) (Assembly Name: {newItem.spawnPrefab?.GetType()?.Assembly?.GetName()?.Name}) (Assembly Version: {newItem.spawnPrefab?.GetType()?.Assembly?.GetName()?.Version}) (Assembly Culture: {newItem.spawnPrefab?.GetType()?.Assembly?.GetName()?.CultureInfo}) (Assembly PublicKeyToken: {newItem.spawnPrefab?.GetType()?.Assembly?.GetName()?.GetPublicKeyToken()})");
                AllItems.Add(newItem);
                NewItemFound?.Invoke(newItem);
            }
        );
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
        
        return $"{item.name} ('{item.GetNodeText()}')";
    }

    public static bool Equals(this Item? item, Item? otherItem)
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