using HarmonyLib;
using Refactor;
using Refactor.View;

namespace ReactiveStatusBars;

[HarmonyPatch(typeof(UnitEntityHUD))]
public static class UnitEntityHUDPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnitEntityHUD.Init))]
    public static void PostfixInit(UnitEntityHUD __instance, IEntity entity)
    {
        var id = __instance.GetInstanceID();
        BarHelper.UnregisterOwner(id);
        EntityHudRegistry.Register(__instance, entity);
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnDestroy")]
    public static void PostfixOnDestroy(UnitEntityHUD __instance)
    {
        EntityHudRegistry.Unregister(__instance);
        BarHelper.UnregisterOwner(__instance.GetInstanceID());
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnitEntityHUD.RefreshStatus))]
    public static void PostfixRefresh(UnitEntityHUD __instance, StatusReader status)
    {
        if (status == null) return;

        EntityHudRegistry.TryGetIsFriendly(__instance, out var isFriendly);
        var ownerId = __instance.GetInstanceID();

        var headCurrent = status.GetValue(StatType.HealthHead);
        var headMax = status.GetValue(StatType.MaxHealthHead);

        BarHelper.UpdateColor(__instance._headHealthBar, headCurrent, headMax, ownerId, isFriendly);
        BarHelper.UpdateColor(__instance._headHealthBarBackground, headCurrent, headMax, ownerId, isFriendly);

        var bodyCurrent = status.GetValue(StatType.HealthBody);
        var bodyMax = status.GetValue(StatType.MaxHealthBody);

        BarHelper.UpdateColor(__instance._bodyHealthBar, bodyCurrent, bodyMax, ownerId, isFriendly);
        BarHelper.UpdateColor(__instance._bodyHealthBarBackground, bodyCurrent, bodyMax, ownerId, isFriendly);
    }
}