using System;
using System.Globalization;
using System.Threading.Tasks;
using BepInEx.Configuration;
using HUDTweaks.Classes;
using HUDTweaks.Patches;
using SpinCore.Translation;
using SpinCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HUDTweaks;

public partial class Plugin
{
    // note to self: BepInEx forces maximum values in the Color type to 1f, for. some reason
    
    private static readonly Vector3 BlueHUDColor = new(0f, 0.703f, 5.283f);
    private static readonly Vector3 RedHUDColor = new(2.996f, 0.063f, 0.157f);
    private static readonly Vector3 YellowHUDColor = new(4.977f, 0.859f, 0f);
    private static readonly Vector3 CyanHUDColor = new(Color.cyan.r, Color.cyan.g, Color.cyan.b); // close enough
    private static readonly Vector3 GreenHUDColor = new(Color.green.r, Color.green.g, Color.green.b); // close enough
    
    internal static ConfigEntry<bool> EnableAccuracyDisplay = null!;
    internal static ConfigEntry<bool> EnablePreciseHealth = null!;
    internal static ConfigEntry<bool> EnablePerfectPlusCount = null!;
    internal static ConfigEntry<bool> ShowOverbeatsAsMisses = null!;
    
    internal static ConfigEntry<Vector3> Multiplier1XColor = null!;
    internal static ConfigEntry<Vector3> Multiplier2XColor = null!;
    internal static ConfigEntry<Vector3> Multiplier3XColor = null!;
    internal static ConfigEntry<Vector3> Multiplier4XColor = null!;
    
    internal static ConfigEntry<Vector3> NumberColor = null!;
    internal static ConfigEntry<Vector3> TextColor = null!;
    internal static ConfigEntry<Vector3> TimeColor = null!;
    internal static ConfigEntry<Vector3> HealthColor = null!;
    internal static ConfigEntry<Vector3> PfcColor = null!;
    internal static ConfigEntry<Vector3> FcColor = null!;
    
    internal static ConfigEntry<bool> EnableMultiplierBar = null!;
    internal static ConfigEntry<bool> EnableMultiplierText = null!;
    internal static ConfigEntry<bool> EnableCombo = null!;
    internal static ConfigEntry<bool> EnableHealthBar = null!;
    internal static ConfigEntry<bool> EnableScore = null!;
    internal static ConfigEntry<bool> EnableHurtFlashing = null!;
    internal static ConfigEntry<bool> EnableTrackStrips = null!;
    internal static ConfigEntry<bool> EnableWheelGrips = null!;

    internal static ConfigEntry<string> TrackInfoText = null!;
    
    internal static ConfigEntry<bool> ShowTimeInBeats = null!;

    internal static ConfigEntry<StrippedNoteTimingAccuracy> IgnoreAccuracyTypesThreshold = null!;
    
    internal static ConfigEntry<int> TrackInfoVerticalOffset = null!;
    internal static ConfigEntry<int> TimeBarWidth = null!;
    internal static ConfigEntry<int> MainHudVerticalOffset = null!;

    private void RegisterConfigEntries()
    {
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}ModName", nameof(HUDTweaks));
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}Toggles", "Toggles");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}Colors", "Colors");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}Extras", "Extras");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}Offsets", "Offsets");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}GitHubButtonText", $"{nameof(HUDTweaks)} Releases (GitHub)");
        
        EnableAccuracyDisplay = Config.Bind("General", nameof(EnableAccuracyDisplay), false,
            "Show the current accuracy in place of the \"SCORE\" label");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnableAccuracyDisplay)}", "Replace \"SCORE\" with accuracy");
        EnablePreciseHealth = Config.Bind("General", nameof(EnablePreciseHealth), false,
            "Show the current exact health value in place of the \"HEALTH\" label");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnablePreciseHealth)}", "Replace \"HEALTH\" with health");
        EnablePerfectPlusCount = Config.Bind("General", nameof(EnablePerfectPlusCount), false,
            "Show Perfect+ count beside accuracy");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnablePerfectPlusCount)}", "Show Perfect+ count beside accuracy");
        
        ShowTimeInBeats = Config.Bind("General", nameof(ShowTimeInBeats), false,
            "Show time values in beats");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(ShowTimeInBeats)}", "Show time in beats");
        
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
        EnableHurtFlashing = Config.Bind("General", nameof(EnableHurtFlashing), true,
            "Show the default subtle HUD flashing when your health drops");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnableHurtFlashing)}", "Show hurt flashing");
        EnableTrackStrips = Config.Bind("General", nameof(EnableTrackStrips), true,
            "Show the highlights/track strip on the sides of the track");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnableTrackStrips)}", "Show track highlights");
        EnableWheelGrips = Config.Bind("General", nameof(EnableWheelGrips), true,
            "Show the ghostly transparent sides of the wheel (the grips)");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnableWheelGrips)}", "Show wheel grips");

        Multiplier1XColor = Config.Bind("Colors", nameof(Multiplier1XColor), BlueHUDColor, 
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
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(Multiplier4XColor)}", "4x multiplier color");
        
        NumberColor = Config.Bind("Colors", nameof(NumberColor), YellowHUDColor, 
            "Color for boldened numbers");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(NumberColor)}", "Number color");
        TextColor = Config.Bind("Colors", nameof(TextColor), YellowHUDColor, 
            "Color for standard text");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(TextColor)}", "Standard text color");
        TimeColor = Config.Bind("Colors", nameof(TimeColor), YellowHUDColor, 
            "Color for time elements");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(TimeColor)}", "Time color");
        HealthColor = Config.Bind("Colors", nameof(HealthColor), RedHUDColor, 
            "Color for health elements");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(HealthColor)}", "Health color");
        PfcColor = Config.Bind("Colors", nameof(PfcColor), CyanHUDColor, 
            "Color for PFC elements");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(PfcColor)}", "PFC color");
        FcColor = Config.Bind("Colors", nameof(FcColor), GreenHUDColor, 
            "Color for FC elements");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(FcColor)}", "FC color");
        
        TrackInfoText = Config.Bind("Info", nameof(TrackInfoText), "%title% - %artist%",
            "Format string for the track information");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(TrackInfoText)}", "Track Information String");
        
        IgnoreAccuracyTypesThreshold = Config.Bind("General", nameof(IgnoreAccuracyTypesThreshold), StrippedNoteTimingAccuracy.PerfectPlus,
            "Ignore adding accuracy types above this value to the accuracy log (exclusive)");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(IgnoreAccuracyTypesThreshold)}", "Hide timings in the accuracy log above");
        ShowOverbeatsAsMisses = Config.Bind("General", nameof(ShowOverbeatsAsMisses), false,
            "Show overbeats as misses in the accuracy log");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(ShowOverbeatsAsMisses)}", "Show overbeats as misses");
        
        MainHudVerticalOffset = Config.Bind("Offsets", nameof(MainHudVerticalOffset), 0,
            "Vertical offset of the main HUD");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(MainHudVerticalOffset)}", "Main HUD vertical offset");
        TrackInfoVerticalOffset = Config.Bind("Offsets", nameof(TrackInfoVerticalOffset), 0,
            "Vertical offset of the track/chart info");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(TrackInfoVerticalOffset)}", "Track info vertical offset");
        TimeBarWidth = Config.Bind("Offsets", nameof(TimeBarWidth), 0,
            "Extra width for the time bar");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(TimeBarWidth)}", "Time bar extra width");
        
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}OtherToggles", "Track + Hud > Position contains visibility options available in the base game");
        
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_Artist", "Who made the current track");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_Charter", "Who charted the chart being played");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_Difficulty", "The difficulty name of the chart being played");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_Duration", "How long the current track is (MM:SS)");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_Rating", "The numeric rating of the chart being played");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_Title", "Title of the current track");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_NewLineCharacter", "New line character");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_TabCharacter", "Tab character");
    }

    private static void CreateModPage()
    {
        CustomPage rootModPage = UIHelper.CreateCustomPage("ModSettings");
        rootModPage.OnPageLoad += RootModPageOnPageLoad;

        UIHelper.RegisterMenuInModSettingsRoot($"{TRANSLATION_PREFIX}ModName", rootModPage);
    }

    private const int TAG_REFERENCE_PREFERRED_WIDTH = 500;

    private static void CreateReferenceTagRow(CustomGroup modGroup, string which, string? tagOverride = null)
    {
        CustomGroup referenceGroup = UIHelper.CreateGroup(modGroup, $"{which}TagReferenceGroup");
        referenceGroup.LayoutDirection = Axis.Horizontal;
        
        CustomTextComponent referenceTag = UIHelper.CreateLabel(referenceGroup, $"{which}TagReferenceName", TranslationReference.Empty);
        referenceTag.ExtraText = tagOverride ?? $"%{which.ToLower()}%";
        LayoutElement referenceTagLayoutComponent = referenceTag.Transform.GetComponent<LayoutElement>();
        referenceTagLayoutComponent.preferredWidth = 0;
        
        CustomTextComponent referenceTagDescription =
            UIHelper.CreateLabel(referenceGroup, $"{which}TagReferenceDescription", $"{TRANSLATION_PREFIX}TagDescription_{which}");
        CustomTextMeshProUGUI referenceTagDescriptionTextComponent = referenceTagDescription.Transform.GetComponent<CustomTextMeshProUGUI>();
        referenceTagDescriptionTextComponent.fontSize /= 1.25f;
        LayoutElement referenceTagDescriptionLayoutComponent = referenceTagDescription.Transform.GetComponent<LayoutElement>();
        referenceTagDescriptionLayoutComponent.preferredWidth = TAG_REFERENCE_PREFERRED_WIDTH;
    }
    private static void RootModPageOnPageLoad(Transform rootModPageTransform)
    {
        CustomGroup modGroup = UIHelper.CreateGroup(rootModPageTransform, nameof(HUDTweaks));
        UIHelper.CreateSectionHeader(modGroup, "ModGroupHeader", $"{TRANSLATION_PREFIX}ModName", false);
        
        UIHelper.CreateButton(modGroup, $"Open{nameof(HUDTweaks)}RepositoryButton", $"{TRANSLATION_PREFIX}GitHubButtonText", () =>
        {
            Application.OpenURL($"https://github.com/TheBlackParrot/{REPO_NAME}/releases/latest");
        });
        
        UIHelper.CreateSectionHeader(modGroup, "TogglesHeader", $"{TRANSLATION_PREFIX}Toggles", false);
        
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
        
        #region EnableHurtFlashing
        CustomGroup enableHurtFlashingGroup = UIHelper.CreateGroup(modGroup, "EnableHurtFlashingGroup");
        enableHurtFlashingGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableHurtFlashingGroup, nameof(EnableHurtFlashing),
            $"{TRANSLATION_PREFIX}{nameof(EnableHurtFlashing)}", EnableHurtFlashing.Value, value =>
            {
                EnableHurtFlashing.Value = value;
                _ = UpdateHudElementsVisibility();
            });
        #endregion
        
        #region EnableTrackStrips
        CustomGroup enableTrackStripsGroup = UIHelper.CreateGroup(modGroup, "EnableTrackStripsGroup");
        enableTrackStripsGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableTrackStripsGroup, nameof(EnableTrackStrips),
            $"{TRANSLATION_PREFIX}{nameof(EnableTrackStrips)}", EnableTrackStrips.Value, value =>
            {
                EnableTrackStrips.Value = value;
                _ = UpdateHudElementsVisibility();
            });
        #endregion
        
        #region EnableWheelGrips
        CustomGroup enableWheelGripsGroup = UIHelper.CreateGroup(modGroup, "EnableWheelGripsGroup");
        enableWheelGripsGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableWheelGripsGroup, nameof(EnableWheelGrips),
            $"{TRANSLATION_PREFIX}{nameof(EnableWheelGrips)}", EnableWheelGrips.Value, value =>
            {
                EnableWheelGrips.Value = value;
                _ = UpdateHudElementsVisibility();
            });
        #endregion

        #region IgnoreAccuracyTypesThreshold
        CustomGroup ignoreAccuracyTypesThresholdGroup = UIHelper.CreateGroup(modGroup, "IgnoreAccuracyTypesThresholdGroup");
        UIHelper.CreateSmallMultiChoiceButton(ignoreAccuracyTypesThresholdGroup, nameof(IgnoreAccuracyTypesThreshold),
            $"{TRANSLATION_PREFIX}{nameof(IgnoreAccuracyTypesThreshold)}", IgnoreAccuracyTypesThreshold.Value, accuracy =>
            {
                IgnoreAccuracyTypesThreshold.Value = accuracy;
            });
        #endregion
        
        UIHelper.CreateLabel(modGroup, "WhereAreMyTogglesPls", $"{TRANSLATION_PREFIX}OtherToggles");
        
        UIHelper.CreateSectionHeader(modGroup, "OffsetsHeader", $"{TRANSLATION_PREFIX}Offsets", false);
        
        #region MainHudVerticalOffset
        CustomGroup mainHudVerticalOffsetGroup = UIHelper.CreateGroup(modGroup, "MainHudVerticalOffsetGroup");
        UIHelper.CreateSmallMultiChoiceButton(mainHudVerticalOffsetGroup, nameof(MainHudVerticalOffset), $"{TRANSLATION_PREFIX}{nameof(MainHudVerticalOffset)}",
            MainHudVerticalOffset.Value, (value) =>
            {
                MainHudVerticalOffset.Value = value;
                DomeHudPatches.UpdateOffsets();
            },
            () => new IntRange(-10, 11),
            v => v.ToString());
        #endregion
        
        #region TrackInfoVerticalOffset
        CustomGroup trackInfoVerticalOffsetGroup = UIHelper.CreateGroup(modGroup, "TrackInfoVerticalOffsetGroup");
        UIHelper.CreateSmallMultiChoiceButton(trackInfoVerticalOffsetGroup, nameof(TrackInfoVerticalOffset), $"{TRANSLATION_PREFIX}{nameof(TrackInfoVerticalOffset)}",
            TrackInfoVerticalOffset.Value, (value) =>
            {
                TrackInfoVerticalOffset.Value = value;
                DomeHudPatches.UpdateOffsets();
            },
            () => new IntRange(-10, 11),
            v => v.ToString());
        #endregion
        
        #region TimeBarWidth
        CustomGroup timeBarWidthGroup = UIHelper.CreateGroup(modGroup, "TimeBarWidthGroup");
        UIHelper.CreateSmallMultiChoiceButton(timeBarWidthGroup, nameof(TimeBarWidth), $"{TRANSLATION_PREFIX}{nameof(TimeBarWidth)}",
            TimeBarWidth.Value, (value) =>
            {
                TimeBarWidth.Value = value;
                DomeHudPatches.UpdateOffsets();
            },
            () => new IntRange(-10, 11),
            v => v.ToString());
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "ColorsHeader", $"{TRANSLATION_PREFIX}Colors", false);
        
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
        
        #region TextColors
        CustomGroup textColorGroup = UIHelper.CreateGroup(modGroup, "TextColorGroup");
        textColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(textColorGroup, "TextColorLabel", $"{TRANSLATION_PREFIX}{nameof(TextColor)}");
        
        CustomInputField textColorInputR = UIHelper.CreateInputField(textColorGroup, "TextColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TextColor.Value = TextColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        textColorInputR.InputField.SetText(TextColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField textColorInputG = UIHelper.CreateInputField(textColorGroup, "TextColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TextColor.Value = TextColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        textColorInputG.InputField.SetText(TextColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField textColorInputB = UIHelper.CreateInputField(textColorGroup, "TextColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TextColor.Value = TextColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        textColorInputB.InputField.SetText(TextColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region TimeColors
        CustomGroup timeColorGroup = UIHelper.CreateGroup(modGroup, "TimeColorGroup");
        timeColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(timeColorGroup, "TimeColorLabel", $"{TRANSLATION_PREFIX}{nameof(TimeColor)}");
        
        CustomInputField timeColorInputR = UIHelper.CreateInputField(timeColorGroup, "TimeColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TimeColor.Value = TimeColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        timeColorInputR.InputField.SetText(TimeColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField timeColorInputG = UIHelper.CreateInputField(timeColorGroup, "TimeColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TimeColor.Value = TimeColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        timeColorInputG.InputField.SetText(TimeColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField timeColorInputB = UIHelper.CreateInputField(timeColorGroup, "TimeColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TimeColor.Value = TimeColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        timeColorInputB.InputField.SetText(TimeColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region HealthColors
        CustomGroup healthColorGroup = UIHelper.CreateGroup(modGroup, "HealthColorGroup");
        healthColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(healthColorGroup, "HealthColorLabel", $"{TRANSLATION_PREFIX}{nameof(HealthColor)}");
        
        CustomInputField healthColorInputR = UIHelper.CreateInputField(healthColorGroup, "HealthColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            HealthColor.Value = HealthColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        healthColorInputR.InputField.SetText(HealthColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField healthColorInputG = UIHelper.CreateInputField(healthColorGroup, "HealthColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            HealthColor.Value = HealthColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        healthColorInputG.InputField.SetText(HealthColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField healthColorInputB = UIHelper.CreateInputField(healthColorGroup, "HealthColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            HealthColor.Value = HealthColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        healthColorInputB.InputField.SetText(HealthColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region PfcColors
        CustomGroup pfcColorGroup = UIHelper.CreateGroup(modGroup, "PfcColorGroup");
        pfcColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(pfcColorGroup, "PfcColorLabel", $"{TRANSLATION_PREFIX}{nameof(PfcColor)}");
        
        CustomInputField pfcColorInputR = UIHelper.CreateInputField(pfcColorGroup, "PfcColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            PfcColor.Value = PfcColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        pfcColorInputR.InputField.SetText(PfcColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField pfcColorInputG = UIHelper.CreateInputField(pfcColorGroup, "PfcColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            PfcColor.Value = PfcColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        pfcColorInputG.InputField.SetText(PfcColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField pfcColorInputB = UIHelper.CreateInputField(pfcColorGroup, "PfcColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            PfcColor.Value = PfcColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        pfcColorInputB.InputField.SetText(PfcColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region FcColors
        CustomGroup fcColorGroup = UIHelper.CreateGroup(modGroup, "FcColorGroup");
        fcColorGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(fcColorGroup, "FcColorLabel", $"{TRANSLATION_PREFIX}{nameof(FcColor)}");
        
        CustomInputField fcColorInputR = UIHelper.CreateInputField(fcColorGroup, "FcColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            FcColor.Value = FcColor.Value with { x = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        fcColorInputR.InputField.SetText(FcColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField fcColorInputG = UIHelper.CreateInputField(fcColorGroup, "FcColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            FcColor.Value = FcColor.Value with { y = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        fcColorInputG.InputField.SetText(FcColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField fcColorInputB = UIHelper.CreateInputField(fcColorGroup, "FcColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            FcColor.Value = FcColor.Value with { z = Math.Max(value, 0f) };
            _ = UpdateColors();
        });
        fcColorInputB.InputField.SetText(FcColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region Multiplier1XColors
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
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "ExtrasHeader", $"{TRANSLATION_PREFIX}Extras", false);
        
        #region EnableAccuracyDisplay
        CustomGroup enableAccuracyDisplayGroup = UIHelper.CreateGroup(modGroup, "EnableAccuracyDisplayGroup");
        enableAccuracyDisplayGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableAccuracyDisplayGroup, nameof(EnableAccuracyDisplay),
            $"{TRANSLATION_PREFIX}{nameof(EnableAccuracyDisplay)}", EnableAccuracyDisplay.Value, value =>
            {
                EnableAccuracyDisplay.Value = value;
                
                if (!value)
                {
                    _ = DomeHudPatches.ResetTranslatedTexts();
                }
            });
        #endregion
        
        #region EnablePerfectPlusCount
        CustomGroup enablePerfectPlusCountGroup = UIHelper.CreateGroup(modGroup, "EnablePerfectPlusCountGroup");
        enablePerfectPlusCountGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enablePerfectPlusCountGroup, nameof(EnablePerfectPlusCount),
            $"{TRANSLATION_PREFIX}{nameof(EnablePerfectPlusCount)}", EnablePerfectPlusCount.Value, value =>
            {
                EnablePerfectPlusCount.Value = value;
            });
        #endregion
        
        #region EnablePreciseHealth
        CustomGroup enablePreciseHealthGroup = UIHelper.CreateGroup(modGroup, "EnablePreciseHealthGroup");
        enablePreciseHealthGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enablePreciseHealthGroup, nameof(EnablePreciseHealth),
            $"{TRANSLATION_PREFIX}{nameof(EnablePreciseHealth)}", EnablePreciseHealth.Value, value =>
            {
                EnablePreciseHealth.Value = value;

                if (!value)
                {
                    _ = DomeHudPatches.ResetTranslatedTexts();
                }
            });
        #endregion
        
        #region ShowTimeInBeats
        CustomGroup showTimeInBeatsGroup = UIHelper.CreateGroup(modGroup, "ShowTimeInBeatsGroup");
        showTimeInBeatsGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(showTimeInBeatsGroup, nameof(ShowTimeInBeats),
            $"{TRANSLATION_PREFIX}{nameof(ShowTimeInBeats)}", ShowTimeInBeats.Value, value =>
            {
                ShowTimeInBeats.Value = value;
            });
        #endregion
        
        #region ShowOverbeatsAsMisses
        CustomGroup showOverbeatsAsMissesGroup = UIHelper.CreateGroup(modGroup, "ShowOverbeatsAsMissesGroup");
        showOverbeatsAsMissesGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(showOverbeatsAsMissesGroup, nameof(ShowOverbeatsAsMisses),
            $"{TRANSLATION_PREFIX}{nameof(ShowOverbeatsAsMisses)}", ShowOverbeatsAsMisses.Value, value =>
            {
                ShowOverbeatsAsMisses.Value = value;
            });
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "TrackInfoHeader", $"{TRANSLATION_PREFIX}{nameof(TrackInfoText)}", false);
        
        #region TrackInfoText
        CustomGroup trackInfoTextGroup = UIHelper.CreateGroup(modGroup, "TrackInfoTextGroup");
        trackInfoTextGroup.LayoutDirection = Axis.Vertical;
        
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

        #region TrackInfoTextTagsReference
        CreateReferenceTagRow(modGroup, "Title");
        CreateReferenceTagRow(modGroup, "Artist");
        CreateReferenceTagRow(modGroup, "Duration");
        CreateReferenceTagRow(modGroup, "Charter");
        CreateReferenceTagRow(modGroup, "Difficulty");
        CreateReferenceTagRow(modGroup, "Rating");
        CreateReferenceTagRow(modGroup, "NewLineCharacter", @"\\n");
        CreateReferenceTagRow(modGroup, "TabCharacter", @"\\t");
        #endregion
    }
}