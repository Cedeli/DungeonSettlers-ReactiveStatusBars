using HarmonyLib;
using Refactor.Main;
using Refactor.View;

namespace ReactiveStatusBars;

[HarmonyPatch(typeof(HUDObjectPool))]
public static class HUDObjectPoolPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HUDObjectPool.Release))]
    public static void PostfixRelease(EntityHUD obj)
    {
        if (obj == null) return;

        var id = obj.GetInstanceID();

        if (Plugin.Debug)
            Plugin.Log.LogInfo($"HUDObjectPool.Release hud id={id}");

        BarHelper.UnregisterOwner(id);
        EntityHudRegistry.UnregisterById(id);
    }
}