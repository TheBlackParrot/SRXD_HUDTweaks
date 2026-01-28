using BepInEx.Configuration;
using SpinCore.Translation;
using SpinCore.UI;
using UnityEngine;

namespace HUDTweaks;

public partial class Plugin
{
    internal static ConfigEntry<bool> EnablePerfectPlusCount = null!;

    private void RegisterConfigEntries()
    {
        EnablePerfectPlusCount = Config.Bind("General", nameof(EnablePerfectPlusCount), false,
            "Show Perfect+ count beside accuracy");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(EnablePerfectPlusCount)}", "Show Perfect+ count beside accuracy");
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
    }
}