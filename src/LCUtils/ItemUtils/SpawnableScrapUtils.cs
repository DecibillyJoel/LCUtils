using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ILUtils;
using ILUtils.HarmonyXtensions;
using UnityEngine.Events;

namespace LCUtils;

/// <summary>
/// Provides utilities for managing and accessing spawnable scrap items in the game.
/// Uses Harmony patching to intercept and track spawnable scrap lists during level initialization.
/// </summary>
[HarmonyPatch(typeof(RoundManager))]
public static class SpawnableScrapUtils
{
    /// <summary>
    /// Gets the current list of spawnable scrap items with their associated rarity values.
    /// This list is automatically updated when a new level is initialized.
    /// </summary>
    /// <value>A read-only list of spawnable items with rarity information.</value>
    public static List<SpawnableItemWithRarity> SpawnableScrapList {get; private set;} = [];

    /// <summary>
    /// Event triggered whenever the spawnable scrap list is updated.
    /// Subscribe to this event to be notified when new spawnable scrap data becomes available.
    /// </summary>
    public static event UnityAction? SpawnableScrapUpdated;

    /// <summary>
    /// Updates the internal spawnable scrap list and triggers the update event.
    /// This method is called automatically by the Harmony transpiler during level initialization.
    /// </summary>
    /// <param name="spawnableScrapList">The new list of spawnable scrap items to store.</param>
    private static void UpdateSpawnableScrap(List<SpawnableItemWithRarity> spawnableScrapList)
    {
        SpawnableScrapList = spawnableScrapList;
        SpawnableScrapUpdated?.Invoke();
    }

    /// <summary>
    /// Gets the total rarity value for the specified item based on the current spawnable scrap list.
    /// The rarity represents how commonly this item appears as scrap in the current level.
    /// </summary>
    /// <param name="item">The item to check rarity for. Can be null.</param>
    /// <returns>
    /// The sum of all rarity values for matching items in the spawnable scrap list.
    /// Returns 0 if the item is null, destroyed, or not found in the spawnable scrap list.
    /// Always returns a non-negative value.
    /// </returns>
    public static int GetRarity(this Item? item)
    {
        // Early return if item is null / destroyed
        if (item == null) return 0;

        return Math.Max(0, SpawnableScrapList.Sum(itemWithRarity => ((itemWithRarity != null) && item.LooselyEquals(itemWithRarity.spawnableItem)) ? itemWithRarity.rarity : 0));
    }

    /// <summary>
    /// Gets the total rarity value for the item referenced by the specified persistent item reference.
    /// This is a convenience method that extracts the item from the reference and calls GetRarity on it.
    /// </summary>
    /// <param name="itemRef">The persistent item reference to check rarity for. Can be null.</param>
    /// <returns>
    /// The sum of all rarity values for the referenced item in the spawnable scrap list.
    /// Returns 0 if the reference is null, the referenced item is null/destroyed, or not found in the spawnable scrap list.
    /// </returns>
    public static int GetRarity(this PersistentItemReference? itemRef)
    {
        // Early return if item is null / destroyed
        if (itemRef == null) return 0;

       return itemRef.Item.GetRarity();
    }

    /// <summary>
    /// Harmony transpiler that patches the RoundManager.SpawnScrapInLevel method to capture
    /// the spawnable scrap list being used during level initialization.
    ///
    /// This transpiler:
    /// 1. Locates the spawnable scrap list being used in the method (supports modded lists for SDM/scrap injection compatibility)
    /// 2. Inserts a call to UpdateSpawnableScrap at the beginning of the method to store the list
    /// 3. Preserves all original method functionality
    /// </summary>
    /// <param name="methodIL">The original IL instructions of the method.</param>
    /// <param name="methodGenerator">The IL generator for creating new instructions.</param>
    /// <param name="methodBase">The method being transpiled.</param>
    /// <returns>The modified IL instructions with the spawnable scrap capture logic added.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transpiler cannot locate the expected IL patterns in the target method.
    /// This typically indicates a game update has changed the method structure.
    /// </exception>
    [HarmonyPatch(nameof(RoundManager.SpawnScrapInLevel))]
    [HarmonyPriority(priority: int.MinValue)]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> SpawnScrapInLevel_Transpiler(IEnumerable<CodeInstruction> methodIL, ILGenerator methodGenerator, MethodBase methodBase)
    {
        ILStepper stepper = new(methodIL, methodGenerator, methodBase);

        // SDM / scrap injection compat (find what list is being used as spawnable scrap)
        stepper.GotoIL((code, index) => code.StoresLocal(index: 15) && index > 0 && stepper.Instructions[index - 1].LoadsConstant(0), errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] For loop initialization (int j = 0;) not found");
        stepper.GotoIL(code => code.opcode.FlowControl == FlowControl.Branch, errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] For loop branch to control statement (j < this.currentLevel.spawnableScrap.Count;) not found");
        stepper.GotoIL(code => code.labels.Contains((Label)stepper.CurrentOperand!), errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] For loop control statement (j < this.currentLevel.spawnableScrap.Count;) not found");
        stepper.GotoIL(code => code.LoadsProperty(type: typeof(List<SpawnableItemWithRarity>), name: "Count"), errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] Load Property List.Count (this.currentLevel.spawnableScrap.Count) not found");

        List<CodeInstruction> SpawnableScrapIL = stepper.GetIL(
            startIndex: stepper.FindIL(ILPatterns.NextEmptyStack(startSize: -1), reverse: true,  errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] Spawnable Scrap List (this.currentLevel.spawnableScrap) not found)")
        ).Select(code => code.Clone()).ToList();

        // Insert RememberSpawnableScrap method call at start of method
        stepper.GotoIndex(0);
        stepper.InsertIL(codeRange: [
            ..SpawnableScrapIL,
            CodeInstructionPolyfills.Call(type: typeof(SpawnableScrapUtils), name: nameof(UpdateSpawnableScrap)),
        ]);

        return stepper.Instructions;
    }
}