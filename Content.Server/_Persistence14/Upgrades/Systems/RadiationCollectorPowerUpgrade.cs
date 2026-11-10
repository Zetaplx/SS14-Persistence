using Content.Server.Singularity.Components;
using Content.Server.Singularity.EntitySystems;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class RadiationCollectorPowerUpgrade : BaseUpgradeSystem<RadiationCollectorComponent>
{
    [Dependency] private RadiationCollectorSystem _radiationCollector = default!;
    protected override ErrorCode ApplyUpgrade(Entity<RadiationCollectorComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        _radiationCollector.SetChargeModifier(entity.Owner, entity.Comp.ChargeModifier + prototype.UpgradeValues[0] * multipler, entity.Comp);
        return ErrorCode.SUCCESS;
    }
}