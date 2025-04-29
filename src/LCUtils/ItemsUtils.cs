using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace LCUtils;

public static class ItemsUtils
{
    public static List<Item> AllItems {get; private set;} = [];
    public static event UnityAction<Item>? NewItemFound;

    public static void UpdateAllItems()
    {
        Resources.FindObjectsOfTypeAll<Item>().DoIf(
            newItem => newItem != null && !AllItems.Any(item => item.GetConfigName() == newItem.GetConfigName()), 
            newItem =>
            {
                Plugin.Log(LogLevel.Debug, $"Registering item: {newItem.GetConfigName()}");
                AllItems.Add(newItem);
                NewItemFound?.Invoke(newItem);
            }
        );
    }

    // Register event handler for SceneManager.activeSceneChanged
    static ItemsUtils()
    {
        SceneManager.activeSceneChanged += (oldScene, newScene) =>
        {
            UpdateAllItems();
        };
    }

    public static string GetNodeText(this Item item)
    {
        return item?.spawnPrefab?.GetComponentInChildren<ScanNodeProperties>()?.headerText ?? "";
    }

    public static string GetConfigName(this Item item)
    {
        return $"{item.name} ('{item.GetNodeText()}')";
    }

    public static void RegisterItemHandler(Action<Item> itemHandler)
    {
        AllItems.Do(itemHandler);
        NewItemFound += itemHandler.Invoke;
    }
}