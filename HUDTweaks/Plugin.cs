using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

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
}