using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HUDTweaks.Patches;
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
        
        _harmony.PatchAll();
    }

    private void OnDisable()
    {
        _harmony.UnpatchSelf();
    }

    private static async Task<PlayStateContainer> GetPlayStateContainer()
    {
        PlayStateContainer? playStateContainer = null;
        
        while (playStateContainer == null)
        {
            playStateContainer = GameObject.Find("PlayStateContainer(Clone)")?.GetComponent<PlayStateContainer>();
#if DEBUG
            Log.LogInfo("Waiting for play state container");
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
        PlayStateContainer playStateContainer = await GetPlayStateContainer();
        
        playStateContainer.Hud.healthBar._spriteMesh.Palette.Colors[1] = NumberColor.Value.ToColor();
        
        while (DomeHudPatches.ScoreTextMeshRenderer == null ||
               DomeHudPatches.ComboTextMeshRenderer == null ||
               DomeHudPatches.InfoTextMeshRenderer == null ||
               DomeHudPatches.HealthTextMeshRenderer == null)
        {
            await Awaitable.EndOfFrameAsync(); // akdfhskdfhsdfs
        }
        
        Color textColor = TextColor.Value.ToColor();
        DomeHudPatches.ScoreTextMeshRenderer.material.SetColor(FaceColor, textColor);
        DomeHudPatches.ComboTextMeshRenderer.material.SetColor(FaceColor, textColor);
        DomeHudPatches.InfoTextMeshRenderer.material.SetColor(FaceColor, textColor);
        foreach (MeshRenderer meshRenderer in DomeHudPatches.TimeElementMeshRenderers)
        {
            meshRenderer.material.SetColor(FaceColor, TimeColor.Value.ToColor());
        }
        
        Color healthColor = HealthColor.Value.ToColor();
        DomeHudPatches.HealthTextMeshRenderer.material.SetColor(FaceColor, healthColor);
        playStateContainer.Hud.healthBar.transform.GetComponent<MeshRenderer>().material.SetColor(FaceColor, healthColor);
        
        foreach (TMP_Text fcText in playStateContainer.Hud.fcTexts)
        {
            // we can't just define our own palettes with color here, as the game's not interpreting palette colors directly
            // so we've *also* got to do this the hard way. sigh
            fcText.transform.GetComponent<HdrMeshEffect>().Palette = WhitePalette;
        }
        playStateContainer.Hud.fcTexts[0].transform.parent.Find("FcStar").GetComponent<SpriteMesh>().Palette = WhitePalette;
        playStateContainer.Hud.fcTexts[0].transform.parent.Find("FcStarOutine").GetComponent<SpriteMesh>().Palette = WhitePalette; // yep, outine
        
        foreach (MeshRenderer meshRenderer in DomeHudPatches.FcElementMeshRenderers)
        {
            meshRenderer.material.SetColor(FaceColor, PfcColor.Value.ToColor());
        }

        ForceMultiplierPalette();
    }

    internal static async Task UpdateHudElementsVisibility()
    {
        PlayStateContainer playStateContainer = await GetPlayStateContainer();
        DomeHud domeHud = playStateContainer.Hud;

        // multiplier text
        domeHud.multiplier.transform.parent.gameObject.SetActive(EnableMultiplierText.Value);
        
        // combo text
        domeHud.streak.transform.parent.parent.gameObject.SetActive(EnableCombo.Value);
        
        // score text
        domeHud.number.transform.parent.parent.gameObject.SetActive(EnableScore.Value);
        
        playStateContainer._trackSkinVisuals?.transform.Find("TrackStrip")?.gameObject.SetActive(EnableTrackStrips.Value);
        playStateContainer.WheelVisuals?.leftGripObject?.gameObject.SetActive(EnableWheelGrips.Value);
        playStateContainer.WheelVisuals?.rightGripObject?.gameObject.SetActive(EnableWheelGrips.Value);

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