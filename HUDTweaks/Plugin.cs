using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HUDTweaks.Classes;
using HUDTweaks.Patches;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace HUDTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("srxd.raoul1808.spincore", "1.1.2")]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;
    private static Harmony _harmony = null!;
    private static readonly int FaceColor = Shader.PropertyToID("_FaceColor");

    private const string TRANSLATION_PREFIX = $"{nameof(HUDTweaks)}_";
    private const string REPO_NAME = $"SRXD_{nameof(HUDTweaks)}";

    internal static ColorPalette WhitePalette = null!;

    private void Awake()
    {
        Log = Logger;
        
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        RegisterConfigEntries();
        CreateModPage();
        
        Log.LogInfo("Plugin loaded");
    }

    private void OnEnable()
    {
        WhitePalette = ScriptableObject.CreateInstance<ColorPalette>();
        WhitePalette.colorArrays = [new ColorPalette.ColorsArray()];
        WhitePalette.colorArrays[0].colors = [Color.white];

        MainCamera.OnCurrentCameraChanged += ForceMultiplierPalette;
        MainCamera.OnCurrentCameraChanged += CheckForUpdates;
        
        _harmony.PatchAll();
    }

    private void OnDisable()
    {
        _harmony.UnpatchSelf();
    }

    private static void CheckForUpdates(Camera _)
    {
        MainCamera.OnCurrentCameraChanged -= CheckForUpdates;

        Task.Run(async () =>
        {
            try
            {
                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("User-Agent",
                    $"{nameof(HUDTweaks)}/{MyPluginInfo.PLUGIN_VERSION} (https://github.com/TheBlackParrot/{REPO_NAME})");
                HttpResponseMessage responseMessage =
                    await httpClient.GetAsync(
                        $"https://api.github.com/repos/TheBlackParrot/{REPO_NAME}/releases/latest");
                responseMessage.EnsureSuccessStatusCode();
                string json = await responseMessage.Content.ReadAsStringAsync();

                ReleaseVersion? releaseVersion = JsonConvert.DeserializeObject<ReleaseVersion>(json);
                if (releaseVersion == null)
                {
                    Log.LogInfo("Could not get newest release version information");
                    return;
                }

                if (releaseVersion.Version == null)
                {
                    Log.LogInfo("Could not get newest release version information");
                    return;
                }

                if (releaseVersion.IsPreRelease)
                {
                    Log.LogInfo("Newest release version is a pre-release");
                    return;
                }

                Version currentVersion = new(MyPluginInfo.PLUGIN_VERSION);
                Version latestVersion = new(releaseVersion.Version);
#if DEBUG
            // just so we can see the notifications
            if(currentVersion != latestVersion)
#else
                if (currentVersion < latestVersion)
#endif
                {
                    Log.LogMessage(
                        $"{nameof(HUDTweaks)} is out of date! (using v{currentVersion}, latest is v{latestVersion})");

                    await Awaitable.MainThreadAsync();
                    NotificationSystemGUI.AddMessage(
                        $"<b>{nameof(HUDTweaks)}</b> has an update available! <alpha=#AA>(v{currentVersion} <alpha=#77>-> <alpha=#AA>v{latestVersion})\n<alpha=#FF><size=67%>See the shortcut button in the Mod Settings page to grab the latest update.",
                        15f);
                }
                else
                {
                    Log.LogMessage($"{nameof(HUDTweaks)} is up to date!");
                }
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
        });
    }

    private static async Task<PlayStateContainer[]> GetPlayStateContainers()
    {
        PlayStateContainer[] playStateContainer = FindObjectsByType<PlayStateContainer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        
        while (playStateContainer.Length <= 0)
        {
            playStateContainer = FindObjectsByType<PlayStateContainer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#if DEBUG
            Log.LogInfo("Waiting for play state containers");
#endif
            await Awaitable.EndOfFrameAsync();
        }
        
        return playStateContainer;
    }

    // grabbing an object out of Resources, feel better modifying it at this point
    private static void ForceMultiplierPalette(Camera? _ = null)
    {
        MainCamera.OnCurrentCameraChanged -= ForceMultiplierPalette;
        Resources.FindObjectsOfTypeAll<ColorPalette>().First(x => x.name.Contains("Multiplier")).colorArrays[0].colors =
        [
            Multiplier1XColor.Value.ToColor(),
            Multiplier2XColor.Value.ToColor(),
            Multiplier3XColor.Value.ToColor(),
            Multiplier4XColor.Value.ToColor()
        ];
    }
    
    internal static async Task UpdateColors()
    {
        Color healthColor = HealthColor.Value.ToColor();
        Color textColor = TextColor.Value.ToColor();
        
        foreach (KeyValuePair<DomeHud, DomeHudContainer> hudContainer in DomeHudPatches.DomeHudContainers)
        {
            DomeHud hud = hudContainer.Key;
            DomeHudContainer container = hudContainer.Value;

            while (hud.healthBar == null || hud.healthBar?._spriteMesh == null)
            {
                await Awaitable.EndOfFrameAsync();
            }
            
            hud.healthBar._spriteMesh.Palette.Colors[1] = NumberColor.Value.ToColor();
            hud.healthBar.transform.GetComponent<MeshRenderer>().material.SetColor(FaceColor, healthColor);
            container.HealthTextMeshRenderer.material.SetColor(FaceColor, healthColor);
            
            foreach (TMP_Text fcText in hud.fcTexts)
            {
                // we can't just define our own palettes with color here, as the game's not interpreting palette colors directly
                // so we've *also* got to do this the hard way. sigh
                fcText.transform.GetComponent<HdrMeshEffect>().Palette = WhitePalette;
            }
            hud.fcTexts[0].transform.parent.Find("FcStar").GetComponent<SpriteMesh>().Palette = WhitePalette;
            hud.fcTexts[0].transform.parent.Find("FcStarOutine").GetComponent<SpriteMesh>().Palette = WhitePalette; // yep, outine
            
            container.ScoreTextMeshRenderer.material.SetColor(FaceColor, textColor);
            container.ComboTextMeshRenderer.material.SetColor(FaceColor, textColor);
            container.InfoTextMeshRenderer.material.SetColor(FaceColor, textColor);
            foreach (MeshRenderer meshRenderer in container.TimeElementMeshRenderers)
            {
                meshRenderer.material.SetColor(FaceColor, TimeColor.Value.ToColor());
            }
        
            foreach (MeshRenderer meshRenderer in container.FcElementMeshRenderers)
            {
                meshRenderer.material.SetColor(FaceColor, PfcColor.Value.ToColor());
            }

            foreach (MeshRenderer meshRenderer in container.HurtBackingElementMeshRenderers)
            {
                meshRenderer.material.SetColor(FaceColor, HurtColor.Value.ToColor());
            }
        }

        ForceMultiplierPalette();
    }

    internal static async Task UpdateHudElementsVisibility()
    {
        PlayStateContainer[] playStateContainers = await GetPlayStateContainers();

        foreach (PlayStateContainer playStateContainer in playStateContainers)
        {
            playStateContainer._trackSkinVisuals?.transform.Find("TrackStrip")?.gameObject
                .SetActive(EnableTrackStrips.Value);
            playStateContainer.WheelVisuals?.leftGripObject?.gameObject.SetActive(EnableWheelGrips.Value);
            playStateContainer.WheelVisuals?.rightGripObject?.gameObject.SetActive(EnableWheelGrips.Value);
        }

        foreach (DomeHud domeHud in DomeHudPatches.DomeHudContainers.Select(hudContainer => hudContainer.Key))
        {
            // multiplier text
            domeHud.multiplier.transform.parent.gameObject.SetActive(EnableMultiplierText.Value);

            // combo text
            domeHud.streak.transform.parent.parent.gameObject.SetActive(EnableCombo.Value);

            // score text
            domeHud.number.transform.parent.parent.gameObject.SetActive(EnableScore.Value);

            for (int i = 0; i < domeHud.healthBar.transform.parent.childCount; i++)
            {
                GameObject rightContainerObject = domeHud.healthBar.transform.parent.GetChild(i).gameObject;

                if (rightContainerObject.name.Contains("Health"))
                {
                    rightContainerObject.SetActive(EnableHealthBar.Value);
                }

                if (rightContainerObject.name == "HurtBacking")
                {
                    rightContainerObject.SetActive(EnableHurtFlashing.Value);
                }
            }

            for (int i = 0; i < domeHud.healthBar.transform.parent.childCount; i++)
            {
                GameObject leftContainerObject = domeHud.multiplierBar.transform.parent.GetChild(i).gameObject;

                if (leftContainerObject.name.Contains("MultiplierBar"))
                {
                    leftContainerObject.SetActive(EnableMultiplierBar.Value);
                }

                if (leftContainerObject.name == "HurtBacking")
                {
                    leftContainerObject.SetActive(EnableHurtFlashing.Value);
                }
            }
        }
    }

    internal static async Task RefreshEverythingGuh()
    {
        await UpdateHudElementsVisibility();
        await Awaitable.MainThreadAsync();
        PlayStateContainer[] playStateContainers = await GetPlayStateContainers();
        foreach (PlayStateContainer playStateContainer in playStateContainers)
        {
            playStateContainer.Hud.UpdateTranslatedElements();
        }
        DomeHudPatches.UpdateOffsets();
        _ = DomeHudPatches.ResetTranslatedTexts();
        await UpdateColors();
    }
}