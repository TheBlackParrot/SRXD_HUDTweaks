using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace HUDTweaks;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private static ManualLogSource? _logger;
    private static Harmony _harmony = null!;

    private void Awake()
    {
        _logger = Logger;
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _logger.LogInfo("Plugin loaded");
    }

    private void OnEnable()
    {
        _harmony.PatchAll();
    }
}