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
    private static Transform? _healthText;
    private static CustomTextMeshPro? _healthTextTMP;

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
            
        if (_scoreText.TryGetComponent(out TranslatedTextMeshPro scoreTranslatedTextMeshPro))
        {
            Object.DestroyImmediate(scoreTranslatedTextMeshPro);
        }

        _healthText = __instance.healthBar.transform.parent.Find("HealthText");
        _healthTextTMP = _healthText.GetComponent<CustomTextMeshPro>();
            
        if (_healthText.TryGetComponent(out TranslatedTextMeshPro healthTranslatedTextMeshPro))
        {
            Object.DestroyImmediate(healthTranslatedTextMeshPro);
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
        
        if (_healthTextTMP == null)
        {
            return;
        }

        _healthTextTMP.text = __instance.PlayState.health.ToString().PadLeft(3, '0');
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

        int trackDuration = trackData.GameplayEndTick.ToSecondsInt().Max(0) + 1;
        Dictionary<string, string> tags = new()
        {
            { "%title%", trackInfoMetadata.title },
            { "%artist%", trackInfoMetadata.artistName },
            { "%charter%", trackInfoMetadata.charter },
            { "%difficulty%", trackData.Difficulty.ToString() },
            { "%rating%", trackData.DifficultyRating.ToString() },
            { "%duration%", $"{(trackDuration / 60d).FloorToInt()}:{(trackDuration % 60).ToString().PadLeft(2, '0')}" }
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
    
    [HarmonyPatch(typeof(DomeHudTrackTimeBar), nameof(DomeHudTrackTimeBar.LateUpdate))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void LateUpdatePatch(DomeHudTrackTimeBar __instance)
    {
        if (!Plugin.ShowTimeInBeats.Value)
        {
            return;
        }
        
        PlayState? playState = __instance._playState;
        PlayableTrackData? trackData = playState?.trackData;
        if (playState == null || trackData == null)
        {
            return;
        }
        
        double currentBeat = trackData.GetBeatAtTime(playState.currentTrackTime).AsDouble;
        __instance.trackTimePassedText.IntParam1 = currentBeat.FloorToInt();
        int fraction = ((currentBeat % 1) * 100).FloorToInt();
        __instance.trackTimePassedText.IntParam2 = (fraction - (fraction % 50)) / 50;

        if (playState.SetupParameters.playType == PlayType.Endless)
        {
            return;
        }
        
        // yeah this updates every frame too, buh
        double finalBeat = trackData.GetBeatAtTime(trackData.GameplayEndTick.ToSecondsInt().Max(0) + 1).AsDouble;
        __instance.trackLengthText.IntParam1 = finalBeat.FloorToInt();
        int lengthFraction = ((finalBeat % 1) * 100).FloorToInt();
        __instance.trackLengthText.IntParam2 = (lengthFraction - (lengthFraction % 50)) / 50;
    }

    /*[HarmonyPatch(typeof(DomeHud), nameof(DomeHud.LateUpdate))]
    [HarmonyPostfix]
    public static void LateUpdatePatch(DomeHud __instance)
    {
        //Plugin.Log.LogInfo(string.Join("\n", __instance.healthBar._spriteMesh.colorIndex));
        //Plugin.Log.LogInfo(__instance.healthBar._spriteMesh.colorIndex);
    }*/
}