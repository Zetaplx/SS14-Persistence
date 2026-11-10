using Content.Server.Power.EntitySystems;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Power.Components;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class BatteryCapacityUpgrade : BaseUpgradeSystem<BatteryComponent>
{
    [Dependency] private BatterySystem _battery = default!;

    protected override ErrorCode ApplyUpgrade(Entity<BatteryComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        _battery.SetMaxCharge(entity.AsNullable(), entity.Comp.MaxCharge + prototype.UpgradeValues[0] * multipler);
        return ErrorCode.SUCCESS;
    }
}