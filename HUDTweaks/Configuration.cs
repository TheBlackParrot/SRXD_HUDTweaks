using System;
using System.Globalization;
using System.Threading.Tasks;
using BepInEx.Configuration;
using HUDTweaks.Classes;
using SpinCore.Translation;
using SpinCore.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HUDTweaks;

public enum TimeMeasurement
{
    Time = 0,
    Beats = 1,
    Measures = 2
}

public partial class Plugin
{
    // note to self: BepInEx forces maximum values in the Color type to 1f, for. some reason
    
    private static readonly Vector3 BlueHUDColor = new(0f, 0.703f, 5.283f);
    private static readonly Vector3 RedHUDColor = new(2.996f, 0.063f, 0.157f);
    private static readonly Vector3 YellowHUDColor = new(4.977f, 0.859f, 0f);
    private static readonly Vector3 CyanHUDColor = new(Color.cyan.r, Color.cyan.g, Color.cyan.b); // close enough
    private static readonly Vector3 GreenHUDColor = new(Color.green.r, Color.green.g, Color.green.b); // close enough

    private static readonly Vector3 DefaultAccuracyMissColor = new(1f, 0f, 0.286f);
    private static readonly Vector3 DefaultAccuracyOkayColor = new(1f, 0.573f, 0f);
    private static readonly Vector3 DefaultAccuracyGoodColor = new(1f, 1f, 0f);
    private static readonly Vector3 DefaultAccuracyGreatColor = new(0.133f, 1f, 0f);
    private static readonly Vector3 DefaultAccuracyPerfectColor = new(0f, 1f, 1f);
    private static readonly Vector3 DefaultAccuracyPerfectPlusColor = new(1f, 0.733f, 1f);
    
    internal static ConfigEntry<bool> EnableAccuracyDisplay = null!;
    internal static ConfigEntry<bool> EnablePreciseHealth = null!;
    internal static ConfigEntry<bool> EnablePerfectPlusCount = null!;
    internal static ConfigEntry<bool> ShowOverbeatsAsMisses = null!;
    
    internal static ConfigEntry<Vector3> Multiplier1XColor = null!;
    internal static ConfigEntry<Vector3> Multiplier2XColor = null!;
    internal static ConfigEntry<Vector3> Multiplier3XColor = null!;
    internal static ConfigEntry<Vector3> Multiplier4XColor = null!;
    
    internal static ConfigEntry<Vector3> AccuracyMissColor = null!;
    internal static ConfigEntry<Vector3> AccuracyOkayColor = null!;
    internal static ConfigEntry<Vector3> AccuracyGoodColor = null!;
    internal static ConfigEntry<Vector3> AccuracyGreatColor = null!;
    internal static ConfigEntry<Vector3> AccuracyPerfectColor = null!;
    internal static ConfigEntry<Vector3> AccuracyPerfectPlusColor = null!;
    
    internal static ConfigEntry<Vector3> NumberColor = null!;
    internal static ConfigEntry<Vector3> TextColor = null!;
    internal static ConfigEntry<Vector3> TimeColor = null!;
    internal static ConfigEntry<Vector3> HealthColor = null!;
    internal static ConfigEntry<Vector3> PfcColor = null!;
    internal static ConfigEntry<Vector3> FcColor = null!;
    internal static ConfigEntry<Vector3> HurtColor = null!;
    
    internal static ConfigEntry<bool> EnableMultiplierBar = null!;
    internal static ConfigEntry<bool> EnableMultiplierText = null!;
    internal static ConfigEntry<bool> EnableCombo = null!;
    internal static ConfigEntry<bool> EnableHealthBar = null!;
    internal static ConfigEntry<bool> EnableScore = null!;
    internal static ConfigEntry<bool> EnableHurtFlashing = null!;
    internal static ConfigEntry<bool> EnableTrackStrips = null!;
    internal static ConfigEntry<bool> EnableWheelGrips = null!;

    internal static ConfigEntry<string> NonCustomCreditText = null!;
    internal static ConfigEntry<string> CustomCreditText = null!;
    internal static ConfigEntry<string> TrackInfoText = null!;
    
    internal static ConfigEntry<TimeMeasurement> TimeMeasurementType = null!;
    internal static ConfigEntry<bool> OffsetMeasureBeats = null!;

    internal static ConfigEntry<StrippedNoteTimingAccuracy> IgnoreAccuracyTypesThreshold = null!;
    
    internal static ConfigEntry<int> TrackInfoVerticalOffset = null!;
    internal static ConfigEntry<int> TimeBarWidth = null!;
    internal static ConfigEntry<int> MainHudVerticalOffset = null!;

    internal static ConfigEntry<int> MaximumAccuracyBarNotes = null!;

    private void RegisterConfigEntries()
    {
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}ModName", nameof(HUDTweaks));
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}Toggles", "Toggles");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}Colors", "Colors");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}MultiplierColors", "Multiplier Colors");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TimingColors", "Timing Judgement Colors");
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
        
        TimeMeasurementType = Config.Bind("General", nameof(TimeMeasurementType), TimeMeasurement.Time,
            "How to display time bar values");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(TimeMeasurementType)}", "Time measurement type");
        OffsetMeasureBeats = Config.Bind("General", nameof(OffsetMeasureBeats), true,
            "Offset beat counter by +1 when showing time in measures");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(OffsetMeasureBeats)}", "Start beats at 1");
        
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
        
        AccuracyMissColor = Config.Bind("Colors", nameof(AccuracyMissColor), DefaultAccuracyMissColor, 
            "Color for misses in the accuracy log");
        AccuracyOkayColor = Config.Bind("Colors", nameof(AccuracyOkayColor), DefaultAccuracyOkayColor, 
            "Color for Okay and Early timing judgements");
        AccuracyGoodColor = Config.Bind("Colors", nameof(AccuracyGoodColor), DefaultAccuracyGoodColor, 
            "Color for Good timing judgements");
        AccuracyGreatColor = Config.Bind("Colors", nameof(AccuracyGreatColor), DefaultAccuracyGreatColor, 
            "Color for Great timing judgements");
        AccuracyPerfectColor = Config.Bind("Colors", nameof(AccuracyPerfectColor), DefaultAccuracyPerfectColor, 
            "Color for Perfect timing judgements");
        AccuracyPerfectPlusColor = Config.Bind("Colors", nameof(AccuracyPerfectPlusColor), DefaultAccuracyPerfectPlusColor, 
            "Color for Perfect+ timing judgements");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(AccuracyMissColor)}", "Miss color");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(AccuracyOkayColor)}", "Okay/Early color");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(AccuracyGoodColor)}", "Good color");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(AccuracyGreatColor)}", "Great color");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(AccuracyPerfectColor)}", "Perfect color");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(AccuracyPerfectPlusColor)}", "Perfect+ color");
        
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
        HurtColor = Config.Bind("Colors", nameof(HurtColor), RedHUDColor, 
            "Color for the areas that flash when getting hurt");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(HurtColor)}", "Hurt flash color");
        PfcColor = Config.Bind("Colors", nameof(PfcColor), CyanHUDColor, 
            "Color for PFC elements");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(PfcColor)}", "PFC color");
        FcColor = Config.Bind("Colors", nameof(FcColor), GreenHUDColor, 
            "Color for FC elements");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(FcColor)}", "FC color");
        
        NonCustomCreditText = Config.Bind("Info", nameof(NonCustomCreditText), "from the %charter%",
            "Credit string for non-custom charts, %charter% turns into the pack the chart is from");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(NonCustomCreditText)}", "Non-custom credit string");
        CustomCreditText = Config.Bind("Info", nameof(CustomCreditText), "by %charter%",
            "Credit string for custom charts, %charter% turns into the credit given for said chart");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(CustomCreditText)}", "Custom credit string");
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
        
        MaximumAccuracyBarNotes = Config.Bind("General", nameof(MaximumAccuracyBarNotes), 8,
            "Amount of notes for the accuracy bar to keep track of");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(MaximumAccuracyBarNotes)}", "Accuracy bar note ticks");
        
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}OtherToggles", "Track + Hud > Position contains visibility options available in the base game");
        
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_Artist", "Who made the current track");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}TagDescription_Charter", "Who charted the chart being played, or the pack the chart is from");
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
    private const int COLOR_LABEL_PREFERRED_WIDTH = 300;

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
                _ = RefreshEverythingGuh();
            });
        #endregion
        
        #region EnableMultiplierText
        CustomGroup enableMultiplierTextGroup = UIHelper.CreateGroup(modGroup, "EnableMultiplierTextGroup");
        enableMultiplierTextGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableMultiplierTextGroup, nameof(EnableMultiplierText),
            $"{TRANSLATION_PREFIX}{nameof(EnableMultiplierText)}", EnableMultiplierText.Value, value =>
            {
                EnableMultiplierText.Value = value;
                _ = RefreshEverythingGuh();
            });
        #endregion
        
        #region EnableCombo
        CustomGroup enableComboGroup = UIHelper.CreateGroup(modGroup, "EnableComboGroup");
        enableComboGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableComboGroup, nameof(EnableCombo),
            $"{TRANSLATION_PREFIX}{nameof(EnableCombo)}", EnableCombo.Value, value =>
            {
                EnableCombo.Value = value;
                _ = RefreshEverythingGuh();
            });
        #endregion
        
        #region EnableScore
        CustomGroup enableScoreGroup = UIHelper.CreateGroup(modGroup, "EnableScoreGroup");
        enableScoreGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableScoreGroup, nameof(EnableScore),
            $"{TRANSLATION_PREFIX}{nameof(EnableScore)}", EnableScore.Value, value =>
            {
                EnableScore.Value = value;
                _ = RefreshEverythingGuh();
            });
        #endregion
        
        #region EnableHealthBar
        CustomGroup enableHealthBarGroup = UIHelper.CreateGroup(modGroup, "EnableHealthBarGroup");
        enableHealthBarGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableHealthBarGroup, nameof(EnableHealthBar),
            $"{TRANSLATION_PREFIX}{nameof(EnableHealthBar)}", EnableHealthBar.Value, value =>
            {
                EnableHealthBar.Value = value;
                _ = RefreshEverythingGuh();
            });
        #endregion
        
        #region EnableHurtFlashing
        CustomGroup enableHurtFlashingGroup = UIHelper.CreateGroup(modGroup, "EnableHurtFlashingGroup");
        enableHurtFlashingGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableHurtFlashingGroup, nameof(EnableHurtFlashing),
            $"{TRANSLATION_PREFIX}{nameof(EnableHurtFlashing)}", EnableHurtFlashing.Value, value =>
            {
                EnableHurtFlashing.Value = value;
                _ = RefreshEverythingGuh();
            });
        #endregion
        
        #region EnableTrackStrips
        CustomGroup enableTrackStripsGroup = UIHelper.CreateGroup(modGroup, "EnableTrackStripsGroup");
        enableTrackStripsGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableTrackStripsGroup, nameof(EnableTrackStrips),
            $"{TRANSLATION_PREFIX}{nameof(EnableTrackStrips)}", EnableTrackStrips.Value, value =>
            {
                EnableTrackStrips.Value = value;
                _ = RefreshEverythingGuh();
            });
        #endregion
        
        #region EnableWheelGrips
        CustomGroup enableWheelGripsGroup = UIHelper.CreateGroup(modGroup, "EnableWheelGripsGroup");
        enableWheelGripsGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableWheelGripsGroup, nameof(EnableWheelGrips),
            $"{TRANSLATION_PREFIX}{nameof(EnableWheelGrips)}", EnableWheelGrips.Value, value =>
            {
                EnableWheelGrips.Value = value;
                _ = RefreshEverythingGuh();
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
                _ = RefreshEverythingGuh();
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
                _ = RefreshEverythingGuh();
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
                _ = RefreshEverythingGuh();
            },
            () => new IntRange(-10, 11),
            v => v.ToString());
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "ColorsHeader", $"{TRANSLATION_PREFIX}Colors", false);
        
        #region NumberColor
        CustomGroup numberColorGroup = UIHelper.CreateGroup(modGroup, "NumberColorGroup");
        numberColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent numberColorLabel =
            UIHelper.CreateLabel(numberColorGroup, "NumberColorLabel", $"{TRANSLATION_PREFIX}{nameof(NumberColor)}");
        numberColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField numberColorInputR = UIHelper.CreateInputField(numberColorGroup, "NumberColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            NumberColor.Value = NumberColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        numberColorInputR.InputField.SetText(NumberColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField numberColorInputG = UIHelper.CreateInputField(numberColorGroup, "NumberColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            NumberColor.Value = NumberColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        numberColorInputG.InputField.SetText(NumberColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField numberColorInputB = UIHelper.CreateInputField(numberColorGroup, "NumberColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            NumberColor.Value = NumberColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        numberColorInputB.InputField.SetText(NumberColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region TextColors
        CustomGroup textColorGroup = UIHelper.CreateGroup(modGroup, "TextColorGroup");
        textColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent textColorLabel =
            UIHelper.CreateLabel(textColorGroup, "TextColorLabel", $"{TRANSLATION_PREFIX}{nameof(TextColor)}");
        textColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField textColorInputR = UIHelper.CreateInputField(textColorGroup, "TextColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TextColor.Value = TextColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        textColorInputR.InputField.SetText(TextColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField textColorInputG = UIHelper.CreateInputField(textColorGroup, "TextColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TextColor.Value = TextColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        textColorInputG.InputField.SetText(TextColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField textColorInputB = UIHelper.CreateInputField(textColorGroup, "TextColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TextColor.Value = TextColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        textColorInputB.InputField.SetText(TextColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region TimeColors
        CustomGroup timeColorGroup = UIHelper.CreateGroup(modGroup, "TimeColorGroup");
        timeColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent timeColorLabel =
            UIHelper.CreateLabel(timeColorGroup, "TimeColorLabel", $"{TRANSLATION_PREFIX}{nameof(TimeColor)}");
        timeColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField timeColorInputR = UIHelper.CreateInputField(timeColorGroup, "TimeColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TimeColor.Value = TimeColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        timeColorInputR.InputField.SetText(TimeColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField timeColorInputG = UIHelper.CreateInputField(timeColorGroup, "TimeColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TimeColor.Value = TimeColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        timeColorInputG.InputField.SetText(TimeColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField timeColorInputB = UIHelper.CreateInputField(timeColorGroup, "TimeColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            TimeColor.Value = TimeColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        timeColorInputB.InputField.SetText(TimeColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region HealthColors
        CustomGroup healthColorGroup = UIHelper.CreateGroup(modGroup, "HealthColorGroup");
        healthColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent healthColorLabel =
            UIHelper.CreateLabel(healthColorGroup, "HealthColorLabel", $"{TRANSLATION_PREFIX}{nameof(HealthColor)}");
        healthColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField healthColorInputR = UIHelper.CreateInputField(healthColorGroup, "HealthColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            HealthColor.Value = HealthColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        healthColorInputR.InputField.SetText(HealthColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField healthColorInputG = UIHelper.CreateInputField(healthColorGroup, "HealthColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            HealthColor.Value = HealthColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        healthColorInputG.InputField.SetText(HealthColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField healthColorInputB = UIHelper.CreateInputField(healthColorGroup, "HealthColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            HealthColor.Value = HealthColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        healthColorInputB.InputField.SetText(HealthColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region HurtColors
        CustomGroup hurtColorGroup = UIHelper.CreateGroup(modGroup, "HurtColorGroup");
        hurtColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent hurtColorLabel =
            UIHelper.CreateLabel(hurtColorGroup, "HurtColorLabel", $"{TRANSLATION_PREFIX}{nameof(HurtColor)}");
        hurtColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField hurtColorInputR = UIHelper.CreateInputField(hurtColorGroup, "HurtColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            HurtColor.Value = HurtColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        hurtColorInputR.InputField.SetText(HurtColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField hurtColorInputG = UIHelper.CreateInputField(hurtColorGroup, "HurtColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            HurtColor.Value = HurtColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        hurtColorInputG.InputField.SetText(HurtColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField hurtColorInputB = UIHelper.CreateInputField(hurtColorGroup, "HurtColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            HurtColor.Value = HurtColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        hurtColorInputB.InputField.SetText(HurtColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region PfcColors
        CustomGroup pfcColorGroup = UIHelper.CreateGroup(modGroup, "PfcColorGroup");
        pfcColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent pfcColorLabel =
            UIHelper.CreateLabel(pfcColorGroup, "PfcColorLabel", $"{TRANSLATION_PREFIX}{nameof(PfcColor)}");
        pfcColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField pfcColorInputR = UIHelper.CreateInputField(pfcColorGroup, "PfcColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            PfcColor.Value = PfcColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        pfcColorInputR.InputField.SetText(PfcColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField pfcColorInputG = UIHelper.CreateInputField(pfcColorGroup, "PfcColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            PfcColor.Value = PfcColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        pfcColorInputG.InputField.SetText(PfcColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField pfcColorInputB = UIHelper.CreateInputField(pfcColorGroup, "PfcColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            PfcColor.Value = PfcColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        pfcColorInputB.InputField.SetText(PfcColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region FcColors
        CustomGroup fcColorGroup = UIHelper.CreateGroup(modGroup, "FcColorGroup");
        fcColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent fcColorLabel =
            UIHelper.CreateLabel(fcColorGroup, "FcColorLabel", $"{TRANSLATION_PREFIX}{nameof(FcColor)}");
        fcColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField fcColorInputR = UIHelper.CreateInputField(fcColorGroup, "FcColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            FcColor.Value = FcColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        fcColorInputR.InputField.SetText(FcColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField fcColorInputG = UIHelper.CreateInputField(fcColorGroup, "FcColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            FcColor.Value = FcColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        fcColorInputG.InputField.SetText(FcColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField fcColorInputB = UIHelper.CreateInputField(fcColorGroup, "FcColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            FcColor.Value = FcColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        fcColorInputB.InputField.SetText(FcColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "MultiplierColorsHeader", $"{TRANSLATION_PREFIX}MultiplierColors", false);
        
        #region Multiplier1XColors
        CustomGroup multiplier1XColorGroup = UIHelper.CreateGroup(modGroup, "Multiplier1XColorGroup");
        multiplier1XColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent multiplier1XColorLabel =
            UIHelper.CreateLabel(multiplier1XColorGroup, "Multiplier1XColorLabel", $"{TRANSLATION_PREFIX}{nameof(Multiplier1XColor)}");
        multiplier1XColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField multiplier1XColorInputR = UIHelper.CreateInputField(multiplier1XColorGroup, "Multiplier1XColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier1XColor.Value = Multiplier1XColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier1XColorInputR.InputField.SetText(Multiplier1XColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier1XColorInputG = UIHelper.CreateInputField(multiplier1XColorGroup, "Multiplier1XColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier1XColor.Value = Multiplier1XColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier1XColorInputG.InputField.SetText(Multiplier1XColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier1XColorInputB = UIHelper.CreateInputField(multiplier1XColorGroup, "Multiplier1XColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier1XColor.Value = Multiplier1XColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier1XColorInputB.InputField.SetText(Multiplier1XColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region Multiplier2XColors
        CustomGroup multiplier2XColorGroup = UIHelper.CreateGroup(modGroup, "Multiplier2XColorGroup");
        multiplier2XColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent multiplier2XColorLabel =
            UIHelper.CreateLabel(multiplier2XColorGroup, "Multiplier2XColorLabel", $"{TRANSLATION_PREFIX}{nameof(Multiplier2XColor)}");
        multiplier2XColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField multiplier2XColorInputR = UIHelper.CreateInputField(multiplier2XColorGroup, "Multiplier2XColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier2XColor.Value = Multiplier2XColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier2XColorInputR.InputField.SetText(Multiplier2XColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier2XColorInputG = UIHelper.CreateInputField(multiplier2XColorGroup, "Multiplier2XColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier2XColor.Value = Multiplier2XColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier2XColorInputG.InputField.SetText(Multiplier2XColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier2XColorInputB = UIHelper.CreateInputField(multiplier2XColorGroup, "Multiplier2XColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier2XColor.Value = Multiplier2XColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier2XColorInputB.InputField.SetText(Multiplier2XColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region Multiplier3XColors
        CustomGroup multiplier3XColorGroup = UIHelper.CreateGroup(modGroup, "Multiplier3XColorGroup");
        multiplier3XColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent multiplier3XColorLabel =
            UIHelper.CreateLabel(multiplier3XColorGroup, "Multiplier3XColorLabel", $"{TRANSLATION_PREFIX}{nameof(Multiplier3XColor)}");
        multiplier3XColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField multiplier3XColorInputR = UIHelper.CreateInputField(multiplier3XColorGroup, "Multiplier3XColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier3XColor.Value = Multiplier3XColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier3XColorInputR.InputField.SetText(Multiplier3XColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier3XColorInputG = UIHelper.CreateInputField(multiplier3XColorGroup, "Multiplier3XColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier3XColor.Value = Multiplier3XColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier3XColorInputG.InputField.SetText(Multiplier3XColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier3XColorInputB = UIHelper.CreateInputField(multiplier3XColorGroup, "Multiplier3XColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier3XColor.Value = Multiplier3XColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier3XColorInputB.InputField.SetText(Multiplier3XColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region Multiplier4XColors
        CustomGroup multiplier4XColorGroup = UIHelper.CreateGroup(modGroup, "Multiplier4XColorGroup");
        multiplier4XColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent multiplier4XColorLabel =
            UIHelper.CreateLabel(multiplier4XColorGroup, "Multiplier4XColorLabel", $"{TRANSLATION_PREFIX}{nameof(Multiplier4XColor)}");
        multiplier4XColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField multiplier4XColorInputR = UIHelper.CreateInputField(multiplier4XColorGroup, "Multiplier4XColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier4XColor.Value = Multiplier4XColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier4XColorInputR.InputField.SetText(Multiplier4XColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier4XColorInputG = UIHelper.CreateInputField(multiplier4XColorGroup, "Multiplier4XColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier4XColor.Value = Multiplier4XColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier4XColorInputG.InputField.SetText(Multiplier4XColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField multiplier4XColorInputB = UIHelper.CreateInputField(multiplier4XColorGroup, "Multiplier4XColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Multiplier4XColor.Value = Multiplier4XColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        multiplier4XColorInputB.InputField.SetText(Multiplier4XColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "TimingColorsHeader", $"{TRANSLATION_PREFIX}TimingColors", false);
        
        #region AccuracyMissColors
        CustomGroup accuracyMissColorGroup = UIHelper.CreateGroup(modGroup, "AccuracyMissColorGroup");
        accuracyMissColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent accuracyMissColorLabel =
            UIHelper.CreateLabel(accuracyMissColorGroup, "AccuracyMissColorLabel", $"{TRANSLATION_PREFIX}{nameof(AccuracyMissColor)}");
        accuracyMissColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField accuracyMissColorInputR = UIHelper.CreateInputField(accuracyMissColorGroup, "AccuracyMissColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyMissColor.Value = AccuracyMissColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyMissColorInputR.InputField.SetText(AccuracyMissColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyMissColorInputG = UIHelper.CreateInputField(accuracyMissColorGroup, "AccuracyMissColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyMissColor.Value = AccuracyMissColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyMissColorInputG.InputField.SetText(AccuracyMissColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyMissColorInputB = UIHelper.CreateInputField(accuracyMissColorGroup, "AccuracyMissColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyMissColor.Value = AccuracyMissColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyMissColorInputB.InputField.SetText(AccuracyMissColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region AccuracyOkayColors
        CustomGroup accuracyOkayColorGroup = UIHelper.CreateGroup(modGroup, "AccuracyOkayColorGroup");
        accuracyOkayColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent accuracyOkayColorLabel =
            UIHelper.CreateLabel(accuracyOkayColorGroup, "AccuracyOkayColorLabel", $"{TRANSLATION_PREFIX}{nameof(AccuracyOkayColor)}");
        accuracyOkayColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField accuracyOkayColorInputR = UIHelper.CreateInputField(accuracyOkayColorGroup, "AccuracyOkayColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyOkayColor.Value = AccuracyOkayColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyOkayColorInputR.InputField.SetText(AccuracyOkayColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyOkayColorInputG = UIHelper.CreateInputField(accuracyOkayColorGroup, "AccuracyOkayColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyOkayColor.Value = AccuracyOkayColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyOkayColorInputG.InputField.SetText(AccuracyOkayColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyOkayColorInputB = UIHelper.CreateInputField(accuracyOkayColorGroup, "AccuracyOkayColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyOkayColor.Value = AccuracyOkayColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyOkayColorInputB.InputField.SetText(AccuracyOkayColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region AccuracyGoodColors
        CustomGroup accuracyGoodColorGroup = UIHelper.CreateGroup(modGroup, "AccuracyGoodColorGroup");
        accuracyGoodColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent accuracyGoodColorLabel =
            UIHelper.CreateLabel(accuracyGoodColorGroup, "AccuracyGoodColorLabel", $"{TRANSLATION_PREFIX}{nameof(AccuracyGoodColor)}");
        accuracyGoodColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField accuracyGoodColorInputR = UIHelper.CreateInputField(accuracyGoodColorGroup, "AccuracyGoodColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyGoodColor.Value = AccuracyGoodColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyGoodColorInputR.InputField.SetText(AccuracyGoodColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyGoodColorInputG = UIHelper.CreateInputField(accuracyGoodColorGroup, "AccuracyGoodColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyGoodColor.Value = AccuracyGoodColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyGoodColorInputG.InputField.SetText(AccuracyGoodColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyGoodColorInputB = UIHelper.CreateInputField(accuracyGoodColorGroup, "AccuracyGoodColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyGoodColor.Value = AccuracyGoodColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyGoodColorInputB.InputField.SetText(AccuracyGoodColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region AccuracyGreatColors
        CustomGroup accuracyGreatColorGroup = UIHelper.CreateGroup(modGroup, "AccuracyGreatColorGroup");
        accuracyGreatColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent accuracyGreatColorLabel =
            UIHelper.CreateLabel(accuracyGreatColorGroup, "AccuracyGreatColorLabel", $"{TRANSLATION_PREFIX}{nameof(AccuracyGreatColor)}");
        accuracyGreatColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField accuracyGreatColorInputR = UIHelper.CreateInputField(accuracyGreatColorGroup, "AccuracyGreatColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyGreatColor.Value = AccuracyGreatColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyGreatColorInputR.InputField.SetText(AccuracyGreatColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyGreatColorInputG = UIHelper.CreateInputField(accuracyGreatColorGroup, "AccuracyGreatColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyGreatColor.Value = AccuracyGreatColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyGreatColorInputG.InputField.SetText(AccuracyGreatColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyGreatColorInputB = UIHelper.CreateInputField(accuracyGreatColorGroup, "AccuracyGreatColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyGreatColor.Value = AccuracyGreatColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyGreatColorInputB.InputField.SetText(AccuracyGreatColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region AccuracyPerfectColors
        CustomGroup accuracyPerfectColorGroup = UIHelper.CreateGroup(modGroup, "AccuracyPerfectColorGroup");
        accuracyPerfectColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent accuracyPerfectColorLabel =
            UIHelper.CreateLabel(accuracyPerfectColorGroup, "AccuracyPerfectColorLabel", $"{TRANSLATION_PREFIX}{nameof(AccuracyPerfectColor)}");
        accuracyPerfectColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField accuracyPerfectColorInputR = UIHelper.CreateInputField(accuracyPerfectColorGroup, "AccuracyPerfectColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyPerfectColor.Value = AccuracyPerfectColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyPerfectColorInputR.InputField.SetText(AccuracyPerfectColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyPerfectColorInputG = UIHelper.CreateInputField(accuracyPerfectColorGroup, "AccuracyPerfectColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyPerfectColor.Value = AccuracyPerfectColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyPerfectColorInputG.InputField.SetText(AccuracyPerfectColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyPerfectColorInputB = UIHelper.CreateInputField(accuracyPerfectColorGroup, "AccuracyPerfectColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyPerfectColor.Value = AccuracyPerfectColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyPerfectColorInputB.InputField.SetText(AccuracyPerfectColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region AccuracyPerfectPlusColors
        CustomGroup accuracyPerfectPlusColorGroup = UIHelper.CreateGroup(modGroup, "AccuracyPerfectPlusColorGroup");
        accuracyPerfectPlusColorGroup.LayoutDirection = Axis.Horizontal;
        CustomTextComponent accuracyPerfectPlusColorLabel =
            UIHelper.CreateLabel(accuracyPerfectPlusColorGroup, "AccuracyPerfectPlusColorLabel", $"{TRANSLATION_PREFIX}{nameof(AccuracyPerfectPlusColor)}");
        accuracyPerfectPlusColorLabel.Transform.GetComponent<LayoutElement>().preferredWidth = COLOR_LABEL_PREFERRED_WIDTH;
        
        CustomInputField accuracyPerfectPlusColorInputR = UIHelper.CreateInputField(accuracyPerfectPlusColorGroup, "AccuracyPerfectPlusColorInputR", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyPerfectPlusColor.Value = AccuracyPerfectPlusColor.Value with { x = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyPerfectPlusColorInputR.InputField.SetText(AccuracyPerfectPlusColor.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyPerfectPlusColorInputG = UIHelper.CreateInputField(accuracyPerfectPlusColorGroup, "AccuracyPerfectPlusColorInputG", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyPerfectPlusColor.Value = AccuracyPerfectPlusColor.Value with { y = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyPerfectPlusColorInputG.InputField.SetText(AccuracyPerfectPlusColor.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField accuracyPerfectPlusColorInputB = UIHelper.CreateInputField(accuracyPerfectPlusColorGroup, "AccuracyPerfectPlusColorInputB", (s, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            AccuracyPerfectPlusColor.Value = AccuracyPerfectPlusColor.Value with { z = Math.Max(value, 0f) };
            _ = RefreshEverythingGuh();
        });
        accuracyPerfectPlusColorInputB.InputField.SetText(AccuracyPerfectPlusColor.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "ExtrasHeader", $"{TRANSLATION_PREFIX}Extras", false);
        
        #region EnableAccuracyDisplay
        CustomGroup enableAccuracyDisplayGroup = UIHelper.CreateGroup(modGroup, "EnableAccuracyDisplayGroup");
        enableAccuracyDisplayGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enableAccuracyDisplayGroup, nameof(EnableAccuracyDisplay),
            $"{TRANSLATION_PREFIX}{nameof(EnableAccuracyDisplay)}", EnableAccuracyDisplay.Value, value =>
            {
                EnableAccuracyDisplay.Value = value;
                _ = RefreshEverythingGuh();
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
                _ = RefreshEverythingGuh();
            });
        #endregion
        
        #region OffsetMeasureBeats
        CustomGroup offsetMeasureBeatsGroup = UIHelper.CreateGroup(modGroup, "OffsetMeasureBeatsGroup");
        offsetMeasureBeatsGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(offsetMeasureBeatsGroup, nameof(OffsetMeasureBeats),
            $"{TRANSLATION_PREFIX}{nameof(OffsetMeasureBeats)}", OffsetMeasureBeats.Value, value =>
            {
                OffsetMeasureBeats.Value = value;
            });
        offsetMeasureBeatsGroup.GameObject.SetActive(TimeMeasurementType.Value is TimeMeasurement.Measures);
        #endregion
        
        #region TimeMeasurementType
        CustomGroup timeMeasurementTypeGroup = UIHelper.CreateGroup(modGroup, "TimeMeasurementTypeGroup");
        UIHelper.CreateSmallMultiChoiceButton(timeMeasurementTypeGroup, nameof(TimeMeasurementType),
            $"{TRANSLATION_PREFIX}{nameof(TimeMeasurementType)}", TimeMeasurementType.Value, value =>
            {
                TimeMeasurementType.Value = value;
                offsetMeasureBeatsGroup.GameObject.SetActive(TimeMeasurementType.Value is TimeMeasurement.Measures);
            });
        timeMeasurementTypeGroup.Transform.SetSiblingIndex(offsetMeasureBeatsGroup.Transform.GetSiblingIndex());
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
        
        #region MaximumAccuracyBarNotes
        CustomGroup maximumAccuracyBarNotesGroup = UIHelper.CreateGroup(modGroup, "MaximumAccuracyBarNotesGroup");
        UIHelper.CreateSmallMultiChoiceButton(maximumAccuracyBarNotesGroup, nameof(MaximumAccuracyBarNotes), $"{TRANSLATION_PREFIX}{nameof(MaximumAccuracyBarNotes)}",
            MaximumAccuracyBarNotes.Value, (value) =>
            {
                MaximumAccuracyBarNotes.Value = value;
                Task.Run(async () =>
                {
                    try
                    {
                        await Awaitable.MainThreadAsync();
                        await UpdateAccuracyBar();
                    }
                    catch (Exception e)
                    {
                        Log.LogError(e);
                    }
                });
            },
            () => new IntRange(1, 101), // we need 101 to hit 100, wtf
            v => v.ToString());
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "TrackInfoHeader", $"{TRANSLATION_PREFIX}{nameof(TrackInfoText)}", false);
        
        #region NonCustomCreditText
        CustomGroup nonCustomCreditTextGroup = UIHelper.CreateGroup(modGroup, "NonCustomCreditTextGroup");
        nonCustomCreditTextGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(nonCustomCreditTextGroup, "NonCustomCreditTextLabel", $"{TRANSLATION_PREFIX}{nameof(NonCustomCreditText)}");
        
        CustomInputField nonCustomCreditTextInput = UIHelper.CreateInputField(nonCustomCreditTextGroup, "NonCustomCreditTextInput", (_, value) =>
        {
            NonCustomCreditText.Value = value;
            
            Task.Run(async () =>
            {
                try
                {
                    await RefreshEverythingGuh();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                }
            });
        });
        nonCustomCreditTextInput.InputField.SetText(NonCustomCreditText.Value);
        #endregion
        
        #region CustomCreditText
        CustomGroup customCreditTextGroup = UIHelper.CreateGroup(modGroup, "CustomCreditTextGroup");
        customCreditTextGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(customCreditTextGroup, "CustomCreditTextLabel", $"{TRANSLATION_PREFIX}{nameof(CustomCreditText)}");
        
        CustomInputField customCreditTextInput = UIHelper.CreateInputField(customCreditTextGroup, "CustomCreditTextInput", (_, value) =>
        {
            CustomCreditText.Value = value;
            
            Task.Run(async () =>
            {
                try
                {
                    await RefreshEverythingGuh();
                }
                catch (Exception ex)
                {
                    Log.LogError(ex);
                }
            });
        });
        customCreditTextInput.InputField.SetText(CustomCreditText.Value);
        #endregion
        
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
                    await RefreshEverythingGuh();
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