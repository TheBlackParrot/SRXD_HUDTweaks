using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace HUDTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("srxd.raoul1808.spincore", "1.1.2")]
public partial class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log = null!;
    private static Harmony _harmony = null!;
    
    private const string TRANSLATION_PREFIX = $"{nameof(HUDTweaks)}_";

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
    
    internal static async Task UpdateColors()
    {
        PlayStateContainer playStateContainer = await GetPlayStateContainer();

        Color textColor = new Color(NumberColor.Value.x, NumberColor.Value.y, NumberColor.Value.z);
        playStateContainer.Hud.healthBar._spriteMesh.Palette.Colors[1] = textColor;

        /*Color[] colors =
        [
            Color.black
        ];

        ColorPalette palette = ScriptableObject.CreateInstance<ColorPalette>();
        for (int i = 0; i < palette.colorArrays.Length; i++)
        {
            palette.colorArrays[i].colors = colors;
        }

        playStateContainer.Hud.multiplierPalette = palette;

        foreach (IPaletteColorizer paletteColorizer in playStateContainer.Hud._multiplierColoring)
        {
            paletteColorizer.Palette = palette;
            paletteColorizer.ColorIndex = 0;
        }*/

        /*Color[] colors =
        [
            new(Multiplier1XColor.Value.x, Multiplier1XColor.Value.y, Multiplier1XColor.Value.z),
            new(Multiplier2XColor.Value.x, Multiplier2XColor.Value.y, Multiplier2XColor.Value.z),
            new(Multiplier3XColor.Value.x, Multiplier3XColor.Value.y, Multiplier3XColor.Value.z),
            new(Multiplier4XColor.Value.x, Multiplier4XColor.Value.y, Multiplier4XColor.Value.z)
        ];

        /*foreach (string propertyName in playStateContainer.Hud.healthBar.material.GetPropertyNames(MaterialPropertyType.Vector))
        {
            Plugin.Log.LogInfo(propertyName);
        }

        foreach (Color colorPaletteColor in playStateContainer.Hud.healthBar._spriteMesh.Palette.Colors)
        {
            Plugin.Log.LogInfo(colorPaletteColor);
        }*/

        /*
        ColorPalette palette = ScriptableObject.CreateInstance<ColorPalette>();
        palette.colorArrays[0].colors = colors;

        playStateContainer.Hud.multiplierPalette = palette;
        playStateContainer.Hud.multiplierBar._spriteMesh.Palette = palette;
        foreach (IPaletteColorizer paletteColorizer in playStateContainer.Hud._multiplierColoring)
        {
            paletteColorizer.Palette = palette;
        }

        Color[] otherColors =
        [
            new(NumberColor.Value.x, NumberColor.Value.y, NumberColor.Value.z)
        ];

        ColorPalette otherPalette = ScriptableObject.CreateInstance<ColorPalette>();
        otherPalette.colorArrays[0].colors = otherColors;

        // ........this changes the textnumber colors? am i going insane?
        playStateContainer.Hud.healthBar._spriteMesh.Palette = otherPalette;*/
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

        for (int i = 0; i < domeHud.healthBar.transform.parent.childCount; i++)
        {
            GameObject rightContainerObject = domeHud.healthBar.transform.parent.GetChild(i).gameObject;
            if (rightContainerObject.name.Contains("Health"))
            {
                rightContainerObject.SetActive(EnableHealthBar.Value);
            }
        }
        

        for (int i = 0; i < domeHud.healthBar.transform.parent.childCount; i++)
        {
            GameObject leftContainerObject = domeHud.multiplierBar.transform.parent.GetChild(i).gameObject;
            if (leftContainerObject.name.Contains("MultiplierBar"))
            {
                leftContainerObject.SetActive(EnableMultiplierBar.Value);
            }
        }
    }
}