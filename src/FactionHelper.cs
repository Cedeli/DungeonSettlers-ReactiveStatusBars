using Refactor;

namespace ReactiveStatusBars;

public static class FactionHelper
{
    public static bool IsFriendly(IEntity entity)
    {
        var factionReader = entity?.GetComponent<EntityInfoReader>();
        if (factionReader == null) return false;

        return factionReader.GetFaction() == FactionType.Player;
    }

    public static bool IsFriendly(UnitEntity unitEntity) =>
        IsFriendly(unitEntity?.Cast<IEntity>());
}