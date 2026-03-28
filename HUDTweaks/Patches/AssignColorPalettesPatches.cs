using HarmonyLib;

namespace HUDTweaks.Patches;

[HarmonyPatch]
public static class AssignColorPalettesPatches
{
    [HarmonyPatch(typeof(ColorSystem), nameof(ColorSystem.AssignColorPalettes))]
    [HarmonyPostfix]
    public static void AssignColorPalettes()
    {
        ColorPalette accuracyPalette = GameSystemSingleton<ColorSystem, ColorSystemSettings>.Settings.accuracyPalette;
        
        accuracyPalette.colorArrays[0].colors =
        [
            Plugin.AccuracyMissColor.Value.ToColor(),
            Plugin.AccuracyOkayColor.Value.ToColor(),
            Plugin.AccuracyGoodColor.Value.ToColor(),
            Plugin.AccuracyGreatColor.Value.ToColor(),
            Plugin.AccuracyPerfectColor.Value.ToColor(),
            Plugin.AccuracyPerfectColor.Value.ToColor()
        ];
        
        accuracyPalette.colorArrays[1].colors =
        [
            Plugin.AccuracyMissColor.Value.ToColor(),
            Plugin.AccuracyOkayColor.Value.ToColor(),
            Plugin.AccuracyGoodColor.Value.ToColor(),
            Plugin.AccuracyGreatColor.Value.ToColor(),
            Plugin.AccuracyPerfectColor.Value.ToColor(),
            Plugin.AccuracyPerfectPlusColor.Value.ToColor()
        ];
    }
}