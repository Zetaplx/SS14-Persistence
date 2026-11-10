using Content.Server.Shuttles.Systems;
using Content.Shared._Persistence14.Upgrades;
using Content.Shared._Persistence14.Upgrades.Prototypes;
using Content.Shared.Shuttles.Components;

namespace Content.Server._Persistence14.Upgrades;

public sealed partial class RadarRangeUpgrade : BaseUpgradeSystem<RadarConsoleComponent>
{
    [Dependency] private RadarConsoleSystem _radar = default!;

    protected override ErrorCode ApplyUpgrade(Entity<RadarConsoleComponent> entity, UpgradePrototype prototype, EntityUid user, float multipler = 1f)
    {
        _radar.SetRange(entity.Owner, entity.Comp.MaxRange + prototype.UpgradeValues[0], entity.Comp);

        return ErrorCode.SUCCESS;
    }
}