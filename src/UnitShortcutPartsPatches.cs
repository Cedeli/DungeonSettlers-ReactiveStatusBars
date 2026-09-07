using HarmonyLib;
using Refactor;
using Refactor.UI;

namespace ReactiveStatusBars;

[HarmonyPatch(typeof(SubUI_UnitShortCutParts))]
public class UnitShortcutPartsPatches
{
    [HarmonyPostfix]
    [HarmonyPatch("OnDestroy")]
    public static void PostfixOnDestroy(SubUI_UnitShortCutParts __instance)
    {
        BarHelper.Unregister(__instance._headHealthBar);
        BarHelper.Unregister(__instance._headHealthBarBackground);
        BarHelper.Unregister(__instance._bodyHealthBar);
        BarHelper.Unregister(__instance._bodyHealthBarBackground);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SubUI_UnitShortCutParts.RefreshUnitStatus))]
    public static void PostfixRefresh(SubUI_UnitShortCutParts __instance, UnitEntity unitEntity)
    {
        var entity = unitEntity?.Cast<IEntity>();
        var status = entity?.GetComponent<StatusReader>();
        if (status == null) return;

        BarHelper.UpdateColor(__instance._headHealthBar,
            status.GetValue(StatType.HealthHead), status.GetValue(StatType.MaxHealthHead));

        BarHelper.UpdateColor(__instance._bodyHealthBar,
            status.GetValue(StatType.HealthBody), status.GetValue(StatType.MaxHealthBody));

        BarHelper.UpdateColor(__instance._energyBar,
            status.GetValue(StatType.Energy), status.GetValue(StatType.MaxEnergy));

        BarHelper.UpdateColor(__instance._hungerBar,
            status.GetValue(StatType.Hunger), status.GetValue(StatType.MaxHunger));
    }
}