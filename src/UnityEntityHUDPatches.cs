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
        EntityHudRegistry.Register(__instance, entity);
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnDestroy")]
    public static void PostfixOnDestroy(UnitEntityHUD __instance)
    {
        EntityHudRegistry.Unregister(__instance);

        BarHelper.Unregister(__instance._headHealthBar);
        BarHelper.Unregister(__instance._headHealthBarBackground);
        BarHelper.Unregister(__instance._bodyHealthBar);
        BarHelper.Unregister(__instance._bodyHealthBarBackground);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnitEntityHUD.RefreshStatus))]
    public static void PostfixRefresh(UnitEntityHUD __instance, StatusReader status)
    {
        if (status == null) return;

        var entity = EntityHudRegistry.TryGet(__instance);
        var isFriendly = FactionHelper.IsFriendly(entity);

        var headCurrent = status.GetValue(StatType.HealthHead);
        var headMax = status.GetValue(StatType.MaxHealthHead);

        if (__instance._headHealthBar != null)
            BarHelper.UpdateColor(__instance._headHealthBar, headCurrent, headMax, isFriendly);

        if (__instance._headHealthBarBackground != null)
            BarHelper.UpdateColor(__instance._headHealthBarBackground, headCurrent, headMax, isFriendly);

        var bodyCurrent = status.GetValue(StatType.HealthBody);
        var bodyMax = status.GetValue(StatType.MaxHealthBody);

        if (__instance._bodyHealthBar != null)
            BarHelper.UpdateColor(__instance._bodyHealthBar, bodyCurrent, bodyMax, isFriendly);

        if (__instance._bodyHealthBarBackground != null)
            BarHelper.UpdateColor(__instance._bodyHealthBarBackground, bodyCurrent, bodyMax, isFriendly);
    }
}