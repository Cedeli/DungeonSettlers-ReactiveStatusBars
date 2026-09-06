using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace ReactiveStatusBars;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("DungeonSettlers.exe")]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    private Harmony _harmony;

    public override void Load()
    {
        Log = base.Log;
        
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), MyPluginInfo.PLUGIN_GUID);
        
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
    
    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }
}
