using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace HUDTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("srxd.raoul1808.spincore", "1.1.2")]
public partial class Plugin : BaseUnityPlugin
{
    private static ManualLogSource? _logger;
    private static Harmony _harmony = null!;
    
    private const string TRANSLATION_PREFIX = $"{nameof(HUDTweaks)}_";

    private void Awake()
    {
        _logger = Logger;
        
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        RegisterConfigEntries();
        CreateModPage();
        
        _logger.LogInfo("Plugin loaded");
    }

    private void OnEnable()
    {
        _harmony.PatchAll();
    }

    private void OnDisable()
    {
        _harmony.UnpatchSelf();
    }
}