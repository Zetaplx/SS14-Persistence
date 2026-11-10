using Content.Server.Power.Generation.Teg;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class TegGeneratorPowerUpgrade : BaseUpgradeSystem<TegGeneratorComponent>
{
    [Dependency] private TegSystem _teg = default!;

    protected override ErrorCode ApplyUpgrade(Entity<TegGeneratorComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        _teg.SetPowerFactor(entity.Owner, entity.Comp, entity.Comp.PowerFactor + prototype.UpgradeValues[0] * multipler);
        return ErrorCode.SUCCESS;
    }
}