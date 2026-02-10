using System;
using System.Globalization;
using System.Threading.Tasks;
using BepInEx.Configuration;
using SpinCore.Translation;
using SpinCore.UI;
using UnityEngine;

namespace HUDTweaks;

public partial class Plugin
{
    // note to self: BepInEx forces maximum values in the Color type to 1f, for. some reason
    
    //private static readonly Vector3 BlueHUDColor = new(0f, 0.703f, 5.283f);
    private static readonly Vector3 YellowHUDColor = new(4.977f, 0.859f, 0f);
    
    internal static ConfigEntry<bool> EnablePerfectPlusCount = null!;
    
    /*internal static ConfigEntry<Vector3> Multiplier1XColor = null!;
    internal static ConfigEntry<Vector3> Multiplier2XColor = null!;
    internal static ConfigEntry<Vector3> Multiplier3XColor = null!;
    internal static ConfigEntry<Vector3> Multiplier4XColor = null!;*/
    
    internal static ConfigEntry<Vector3> NumberColor = null!;
    
    internal static ConfigEntry<bool> EnableMultiplierBar = null!;
    internal static ConfigEntry<bool> EnableMultiplierText = null!;
    internal static ConfigEntry<bool> EnableCombo = null!;
    internal static ConfigEntry<bool> EnableHealthBar = null!;
    internal static ConfigEntry<bool> EnableScore = null!;

    internal static ConfigEntry<string> TrackInfoText = null!;

    private void RegisterConfigEntries()
    {
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}ModName", nameof(HUDTweaks));
        
        EnablePerfectPlusCount = Config.Bind("General", nameof(EnablePerfectPlusCount), false,
            "Show Perfect+ count beside accuracy");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnablePerfectPlusCount)}", "Show Perfect+ count beside accuracy");
        
        EnableMultiplierBar = Config.Bind("General", nameof(EnableMultiplierBar), true,
            "Show the multiplier bar");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnableMultiplierBar)}", "Show multiplier bar");
        EnableMultiplierText = Config.Bind("General", nameof(EnableMultiplierText), true,
            "Show the multiplier text");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnableMultiplierText)}", "Show multiplier text");
        EnableCombo = Config.Bind("General", nameof(EnableCombo), true,
            "Show the current combo");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnableCombo)}", "Show combo");
        EnableHealthBar = Config.Bind("General", nameof(EnableHealthBar), true,
            "Show the health bar");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnableHealthBar)}", "Show health bar");
        EnableScore = Config.Bind("General", nameof(EnableScore), true,
            "Show the current score");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnableScore)}", "Show score");

        /*Multiplier1XColor = Config.Bind("Colors", nameof(Multiplier1XColor), BlueHUDColor, 
            "Color for the multiplier at 1x");
        Multiplier2XColor = Config.Bind("Colors", nameof(Multiplier2XColor), BlueHUDColor, 
            "Color for the multiplier at 2x");
        Multiplier3XColor = Config.Bind("Colors", nameof(Multiplier3XColor), BlueHUDColor, 
            "Color for the multiplier at 3x");
        Multiplier4XColor = Config.Bind("Colors", nameof(Multiplier4XColor), YellowHUDColor, 
            "Color for the multiplier at 4x");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(Multiplier1XColor)}", "1x multiplier color");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(Multiplier2XColor)}", "2x multiplier color");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(Multiplier3XColor)}", "3x multiplier color");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(Multiplier4XColor)}", "4x multiplier color");*/
        
        NumberColor = Config.Bind("Colors", nameof(NumberColor), YellowHUDColor, 
            "Color for numbers");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(NumberColor)}", "Number color");
        
        TrackInfoText = Config.Bind("Info", nameof(TrackInfoText), "%title% - %artist%",
            "Format string for the track information");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(TrackInfoText)}", "Track information string");
    }

    private static void CreateModPage()
    {
        CustomPage rootModPage = UIHelper.CreateCustomPage("ModSettings");
        rootModPage.OnPageLoad += RootModPageOnPageLoad;

        UIHelper.RegisterMenuInModSettingsRoot($"{TRANSLATION_PREFIX}ModName", rootModPage);
    }

    private static void RootModPageOnPageLoad(Transform rootModPageTransform)
    {
        CustomGroup modGroup = UIHelper.CreateGroup(rootModPageTransform, nameof(HUDTweaks));
        UIHelper.CreateSectionHeader(modGroup, "ModGroupHeader", $"{TRANSLATION_PREFIX}ModName", false);
        
        #region EnablePerfectPlusCount
        CustomGroup enablePerfectPlusCountGroup = UIHelper.CreateGroup(modGroup, "EnablePerfectPlusCountGroup");
        enablePerfectPlusCountGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enablePerfectPlusCountGroup, nameof(EnablePerfectPlusCount),
            $"{TRANSLATION_PREFIX}{nameof(EnablePerfectPlusCount)}", EnablePerfectPlusCount.Value, value =>
            {
                EnablePerfectPlusCount.Value = value;
            });
        #endregion
        
        #region EnableMultiplierBar
        CustomGroup enableMultiplierBarGroup = UIHelper.CreateGroup(modGroup, "EnableMultiplierBarGroup");
        enableMultiplierBarGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableMultiplierBarGroup, nameof(EnableMultiplierBar),
            $"{TRANSLATION_PREFIX}{nameof(EnableMultiplierBar)}", EnableMultiplierBar.Value, value =>
            {
                EnableMultiplierBar.Value = value;
                _ = UpdateHudElementsVisibility();
            });
        #endregion
        
        #region EnableMultiplierText
        CustomGroup enableMultiplierTextGroup = UIHelper.CreateGroup(modGroup, "EnableMultiplierTextGroup");
        enableMultiplierTextGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableMultiplierTextGroup, nameof(EnableMultiplierText),
            $"{TRANSLATION_PREFIX}{nameof(EnableMultiplierText)}", EnableMultiplierText.Value, value =>
            {
                EnableMultiplierText.Value = value;
                _ = UpdateHudElementsVisibility();
            });
        #endregion
        
        #region EnableCombo
        CustomGroup enableComboGroup = UIHelper.CreateGroup(modGroup, "EnableComboGroup");
        enableComboGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableComboGroup, nameof(EnableCombo),
            $"{TRANSLATION_PREFIX}{nameof(EnableCombo)}", EnableCombo.Value, value =>
            {
                EnableCombo.Value = value;
                _ = UpdateHudElementsVisibility();
            });
        #endregion
        
        #region EnableHealthBar
        CustomGroup enableHealthBarGroup = UIHelper.CreateGroup(modGroup, "EnableHealthBarGroup");
        enableHealthBarGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableHealthBarGroup, nameof(EnableHealthBar),
            $"{TRANSLATION_PREFIX}{nameof(EnableHealthBar)}", EnableHealthBar.Value, value =>
            {
                EnableHealthBar.Value = value;
                _ = UpdateHudElementsVisibility();
            });
        #endregion
        
        #region EnableScore
        CustomGroup enableScoreGroup = UIHelper.CreateGroup(modGroup, "EnableScoreGroup");
        enableScoreGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableScoreGroup, nameof(EnableScore),
            $"{TRANSLATION_PREFIX}{nameof(EnableScore)}", EnableScore.Value, value =>
            {
                EnableScore.Value = value;
                _ = UpdateHudElementsVisibility();
            });
        #endregion
        
        #region NumberColor
        CustomGroup numberColorGroup = UIHelper.CreateGroup(modGroup, "NumberColorGroup");
        numberColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(numberColorGroup, "NumberColorLabel", $"{TRANSLATION_PREFIX}{nameof(NumberColor)}");
        
        CustomInputField numberColorInputR = UIHelper.CreateInputField(numberColorGroup, "NumberColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            NumberColor.Value = NumberColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        numberColorInputR.InputField.SetText(NumberColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField numberColorInputG = UIHelper.CreateInputField(numberColorGroup, "NumberColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            NumberColor.Value = NumberColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        numberColorInputG.InputField.SetText(NumberColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField numberColorInputB = UIHelper.CreateInputField(numberColorGroup, "NumberColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            NumberColor.Value = NumberColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        numberColorInputB.InputField.SetText(NumberColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region TrackInfoText
        CustomGroup trackInfoTextGroup = UIHelper.CreateGroup(modGroup, "TrackInfoTextGroup");
        trackInfoTextGroup.LayoutDirection = Axis.Vertical;
        UIHelper.CreateLabel(trackInfoTextGroup, "TrackInfoTextLabel", $"{TRANSLATION_PREFIX}{nameof(TrackInfoText)}");
        
        CustomInputField trackInfoTextInput = UIHelper.CreateInputField(trackInfoTextGroup, "TrackInfoTextInput", (_, value) =>
        {
            TrackInfoText.Value = value;
            
            Task.Run(async () =>
            {
                try
                {
                    await Awaitable.MainThreadAsync();
                    PlayStateContainer playStateContainer = await GetPlayStateContainer();
                    playStateContainer.Hud.UpdateTranslatedElements();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                }
            });
        });
        trackInfoTextInput.InputField.SetText(TrackInfoText.Value);
        #endregion

        /*#region Multiplier1XColors
        CustomGroup multiplier1XColorGroup = UIHelper.CreateGroup(modGroup, "Multiplier1XColorGroup");
        multiplier1XColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(multiplier1XColorGroup, "Multiplier1XColorLabel", $"{TRANSLATION_PREFIX}{nameof(Multiplier1XColor)}");
        
        CustomInputField multiplier1XColorInputR = UIHelper.CreateInputField(multiplier1XColorGroup, "Multiplier1XColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier1XColor.Value = Multiplier1XColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier1XColorInputR.InputField.SetText(Multiplier1XColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier1XColorInputG = UIHelper.CreateInputField(multiplier1XColorGroup, "Multiplier1XColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier1XColor.Value = Multiplier1XColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier1XColorInputG.InputField.SetText(Multiplier1XColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier1XColorInputB = UIHelper.CreateInputField(multiplier1XColorGroup, "Multiplier1XColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier1XColor.Value = Multiplier1XColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier1XColorInputB.InputField.SetText(Multiplier1XColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region Multiplier2XColors
        CustomGroup multiplier2XColorGroup = UIHelper.CreateGroup(modGroup, "Multiplier2XColorGroup");
        multiplier2XColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(multiplier2XColorGroup, "Multiplier2XColorLabel", $"{TRANSLATION_PREFIX}{nameof(Multiplier2XColor)}");
        
        CustomInputField multiplier2XColorInputR = UIHelper.CreateInputField(multiplier2XColorGroup, "Multiplier2XColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier2XColor.Value = Multiplier2XColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier2XColorInputR.InputField.SetText(Multiplier2XColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier2XColorInputG = UIHelper.CreateInputField(multiplier2XColorGroup, "Multiplier2XColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier2XColor.Value = Multiplier2XColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier2XColorInputG.InputField.SetText(Multiplier2XColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier2XColorInputB = UIHelper.CreateInputField(multiplier2XColorGroup, "Multiplier2XColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier2XColor.Value = Multiplier2XColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier2XColorInputB.InputField.SetText(Multiplier2XColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region Multiplier3XColors
        CustomGroup multiplier3XColorGroup = UIHelper.CreateGroup(modGroup, "Multiplier3XColorGroup");
        multiplier3XColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(multiplier3XColorGroup, "Multiplier3XColorLabel", $"{TRANSLATION_PREFIX}{nameof(Multiplier3XColor)}");
        
        CustomInputField multiplier3XColorInputR = UIHelper.CreateInputField(multiplier3XColorGroup, "Multiplier3XColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier3XColor.Value = Multiplier3XColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier3XColorInputR.InputField.SetText(Multiplier3XColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier3XColorInputG = UIHelper.CreateInputField(multiplier3XColorGroup, "Multiplier3XColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier3XColor.Value = Multiplier3XColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier3XColorInputG.InputField.SetText(Multiplier3XColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier3XColorInputB = UIHelper.CreateInputField(multiplier3XColorGroup, "Multiplier3XColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier3XColor.Value = Multiplier3XColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier3XColorInputB.InputField.SetText(Multiplier3XColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region Multiplier4XColors
        CustomGroup multiplier4XColorGroup = UIHelper.CreateGroup(modGroup, "Multiplier4XColorGroup");
        multiplier4XColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(multiplier4XColorGroup, "Multiplier4XColorLabel", $"{TRANSLATION_PREFIX}{nameof(Multiplier4XColor)}");
        
        CustomInputField multiplier4XColorInputR = UIHelper.CreateInputField(multiplier4XColorGroup, "Multiplier4XColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier4XColor.Value = Multiplier4XColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier4XColorInputR.InputField.SetText(Multiplier4XColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier4XColorInputG = UIHelper.CreateInputField(multiplier4XColorGroup, "Multiplier4XColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier4XColor.Value = Multiplier4XColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier4XColorInputG.InputField.SetText(Multiplier4XColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier4XColorInputB = UIHelper.CreateInputField(multiplier4XColorGroup, "Multiplier4XColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier4XColor.Value = Multiplier4XColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        multiplier4XColorInputB.InputField.SetText(Multiplier4XColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion*/
    }
}