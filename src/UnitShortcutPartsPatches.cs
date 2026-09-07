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
        BarHelper.UnregisterOwner(__instance.GetInstanceID());
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SubUI_UnitShortCutParts.RefreshUnitStatus))]
    public static void PostfixRefresh(SubUI_UnitShortCutParts __instance, UnitEntity unitEntity)
    {
        var entity = unitEntity?.Cast<IEntity>();
        var status = entity?.GetComponent<StatusReader>();
        if (status == null) return;

        var ownerId = __instance.GetInstanceID();

        BarHelper.UpdateColor(__instance._headHealthBar,
            status.GetValue(StatType.HealthHead), status.GetValue(StatType.MaxHealthHead), ownerId);

        BarHelper.UpdateColor(__instance._bodyHealthBar,
            status.GetValue(StatType.HealthBody), status.GetValue(StatType.MaxHealthBody), ownerId);

        BarHelper.UpdateColor(__instance._energyBar,
            status.GetValue(StatType.Energy), status.GetValue(StatType.MaxEnergy), ownerId);

        BarHelper.UpdateColor(__instance._hungerBar,
            status.GetValue(StatType.Hunger), status.GetValue(StatType.MaxHunger), ownerId);
    }
}