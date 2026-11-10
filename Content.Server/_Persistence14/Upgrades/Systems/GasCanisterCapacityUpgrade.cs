using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Atmos.Piping.Unary.Components;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class GasCanisterCapacityUpgrade : BaseUpgradeSystem<GasCanisterComponent>
{
    protected override ErrorCode ApplyUpgrade(Entity<GasCanisterComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        entity.Comp.Air.Volume += prototype.UpgradeValues[0] * multipler;
        return ErrorCode.SUCCESS;
    }
}