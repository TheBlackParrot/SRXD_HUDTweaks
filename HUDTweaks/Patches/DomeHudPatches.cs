using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HUDTweaks.Patches;

[HarmonyPatch]
internal static class DomeHudPatches
{
    private static Transform? _scoreText;
    private static CustomTextMeshPro? _scoreTextTMP;

    [HarmonyPatch(typeof(PlayingTrackGameState), nameof(PlayingTrackGameState.OnBecameActive))]
    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Init))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void ConfigUpdatePatches()
    {
        _ = Plugin.UpdateColors();
        _ = Plugin.UpdateHudElementsVisibility();
    }

    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Init))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void DomeHud_InitPatch(DomeHud __instance)
    {
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

    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.UpdateTranslatedElements))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static bool UpdateTranslatedElementsPatch(DomeHud __instance)
    {
        if (__instance._playState == null)
        {
            return true;
        }
        if (__instance._playState.IsLocalMultiplayer)
        {
            return true;
        }
        
        PlayableTrackData? trackData = __instance._playState.trackData;
        TrackInfoMetadata? trackInfoMetadata = trackData?.Setup.TrackDataSegments[__instance._currentTrackSectionIndex.GetValueOrDefault()].GetTrackInfoMetadata();
        if (trackData == null || trackInfoMetadata == null)
        {
            return true;
        }

        Dictionary<string, string> tags = new()
        {
            { "%title%", trackInfoMetadata.title },
            { "%artist%", trackInfoMetadata.artistName },
            { "%charter%", trackInfoMetadata.charter },
            { "%difficulty%", trackData.Difficulty.ToString() },
            { "%rating%", trackData.DifficultyRating.ToString() }
        };
        
        string formattedText = Plugin.TrackInfoText.Value;
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (KeyValuePair<string, string> tag in tags)
        {
            formattedText = formattedText.Replace(tag.Key, tag.Value);
        }

        __instance.trackTitleText.SetText(formattedText);
        
        return false;
    }

    /*[HarmonyPatch(typeof(DomeHud), nameof(DomeHud.LateUpdate))]
    [HarmonyPostfix]
    public static void LateUpdatePatch(DomeHud __instance)
    {
        //Plugin.Log.LogInfo(string.Join("\n", __instance.healthBar._spriteMesh.colorIndex));
        //Plugin.Log.LogInfo(__instance.healthBar._spriteMesh.colorIndex);
    }*/
}