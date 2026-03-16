using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using HUDTweaks.Classes;
using TMPro;
using UnityEngine;

namespace HUDTweaks.Patches;

internal class DomeHudContainer
{
    private DomeHud _domeHud;
    
    private Transform _scoreText;
    private CustomTextMeshPro _scoreTextTMP;
    internal MeshRenderer ScoreTextMeshRenderer;
    
    private Transform _healthText;
    private CustomTextMeshPro _healthTextTMP;
    internal MeshRenderer HealthTextMeshRenderer;
    
    private Transform _comboText;
    internal MeshRenderer ComboTextMeshRenderer;
    
    private Transform _infoText;
    internal MeshRenderer InfoTextMeshRenderer;
    
    private readonly List<Transform> TimeElements = [];
    internal List<MeshRenderer> TimeElementMeshRenderers = [];

    private readonly List<Transform> FcElements = [];
    internal readonly List<MeshRenderer> FcElementMeshRenderers = [];
    
    private readonly List<Transform> MultiplierElements = [];
    private readonly List<MeshRenderer> MultiplierElementMeshRenderers = [];
    
    private readonly List<Transform> HurtBackingElements = [];
    internal readonly List<MeshRenderer> HurtBackingElementMeshRenderers = [];
    
    private ColorPalette _palette;
    private readonly int FaceColor = Shader.PropertyToID("_FaceColor");

    private Transform _trackInfoContainer;
    private Transform _timeBarContainer;
    private Transform _mainHudLeftContainer;
    private Transform _mainHudRightContainer;

    public DomeHudContainer(DomeHud domeHud)
    {
        _domeHud = domeHud;
        
        _trackInfoContainer = domeHud.wheelWarpTransform.Find("HudWheelRect/Backing Container");
        _timeBarContainer = domeHud.wheelWarpTransform.Find("HudWheelRect/Time Bar Container");
        _mainHudLeftContainer = domeHud.warpTransform.Find("XMover/HudRect/LeftContainer");
        _mainHudRightContainer = domeHud.warpTransform.Find("XMover/HudRect/RightContainer");
        
        _scoreText = domeHud.number.gameObject.transform.parent.parent.Find("ScoreText");
        _scoreTextTMP = _scoreText.GetComponent<CustomTextMeshPro>();
        ScoreTextMeshRenderer = _scoreText.GetComponent<MeshRenderer>();
        _scoreText.GetComponent<HdrMeshEffect>().Palette = Plugin.WhitePalette;
        
        _healthText = domeHud.healthBar.transform.parent.Find("HealthText");
        _healthTextTMP = _healthText.GetComponent<CustomTextMeshPro>();
        HealthTextMeshRenderer = _healthText.GetComponent<MeshRenderer>();
        _healthText.GetComponent<HdrMeshEffect>().Palette = Plugin.WhitePalette;
        
        _comboText = domeHud.streak.transform.parent.parent.Find("StreakText");
        ComboTextMeshRenderer = _comboText.GetComponent<MeshRenderer>();
        _comboText.GetComponent<HdrMeshEffect>().Palette = Plugin.WhitePalette;
        
        _infoText = domeHud.trackTitleText.transform;
        InfoTextMeshRenderer = _infoText.GetComponent<MeshRenderer>();
        _infoText.GetComponent<HdrMeshEffect>().Palette = Plugin.WhitePalette;
        
        Transform timeContainer = domeHud.wheelWarpTransform.Find("HudWheelRect/Time Bar Container");
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
        
        FcElements.Add(domeHud.fcTexts[0].transform.parent.Find("FcStar"));
        FcElements.Add(domeHud.fcTexts[0].transform.parent.Find("FcStarOutine")); // yep, outine
        foreach (TMP_Text fcText in domeHud.fcTexts)
        {
            FcElements.Add(fcText.transform);
        }
        foreach (Transform transform in FcElements)
        {
            FcElementMeshRenderers.Add(transform.GetComponent<MeshRenderer>());
        }
        
        MultiplierElements.Add(domeHud.multiplierBar.transform);
        for(int i = 0; i < domeHud.multiplier.transform.parent.childCount; i++)
        {
            MultiplierElements.Add(domeHud.multiplier.transform.parent.GetChild(i));
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
        
        HurtBackingElements.Add(_mainHudLeftContainer.Find("HurtBacking/Quad"));
        HurtBackingElements.Add(_mainHudRightContainer.Find("HurtBacking/Quad"));
        foreach (Transform transform in HurtBackingElements)
        {
            HurtBackingElementMeshRenderers.Add(transform.GetComponent<MeshRenderer>());
            if (transform.TryGetComponent(out SpriteMesh spriteMesh))
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
    
    internal async Task ResetTranslatedTexts()
    {
        if (_scoreText.TryGetComponent(out TranslatedTextMeshPro scoreTranslatedTextMeshPro))
        {
            scoreTranslatedTextMeshPro.Translation = "";
            await Awaitable.EndOfFrameAsync();
            scoreTranslatedTextMeshPro.Translation = "Score";
        }

        // ReSharper disable once InvertIf
        if (_healthText.TryGetComponent(out TranslatedTextMeshPro healthTranslatedTextMeshPro))
        {
            healthTranslatedTextMeshPro.Translation = "";
            await Awaitable.EndOfFrameAsync();
            healthTranslatedTextMeshPro.Translation = "Health";
        }
    }

    internal void Update()
    {
        if (_scoreTextTMP != null && Plugin.EnableAccuracyDisplay.Value)
        {
            ScoreState scoreState = _domeHud.PlayState.scoreState;
            float accuracy = (scoreState.TotalScore / (float)((scoreState.CurrentTotals.baseScore + scoreState.CurrentTotals.baseScoreLost) * 4)) * 100;

            string accString = $"{(float.IsNaN(accuracy) ? 100 : accuracy):0.00}%";
            string perfectPlusString = Plugin.EnablePerfectPlusCount.Value
                ? $" <alpha=#80>({scoreState.CurrentTotals.flawlessPlusCount})"
                : "";

            _scoreTextTMP.text = $"{accString}{perfectPlusString}";
        }
        
        if (_healthTextTMP != null && Plugin.EnablePreciseHealth.Value)
        {
            _healthTextTMP.text = _domeHud.PlayState.health.ToString().PadLeft(3, '0');
        }

        FullComboState fcState = _domeHud._playState?.scoreState?.fullComboState ?? FullComboState.None;
        foreach (MeshRenderer meshRenderer in FcElementMeshRenderers)
        {
            meshRenderer.material.SetColor(FaceColor,
                fcState == FullComboState.PerfectPlus
                    ? Plugin.PfcColor.Value.ToColor()
                    : Plugin.FcColor.Value.ToColor());
        }
    }

    internal void MultiplierBarResultCallback()
    {
        ScoreState? scoreState = _domeHud.PlayState?.scoreState;
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

    internal bool UpdateTranslatedElements()
    {
        if (_domeHud._playState == null)
        {
            return true;
        }
        if (_domeHud._playState.IsLocalMultiplayer)
        {
            return true;
        }
        
        PlayableTrackData? trackData = _domeHud._playState.trackData;
        TrackInfoMetadata? trackInfoMetadata = trackData?.Setup.TrackDataSegments[_domeHud._currentTrackSectionIndex.GetValueOrDefault()].GetTrackInfoMetadata();
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

        _domeHud.trackTitleText.SetText(formattedText);
        
        return false;
    }

    internal void UpdateOffsets()
    {
        if (_trackInfoContainer == null ||
            _timeBarContainer == null ||
            _mainHudLeftContainer == null ||
            _mainHudRightContainer == null)
        {
            return;
        }
        
        int hudTrackInfoSetting = XROverridablePlayerSettings<HUDPlayerSettings>.Instance.HudTrackInfo.Value;
        
        // game's doing something finicky with alpha (and _trackInfoContainer's enabled state),
        // but if i set the text on the element, it overrides whatever the game's doing
        // so. this. i hate it.
        _trackInfoContainer.localPosition =
            _trackInfoContainer.localPosition with { y = (hudTrackInfoSetting == 2 ? Plugin.TrackInfoVerticalOffset.Value * 2 : -99999) };

        RectTransform timeBarTransform = _timeBarContainer.GetComponent<RectTransform>();
        timeBarTransform.anchorMin = timeBarTransform.anchorMin with { x = 0.5f - (0.06f * (Plugin.TimeBarWidth.Value / 10f)) };
        timeBarTransform.anchorMax = timeBarTransform.anchorMax with { x = 0.5f + (0.06f * (Plugin.TimeBarWidth.Value / 10f)) };
        
        RectTransform leftBarTransform = _mainHudLeftContainer.GetComponent<RectTransform>();
        leftBarTransform.offsetMin = leftBarTransform.offsetMin with { y = -210f + (120f * (Plugin.MainHudVerticalOffset.Value / 10f)) };
        leftBarTransform.offsetMax = leftBarTransform.offsetMax with { y = 90f + (120f * (Plugin.MainHudVerticalOffset.Value / 10f)) };
        RectTransform rightBarTransform = _mainHudRightContainer.GetComponent<RectTransform>();
        rightBarTransform.offsetMin = rightBarTransform.offsetMin with { y = -210f + (120f * (Plugin.MainHudVerticalOffset.Value / 10f)) };
        rightBarTransform.offsetMax = rightBarTransform.offsetMax with { y = 90f + (120f * (Plugin.MainHudVerticalOffset.Value / 10f)) };
    }
}

[HarmonyPatch]
internal static class DomeHudPatches
{
    internal static readonly Dictionary<DomeHud, DomeHudContainer> DomeHudContainers = new();
    
    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Init))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void DomeHud_InitPatch(DomeHud __instance)
    {
        if (DomeHudContainers.ContainsKey(__instance))
        {
            // already initialized for this DomeHud instance
            return;
        }
        
        DomeHudContainers.Add(__instance, new DomeHudContainer(__instance));
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
    
    internal static async Task ResetTranslatedTexts()
    {
        foreach (KeyValuePair<DomeHud, DomeHudContainer> container in DomeHudContainers)
        {
            await container.Value.ResetTranslatedTexts();
        }
    }

    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.Update))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void UpdatePatch(DomeHud __instance)
    {
        DomeHudContainers[__instance].Update();
    }
    
    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.MultiplierBarResultCallback))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void DomeHud_MultiplierBarResultCallbackPatch(DomeHud __instance)
    {
        DomeHudContainers[__instance].MultiplierBarResultCallback();
    }

    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.UpdateTranslatedElements))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    public static bool UpdateTranslatedElementsPatch(DomeHud __instance)
    {
        return DomeHudContainers[__instance].UpdateTranslatedElements();
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
    // ReSharper disable once InconsistentNaming
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

    internal static void UpdateOffsets()
    {
        foreach (KeyValuePair<DomeHud, DomeHudContainer> container in DomeHudContainers)
        {
            container.Value.UpdateOffsets();
        }
    }

    [HarmonyPatch(typeof(DomeHud), nameof(DomeHud.UpdateLayout))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void DomeHud_UpdateLayoutPatch(DomeHud __instance)
    {
        DomeHudContainers[__instance].UpdateOffsets();
    } 
    
    [HarmonyPatch(typeof(ScoreState), nameof(ScoreState.AddOverbeat))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void ScoreState_AddOverbeatPatch(ScoreState __instance)
    {
        if (Plugin.ShowOverbeatsAsMisses.Value)
        {
            PlayState.Active.Hud.AddToAccuracyLog(NoteTimingAccuracy.Failed);
        }
    }
}