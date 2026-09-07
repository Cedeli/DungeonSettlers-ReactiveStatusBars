using System.Collections.Generic;
using Refactor;
using Refactor.View;

namespace ReactiveStatusBars;

public static class EntityHudRegistry
{
    private readonly struct HudEntry(IEntity entity, bool isFriendly)
    {
        public readonly IEntity Entity = entity;
        public readonly bool IsFriendly = isFriendly;
    }

    private static readonly Dictionary<int, HudEntry> EntriesByHud = new();

    public static void Register(UnitEntityHUD hud, IEntity entity)
    {
        if (hud == null) return;

        var id = hud.GetInstanceID();
        var isFriendly = FactionHelper.IsFriendly(entity);
        EntriesByHud[id] = new HudEntry(entity, isFriendly);

        if (Plugin.Debug)
            Plugin.Log.LogInfo($"EntityHudRegistry.Register hud id={id} " +
                               $"entity={entity?.GetHashCode()} isFriendly={isFriendly} totalHuds={EntriesByHud.Count}");
    }

    public static void Unregister(UnitEntityHUD hud)
    {
        if (hud == null) return;
        EntriesByHud.Remove(hud.GetInstanceID());
    }

    public static void UnregisterById(int hudId)
    {
        var removed = EntriesByHud.Remove(hudId);
        
        if (Plugin.Debug && removed)
            Plugin.Log.LogInfo($"EntityHudRegistry.UnregisterById hud id={hudId} totalHuds={EntriesByHud.Count}");
    }

    public static IEntity TryGetEntity(UnitEntityHUD hud)
    {
        return hud != null && EntriesByHud.TryGetValue(hud.GetInstanceID(), out var entry)
            ? entry.Entity
            : null;
    }

    public static bool TryGetIsFriendly(UnitEntityHUD hud, out bool isFriendly)
    {
        if (hud != null && EntriesByHud.TryGetValue(hud.GetInstanceID(), out var entry))
        {
            isFriendly = entry.IsFriendly;
            return true;
        }

        isFriendly = false;
        return false;
    }
}