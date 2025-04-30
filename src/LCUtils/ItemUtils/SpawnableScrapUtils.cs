using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Logging;
using HarmonyLib;
using ILUtils;
using ILUtils.HarmonyXtensions;

namespace LCUtils;
public static class SpawnableScrapUtils
{
    [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SpawnScrapInLevel))]
    [HarmonyPriority(priority: int.MinValue)]
    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    public static List<SpawnableItemWithRarity>? GetSpawnableScrap()
    {
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> methodIL, ILGenerator methodGenerator, MethodBase methodBase)
        {
            ILStepper stepper = new(methodIL, methodGenerator, methodBase);

            // SDM / scrap injection compat (find what list is being used as spawnable scrap)
            stepper.GotoIL((code, index) => code.StoresLocal(index: 15) && index > 0 && stepper.Instructions[index - 1].LoadsConstant(0), errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] For loop initialization (int j = 0;) not found");
            stepper.GotoIL(code => code.opcode.FlowControl == FlowControl.Branch, errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] For loop branch to control statement (j < this.currentLevel.spawnableScrap.Count;) not found");
            stepper.GotoIL(code => code.labels.Contains((Label)stepper.CurrentOperand!), errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] For loop control statement (j < this.currentLevel.spawnableScrap.Count;) not found");
            stepper.GotoIL(code => code.LoadsProperty(type: typeof(List<SpawnableItemWithRarity>), name: "Count"), errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] Load Property List.Count (this.currentLevel.spawnableScrap.Count) not found");

            List<CodeInstruction> SpawnableScrapIL = stepper.GetIL(
                startIndex: stepper.FindIL(ILPatterns.NextEmptyStack(startSize: -1), reverse: true,  errorMessage: "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrap] Spawnable Scrap List (this.currentLevel.spawnableScrap) not found)")
            )
            // Change "this" to RoundManager.Instance
            .Select(code => !code.IsLdarg(0) ? code : CodeInstructionPolyfills.LoadProperty(type: typeof(RoundManager), name: nameof(RoundManager.Instance)))
            .ToList();

            return SpawnableScrapIL;
        }

        _ = Transpiler(null!, null!, null!);
        return null;
    }

    public static List<SpawnableItemWithRarity> GetSpawnableScrapOrDefault()
    {
        List<SpawnableItemWithRarity>? spawnableScrap = GetSpawnableScrap();
        if (spawnableScrap != null && spawnableScrap.Count > 0) return spawnableScrap;

        Plugin.Log(LogLevel.Warning, "[ItemUtils.SpawnableScrapUtils.GetSpawnableScrapSafely] Spawnable Scrap List (this.currentLevel.spawnableScrap) not found from IL, or is empty. Trying direct instance access");
        return RoundManager.Instance?.currentLevel?.spawnableScrap ?? [];
    }
}