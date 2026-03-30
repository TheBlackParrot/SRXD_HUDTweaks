// keeping this around in case i need it, this did not fix the problem at hand

/*
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace HUDTweaks.Patches;

[HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Update))]
[HarmonyPriority(Priority.First)]
internal static class QrCodeTranspiler
{
    // we need to patch out the original <TextNumber>.desiredNumber update in DomeHud.Update to fix numbers "jittering" during sliders
    
    [SuppressMessage("ReSharper", "InvertIf")]
    [UsedImplicitly]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        int foundIndex = -1;
        
        List<CodeInstruction> codes = new(instructions);
#if DEBUG
        Plugin.Log.LogInfo("BEFORE:");
        foreach (CodeInstruction t in codes)
        {
            Plugin.Log.LogInfo($"<< {t.opcode}: {t.operand}");
        }
#endif
        
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString() == "Int32 get_TotalScore()")
            {
                if (codes[i + 1].opcode == OpCodes.Stfld && codes[i + 1].operand.ToString() == "System.Int32 desiredNumber")
                {
                    foundIndex = i + 1;
                    Plugin.Log.LogInfo("-- Found what we need to patch out");
                    break;
                }
            }
        }
        
        if (foundIndex > -1)
        {
            codes[foundIndex].opcode = OpCodes.Nop;
#if DEBUG
            Plugin.Log.LogInfo("AFTER:");
            foreach (CodeInstruction t in codes)
            {
                Plugin.Log.LogInfo($">> {t.opcode}: {t.operand}");
            }
#endif
        }
        
        return codes.AsEnumerable();
    }
}
*/