using Refactor;

namespace ReactiveStatusBars;

public static class FactionHelper
{
    public static bool IsFriendly(IEntity entity)
    {
        if (entity == null) return false;

        var factionReader = entity.GetComponent<EntityInfoReader>();
        var faction = factionReader?.GetFaction();
        return faction is FactionType.Player or FactionType.Ally;
    }

    public static bool IsFriendly(UnitEntity unitEntity) => IsFriendly(unitEntity?.Cast<IEntity>());
}