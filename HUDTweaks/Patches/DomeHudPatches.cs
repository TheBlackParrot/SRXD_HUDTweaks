using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HUDTweaks.Patches;

[HarmonyPatch]
internal static class DomeHudPatches
{
    private static Transform? _scoreText;
    private static CustomTextMeshPro? _scoreTextTMP;
    
    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Init))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void InitPatch(DomeHud __instance)
    {
        _ = Plugin.UpdateColors();
        
        if (_scoreText != null)
        {
            return;
        }
        
        _scoreText = __instance.number.gameObject.transform.parent.parent.Find("ScoreText");
        _scoreTextTMP = _scoreText.GetComponent<CustomTextMeshPro>();
            
        if (_scoreText.TryGetComponent(out TranslatedTextMeshPro translatedTextMeshPro))
        {
            Object.DestroyImmediate(translatedTextMeshPro);
        }
    }

    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Update))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void UpdatePatch(DomeHud __instance)
    {
        if (_scoreTextTMP == null)
        {
            return;
        }

        ScoreState scoreState = __instance.PlayState.scoreState;
        float accuracy = (scoreState.TotalScore / (float)((scoreState.CurrentTotals.baseScore + scoreState.CurrentTotals.baseScoreLost) * 4)) * 100;

        string accString = $"{(float.IsNaN(accuracy) ? 100 : accuracy):0.00}%";
        string perfectPlusString = Plugin.EnablePerfectPlusCount.Value
            ? $" <alpha=#80>({scoreState.CurrentTotals.flawlessPlusCount})"
            : "";

        _scoreTextTMP.text = $"{accString}{perfectPlusString}";
    }

    /*[HarmonyPatch(typeof(DomeHud), nameof(DomeHud.LateUpdate))]
    [HarmonyPostfix]
    public static void LateUpdatePatch(DomeHud __instance)
    {
        //Plugin.Log.LogInfo(string.Join("\n", __instance.healthBar._spriteMesh.colorIndex));
        //Plugin.Log.LogInfo(__instance.healthBar._spriteMesh.colorIndex);
    }*/
}