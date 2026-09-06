using System.Collections.Generic;
using Refactor;
using Refactor.View;

namespace ReactiveStatusBars;

public static class EntityHudRegistry
{
    private static readonly Dictionary<int, IEntity> EntitiesByHud = new();

    public static void Register(UnitEntityHUD hud, IEntity entity)
    {
        if (hud == null) return;
        EntitiesByHud[hud.GetInstanceID()] = entity;
    }

    public static void Unregister(UnitEntityHUD hud)
    {
        if (hud == null) return;
        EntitiesByHud.Remove(hud.GetInstanceID());
    }

    public static IEntity TryGet(UnitEntityHUD hud)
    {
        return hud == null ? null : EntitiesByHud.GetValueOrDefault(hud.GetInstanceID());
    }
}