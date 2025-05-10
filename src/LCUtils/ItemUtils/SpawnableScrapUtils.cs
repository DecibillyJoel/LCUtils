using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ILUtils;
using ILUtils.HarmonyXtensions;
using UnityEngine.Events;

namespace LCUtils;

[HarmonyPatch(typeof(RoundManager))]
public static class SpawnableScrapUtils
{
    public static Dictionary<PersistentItemReference, int> SpawnableScrap { get; private set; } = [];
    public static event UnityAction? SpawnableScrapUpdated;

    private static void UpdateSpawnableScrap(List<SpawnableItemWithRarity> spawnableScrapList)
    {
        SpawnableScrap.Clear();
        spawnableScrapList.Do(newSpawnableItem => {
            PersistentItemReference? itemRef = newSpawnableItem.spawnableItem?.GetPersistentReference();
            if (itemRef == null) return;

            SpawnableScrap.Add(itemRef, newSpawnableItem.rarity);
        });
        SpawnableScrapUpdated?.Invoke();
    }

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