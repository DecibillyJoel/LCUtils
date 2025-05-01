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

        // StartOfRound.Instance.allItemsList.itemsList.DoIf(
        Resources.FindObjectsOfTypeAll<Item>().DoIf(
            newItem => newItem != null && !AllItems.Any(newItem.LooselyEquals), 
            newItem =>
            {
                Plugin.Log(LogLevel.Debug, $"Registering item: {newItem.GetConfigName()}");
                AllItems.Add(newItem);
                NewItemFound?.Invoke(newItem);
            }
        );
    }

    // Register event handler for updating all items
    static ItemUtils()
    {
        SceneManager.activeSceneChanged += (oldScene, newScene) =>
        {
            UpdateAllItems();
        };
    }

    public static string GetNodeText(this Item? item, bool returnNameIfEmpty = true)
    {
        string headerText = item?.spawnPrefab?.GetComponentInChildren<ScanNodeProperties>()?.headerText ?? "";
        return headerText != "" ? headerText : (returnNameIfEmpty ? item?.itemName ?? "" : "");
    }

    public static string GetConfigName(this Item? item)
    {
        if (item == null) return "";
        
        return $"{item.GetNodeText()} ({item.itemName} || {item.name})".Replace("\r", " ").Replace("\n", " ").Replace("\\", "/").Replace("\"", "|").Replace("\'", "|").Replace("[", "{").Replace("]", "}");
    }

    public static bool LooselyEquals(this Item? item, Item? otherItem)
    {
        // Check if items are the same reference
        if (item == otherItem) return true;

        // If either is null at this point, they cannot be equal
        if (item == null || otherItem == null) return false;

        // Disqualify if names or item IDs are different
        if (item.name != otherItem.name) return false;
        if (item.itemName != otherItem.itemName) return false;
        if (item.itemId != otherItem.itemId) return false;

        // If they share the same spawnPrefab at this point, they are equal
        return item.spawnPrefab == otherItem.spawnPrefab;
    }

    public static void RegisterItemHandler(Action<Item> itemHandler)
    {
        AllItems.Do(itemHandler);
        NewItemFound += itemHandler.Invoke;
    }
}