using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Atmos.Components;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class AirTankCapacityUpgrade : BaseUpgradeSystem<GasTankComponent>
{
    protected override ErrorCode ApplyUpgrade(Entity<GasTankComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        entity.Comp.Air.Volume += prototype.UpgradeValues[0] * multipler;
        return ErrorCode.SUCCESS;
    }
}