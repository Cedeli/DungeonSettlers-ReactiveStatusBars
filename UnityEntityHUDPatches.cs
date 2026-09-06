using HarmonyLib;
using Refactor;
using Refactor.View;

namespace ReactiveStatusBars;

[HarmonyPatch(typeof(UnitEntityHUD))]
public static class UnitEntityHUDPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(UnitEntityHUD.RefreshStatus))]
    public static void PostfixRefresh(UnitEntityHUD __instance, StatusReader status)
    {
        if (status == null) return;

        var headCurrent = status.GetValue(StatType.HealthHead);
        var headMax = status.GetValue(StatType.MaxHealthHead);

        if (__instance._headHealthBar != null)
            BarHelper.UpdateColor(__instance._headHealthBar, headCurrent, headMax);

        if (__instance._headHealthBarBackground != null)
            BarHelper.UpdateColor(__instance._headHealthBarBackground, headCurrent, headMax);

        var bodyCurrent = status.GetValue(StatType.HealthBody);
        var bodyMax = status.GetValue(StatType.MaxHealthBody);

        if (__instance._bodyHealthBar != null)
            BarHelper.UpdateColor(__instance._bodyHealthBar, bodyCurrent, bodyMax);

        if (__instance._bodyHealthBarBackground != null)
            BarHelper.UpdateColor(__instance._bodyHealthBarBackground, bodyCurrent, bodyMax);
    }
}