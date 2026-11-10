using Content.Shared._Persistence14.Upgrades.Components;
using Content.Shared._Persistence14.Upgrades.Prototypes;

namespace Content.Shared._Persistence14.Upgrades;

public abstract partial class BaseUpgradeSystem : EntitySystem
{
    public abstract bool TryApplyUpgrade(Entity<UpgradeableComponent> entity, UpgradePrototype prototype, EntityUid user, float multiplier = 1f);
}

public abstract partial class BaseUpgradeSystem<TComp> : BaseUpgradeSystem where TComp : Component
{
    /// <summary>
    /// Attempts to apply a given upgrade prototype to a target upgradable entity. Returns whether the upgrade was successfully applied.
    /// </summary>
    public override bool TryApplyUpgrade(Entity<UpgradeableComponent> entity, UpgradePrototype prototype, EntityUid user, float multiplier = 1f)
    {
        if (!TryComp<TComp>(entity.Owner, out var comp))
            return false;

        return ApplyUpgrade((entity.Owner, comp), prototype, user, multiplier) == ErrorCode.SUCCESS;
    }

    protected abstract ErrorCode ApplyUpgrade(Entity<TComp> entity, UpgradePrototype prototype, EntityUid user, float multiplier = 1f);

    protected enum ErrorCode
    {
        SUCCESS = 0,
        FAILURE = 1
    }
}