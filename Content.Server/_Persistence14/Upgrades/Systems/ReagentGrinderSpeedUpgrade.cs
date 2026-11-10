using Content.Server.Kitchen.EntitySystems;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Kitchen.Components;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class ReagentGrinderSpeedUpgrade : BaseUpgradeSystem<ReagentGrinderComponent>
{
    [Dependency] private ReagentGrinderSystem _reagentGrinder = default!;

    protected override ErrorCode ApplyUpgrade(Entity<ReagentGrinderComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        _reagentGrinder.SetSpeedMultiplier(entity, entity.Comp.WorkTimeMultiplier - prototype.UpgradeValues[0] * multipler);
        return ErrorCode.SUCCESS;
    }
}