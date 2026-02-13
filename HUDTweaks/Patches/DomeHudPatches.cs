using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using HUDTweaks.Classes;
using TMPro;
using UnityEngine;

namespace HUDTweaks.Patches;

[HarmonyPatch]
internal static class DomeHudPatches
{
    private static Transform? _scoreText;
    private static CustomTextMeshPro? _scoreTextTMP;
    internal static MeshRenderer? ScoreTextMeshRenderer;
    
    private static Transform? _healthText;
    private static CustomTextMeshPro? _healthTextTMP;
    internal static MeshRenderer? HealthTextMeshRenderer;
    
    private static Transform? _comboText;
    internal static MeshRenderer? ComboTextMeshRenderer;
    
    private static Transform? _infoText;
    internal static MeshRenderer? InfoTextMeshRenderer;

    private static readonly List<Transform> TimeElements = [];
    internal static List<MeshRenderer> TimeElementMeshRenderers = [];

    private static readonly List<Transform> FcElements = [];
    internal static readonly List<MeshRenderer> FcElementMeshRenderers = [];
    
    private static readonly List<Transform> MultiplierElements = [];
    private static readonly List<MeshRenderer> MultiplierElementMeshRenderers = [];
    
    private static ColorPalette? _palette;
    private static readonly int FaceColor = Shader.PropertyToID("_FaceColor");

    internal static async Task ResetTranslatedTexts()
    {
        if (_scoreText?.TryGetComponent(out TranslatedTextMeshPro scoreTranslatedTextMeshPro) ?? false)
        {
            scoreTranslatedTextMeshPro.Translation = "";
            await Awaitable.EndOfFrameAsync();
            scoreTranslatedTextMeshPro.Translation = "Score";
        }

        // ReSharper disable once InvertIf
        if (_healthText?.TryGetComponent(out TranslatedTextMeshPro healthTranslatedTextMeshPro) ?? false)
        {
            healthTranslatedTextMeshPro.Translation = "";
            await Awaitable.EndOfFrameAsync();
            healthTranslatedTextMeshPro.Translation = "Health";
        }
    }

    [HarmonyPatch(typeof(PlayingTrackGameState), nameof(PlayingTrackGameState.OnBecameActive))]
    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Init))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void OnBecameActive_Patch()
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
            // if it's set, we gathered everything already
            return;
        }
        
        _scoreText = __instance.number.gameObject.transform.parent.parent.Find("ScoreText");
        _scoreTextTMP = _scoreText.GetComponent<CustomTextMeshPro>();
        ScoreTextMeshRenderer = _scoreText.GetComponent<MeshRenderer>();
        _scoreText.GetComponent<HdrMeshEffect>().Palette = Plugin.WhitePalette;
        
        _healthText = __instance.healthBar.transform.parent.Find("HealthText");
        _healthTextTMP = _healthText.GetComponent<CustomTextMeshPro>();
        HealthTextMeshRenderer = _healthText.GetComponent<MeshRenderer>();
        _healthText.GetComponent<HdrMeshEffect>().Palette = Plugin.WhitePalette;
        
        _comboText = __instance.streak.transform.parent.parent.Find("StreakText");
        ComboTextMeshRenderer = _comboText.GetComponent<MeshRenderer>();
        _comboText.GetComponent<HdrMeshEffect>().Palette = Plugin.WhitePalette;

        _infoText = __instance.trackTitleText.transform;
        InfoTextMeshRenderer = _infoText.GetComponent<MeshRenderer>();
        _infoText.GetComponent<HdrMeshEffect>().Palette = Plugin.WhitePalette;

        Transform timeContainer = __instance.wheelWarpTransform.Find("HudWheelRect/Time Bar Container");
        TimeElements.Add(timeContainer.Find("TrackTimePassedText"));
        TimeElements.Add(timeContainer.Find("TrackLengthText"));
        Transform timeBarContainer = timeContainer.Find("Time Bar Fill");
        for (int i = 0; i < timeBarContainer.childCount; i++)
        {
            TimeElements.Add(timeBarContainer.GetChild(i));   
        }
        TimeElements.Add(timeContainer.Find("Time Bar Fill Scaled"));

        TimeElementMeshRenderers = TimeElements.Select(x => x.GetComponent<MeshRenderer>()).ToList();
        foreach (Transform transform in TimeElements)
        {
            if (transform.TryGetComponent(out HdrMeshEffect hdrMeshEffect))
            {
                hdrMeshEffect.Palette = Plugin.WhitePalette;
            }
        }
        
        FcElements.Add(__instance.fcTexts[0].transform.parent.Find("FcStar"));
        FcElements.Add(__instance.fcTexts[0].transform.parent.Find("FcStarOutine")); // yep, outine
        foreach (TMP_Text fcText in __instance.fcTexts)
        {
            FcElements.Add(fcText.transform);
        }
        foreach (Transform transform in FcElements)
        {
            FcElementMeshRenderers.Add(transform.GetComponent<MeshRenderer>());
        }
        
        MultiplierElements.Add(__instance.multiplierBar.transform);
        for(int i = 0; i < __instance.multiplier.transform.parent.childCount; i++)
        {
            MultiplierElements.Add(__instance.multiplier.transform.parent.GetChild(i));
        }
        
        foreach (Transform transform in MultiplierElements)
        {
            MeshRenderer? meshRenderer = transform.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                continue;
            }
            
            MultiplierElementMeshRenderers.Add(meshRenderer);

            if (transform.TryGetComponent(out HdrMeshEffect hdrMeshEffect))
            {
                hdrMeshEffect.Palette = Plugin.WhitePalette;
            }
            else if (transform.TryGetComponent(out SpriteMesh spriteMesh))
            {
                spriteMesh.Palette = Plugin.WhitePalette;
            }
        }
        
        if (_palette == null)
        {
            _palette = Resources.FindObjectsOfTypeAll<ColorPalette>().First(x => x.name == "PaletteHUD");
        }
        // whatever palette this is (how it determines it is. beyond me), it's only white. i think. so that works out.
        _palette.GlobalPaletteIndex = 1;
    }

    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Update))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void UpdatePatch(DomeHud __instance)
    {
        if (_scoreTextTMP != null && Plugin.EnableAccuracyDisplay.Value)
        {
            ScoreState scoreState = __instance.PlayState.scoreState;
            float accuracy = (scoreState.TotalScore / (float)((scoreState.CurrentTotals.baseScore + scoreState.CurrentTotals.baseScoreLost) * 4)) * 100;

            string accString = $"{(float.IsNaN(accuracy) ? 100 : accuracy):0.00}%";
            string perfectPlusString = Plugin.EnablePerfectPlusCount.Value
                ? $" <alpha=#80>({scoreState.CurrentTotals.flawlessPlusCount})"
                : "";

            _scoreTextTMP.text = $"{accString}{perfectPlusString}";
        }
        
        if (_healthTextTMP != null && Plugin.EnablePreciseHealth.Value)
        {
            _healthTextTMP.text = __instance.PlayState.health.ToString().PadLeft(3, '0');
        }

        FullComboState fcState = __instance._playState?.scoreState?.fullComboState ?? FullComboState.None;
        foreach (MeshRenderer meshRenderer in FcElementMeshRenderers)
        {
            meshRenderer.material.SetColor(FaceColor,
                fcState == FullComboState.PerfectPlus
                    ? Plugin.PfcColor.Value.ToColor()
                    : Plugin.FcColor.Value.ToColor());
        }
    }
    
    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.MultiplierBarResultCallback))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void DomeHud_MultiplierBarResultCallbackPatch(DomeHud __instance)
    {
        ScoreState? scoreState = __instance.PlayState?.scoreState;
        if (scoreState == null)
        {
            return;
        }

        Color color = scoreState.TotalMultiplierBucketProgress switch
        {
            < 1f => Plugin.Multiplier1XColor.Value.ToColor(),
            >= 1f and < 2f => Plugin.Multiplier2XColor.Value.ToColor(),
            >= 2f and < 3f => Plugin.Multiplier3XColor.Value.ToColor(),
            _ => Plugin.Multiplier4XColor.Value.ToColor()
        };
        
        foreach (MeshRenderer? meshRenderer in MultiplierElementMeshRenderers)
        {
            meshRenderer?.material.SetColor(FaceColor, color);
        }
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
    public static void DomeHudTrackTimeBar_LateUpdatePatch(DomeHudTrackTimeBar __instance)
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
    
    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.AddToAccuracyLog))]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPrefix]
    public static bool AddToAccuracyLogPatch(DomeHud __instance, ref NoteTimingAccuracy accuracy)
    {
        // re-creating this since i need to be in the middle of it, it seems like
        
        if (__instance.PlayState.PlayerSettings.NoPerfectPlus)
        {
            accuracy = accuracy switch
            {
                NoteTimingAccuracy.PerfectPlus => NoteTimingAccuracy.Perfect,
                NoteTimingAccuracy.EarlyPerfectPlus => NoteTimingAccuracy.EarlyPerfect,
                _ => accuracy
            };
        }

        NoteTimingAccuracy signedAccuracy = accuracy.SetSignBit(false);
        
        if (signedAccuracy is >= NoteTimingAccuracy.Okay and <= NoteTimingAccuracy.PerfectPlus)
        {
            StrippedNoteTimingAccuracy stripped = (StrippedNoteTimingAccuracy)signedAccuracy;
            if (stripped > Plugin.IgnoreAccuracyTypesThreshold.Value)
            {
                return false;
            }
        }
        
        // Okay has 2 possible readings: EarlyOkay is Early, regular Okay is Okay, so we check for that
        AccuracyLogType logType = __instance.GetLogType(accuracy == NoteTimingAccuracy.EarlyOkay ? accuracy : signedAccuracy);
        if (!logType)
        {
            return false;
        }
        
        __instance._activeAccuracyLogInstances.SetActiveElement(__instance.currentLogIndex, logType.Get());
        __instance.currentLogIndex++;
        
        return false;
    }
    
    [HarmonyPatch(typeof(ScoreState), nameof(ScoreState.AddOverbeat))]
    [HarmonyPostfix]
    private static void ScoreState_AddOverbeatPatch(ScoreState __instance)
    {
        if (Plugin.ShowOverbeatsAsMisses.Value)
        {
            PlayState.Active.Hud.AddToAccuracyLog(NoteTimingAccuracy.Failed);
        }
    }
}